using System.ComponentModel.DataAnnotations;

namespace KeeperInfrastructure;

public class ButtonCollectionEf
{
    public int Id { get; set; }
    [MaxLength(50)] public string Name { get; set; } = null!;
    [MaxLength(100)] public string AccountIdsString { get; set; } = null!;
}