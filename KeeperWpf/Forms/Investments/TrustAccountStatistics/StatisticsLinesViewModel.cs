using System.Collections.Generic;

namespace KeeperWpf;

[ExportViewModel]
public class StatisticsLinesViewModel
{
    public List<TrustStatisticsLine> Rows { get; set; } = null!;
}
