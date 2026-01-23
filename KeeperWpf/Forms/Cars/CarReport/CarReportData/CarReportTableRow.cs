using System;

namespace KeeperWpf;

public class CarReportTableRow
{
    public DateTime Date;
    public string AmountInCurrency = string.Empty;
    public decimal AmountInUsd;
    public int Mileage;
    public string Comment = string.Empty;
}