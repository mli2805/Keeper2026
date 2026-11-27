using System.ComponentModel.DataAnnotations;

namespace KeeperDomain;

public enum RateType
{
    Floating,
    Linked,
    Fixed,
}

public class DepositConditions
{
    public int Id { get; set; } //PK
    public int DepositOfferId { get; set; } 
    public DateTime DateFrom { get; set; }

    [MaxLength(20)] public string RateFormula { get; set; } = string.Empty;
    public bool IsFactDays { get; set; } // true 28-31/365 false 30/360
    public bool EveryStartDay { get; set; }
    public bool EveryFirstDayOfMonth { get; set; } 
    public bool EveryLastDayOfMonth { get; set; }

    public bool EveryNDays { get; set; }
    public int NDays { get; set; }

    public bool IsCapitalized { get; set; }

    public bool HasAdditionalPercent { get; set; }
    public double AdditionalPercent { get; set; }

    [MaxLength(100)] public string Comment { get; set; } = string.Empty;
}
