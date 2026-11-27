using System.ComponentModel.DataAnnotations;

namespace KeeperDomain;

public class PayCard
{
    public int Id { get; set; } // совпадает с ID Account'a и BankAccount'a
    
    [MaxLength(20)] public string CardNumber { get; set; } = string.Empty;
    [MaxLength(50)] public string CardHolder { get; set; } = string.Empty;

    public PaymentSystem PaymentSystem { get; set; }
    public bool IsVirtual { get; set; }
    public bool IsPayPass { get; set; } // if not virtual

    public PayCard Clone()
    {
        return (PayCard)MemberwiseClone();
    }
}
