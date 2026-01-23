using System.Collections.Generic;
using System.Windows.Media;
using KeeperDomain;

namespace KeeperWpf;

public class AssetAnalysisModel
{
    public List<string> Content { get; set; } = new List<string>();
    public decimal ProfitInUsd { get; set; }
    public Brush Brush => ProfitInUsd > 0 ? Brushes.Blue : Brushes.Red;
}

public class AssetOnPeriodReportModel
{
    public Period Period { get; set; } = null!;

    public List<string> BeforeState { get; set; } = null!;
    public List<string> BeforeFeesAndCoupons { get; set; } = null!;

    public List<string> OnStartState { get; set; } = null!;
    public List<string> OnStartFeesAndCoupons { get; set; } = null!;        
    public AssetAnalysisModel OnStartAnalysis { get; set; } = null!;

    public List<string> InBetweenTrans { get; set; } = null!;
    public List<string> InBetweenFeesAndCoupons { get; set; } = null!;

    public List<string> AtEndState { get; set; } = null!;
    public List<string> AtEndFeesAndCoupons { get; set; } = null!;
    public AssetAnalysisModel AtEndAnalysis { get; set; } = null!;  
}
