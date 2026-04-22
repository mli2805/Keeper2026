using Caliburn.Micro;
using KeeperDomain;
using KeeperInfrastructure;
using KeeperModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace KeeperWpf;

[ExportViewModel]
public class CarsViewModel(PathFinder pathFinder, KeeperDataModel dataModel, IWindowManager windowManager,
    CarRepository carRepository, FuelViewModel fuelViewModel, OwnershipCostViewModel ownershipCostViewModel) : Screen
{
    public BindableCollection<CarModel> Cars { get; set; } = null!;

    private CarModel _selectedCar = null!;
    public CarModel SelectedCar
    {
        get => _selectedCar;
        set
        {
            if (Equals(value, _selectedCar)) return;
            carRepository.SaveCarWithMileages(_selectedCar);
            _selectedCar = value;
            YearMileagesToShow = [.. _selectedCar.YearsMileage];
            EvaluateYearMileageToShow();
            NotifyOfPropertyChange();
            NotifyOfPropertyChange(nameof(IsLastCarVisibility));
            NotifyOfPropertyChange(nameof(YearMileagesToShow));
            NotifyOfPropertyChange(nameof(Total));
            NotifyOfPropertyChange(nameof(TotalPlus));
            NotifyOfPropertyChange(nameof(CanStartNewYearMileage));
        }
    }

    public BindableCollection<YearMileageModel> YearMileagesToShow { get; set; } = null!;
    public YearMileageModel Total { get; set; } = null!;
    public YearMileageModel TotalPlus { get; set; } = null!;

    public Visibility IsLastCarVisibility => SelectedCar.Id == Cars.Last().Id
        ? Visibility.Visible : Visibility.Collapsed;

    public void Initialize()
    {
        dataModel.Cars.Last().SaleDate = DateTime.Today;

        Cars = [.. dataModel.Cars];
        _selectedCar = Cars.Last();
        YearMileagesToShow = [.. _selectedCar.YearsMileage];
        EvaluateYearMileageToShow();
    }

    protected override void OnViewLoaded(object view)
    {
        DisplayName = "Автомобили";
    }

    private void EvaluateYearMileageToShow()
    {
        var prevOdometer = SelectedCar.PurchaseMileage;
        // это ссылки на то, что хранится в базе, если изменить, то сохранится в базе,
        // но последняя строка добавляемая на лету не будет сохраняться
        for (int i = 0; i < YearMileagesToShow.Count; i++)
        {
            var yearMileageModel = YearMileagesToShow[i];
            Period period = new Period(SelectedCar.PurchaseDate.AddYears(i),
                SelectedCar.PurchaseDate.AddYears(i + 1).AddMilliseconds(-1));
            if (period.FinishMoment > SelectedCar.SaleDate) period.FinishMoment = SelectedCar.SaleDate;
            yearMileageModel.YearNumber = i + 1;
            yearMileageModel.Period = period;

            yearMileageModel.Mileage = yearMileageModel.Odometer - prevOdometer;
            prevOdometer = yearMileageModel.Odometer;

            EvaluateAmount(yearMileageModel);
        }

        if (SelectedCar == Cars.Last())
        {
            var lastYear = YearMileagesToShow.Last();
            if (lastYear.Period.FinishMoment.Date < DateTime.Today)
            {
                var currentYear = new YearMileageModel()
                {
                    CarId = SelectedCar.CarAccountId,
                    Period = new Period(lastYear.Period.FinishMoment.Date.AddDays(1), DateTime.Today.AddDays(1).AddMilliseconds(-1)),
                    YearNumber = lastYear.YearNumber + 1,
                    Odometer = SelectedCar.SaleMileage,
                    Mileage = SelectedCar.SaleMileage - prevOdometer
                };
                EvaluateAmount(currentYear);
                YearMileagesToShow.Add(currentYear);
            }
        }

        var fullPeriod = new Period(SelectedCar.PurchaseDate, YearMileagesToShow.Last().Period.FinishMoment);
        Total = new YearMileageModel()
        {
            Mileage = YearMileagesToShow.Sum(y => y.Mileage),
            Period = fullPeriod,
            YearAmount = YearMileagesToShow.Sum(y => y.YearAmount),
        };
        Total.DayAmount = Total.YearAmount / fullPeriod.ToDays();
        TotalPlus = new YearMileageModel()
        {
            CarId = SelectedCar.CarAccountId,
            Mileage = YearMileagesToShow.Sum(y => y.Mileage),
            Period = fullPeriod,
            YearAmount = YearMileagesToShow.Sum(y => y.YearAmount),
        };
        EvaluateAmount(TotalPlus, true);
    }

    // вычисляем расходы за указанный год
    private void EvaluateAmount(YearMileageModel yearMileageModel, bool includePurchase = false)
    {
        yearMileageModel.YearAmount = dataModel.Transactions.Values
            .Where(t => yearMileageModel.Period.Includes(t.Timestamp) &&
                        t.Operation == OperationType.Расход &&
                        t.Category!.Parent!.Is(SelectedCar.CarAccountId) &&
                        (t.Tags.All(tag => tag.Id != 1064) || includePurchase)) // 1064 тэг покупки-продажи авто
            .Sum(t => t.GetAmountInUsd(dataModel));

        if (yearMileageModel.CarId == Cars.Last().CarAccountId && includePurchase)
        {
            yearMileageModel.YearAmount -= SelectedCar.SupposedSalePrice;
        }

        yearMileageModel.DayAmount = yearMileageModel.YearAmount / yearMileageModel.Period.ToDays();
    }

    public bool CanStartNewYearMileage => SelectedCar.Id == Cars.Last().Id &&
                                YearMileagesToShow.Last().Period.FinishMoment.Date > DateTime.Today;

    public async Task StartNewYearMileage()
    {
        if (SelectedCar.Id != Cars.Last().Id) return;
        if (YearMileagesToShow.Last().Period.FinishMoment.Date < DateTime.Today) return;

        var lastYear = YearMileagesToShow.Last();
        lastYear.Period = new Period(lastYear.Period.StartDate, lastYear.Period.StartDate.AddYears(1).AddMicroseconds(-1));

        var newYearMileage = new YearMileageModel()
        {
            CarId = SelectedCar.Id,
            Period = new Period(lastYear.Period.StartDate.Date.AddYears(1), DateTime.Today.AddDays(1).AddMilliseconds(-1)),
            YearNumber = lastYear.YearNumber + 1,
            Odometer = lastYear.Odometer,
            Mileage = 0
        };
        EvaluateAmount(newYearMileage);
        YearMileagesToShow.Add(newYearMileage);
        Cars.Last().YearsMileage = [.. YearMileagesToShow];
        await carRepository.SaveCarWithMileages(SelectedCar);
    }

    public void AddNewCar()
    {

    }

    public async Task Fuelling()
    {
        fuelViewModel.Initialize();
        await windowManager.ShowWindowAsync(fuelViewModel);
    }

    public bool IsByTags { get; set; }
    public bool IsBynInReport { get; set; }
    public async Task ShowCarReport()
    {
        if (SelectedCar.Id < 3) return;
        var document = dataModel.CreateCarReport(SelectedCar.Id, IsByTags, IsBynInReport);

        try
        {
            var dataFolder = pathFinder.GetDataFolder();
            string filename = $@"reports\{SelectedCar.Title}.pdf";
            var path = System.IO.Path.Combine(dataFolder, filename);
            await document.SaveAsync(path);
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = path,
                    UseShellExecute = true
                }
            };

            process.Start();
        }
        catch (Exception e)
        {
            MessageBox.Show(e.Message);
        }
    }

    public async Task ShowOwnershipCostChart()
    {
        ownershipCostViewModel.Initialize(_selectedCar);
        await windowManager.ShowDialogAsync(ownershipCostViewModel);
    }

    public override async Task<bool> CanCloseAsync(CancellationToken cancellationToken = default)
    {
        await carRepository.SaveCarWithMileages(SelectedCar);
        return await base.CanCloseAsync(cancellationToken);
    }

    public async Task CloseView()
    {
        await TryCloseAsync();
    }
}
