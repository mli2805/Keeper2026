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
        NotifyOfPropertyChange(nameof(IsSet));
        NotifyOfPropertyChange(nameof(IsNotSet));
        NotifyOfPropertyChange(nameof(Value));
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
                SetValue(GetValue() ?? 0.0);
            }
            NotifyOfPropertyChange(nameof(Value));
        }
    }

    public bool IsNotSet
    {
        get => !_isSet;
        set => IsSet = !value;
    }

    public double? Value
    {
        get => _isSet ? GetValue() : null;
        set
        {
            if (!_isSet || !value.HasValue) return;
            SetValue(value.Value);
            NotifyOfPropertyChange();
        }
    }

    private double? GetValue()
    {
        var raw = _property.GetValue(_owner);
        if (raw == null) return null;
        return System.Convert.ToDouble(raw, CultureInfo.InvariantCulture);
    }

    private void SetValue(double? value)
    {
        object? converted = value.HasValue ? ConvertToTarget(value.Value) : null;
        var current = _property.GetValue(_owner);
        if (Equals(current, converted)) return;
        _property.SetValue(_owner, converted);
    }

    private object ConvertToTarget(double value)
    {
        var underlying = Nullable.GetUnderlyingType(_property.PropertyType) ?? _property.PropertyType;
        if (underlying == typeof(decimal)) return (decimal)value;
        if (underlying == typeof(double)) return value;
        if (underlying == typeof(float)) return (float)value;
        if (underlying == typeof(int)) return (int)Math.Round(value, MidpointRounding.AwayFromZero);
        if (underlying == typeof(long)) return (long)Math.Round(value, MidpointRounding.AwayFromZero);
        return System.Convert.ChangeType(value, underlying, CultureInfo.InvariantCulture);
    }

    private void OwnerPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != _propertyName) return;
        RefreshFromOwner();
    }
}
