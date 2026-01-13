using KeeperDomain;

namespace KeeperWpf;

public class AssetOnPeriodData
{
    public Period Period;

    public AssetState Before { get; set; }
    public AssetState OnStart { get; set; }
    public AssetState InBetween { get; set; }
    public AssetState AtEnd { get; set; }
}