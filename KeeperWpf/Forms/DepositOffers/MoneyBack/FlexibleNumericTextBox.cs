using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
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
        typeof(double?),
        typeof(FlexibleNumericTextBox),
        new FrameworkPropertyMetadata(
            null,
            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
            ValueChanged));

    public double? Value
    {
        get => (double?)GetValue(ValueProperty);
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
        SetCurrentValue(ValueProperty, (double?)value);
        _updatingValueFromText = false;
    }

    protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
    {
        base.OnLostKeyboardFocus(e);

        if (TryParse(Text, out var value))
        {
            _updatingValueFromText = true;
            SetCurrentValue(ValueProperty, (double?)value);
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

        textBox.UpdateText((double?)e.NewValue);
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

    private void UpdateText(double? value)
    {
        var formatted = value?.ToString("0.################", CultureInfo.InvariantCulture) ?? string.Empty;
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

    private static bool TryParse(string text, out double value)
    {
        var normalized = text.Replace(',', '.');
        return double.TryParse(normalized, NumberStyle, CultureInfo.InvariantCulture, out value) &&
               double.IsFinite(value);
    }
}
