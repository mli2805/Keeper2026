using System.Globalization;
using System.IO;
using System.Reflection;
using CsvHelper;
using KeeperDomain;

namespace KeeperInfrastructure;

internal sealed class KeeperDomainModelCsvConfiguration
{
    private const string NullValue = "\\N";
    private static readonly CultureInfo Culture = CultureInfo.InvariantCulture;
    private readonly ValueMap _root;

    internal static readonly (PropertyInfo Property, KeeperDomainModelCsvConfiguration Mapping)[] Collections =
        typeof(KeeperDomainModel).GetProperties()
            .Where(property => property.PropertyType.IsGenericType &&
                property.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
            .Select(property => (property, new KeeperDomainModelCsvConfiguration(property.PropertyType.GenericTypeArguments[0])))
            .ToArray();

    internal string[] Headers { get; }

    private KeeperDomainModelCsvConfiguration(Type type)
    {
        var headers = new List<string>();
        _root = new ValueMap(type, null, "", headers);
        Headers = headers.ToArray();
    }

    internal void Write(CsvWriter writer, object record)
    {
        ArgumentNullException.ThrowIfNull(record);
        _root.Write(writer, record);
    }

    internal object Read(CsvReader reader, int[] indices)
    {
        var fields = indices.Select(reader.GetString).ToArray();
        return _root.Read(fields, true)!;
    }

    private sealed class ValueMap
    {
        private readonly Type _type;
        private readonly Type _valueType;
        private readonly MemberInfo? _member;
        private readonly string _name;
        private readonly int _index;
        private readonly int _count;
        private readonly ValueMap[]? _children;

        internal ValueMap(Type type, MemberInfo? member, string name, List<string> headers)
        {
            _type = type;
            _valueType = Nullable.GetUnderlyingType(type) ?? type;
            _member = member;
            _name = name;
            _index = headers.Count;

            if (_valueType.IsPrimitive || _valueType.IsEnum || _valueType == typeof(string) ||
                _valueType == typeof(decimal) || _valueType == typeof(DateTime) ||
                _valueType == typeof(DateOnly) || _valueType == typeof(List<int>))
            {
                headers.Add(name);
            }
            else
            {
                _children = type.GetMembers(BindingFlags.Instance | BindingFlags.Public)
                    .Where(memberInfo => memberInfo is PropertyInfo property &&
                        property.GetMethod?.IsPublic == true && property.SetMethod?.IsPublic == true &&
                        property.GetIndexParameters().Length == 0 ||
                        memberInfo is FieldInfo { IsInitOnly: false, IsLiteral: false })
                    .Select(memberInfo => new ValueMap(
                        memberInfo is PropertyInfo property ? property.PropertyType : ((FieldInfo)memberInfo).FieldType,
                        memberInfo, name.Length == 0 ? memberInfo.Name : name + "." + memberInfo.Name, headers))
                    .ToArray();

                if (_children.Length == 0)
                {
                    throw new NotSupportedException($"No writable CSV members found for '{type.FullName}'.");
                }
            }

            _count = headers.Count - _index;
        }

        internal void Write(CsvWriter writer, object? value)
        {
            if (_children == null)
            {
                writer.WriteField(Format(value));
                return;
            }

            foreach (var child in _children)
            {
                var childValue = value == null ? null : child._member is PropertyInfo property
                    ? property.GetValue(value) : ((FieldInfo)child._member!).GetValue(value);
                child.Write(writer, childValue);
            }
        }

        internal object? Read(string[] fields, bool root = false)
        {
            if (_children == null)
            {
                try
                {
                    return Parse(fields[_index]);
                }
                catch (Exception error) when (error is FormatException or OverflowException or ArgumentException)
                {
                    throw new InvalidDataException($"Invalid CSV value for column '{_name}' ({_type.Name}).", error);
                }
            }

            // A null nested object has the null marker in every one of its columns.
            if (!root && Enumerable.Range(_index, _count).All(index => fields[index] == NullValue))
            {
                return null;
            }

            var result = Activator.CreateInstance(_type)!;
            foreach (var child in _children)
            {
                var value = child.Read(fields);
                if (child._member is PropertyInfo property)
                {
                    property.SetValue(result, value);
                }
                else
                {
                    ((FieldInfo)child._member!).SetValue(result, value);
                }
            }

            return result;
        }

        private static string Format(object? value)
        {
            return value switch
            {
                null => NullValue,
                // Escape leading backslashes so a literal null marker remains ordinary text.
                string text => text.StartsWith('\\') ? "\\" + text : text,
                DateTime date => date.ToString("O", Culture),
                DateOnly date => date.ToString("yyyy-MM-dd", Culture),
                double number => number.ToString("R", Culture),
                float number => number.ToString("R", Culture),
                List<int> ids => string.Join("|", ids.Select(id => id.ToString(Culture))),
                Enum enumeration => enumeration.ToString("D"),
                IFormattable formattable => formattable.ToString(null, Culture),
                _ => value.ToString()!
            };
        }

        private object? Parse(string text)
        {
            if (text == NullValue)
            {
                if (!_type.IsValueType || Nullable.GetUnderlyingType(_type) != null)
                {
                    return null;
                }

                throw new FormatException("Null is not allowed for this value type.");
            }

            if (_valueType == typeof(string))
            {
                return text.StartsWith("\\\\", StringComparison.Ordinal) ? text[1..] : text;
            }

            if (_valueType == typeof(List<int>))
            {
                return text.Length == 0 ? new List<int>() : text.Split('|')
                    .Select(value => int.Parse(value, NumberStyles.Integer, Culture)).ToList();
            }

            if (_valueType == typeof(DateTime))
            {
                return DateTime.ParseExact(text, "O", Culture, DateTimeStyles.RoundtripKind);
            }

            if (_valueType == typeof(DateOnly))
            {
                return DateOnly.ParseExact(text, "yyyy-MM-dd", Culture);
            }

            if (_valueType.IsEnum)
            {
                return Enum.Parse(_valueType, text);
            }

            if (_valueType == typeof(double))
            {
                return double.Parse(text, NumberStyles.Float, Culture);
            }

            if (_valueType == typeof(float))
            {
                return float.Parse(text, NumberStyles.Float, Culture);
            }

            if (_valueType == typeof(decimal))
            {
                return decimal.Parse(text, NumberStyles.Float, Culture);
            }

            return Convert.ChangeType(text, _valueType, Culture);
        }
    }
}
