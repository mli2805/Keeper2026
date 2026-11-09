using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using Caliburn.Micro;

namespace KeeperWpf;

public class NullableNumericControlViewModel : PropertyChangedBase
{
    private object _owner = null!;
    private INotifyPropertyChanged? _notifier;
    private PropertyInfo _property = null!;
    private string _propertyName = null!;

    public string Explanation { get; }

    public NullableNumericControlViewModel(object owner, string propertyName, string explanation)
    {
        Explanation = explanation;
        SetOwner(owner, propertyName);
    }

    private NullableNumericControlViewModel(string explanation)
    {
        Explanation = explanation;
    }

    public void SetOwner(object owner, string propertyName)
    {
        if (_notifier != null)
            _notifier.PropertyChanged -= OwnerPropertyChanged;

        _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        _propertyName = propertyName;
        _property = owner.GetType().GetProperty(propertyName)
            ?? throw new ArgumentException($"Свойство '{propertyName}' не найдено у {owner.GetType().Name}.");

        _notifier = _owner as INotifyPropertyChanged;
        if (_notifier != null)
            _notifier.PropertyChanged += OwnerPropertyChanged;

        RefreshFromOwner();
    }

    public void SwitchModel(object owner) => SetOwner(owner, _propertyName);

    private void RefreshFromOwner()
    {
        var v = GetValue();
        _isSet = v.HasValue;
        _text = v.HasValue ? FormatValue(v.Value) : "";
        NotifyOfPropertyChange(nameof(IsSet));
        NotifyOfPropertyChange(nameof(IsNotSet));
        NotifyOfPropertyChange(nameof(Text));
        NotifyOfPropertyChange(nameof(DisplayText));
    }

    private bool _isSet;
    public bool IsSet
    {
        get => _isSet;
        set
        {
            if (value == _isSet) return;
            _isSet = value;
            NotifyOfPropertyChange();
            NotifyOfPropertyChange(nameof(IsNotSet));

            if (!_isSet)
            {
                SetValue(null);
            }
            else
            {
                if (!TryParse(_text, out var parsed))
                {
                    parsed = GetValue() ?? 0m;
                    _text = FormatValue(parsed);
                }
                SetValue(parsed);
            }
            NotifyOfPropertyChange(nameof(Text));
            NotifyOfPropertyChange(nameof(DisplayText));
        }
    }

    public bool IsNotSet
    {
        get => !_isSet;
        set => IsSet = !value;
    }

    private string _text = "";
    public string Text
    {
        get => _isSet ? _text : "";
        set
        {
            if (!_isSet) return;
            var cleaned = (value ?? "").Replace('.', ',');
            if (cleaned == _text) return;
            _text = cleaned;
            NotifyOfPropertyChange();
            NotifyOfPropertyChange(nameof(DisplayText));

            if (TryParse(cleaned, out var parsed))
                SetValue(parsed);
        }
    }

    public string DisplayText => Text;

    private decimal? GetValue()
    {
        var raw = _property.GetValue(_owner);
        if (raw == null) return null;
        return System.Convert.ToDecimal(raw, CultureInfo.InvariantCulture);
    }

    private void SetValue(decimal? value)
    {
        object? converted = value.HasValue ? ConvertToTarget(value.Value) : null;
        var current = _property.GetValue(_owner);
        if (Equals(current, converted)) return;
        _property.SetValue(_owner, converted);
    }

    private object ConvertToTarget(decimal value)
    {
        var underlying = Nullable.GetUnderlyingType(_property.PropertyType) ?? _property.PropertyType;
        if (underlying == typeof(decimal)) return value;
        if (underlying == typeof(double)) return (double)value;
        if (underlying == typeof(float)) return (float)value;
        if (underlying == typeof(int)) return (int)Math.Round(value, MidpointRounding.AwayFromZero);
        if (underlying == typeof(long)) return (long)Math.Round(value, MidpointRounding.AwayFromZero);
        return System.Convert.ChangeType(value, underlying, CultureInfo.InvariantCulture);
    }

    private static bool TryParse(string text, out decimal result)
    {
        result = 0;
        if (string.IsNullOrWhiteSpace(text)) return false;
        return decimal.TryParse(text, NumberStyles.Number, CultureInfo.CurrentUICulture, out result);
    }

    private static string FormatValue(decimal value)
        => value.ToString("0.##", CultureInfo.CurrentUICulture);

    private void OwnerPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != _propertyName) return;
        RefreshFromOwner();
    }
}
