using System;
using System.Windows.Media;

namespace KeeperWpf;

public class SalaryLineModel
{
    public bool IsAggregatedLine;
    public DateTime Timestamp;
    public string DateStr => IsAggregatedLine ? $"{Timestamp:MM/yyyy}" : $"{Timestamp:dd/MM/yyyy}";
    public string Employer { get; set; }
    public string Amount { get; set; }
    public decimal AmountInUsd { get; set; }
    public string Comment { get; set; }

    public Brush Background => Timestamp.Month % 2 == 0 ? Brushes.White : Brushes.LightGray;
}