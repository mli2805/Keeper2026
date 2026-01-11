using System.ComponentModel.DataAnnotations;

namespace KeeperInfrastructure;

public class DepositConditionsEf
{
    public int Id { get; set; }
    public int DepositOfferId { get; set; }
    public DateTime DateFrom { get; set; }

    [MaxLength(20)] public string RateFormula { get; set; }
    public bool IsFactDays { get; set; }
    public bool EveryStartDay { get; set; }
    public bool EveryFirstDayOfMonth { get; set; }
    public bool EveryLastDayOfMonth { get; set; }

    public bool EveryNDays { get; set; }
    public int NDays { get; set; }

    public bool IsCapitalized { get; set; }

    public bool HasAdditionalPercent { get; set; }
    public double AdditionalPercent { get; set; }

    [MaxLength(100)] public string Comment { get; set; }
}