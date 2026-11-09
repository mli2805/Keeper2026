using System.ComponentModel.DataAnnotations;
using KeeperDomain;

namespace KeeperInfrastructure;

public class MoneyBackRulesEf
{
    public int Id { get; set; }
    public int DepositOfferId { get; set; }
    public DateTime StartDate { get; set; }

    public MoneyBackType MoneyBackType { get; set; }
    public double Percent { get; set; }

    #region Restrictions
    public double? MinPurchaseAmount { get; set; }
    public double? MaxPurchaseAmount { get; set; }
    public double? MaxMoneyBackAmount { get; set; }
    public double? MinMonthPurchasesAmount { get; set; }
    #endregion

    [MaxLength(1024)] public string Comment { get; set; } = string.Empty;

    // Навигационные свойства
    public DepositOfferEf DepositOffer { get; set; } = null!;
    public List<MoneyBackCategoryEf> SpecificCategories { get; set; } = null!;
    public List<MoneyBackPartnerEf> Partners { get; set; } = null!;
}
