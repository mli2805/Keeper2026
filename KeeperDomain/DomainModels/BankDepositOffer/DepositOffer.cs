using System.ComponentModel.DataAnnotations;

namespace KeeperDomain;

public class DepositOffer
{
    public int Id { get; set; } //PK
    [MaxLength(100)] public string Title { get; set; } = null!;
    public bool IsNotRevocable { get; set; }
    public RateType RateType { get; set; }
    public bool IsAddLimited { get; set; }
    public int AddLimitInDays { get; set; }


    // при создании депозита или карточки эти поля возмутся из офера
    // а при создании банковского счета их надо будет выбрать руками
    public int BankId { get; set; }
    public CurrencyCode MainCurrency { get; set; }
    //

    public Duration DepositTerm { get; set; } = null!;

    [MaxLength(250)] public string Comment { get; set; } = string.Empty;
}
