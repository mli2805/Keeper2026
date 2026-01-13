using System.ComponentModel.DataAnnotations;

namespace KeeperInfrastructure;

public class CarEf
{
    public int Id { get; private set; }

    public int CarAccountId { get; set; }
    [MaxLength(100)] public string Title { get; set; }
    public int IssueYear { get; set; }
    [MaxLength(25)] public string Vin { get; set; }
    [MaxLength(15)] public string StateRegNumber { get; set; }

    public DateTime PurchaseDate { get; set; }
    public int PurchaseMileage { get; set; }
    public DateTime SaleDate { get; set; }
    public int SaleMileage { get; set; }

    public int SupposedSalePrice { get; set; }
    [MaxLength(100)] public string Comment { get; set; }

    // Навигационное свойство
    public List<CarYearMileageEf> YearMileages { get; set; } = new List<CarYearMileageEf>();
}
