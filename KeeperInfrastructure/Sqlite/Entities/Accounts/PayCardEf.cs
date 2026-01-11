using KeeperDomain;
using System.ComponentModel.DataAnnotations;

namespace KeeperInfrastructure;

public class PayCardEf
{
    public int Id { get; set; } // совпадает с ID Account'a и BankAccount'a

    [MaxLength(20)] public string CardNumber { get; set; } = "";
    [MaxLength(50)] public string CardHolder { get; set; } = "";

    public PaymentSystem PaymentSystem { get; set; }
    public bool IsVirtual { get; set; }
    public bool IsPayPass { get; set; } // if not virtual
}
