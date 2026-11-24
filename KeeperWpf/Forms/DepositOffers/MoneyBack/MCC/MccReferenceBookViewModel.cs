using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace KeeperWpf;

[ExportViewModel(ViewModelLifetime.SingleInstance)]
public class MccReferenceBookViewModel : Screen
{
    public BindableCollection<MccCodeItem> Rows { get; } = new BindableCollection<MccCodeItem>();

    private string _filterText = string.Empty;
    public string FilterText
    {
        get => _filterText;
        set
        {
            if (Set(ref _filterText, value))
                NotifyOfPropertyChange(nameof(RowsFiltered));
        }
    }

    public IEnumerable<MccCodeItem> RowsFiltered => string.IsNullOrWhiteSpace(FilterText)
        ? Rows
        : Rows.Where(r => r.Code.Contains(FilterText, StringComparison.OrdinalIgnoreCase)
                      || r.Name.Contains(FilterText, StringComparison.OrdinalIgnoreCase)
                      || r.Description.Contains(FilterText, StringComparison.OrdinalIgnoreCase));

    protected override void OnViewLoaded(object view)
    {
        DisplayName = "Справочник MCC кодов";
        if (Rows.Count > 0) return;

        using var stream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream("KeeperWpf.assets.mcc_codes.csv");
        if (stream == null) return;

        using var reader = new StreamReader(stream, Encoding.UTF8);
        Rows.AddRange(MccCodesParser.Parse(reader.ReadToEnd()));
    }
}
