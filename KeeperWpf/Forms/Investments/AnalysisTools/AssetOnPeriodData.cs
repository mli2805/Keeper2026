using KeeperDomain;

namespace KeeperWpf;

public class AssetOnPeriodData
{
    public Period Period = null!;

    public AssetState Before { get; set; } = null!;
    public AssetState OnStart { get; set; } = null!;
    public AssetState InBetween { get; set; } = null!;
    public AssetState AtEnd { get; set; } = null!;
}