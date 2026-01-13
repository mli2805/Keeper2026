using Caliburn.Micro;
using OxyPlot.Axes;
using System.Collections.Generic;
using System.Windows.Input;


namespace KeeperWpf;

public class LongTermChartViewModel : Screen
{
    private string _caption;

    public LongTermChartModel Model { get; set; }

    protected override void OnViewLoaded(object view)
    {
        DisplayName = _caption;
    }

    public void Initalize(string caption, List<OfficialRatesModel> rates, KeeperDataModel keeperDataModel)
    {
        _caption = caption;
        Model = new LongTermChartModel();
        Model.Build(rates, keeperDataModel);
    }


    public void ToggleLogarithm(KeyEventArgs e)
    {
        if (e != null && e.Key != Key.L) return;

        var currentIsLogarithmic = Model.LongTermModel.Axes[1].GetType() == typeof(LogarithmicAxis);
        if (currentIsLogarithmic)
            Model.LongTermModel.Axes[1] = new LinearAxis { Position = AxisPosition.Left };
        else
            Model.LongTermModel.Axes[1] = new LogarithmicAxis { Position = AxisPosition.Left };
        Model.LongTermModel.InvalidatePlot(true);
    }

}
