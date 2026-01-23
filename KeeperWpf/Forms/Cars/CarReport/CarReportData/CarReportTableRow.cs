using System;

namespace KeeperWpf;

public class CarReportTableRow
{
    public DateTime Date;
    public string? AmountInCurrency;
    public decimal AmountInUsd;
    public int Mileage;
    public string Comment = string.Empty;
}