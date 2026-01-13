using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Caliburn.Micro;
using KeeperDomain;

namespace KeeperWpf;

public class TrustAccountsViewModel : Screen
{
    private readonly KeeperDataModel _dataModel;
    private readonly IWindowManager _windowManager;
    private readonly TrustAccountStateViewModel _trustAccountStateViewModel;
    private readonly TrustAccountAnalysisViewModel _trustAccountAnalysisViewModel;

    public ObservableCollection<TrustAccount> TrustAccounts { get; set; }
    public TrustAccount SelectedAccount { get; set; }

    public List<string> BasicFeeExplanation { get; } = new List<string>()
    {
        "* Базовое вознаграждение по счету (до 100K$ / 7MRub)",
        "                          - 1% годовых от ежедневного остатка средств",
        "  (берем остаток за каждый день * 1% / 365 * курс НБ и суммируем за месяц)",
        "",
        " Например, в июле 2022 сумма средств не менялась и составляла 311000 rub,",
        " т.о. 311000 / 100 / 365 * 31 = 264.137 rub",
        " курс НБ в июле был от 4.88 до 4.34, если взять примерно 4.5 -",
        "   264.137 / 100 * 4.5 = 11.88 byn  (выставили 11.87)",
        "",
    };

    public List<string> Comments { get; } = new List<string>()
    {
        "По долларовому счету -",
        "   не берут базовую комиссию начиная с мая 2022 г. (за апрель была)",
        "По рублевому счету -",
        "   не брали базовую комиссию за март - июнь 2022 г.",
        "   за полмарта взяли/вернули",
        "",
    };


    public TrustAccountsViewModel(KeeperDataModel dataModel, IWindowManager windowManager,
        TrustAccountStateViewModel trustAccountStateViewModel, TrustAccountAnalysisViewModel trustAccountAnalysisViewModel)
    {
        _dataModel = dataModel;
        _windowManager = windowManager;
        _trustAccountStateViewModel = trustAccountStateViewModel;
        _trustAccountAnalysisViewModel = trustAccountAnalysisViewModel;
    }

    public void Initialize()
    {
        TrustAccounts = new ObservableCollection<TrustAccount>(_dataModel.TrustAccounts);
    }

    protected override void OnViewLoaded(object view)
    {
        DisplayName = "Трастовые счета";
    }

    public async Task ShowTrustAccountState()
    {
        _trustAccountStateViewModel.Evaluate(SelectedAccount);
        await _windowManager.ShowDialogAsync(_trustAccountStateViewModel);
    }

    public async Task ShowTrustAccountAnalysis()
    {
        _trustAccountAnalysisViewModel.Initialize(SelectedAccount, DateTime.Today);
        await _windowManager.ShowDialogAsync(_trustAccountAnalysisViewModel);
    }

    public async Task OnPreviewKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Delete && Keyboard.Modifiers == ModifierKeys.Control)
        {
            DeleteSelected();
            e.Handled = true;
        }
        else if (e.Key == Key.Escape)
        {
            await Close();
            e.Handled = true;

        }
    }

    public void DeleteSelected()
    {
        if (SelectedAccount != null)
            TrustAccounts.Remove(SelectedAccount);
    }

    public async Task Close()
    {
       await TryCloseAsync();
    }

    public override async Task<bool> CanCloseAsync(CancellationToken cancellationToken = default)
    {
        _dataModel.TrustAccounts = TrustAccounts.ToList();
        return await base.CanCloseAsync(cancellationToken);
    }
}
