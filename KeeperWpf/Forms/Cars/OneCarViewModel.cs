using Caliburn.Micro;
using System.Threading.Tasks;

namespace KeeperWpf;

[ExportViewModel]
public class OneCarViewModel : Screen
{
    private bool _isAddMode;

    public CarModel CarInWork { get; private set; } = null!;

    public void Initialize(CarModel car)
    {
        _isAddMode = car.Id == 0;
        CarInWork = new CarModel
        {
            Id = car.Id,
            CarAccountId = car.CarAccountId,
            Title = car.Title,
            IssueYear = car.IssueYear,
            Vin = car.Vin,
            StateRegNumber = car.StateRegNumber,
            PurchaseDate = car.PurchaseDate,
            PurchaseMileage = car.PurchaseMileage,
            SaleDate = car.SaleDate,
            SaleMileage = car.SaleMileage,
            SupposedSalePrice = car.SupposedSalePrice,
            Comment = car.Comment,
            YearsMileage = [.. car.YearsMileage]
        };
    }

    protected override void OnViewLoaded(object view)
    {
        DisplayName = _isAddMode ? "Добавить автомобиль" : "Редактировать автомобиль";
    }

    public async Task Save()
    {
        await TryCloseAsync(true);
    }

    public async Task Cancel()
    {
        await TryCloseAsync(false);
    }
}
