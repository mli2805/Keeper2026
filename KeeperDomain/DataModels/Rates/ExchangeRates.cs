using System.Globalization;

namespace KeeperDomain;

/// <summary>
/// 
/// Let's rates are 2.5 - 2.6
/// I have 250 byn, I can buy 250 * (1 / 2.6) = 96 usd
/// I have 100 usd, I can buy 100 * 2.5 = 250 byn
/// 
/// I have 56000 rub, I can buy 56000/72.5 = 772 usd ==  56000* (1/72.5) 
/// I have 800 usd, I can buy 800*56 = 44800 rub
/// 
///  /// </summary>
[Serializable]
public class ExchangeRates : IDumpable, IParsable<ExchangeRates>
{
    // Кешируем CultureInfo на уровне класса
    private static readonly CultureInfo _enUsCulture = CultureInfo.GetCultureInfo("en-US");
    private static readonly CultureInfo _invariantCulture = CultureInfo.InvariantCulture;

    public int Id { get; set; }
    public DateTime Date { get; set; }


    // на табло 2.5 - 2.6
    public double UsdToByn { get; set; } // 2.5
    public double BynToUsd { get; set; } // 2.6

    public string BynUsd => $"{UsdToByn} - {BynToUsd}";

    public double EurToByn { get; set; }
    public double BynToEur { get; set; }

    public string BynEur => $"{EurToByn} - {BynToEur}";

    // хранить не за 100, а за 1 (для унификации)
    public double RubToByn { get; set; }
    public double BynToRub { get; set; }

    public string BynRub => $"{RubToByn} - {BynToRub}";
  
    // 1.05 - 1.10
    public double EurToUsd { get; set; } // 1.05
    public double UsdToEur { get; set; } // 1.10

    public string UsdEur => $"{EurToUsd} - {UsdToEur}";
  
    // 56 - 72.5
    public double UsdToRub { get; set; } // 56
    public double RubToUsd { get; set; } // 72.5

    public string UsdRub => $"{UsdToRub} - {RubToUsd}";
    
    public double EurToRub { get; set; }
    public double RubToEur { get; set; }
    public string EurRub => $"{EurToRub} - {RubToEur}";
    
    public string Dump()
    {
        return Id + " ; " + Date.ToString("dd/MM/yyyy") + " ; " +
               UsdToByn.ToString(_enUsCulture) + " ; " +
               BynToUsd.ToString(_enUsCulture) + " ; " +
               EurToByn.ToString(_enUsCulture) + " ; " +
               BynToEur.ToString(_enUsCulture) + " ; " +
               RubToByn.ToString(_enUsCulture) + " ; " +
               BynToRub.ToString(_enUsCulture) + " ; " +
               EurToUsd.ToString(_enUsCulture) + " ; " +
               UsdToEur.ToString(_enUsCulture) + " ; " +
               UsdToRub.ToString(_enUsCulture) + " ; " +
               RubToUsd.ToString(_enUsCulture) + " ; " +
               EurToRub.ToString(_enUsCulture) + " ; " +
               RubToEur.ToString(_enUsCulture);
    }

    public ExchangeRates Clone()
    {
        return (ExchangeRates)MemberwiseClone();
    }

    public ExchangeRates FromString(string s)
    {
        var substrings = s.Split(';');
        Id = int.Parse(substrings[0]);
        Date = DateTime.ParseExact(substrings[1].Trim(), "dd.MM.yyyy", _invariantCulture);
        
        // Используем кешированный CultureInfo вместо создания нового каждый раз
        UsdToByn = double.Parse(substrings[2], _enUsCulture);
        BynToUsd = double.Parse(substrings[3], _enUsCulture);
        EurToByn = double.Parse(substrings[4], _enUsCulture);
        BynToEur = double.Parse(substrings[5], _enUsCulture);
        RubToByn = double.Parse(substrings[6], _enUsCulture);
        BynToRub = double.Parse(substrings[7], _enUsCulture);
        EurToUsd = double.Parse(substrings[8], _enUsCulture);
        UsdToEur = double.Parse(substrings[9], _enUsCulture);
        UsdToRub = double.Parse(substrings[10], _enUsCulture);
        RubToUsd = double.Parse(substrings[11], _enUsCulture);
        EurToRub = double.Parse(substrings[12], _enUsCulture);
        RubToEur = double.Parse(substrings[13], _enUsCulture);
        
        return this;
    }
}