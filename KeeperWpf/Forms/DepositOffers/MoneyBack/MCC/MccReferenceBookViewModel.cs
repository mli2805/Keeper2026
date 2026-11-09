using Caliburn.Micro;
using System.IO;
using System.Reflection;
using System.Text;

namespace KeeperWpf;

[ExportViewModel(ViewModelLifetime.SingleInstance)]
public class MccReferenceBookViewModel : Screen
{
    public BindableCollection<MccCodeItem> Rows { get; } = new BindableCollection<MccCodeItem>();

    protected override void OnViewLoaded(object view)
    {
        DisplayName = "Справочник MCC кодов";
        if (Rows.Count > 0) return;

        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("KeeperWpf.assets.mcc_codes.csv");
        if (stream == null) return;

        using var reader = new StreamReader(stream, Encoding.UTF8);
        Rows.AddRange(MccCodesParser.Parse(reader.ReadToEnd()));
    }
}
