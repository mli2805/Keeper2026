using System.Collections.ObjectModel;
using Caliburn.Micro;

namespace KeeperWpf;

public class TagPickerVm : PropertyChangedBase
{
    public ObservableCollection<AccName> Tags { get; set; } = new ObservableCollection<AccName>();
    public AccName? TagInWork { get; set; }
    public AccNameSelectorVm TagSelectorVm { get; set; }
}
