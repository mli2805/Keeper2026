using System;
using System.Windows.Media;

namespace KeeperWpf;

public class SalaryLineModel
{
    public bool IsAggregatedLine;
    public DateTime Timestamp;
    public string DateStr => IsAggregatedLine ? $"{Timestamp:MM/yyyy}" : $"{Timestamp:dd/MM/yyyy}";
    public string Employer { get; set; } = null!;
    public string Amount { get; set; } = null!;
    public decimal AmountInUsd { get; set; }
    public string Comment { get; set; } = null!;

    public Brush Background => Timestamp.Month % 2 == 0 ? Brushes.White : Brushes.LightGray;
}