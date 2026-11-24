using KeeperDomain;

namespace KeeperModels;

public class MoneyBackRulesModel
{
    public int Id { get; set; }
    public int DepositOfferId { get; set; }
    public DateTime StartDate { get; set; }

    public MoneyBackType MoneyBackType { get; set; }
    public MoneyBackWhen MoneyBackWhen { get; set; }
    public double Percent { get; set; } // например 0.5% от суммы платежа

    public List<MoneyBackCategoryModel> SpecificCategories { get; set; } = new List<MoneyBackCategoryModel>();
    public List<MoneyBackPartnerModel> Partners { get; set; } = new List<MoneyBackPartnerModel>();

    #region Restrictions
    public double? MinPurchaseAmount { get; set; } // минимальная сумма покупки, на которую начисляют манибэк
    public double? MaxPurchaseAmount { get; set; } // максимальная сумма покупки, на которую начисляют манибэк

    public double? MaxMoneyBackAmount { get; set; } // максимальная сумма манибэка за месяц

    public double? MinMonthPurchasesAmount { get; set; } // минимальная сумма покупок за месяц, чтобы начислили манибэк
    #endregion

    public string Comment { get; set; } = string.Empty;

    public override string ToString()
    {
        return $"Правила начисления манибэка с {StartDate:dd.MM.yyyy}";
    }
}
