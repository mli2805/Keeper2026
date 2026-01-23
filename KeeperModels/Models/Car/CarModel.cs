namespace KeeperModels;

public class CarModel
{
    public int Id { get; set; } //PK
    public int CarAccountId { get; set; }
    public string Title { get; set; } = null!;
    public int IssueYear { get; set; }
    public string Vin { get; set; } = string.Empty;
    public string StateRegNumber { get; set; } = string.Empty;

    public DateTime PurchaseDate { get; set; }
    public int PurchaseMileage { get; set; }
    public DateTime SaleDate { get; set; }
    public int SaleMileage { get; set; }

    public int SupposedSalePrice { get; set; }
    public string Comment { get; set; } = string.Empty;
    

    public int MileageDifference => SaleMileage - PurchaseMileage;
    public int MileageAday => MileageDifference / (SaleDate - PurchaseDate).Days;
    public List<YearMileageModel> YearsMileage { get; set; } = new List<YearMileageModel>();

}