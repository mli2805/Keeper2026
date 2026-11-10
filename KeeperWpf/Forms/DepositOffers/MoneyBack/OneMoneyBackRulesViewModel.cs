using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

    public List<MoneyBackWhen> MoneyBackWhenOptions { get; } =
        Enum.GetValues(typeof(MoneyBackWhen)).OfType<MoneyBackWhen>().ToList();

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

    public MoneyBackWhen MoneyBackWhen
    {
        get => _currentModel?.MoneyBackWhen ?? MoneyBackWhen.NextMonth;
        set
        {
            if (_currentModel == null || _currentModel.MoneyBackWhen == value) return;
            _currentModel.MoneyBackWhen = value;
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

    public string Comment
    {
        get => _currentModel?.Comment ?? string.Empty;
        set
        {
            if (_currentModel == null || _currentModel.Comment == value) return;
            _currentModel.Comment = value;
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
        ApplyControlChangesToModel(); // переносим изменения из контролов в предыдущую модель
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
        NotifyOfPropertyChange(nameof(MoneyBackWhen));
        NotifyOfPropertyChange(nameof(Percent));
        NotifyOfPropertyChange(nameof(Comment));
    }

    // контролы категорий и партнёров работают с копиями списков модели,
    // поэтому добавленные/удалённые строки переносим обратно в модель
    public void ApplyControlChangesToModel()
    {
        if (_currentModel == null) return;
        _currentModel.SpecificCategories = CategoriesControl.Categories.ToList();
        _currentModel.Partners = PartnersControl.Partners.ToList();
    }

    public override Task<bool> CanCloseAsync(CancellationToken cancellationToken = default)
    {
        ApplyControlChangesToModel();
        return base.CanCloseAsync(cancellationToken);
    }
}
