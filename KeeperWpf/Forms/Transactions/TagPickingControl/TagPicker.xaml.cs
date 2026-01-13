using System.Windows;
using System.Windows.Controls;

namespace KeeperWpf;

/// <summary>
/// Interaction logic for TagPicker.xaml
/// </summary>
public partial class TagPicker
{
    public TagPicker()
    {
        InitializeComponent();
    }

    private void DeleteTagOnClick(object sender, RoutedEventArgs e)
    {
        var tagPickerVm = (TagPickerVm)DataContext;
        tagPickerVm.TagInWork = (AccName)((Button)sender).Tag;
        tagPickerVm.Tags.Remove(tagPickerVm.TagInWork);
    }

    private void AddTagOnClick(object sender, RoutedEventArgs e)
    {
        var tagPickerVm = (TagPickerVm)DataContext;
        tagPickerVm.TagInWork = tagPickerVm.TagSelectorVm.MyAccName;
        tagPickerVm.Tags.Add(tagPickerVm.TagInWork);
    }


}
