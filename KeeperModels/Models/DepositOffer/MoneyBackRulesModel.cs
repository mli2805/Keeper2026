using KeeperDomain;

namespace KeeperModels;

public class MoneyBackRulesModel
{
    public int Id { get; set; }
    public int DepositOfferId { get; set; }
    public DateTime StartDate { get; set; }

    public MoneyBackType MoneyBackType { get; set; }
    public double Percent { get; set; } // например 0.5% от суммы платежа

    public List<MoneyBackSpecificCategoryModel> SpecificCategories { get; set; } = new List<MoneyBackSpecificCategoryModel>();
    public List<MoneyBackPartnerModel> Partners { get; set; } = new List<MoneyBackPartnerModel>();

    #region Restrictions
    public decimal? MinPurchaseAmount { get; set; } // минимальная сумма покупки, на которую начисляют манибэк
    public decimal? MaxPurchaseAmount { get; set; } // максимальная сумма покупки, на которую начисляют манибэк

    public decimal? MaxMoneyBackAmount { get; set; } // максимальная сумма манибэка за месяц
    public decimal? MaxMoneyBackOnCategoryAmount { get; set; } // максимальная сумма манибэка по категории за месяц

    public decimal? MinMonthPurchasesAmount { get; set; } // минимальная сумма покупок за месяц, чтобы начислили манибэк
    #endregion

    public override string ToString()
    {
        return $"Правила начисления манибэка с {StartDate:dd.MM.yyyy}";
    }
}
