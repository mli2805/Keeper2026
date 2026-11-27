using System.ComponentModel.DataAnnotations;

namespace KeeperDomain;

public class MoneyBackRules
{
    public int Id { get; set; } //PK
    public int DepositOfferId { get; set; }
    public DateTime StartDate { get; set; } // некотрые банки постоянно меняют правила начисления манибэка

    public MoneyBackType MoneyBackType { get; set; }
    public MoneyBackWhen MoneyBackWhen { get; set; }
    public double Percent { get; set; } // например 0.5% от суммы платежа



    #region Restrictions
    public double? MinPurchaseAmount { get; set; } // минимальная сумма покупки на которую начисляют
    public double? MaxPurchaseAmount { get; set; } // максимальная сумма покупки на которую начисляют

    public double? MaxMoneyBackAmount { get; set; } // максимальная сумма манибэка за месяц

    public double? MinMonthPurchasesAmount { get; set; } // минимальная сумма покупок за месяц, чтобы начислили
    #endregion

    [MaxLength(1024)] public string Comment { get; set; } = string.Empty;
}
