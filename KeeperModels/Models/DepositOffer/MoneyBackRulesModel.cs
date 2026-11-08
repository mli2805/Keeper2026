using KeeperDomain;

namespace KeeperModels;

[Serializable]
public class MoneyBackRulesModel
{
    public int Id { get; set; }
    public int DepositOfferId { get; set; }
    public DateTime StartDate { get; set; }

    public MoneyBackType MoneyBackType { get; set; }
    public double Percent { get; set; } // например 0.5% от суммы платежа

    public List<MoneyBackSpecificRuleModel> ExemptionsFromAll { get; set; } = new List<MoneyBackSpecificRuleModel>();
    public List<MoneyBackSpecificRuleModel> SpecificCategories { get; set; } = new List<MoneyBackSpecificRuleModel>();
    public List<MoneyBackSpecificRuleModel> PartnerCounterparties { get; set; } = new List<MoneyBackSpecificRuleModel>();

    #region Restrictions
    public decimal? MinPurchaseAmount { get; set; } // минимальная сумма покупки на которую начисляют
    public decimal? MaxPurchaseAmount { get; set; } // максимальная сумма покупки на которую начисляют

    public decimal? MaxMoneyBackAmount { get; set; } // максимальная сумма манибэка за месяц
    public decimal? MaxMoneyBackOnCategoryAmount { get; set; } // максимальная сумма манибэка по категории за месяц

    public decimal? MinMonthPurchasesAmount { get; set; } // минимальная сумма покупок за месяц, чтобы начислили
    #endregion
}
