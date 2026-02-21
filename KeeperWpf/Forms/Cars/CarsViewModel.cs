using Caliburn.Micro;
using KeeperDomain;
using KeeperModels;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace KeeperWpf;

public class CarsViewModel : Screen
{
    private readonly IConfiguration _configuration;
    private readonly KeeperDataModel _dataModel;
    private readonly IWindowManager _windowManager;
    private readonly FuelViewModel _fuelViewModel;
    private readonly OwnershipCostViewModel _ownershipCostViewModel;

    public List<CarModel> Cars { get; set; } = null!;

    private CarModel _selectedCar = null!;
    public CarModel SelectedCar
    {
        get => _selectedCar;
        set
        {
            if (Equals(value, _selectedCar)) return;
            _selectedCar = value;
            YearMileagesToShow = new List<YearMileageModel>(_selectedCar.YearsMileage);
            EvaluateYearMileageToShow();
            NotifyOfPropertyChange();
            NotifyOfPropertyChange(nameof(IsLastCarVisibility));
            NotifyOfPropertyChange(nameof(YearMileagesToShow));
            NotifyOfPropertyChange(nameof(Total));
            NotifyOfPropertyChange(nameof(TotalPlus));
        }
    }

    public List<YearMileageModel> YearMileagesToShow { get; set; } = null!;
    public YearMileageModel Total { get; set; } = null!;
    public YearMileageModel TotalPlus { get; set; } = null!;

    public Visibility IsLastCarVisibility => SelectedCar.Id == Cars.Last().Id
        ? Visibility.Visible : Visibility.Collapsed;

    public CarsViewModel(IConfiguration configuration, KeeperDataModel dataModel, IWindowManager windowManager,
        FuelViewModel fuelViewModel, OwnershipCostViewModel ownershipCostViewModel)
    {
        _configuration = configuration;
        _dataModel = dataModel;
        _windowManager = windowManager;
        _fuelViewModel = fuelViewModel;
        _ownershipCostViewModel = ownershipCostViewModel;
    }

    public void Initialize()
    {
        _dataModel.Cars.Last().SaleDate = DateTime.Today;

        Cars = _dataModel.Cars;
        SelectedCar = Cars.Last();
    }

    protected override void OnViewLoaded(object view)
    {
        DisplayName = "Автомобили";
    }

    private void EvaluateYearMileageToShow()
    {
        var prevOdometer = SelectedCar.PurchaseMileage;
        // меняем отдельную копию , а не то что хранится в базе
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

    private void EvaluateAmount(YearMileageModel yearMileageModel, bool includePurchase = false)
    {
        yearMileageModel.YearAmount = _dataModel.Transactions.Values
            .Where(t => yearMileageModel.Period.Includes(t.Timestamp) &&
                        t.Operation == OperationType.Расход &&
                        t.Category!.Parent!.Is(SelectedCar.CarAccountId) &&
                        (t.Tags.All(tag => tag.Id != 1064) || includePurchase)) // 1064 тэг покупки-продажи авто
            .Sum(t => t.GetAmountInUsd(_dataModel));

        if (yearMileageModel.CarId == Cars.Last().CarAccountId && includePurchase)
        {
            yearMileageModel.YearAmount -= SelectedCar.SupposedSalePrice;
        }

        yearMileageModel.DayAmount = yearMileageModel.YearAmount / yearMileageModel.Period.ToDays();
    }

    public void AddNewCar()
    {

    }

    public async Task Fuelling()
    {
        _fuelViewModel.Initialize();
       await _windowManager.ShowWindowAsync(_fuelViewModel);
    }

    public bool IsByTags { get; set; }
    public bool IsBynInReport { get; set; }
    public async Task ShowCarReport()
    {
        if (SelectedCar.Id < 3) return;
        var document = _dataModel.CreateCarReport(SelectedCar.Id, IsByTags, IsBynInReport);

        try
        {
            var dataFolder = _configuration["DataFolder"] ?? "";
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
        _ownershipCostViewModel.Initialize(_selectedCar);
        await _windowManager.ShowDialogAsync(_ownershipCostViewModel);
    }

    public override async Task<bool> CanCloseAsync(CancellationToken cancellationToken = default)
    {
        Save();
        return await base.CanCloseAsync(cancellationToken);
    }

    public async Task CloseView()
    {
        await TryCloseAsync();
    }

    private void Save()
    {
        var yId = 1;
        foreach (var carModel in Cars)
        {
            foreach (var yearMileageModel in carModel.YearsMileage)
            {
                yearMileageModel.Id = yId++;
                yearMileageModel.CarId = carModel.Id;
            }
        }

        _dataModel.Cars = Cars;
    }

}
