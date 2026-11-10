using System;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace KeeperWpf;

public class FlexibleNumericTextBox : TextBox
{
    private static readonly NumberStyles NumberStyle =
        NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint;

    private bool _updatingValueFromText;
    private bool _updatingTextFromValue;

    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        nameof(Value),
        typeof(object),
        typeof(FlexibleNumericTextBox),
        new FrameworkPropertyMetadata(
            null,
            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
            ValueChanged));

    public object? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public FlexibleNumericTextBox()
    {
        DataObject.AddPastingHandler(this, Pasting);
    }

    protected override void OnInitialized(EventArgs e)
    {
        base.OnInitialized(e);
        UpdateText(Value);
    }

    protected override void OnPreviewTextInput(TextCompositionEventArgs e)
    {
        e.Handled = !IsPotentiallyValid(GetTextAfterInput(e.Text));
        base.OnPreviewTextInput(e);
    }

    protected override void OnTextChanged(TextChangedEventArgs e)
    {
        base.OnTextChanged(e);

        if (_updatingTextFromValue || !TryParse(Text, out var value))
            return;

        _updatingValueFromText = true;
        SetCurrentValue(ValueProperty, value);
        _updatingValueFromText = false;
    }

    protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
    {
        base.OnLostKeyboardFocus(e);

        if (TryParse(Text, out var value))
        {
            _updatingValueFromText = true;
            SetCurrentValue(ValueProperty, value);
            _updatingValueFromText = false;
            UpdateText(value);
        }
        else
        {
            UpdateText(Value);
        }
    }

    private static void ValueChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
        var textBox = (FlexibleNumericTextBox)dependencyObject;
        if (textBox._updatingValueFromText || textBox.IsKeyboardFocusWithin)
            return;

        textBox.UpdateText(e.NewValue);
    }

    private void Pasting(object sender, DataObjectPastingEventArgs e)
    {
        if (!e.SourceDataObject.GetDataPresent(DataFormats.UnicodeText) ||
            e.SourceDataObject.GetData(DataFormats.UnicodeText) is not string pastedText ||
            !IsPotentiallyValid(GetTextAfterInput(pastedText)))
        {
            e.CancelCommand();
        }
    }

    private string GetTextAfterInput(string input)
    {
        var start = SelectionStart;
        return Text.Remove(start, SelectionLength).Insert(start, input);
    }

    private void UpdateText(object? value)
    {
        var format = value switch
        {
            decimal => "0.############################",
            float => "0.#######",
            _ => "0.################"
        };
        var formatted = value is IFormattable formattable
            ? formattable.ToString(format, CultureInfo.InvariantCulture)
            : string.Empty;
        if (Text == formatted)
            return;

        _updatingTextFromValue = true;
        Text = formatted;
        CaretIndex = Text.Length;
        _updatingTextFromValue = false;
    }

    private static bool IsPotentiallyValid(string text)
    {
        if (string.IsNullOrEmpty(text) || text is "+" or "-" or "." or "," or "+." or "+," or "-." or "-,")
            return true;

        var separatorFound = false;
        for (var index = 0; index < text.Length; index++)
        {
            var character = text[index];
            if (char.IsDigit(character))
                continue;

            if ((character == '.' || character == ',') && !separatorFound)
            {
                separatorFound = true;
                continue;
            }

            if ((character == '+' || character == '-') && index == 0)
                continue;

            return false;
        }

        return true;
    }

    private bool TryParse(string text, out object value)
    {
        var normalized = text.Replace(',', '.');
        var numericType = GetNumericType();

        if (numericType == typeof(decimal) &&
            decimal.TryParse(normalized, NumberStyle, CultureInfo.InvariantCulture, out var decimalValue))
        {
            value = decimalValue;
            return true;
        }

        if (numericType == typeof(float) &&
            float.TryParse(normalized, NumberStyle, CultureInfo.InvariantCulture, out var floatValue) &&
            float.IsFinite(floatValue))
        {
            value = floatValue;
            return true;
        }

        if (double.TryParse(normalized, NumberStyle, CultureInfo.InvariantCulture, out var doubleValue) &&
            double.IsFinite(doubleValue))
        {
            value = doubleValue;
            return true;
        }

        value = default(double);
        return false;
    }

    private Type GetNumericType()
    {
        if (Value is not null)
            return Value.GetType();

        var bindingExpression = BindingOperations.GetBindingExpression(this, ValueProperty);
        var sourcePropertyName = bindingExpression?.ResolvedSourcePropertyName;
        if (bindingExpression?.DataItem is not null && !string.IsNullOrEmpty(sourcePropertyName))
        {
            var resolvedSourceProperty = bindingExpression.DataItem.GetType().GetProperty(
                sourcePropertyName,
                BindingFlags.Instance | BindingFlags.Public);
            if (resolvedSourceProperty is not null)
                return Nullable.GetUnderlyingType(resolvedSourceProperty.PropertyType) ?? resolvedSourceProperty.PropertyType;
        }

        var binding = BindingOperations.GetBinding(this, ValueProperty);
        object? source = binding?.Source ?? DataContext;
        Type? propertyType = null;
        foreach (var pathPart in binding?.Path?.Path.Split('.') ?? [])
        {
            if (source is null)
                break;

            var sourceProperty = source.GetType().GetProperty(pathPart, BindingFlags.Instance | BindingFlags.Public);
            if (sourceProperty is null)
                break;

            propertyType = sourceProperty.PropertyType;
            source = sourceProperty.GetValue(source);
        }

        return Nullable.GetUnderlyingType(propertyType ?? typeof(double)) ?? propertyType ?? typeof(double);
    }
}
