using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;

namespace KeeperWpf;

[ExportViewModel]
public class OneMoneyBackRulesViewModel : Screen
{
    public NullableNumericControlViewModel MinPurchaseAmountControl { get; }
    public NullableNumericControlViewModel MaxPurchaseAmountControl { get; }
    public NullableNumericControlViewModel MaxMoneyBackAmountControl { get; }
    public NullableNumericControlViewModel MinMonthPurchasesAmountControl { get; }

    public CategoriesMoneyBackControlModel CategoriesControl { get; } = new CategoriesMoneyBackControlModel();
    public PartnersMoneyBackControlModel PartnersControl { get; } = new PartnersMoneyBackControlModel();

    public List<MoneyBackType> MoneyBackTypes { get; } =
        Enum.GetValues(typeof(MoneyBackType)).OfType<MoneyBackType>().ToList();

    private MoneyBackRulesModel? _currentModel;

    public DateTime StartDate
    {
        get => _currentModel?.StartDate ?? DateTime.Today;
        set
        {
            if (_currentModel == null || _currentModel.StartDate == value) return;
            _currentModel.StartDate = value;
            NotifyOfPropertyChange();
        }
    }

    public MoneyBackType MoneyBackType
    {
        get => _currentModel?.MoneyBackType ?? MoneyBackType.AnyWithExemptions;
        set
        {
            if (_currentModel == null || _currentModel.MoneyBackType == value) return;
            _currentModel.MoneyBackType = value;
            NotifyOfPropertyChange();
        }
    }

    public double Percent
    {
        get => _currentModel?.Percent ?? 0.0;
        set
        {
            if (_currentModel == null || _currentModel.Percent.Equals(value)) return;
            _currentModel.Percent = value;
            NotifyOfPropertyChange();
        }
    }

    public OneMoneyBackRulesViewModel()
    {
        MinPurchaseAmountControl = Create(nameof(MoneyBackRulesModel.MinPurchaseAmount),
            "минимальная сумма покупки, на которую начисляют манибэк");
        MaxPurchaseAmountControl = Create(nameof(MoneyBackRulesModel.MaxPurchaseAmount),
            "максимальная сумма покупки, на которую начисляют манибэк");
        MaxMoneyBackAmountControl = Create(nameof(MoneyBackRulesModel.MaxMoneyBackAmount),
            "максимальная сумма манибэка за месяц");
        MinMonthPurchasesAmountControl = Create(nameof(MoneyBackRulesModel.MinMonthPurchasesAmount),
            "минимальная сумма покупок за месяц, чтобы начислили манибэк");
    }

    private static NullableNumericControlViewModel Create(string propertyName, string explanation)
        => new NullableNumericControlViewModel(new MoneyBackRulesModel(), propertyName, explanation);

    public void ApplyModel(MoneyBackRulesModel? model)
    {
        if (ReferenceEquals(model, _currentModel)) return;
        _currentModel = model;
        if (model == null) return;

        MinPurchaseAmountControl.SwitchModel(model);
        MaxPurchaseAmountControl.SwitchModel(model);
        MaxMoneyBackAmountControl.SwitchModel(model);
        MinMonthPurchasesAmountControl.SwitchModel(model);
        CategoriesControl.SwitchModel(model.SpecificCategories);
        PartnersControl.SwitchModel(model.Partners);

        NotifyOfPropertyChange(nameof(StartDate));
        NotifyOfPropertyChange(nameof(MoneyBackType));
        NotifyOfPropertyChange(nameof(Percent));
    }
}
