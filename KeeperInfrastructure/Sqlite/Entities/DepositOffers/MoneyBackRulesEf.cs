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
    public decimal? MinPurchaseAmount { get; set; }
    public decimal? MaxPurchaseAmount { get; set; }
    public decimal? MaxMoneyBackAmount { get; set; }
    public decimal? MaxMoneyBackOnCategoryAmount { get; set; }
    public decimal? MinMonthPurchasesAmount { get; set; }
    #endregion

    // Навигационные свойства
    public DepositOfferEf DepositOffer { get; set; } = null!;
    public List<MoneyBackSpecificRuleEf>? ExemptionsFromAll { get; set; }
    public List<MoneyBackSpecificRuleEf>? SpecificCategories { get; set; }
    public List<MoneyBackSpecificRuleEf>? PartnerCounterparties { get; set; }
}
