using System.Globalization;

namespace KeeperDomain;

public enum MoneyBackType
{
    AnyWithExemptions,
    SpecificCategories,
    PartnerCounterparties,
}


public class MoneyBackRules : IDumpable, IParsable<MoneyBackRules>
{
    public int Id { get; set; } //PK
    public int DepositOfferId { get; set; }
    public DateTime StartDate { get; set; } // некотрые банки постоянно меняют правила начисления манибэка

    public MoneyBackType MoneyBackType { get; set; }


    public double Percent { get; set; } // например 0.5% от суммы платежа

    // хранятся в отдельной таблице (отдельном файле). будут реализованы в MoneyBackRulesModel
    //public List<MoneyBackSpecificRule>? ExemptionsFromAll { get; set; }
    //public List<MoneyBackSpecificRule>? SpecificCategories { get; set; }
    //public List<MoneyBackSpecificRule>? PartnerCounterparties { get; set; }

    #region Restrictions
    public decimal? MinPurchaseAmount { get; set; } // минимальная сумма покупки на которую начисляют
    public decimal? MaxPurchaseAmount { get; set; } // максимальная сумма покупки на которую начисляют

    public decimal? MaxMoneyBackAmount { get; set; } // максимальная сумма манибэка за месяц
    public decimal? MaxMoneyBackOnCategoryAmount { get; set; } // максимальная сумма манибэка по категории за месяц

    public decimal? MinMonthPurchasesAmount { get; set; } // минимальная сумма покупок за месяц, чтобы начислили
    #endregion

    public string Dump()
    {
        return Id + " ; " + DepositOfferId + " ; " + $"{StartDate:dd/MM/yyyy}" + " ; " +
               MoneyBackType + " ; " +
               Percent.ToString(new CultureInfo("en-US")) + " ; " +
               MinPurchaseAmount.DumpNullableDecimal() + " ; " + MaxPurchaseAmount.DumpNullableDecimal() + " ; " +
               MaxMoneyBackAmount.DumpNullableDecimal() + " ; " + MaxMoneyBackOnCategoryAmount.DumpNullableDecimal() + " ; " +
               MinMonthPurchasesAmount.DumpNullableDecimal();
    }

    public MoneyBackRules FromString(string s)
    {
        var substrings = s.Split(';');
        Id = int.Parse(substrings[0]);
        DepositOfferId = int.Parse(substrings[1]);
        StartDate = DateTime.ParseExact(substrings[2].Trim(), "dd.MM.yyyy", CultureInfo.InvariantCulture);
        MoneyBackType = Enum.Parse<MoneyBackType>(substrings[3].Trim());
        Percent = double.Parse(substrings[4], CultureInfo.InvariantCulture);
        MinPurchaseAmount = substrings[5].ParseNullableDecimal();
        MaxPurchaseAmount = substrings[6].ParseNullableDecimal();
        MaxMoneyBackAmount = substrings[7].ParseNullableDecimal();
        MaxMoneyBackOnCategoryAmount = substrings[8].ParseNullableDecimal();
        MinMonthPurchasesAmount = substrings[9].ParseNullableDecimal();
        return this;
    }

   
}
