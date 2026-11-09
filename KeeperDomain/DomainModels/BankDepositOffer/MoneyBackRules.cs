using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace KeeperDomain;

public class MoneyBackRules : IDumpable, IParsable<MoneyBackRules>
{
    public int Id { get; set; } //PK
    public int DepositOfferId { get; set; }
    public DateTime StartDate { get; set; } // некотрые банки постоянно меняют правила начисления манибэка

    public MoneyBackType MoneyBackType { get; set; }
    public double Percent { get; set; } // например 0.5% от суммы платежа

    #region Restrictions
    public double? MinPurchaseAmount { get; set; } // минимальная сумма покупки на которую начисляют
    public double? MaxPurchaseAmount { get; set; } // максимальная сумма покупки на которую начисляют

    public double? MaxMoneyBackAmount { get; set; } // максимальная сумма манибэка за месяц

    public double? MinMonthPurchasesAmount { get; set; } // минимальная сумма покупок за месяц, чтобы начислили
    #endregion

    [MaxLength(1024)] public string Comment { get; set; } = string.Empty;

    public string Dump()
    {
        return Id + " ; " + DepositOfferId + " ; " + $"{StartDate:dd/MM/yyyy}" + " ; " +
               MoneyBackType + " ; " +
               Percent.ToString(new CultureInfo("en-US")) + " ; " +
               MinPurchaseAmount.DumpNullableDouble() + " ; " + MaxPurchaseAmount.DumpNullableDouble() + " ; " +
               MaxMoneyBackAmount.DumpNullableDouble() + " ; " +
               MinMonthPurchasesAmount.DumpNullableDouble() + " ; " +
               Comment;
    }

    public MoneyBackRules FromString(string s)
    {
        var substrings = s.Split(';');
        Id = int.Parse(substrings[0]);
        DepositOfferId = int.Parse(substrings[1]);
        StartDate = DateTime.ParseExact(substrings[2].Trim(), "dd.MM.yyyy", CultureInfo.InvariantCulture);
        MoneyBackType = Enum.Parse<MoneyBackType>(substrings[3].Trim());
        Percent = double.Parse(substrings[4], CultureInfo.InvariantCulture);
        MinPurchaseAmount = substrings[5].ParseNullableDouble();
        MaxPurchaseAmount = substrings[6].ParseNullableDouble();
        MaxMoneyBackAmount = substrings[7].ParseNullableDouble();
        MinMonthPurchasesAmount = substrings[8].ParseNullableDouble();
        Comment = substrings.Length > 9 ? substrings[9].Trim() : string.Empty;
        return this;
    }

   
}
