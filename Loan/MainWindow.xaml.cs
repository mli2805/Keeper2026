using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Loan;

public partial class MainWindow : Window
{
    private static readonly CultureInfo Culture = CultureInfo.GetCultureInfo("ru-RU");
    private static readonly string SavePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Loan", "loan.txt");

    private bool _isLoading;

    public MainWindow()
    {
        InitializeComponent();
        LoadData();
        Recalculate();
    }

    private void OnInputChanged(object sender, RoutedEventArgs e)
    {
        if (_isLoading) return;
        Recalculate();
        SaveData();
    }

    private void Recalculate()
    {
        Loan1BodyText.Text = string.Empty;
        Loan1InterestText.Text = string.Empty;
        Loan1TotalText.Text = string.Empty;
        Loan2BodyText.Text = string.Empty;
        Loan2InterestText.Text = string.Empty;
        Loan2TotalText.Text = string.Empty;
        Loan3BodyText.Text = string.Empty;
        Loan3InterestText.Text = string.Empty;
        Loan3TotalText.Text = string.Empty;
        Deposit1ResultText.Text = string.Empty;
        Deposit2ResultText.Text = string.Empty;
        Deposit3ResultText.Text = string.Empty;

        if (!TryParseDecimal(AmountBox.Text, out var amount) || amount <= 0) return;
        if (OpenDatePicker.SelectedDate is not { } openDate) return;

        var hasTerm = int.TryParse(TermYearsBox.Text, out var termYears) && termYears > 0;
        if (hasTerm)
        {
            RecalculateLoan(amount, openDate, termYears, 1, Grace1RateBox, Loan1BodyText, Loan1InterestText, Loan1TotalText);
            RecalculateLoan(amount, openDate, termYears, 2, Grace2RateBox, Loan2BodyText, Loan2InterestText, Loan2TotalText);
            RecalculateLoan(amount, openDate, termYears, 3, Grace3RateBox, Loan3BodyText, Loan3InterestText, Loan3TotalText);
        }

        if (TryParseDecimal(DepositRateBox.Text, out var depositRate) && depositRate >= 0)
        {
            Deposit1ResultText.Text = FormatDeposit(amount, openDate, 1, depositRate);
            Deposit2ResultText.Text = FormatDeposit(amount, openDate, 2, depositRate);
            Deposit3ResultText.Text = FormatDeposit(amount, openDate, 3, depositRate);
        }
    }

    private static void RecalculateLoan(decimal amount, DateTime openDate, int termYears,
        int graceYears, TextBox graceRateBox, TextBlock bodyText, TextBlock interestText, TextBlock totalText)
    {
        if (!TryParseDecimal(graceRateBox.Text, out var graceRate) || graceRate < 0) return;

        var result = LoanCalculator.Calculate(amount, openDate, termYears, graceYears, graceRate);
        bodyText.Text = $"Тело кредита: {result.BodyPaid:N2}";
        interestText.Text = $"Проценты: {result.InterestPaid:N2}";
        totalText.Text = $"Итого: {result.BodyPaid + result.InterestPaid:N2}";
    }

    private static string FormatDeposit(decimal amount, DateTime openDate, int years, decimal rate)
    {
        var income = DepositCalculator.Calculate(amount, openDate, years, rate);
        return $"{income:N2}";
    }

    private void LoadData()
    {
        _isLoading = true;
        try
        {
            if (File.Exists(SavePath))
            {
                var lines = File.ReadAllLines(SavePath);
                if (lines.Length >= 8)
                {
                    AmountBox.Text = lines[0];
                    if (DateTime.TryParse(lines[1], Culture, DateTimeStyles.None, out var date))
                        OpenDatePicker.SelectedDate = date;
                    TermYearsBox.Text = lines[2];
                    RateBox.Text = lines[3];
                    Grace1RateBox.Text = lines[4];
                    Grace2RateBox.Text = lines[5];
                    Grace3RateBox.Text = lines[6];
                    DepositRateBox.Text = lines[7];
                    return;
                }
            }
        }
        catch (IOException)
        {
        }
        finally
        {
            _isLoading = false;
        }

        if (string.IsNullOrEmpty(TermYearsBox.Text))
            TermYearsBox.Text = "10";
        if (OpenDatePicker.SelectedDate == null)
            OpenDatePicker.SelectedDate = DateTime.Today;
    }

    private void SaveData()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(SavePath)!);
            File.WriteAllLines(SavePath, new[]
            {
                AmountBox.Text,
                OpenDatePicker.SelectedDate?.ToString("yyyy-MM-dd") ?? string.Empty,
                TermYearsBox.Text,
                RateBox.Text,
                Grace1RateBox.Text,
                Grace2RateBox.Text,
                Grace3RateBox.Text,
                DepositRateBox.Text
            });
        }
        catch (IOException)
        {
        }
    }

    private static bool TryParseDecimal(string text, out decimal value)
    {
        return decimal.TryParse(text.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out value);
    }
}
