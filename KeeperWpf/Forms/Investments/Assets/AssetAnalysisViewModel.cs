using System;
using System.Windows.Input;
using Caliburn.Micro;
using KeeperDomain;
using KeeperModels;

namespace KeeperWpf;

public class AssetAnalysisViewModel : Screen
{
    private readonly KeeperDataModel _dataModel;
    private TrustAssetModel _asset = null!;

    private Period _activePeriod = null!;

    private AssetOnPeriodReportModel _reportModel = null!;
    public AssetOnPeriodReportModel ReportModel
    {
        get => _reportModel;
        set
        {
            if (Equals(value, _reportModel)) return;
            _reportModel = value;
            NotifyOfPropertyChange();
        }
    }

    public AssetAnalysisViewModel(KeeperDataModel dataModel)
    {
        _dataModel = dataModel;
    }

    public void Initialize(TrustAssetModel asset)
    {
        _asset = asset;
        _activePeriod = DateTime.Today.GetFullMonthForDate();
        ReportModel = _dataModel.Analyze(_asset, _activePeriod).CreateReport();
    }

    protected override void OnViewLoaded(object view)
    {
        DisplayName = _asset.Title + "  " + _activePeriod.ToStringD();
    }

    public void OnPreviewKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Left)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                ShowPreviousYear();
            }
            else if (Keyboard.Modifiers == ModifierKeys.Shift)
            {
                ShowPreviousQuarter();
            }
            else
            {
                ShowPreviousMonth();
            }
            e.Handled = true;
        }
        else if (e.Key == Key.Right)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                ShowNextYear();
            }
            else if (Keyboard.Modifiers == ModifierKeys.Shift)
            {
                ShowNextQuarter();
            }
            else
            {
                ShowNextMonth();
            }
            e.Handled = true;
        }

    }

    public void ShowPreviousMonth()
    {
        _activePeriod = _activePeriod.StartDate.AddMonths(-1).GetFullMonthForDate();
        ReportModel = _dataModel.Analyze(_asset, _activePeriod).CreateReport();
    }

    public void ShowNextMonth()
    {
        _activePeriod = _activePeriod.StartDate.AddMonths(1).GetFullMonthForDate();
        ReportModel = _dataModel.Analyze(_asset, _activePeriod).CreateReport();
    }

    public void ShowPreviousQuarter()
    {
        ReportModel = _dataModel.Analyze(_asset, _activePeriod).CreateReport();
    }

    public void ShowNextQuarter()
    {
        ReportModel = _dataModel.Analyze(_asset, _activePeriod).CreateReport();
    }
    public void ShowNextYear()
    {
        ReportModel = _dataModel.Analyze(_asset, _activePeriod).CreateReport();
    }

    public void ShowPreviousYear()
    {
        ReportModel = _dataModel.Analyze(_asset, _activePeriod).CreateReport();
    }

}
