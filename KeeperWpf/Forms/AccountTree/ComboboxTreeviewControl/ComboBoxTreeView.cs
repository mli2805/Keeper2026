//http://stackoverflow.com/questions/722700/wpf-treeview-inside-a-combobox

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace KeeperWpf;

public class ComboBoxTreeView : ComboBox
{
    private ExtendedTreeView _treeView = null!;
    private ContentPresenter _contentPresenter = null!;
    private ScrollViewer _scrollViewer = null!;

    static ComboBoxTreeView()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ComboBoxTreeView), new FrameworkPropertyMetadata(typeof(ComboBoxTreeView)));
    }

    private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T typedChild)
            {
                return typedChild;
            }

            var result = FindVisualChild<T>(child);
            if (result != null)
            {
                return result;
            }
        }
        return null;
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _treeView = (ExtendedTreeView)GetTemplateChild("treeView");
        if (_treeView != null)
        {
            _treeView.OnHierarchyMouseUp += OnTreeViewHierarchyMouseUp;
            _treeView.PreviewMouseWheel += OnTreeViewPreviewMouseWheel;
        }

        _scrollViewer = (ScrollViewer)GetTemplateChild("DropDownScrollViewer");
        
        _contentPresenter = (ContentPresenter)GetTemplateChild("ContentPresenter");

        SetSelectedItemToHeader();
    }

    private void OnTreeViewPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (_scrollViewer == null) return;

        if (e.Delta > 0)
        {
            _scrollViewer.LineUp();
        }
        else
        {
            _scrollViewer.LineDown();
        }
        e.Handled = true;
    }

    protected override void OnDropDownClosed(EventArgs e)
    {
        base.OnDropDownClosed(e);
        var item = (ITreeViewItemModel)_treeView.SelectedItem;
        if (item == null) return;

        //if branch (not a leaf) is chosen - changes are not accepted
        //if  (!IsBranchSelectionEnabled  &&  item.GetChildren().Any() ) return;
        if (!IsBranchSelectionEnabled && ((AccName)item).IsFolder) return;

        SelectedItem = _treeView.SelectedItem;
        SetSelectedItemToHeader();
    }

    protected override void OnDropDownOpened(EventArgs e)
    {
        base.OnDropDownOpened(e);
        SetSelectedItemToHeader();
        ScrollToSelectedItem();
    }

    private void ScrollToSelectedItem()
    {
        if (_treeView == null || SelectedItem == null) return;

        // Даем время на отрисовку визуального дерева
        Dispatcher.BeginInvoke(new Action(() =>
        {
            var selectedItem = SelectedItem as ITreeViewItemModel;
            if (selectedItem == null) return;

            // Раскрываем все родительские элементы до выбранного
            var hierarchy = selectedItem.GetHierarchy().ToList();
            foreach (var item in hierarchy.Take(hierarchy.Count - 1))
            {
                item.IsExpanded = true;
            }

            // Устанавливаем выбранный элемент
            selectedItem.IsSelected = true;

            // Даем время на раскрытие элементов
            Dispatcher.BeginInvoke(new Action(() =>
            {
                // Находим TreeViewItem в визуальном дереве
                var treeViewItem = FindTreeViewItem(_treeView, selectedItem);
                if (treeViewItem != null)
                {
                    treeViewItem.BringIntoView();
                }
            }), System.Windows.Threading.DispatcherPriority.Loaded);

        }), System.Windows.Threading.DispatcherPriority.Background);
    }

    private TreeViewItem? FindTreeViewItem(ItemsControl container, object item)
    {
        if (container == null) return null;

        var itemContainer = container.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;
        if (itemContainer != null)
        {
            return itemContainer;
        }

        // Ищем рекурсивно в дочерних элементах
        foreach (var childItem in container.Items)
        {
            var parent = container.ItemContainerGenerator.ContainerFromItem(childItem) as TreeViewItem;
            if (parent == null) continue;

            var foundItem = FindTreeViewItem(parent, item);
            if (foundItem != null)
            {
                return foundItem;
            }
        }

        return null;
    }

    /// <summary>
    /// Handles clicks on any item in the tree view
    /// </summary>
    private void OnTreeViewHierarchyMouseUp(object sender, MouseEventArgs e)
    {
        var item = (ITreeViewItemModel)_treeView.SelectedItem;
        // if branch (not a leaf) is chosen don't close combobox
        // if (IsBranchSelectionEnabled || !item.GetChildren().Any())
        if (IsBranchSelectionEnabled || !((AccName)item).IsFolder)
            IsDropDownOpen = false;
    }

    /// <summary>
    /// Selected item of the TreeView
    /// </summary>
    public new object SelectedItem
    {
        get { return GetValue(SelectedItemProperty); }
        set { SetValue(SelectedItemProperty, value); }
    }

    public new static readonly DependencyProperty SelectedItemProperty =
        DependencyProperty.Register("SelectedItem", typeof(object), typeof(ComboBoxTreeView), new PropertyMetadata(null, OnSelectedItemChanged));

    private static void OnSelectedItemChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        ((ComboBoxTreeView)sender).UpdateSelectedItem();
    }


    public bool IsBranchSelectionEnabled
    {
        get { return (bool)GetValue(IsBranchSelectionEnabledProperty); }
        set { SetValue(IsBranchSelectionEnabledProperty, value); }
    }

    public static readonly DependencyProperty IsBranchSelectionEnabledProperty =
        DependencyProperty.Register("IsBranchSelectionEnabled", typeof(bool), typeof(ComboBoxTreeView));

    /// <summary>
    /// Selected hierarchy of the treeview
    /// </summary>
    public IEnumerable<string> SelectedHierarchy
    {
        get { return (IEnumerable<string>)GetValue(SelectedHierarchyProperty); }
        set { SetValue(SelectedHierarchyProperty, value); }
    }

    public static readonly DependencyProperty SelectedHierarchyProperty =
        DependencyProperty.Register("SelectedHierarchy", typeof(IEnumerable<string>), typeof(ComboBoxTreeView), new PropertyMetadata(null, OnSelectedHierarchyChanged));

    private static void OnSelectedHierarchyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        ((ComboBoxTreeView)sender).UpdateSelectedHierarchy();
    }

    private void UpdateSelectedItem()
    {
        if (SelectedItem is TreeViewItem)
        {
            //I would rather use a correct object instead of TreeViewItem
            SelectedItem = ((TreeViewItem)SelectedItem).DataContext;
        }
        else
        {
            //Update the selected hierarchy and displays
            var model = SelectedItem as ITreeViewItemModel;
            if (model != null)
            {
                SelectedHierarchy = model.GetHierarchy().Select(h => h.SelectedValuePath).ToList();
            }

            SetSelectedItemToHeader();
        }
    }

    private void UpdateSelectedHierarchy()
    {
        if (ItemsSource != null && SelectedHierarchy != null)
        {
            //Find corresponding items and expand or select them
            var source = ItemsSource.OfType<ITreeViewItemModel>();
            var item = SelectItem(source, SelectedHierarchy);
            SelectedItem = item!;
        }
    }

    /// <summary>
    /// Searches the items of the hierarchy inside the items source and selects the last found item
    /// </summary>
    private static ITreeViewItemModel? SelectItem(IEnumerable<ITreeViewItemModel> items, IEnumerable<string> selectedHierarchy)
    {
        if (items == null || selectedHierarchy == null)
        {
            return null;
        }

        var hierarchy = selectedHierarchy.ToList();
        var currentItems = items.ToList();

        if (!hierarchy.Any() || !currentItems.Any()) return null;

        ITreeViewItemModel? selectedItem = null;

        for (int i = 0; i < hierarchy.Count; i++)
        {
            // get next item in the hierarchy from the collection of child items
            var currentItem = currentItems.FirstOrDefault(ci => ci.SelectedValuePath == hierarchy[i]);
            if (currentItem == null)
            {
                break;
            }

            selectedItem = currentItem;

            // rewrite the current collection of child items
            currentItems = selectedItem.GetChildren().ToList();
            if (!currentItems.Any())
            {
                break;
            }

            // the intermediate items will be expanded
            if (i != hierarchy.Count - 1)
            {
                selectedItem.IsExpanded = true;
            }
        }

        if (selectedItem != null)
        {
            selectedItem.IsSelected = true;
        }

        return selectedItem;
    }

    /// <summary>
    /// Gets the hierarchy of the selected tree item and displays it at the combobox header
    /// </summary>
    private void SetSelectedItemToHeader()
    {
        string? content = null;

        var item = SelectedItem as ITreeViewItemModel;
        if (item != null)
        {
            content = item.DisplayValuePath;
        }

        SetContentAsTextBlock(content);
    }

    /// <summary>
    /// Gets the combobox header and displays the specified content there
    /// </summary>
    private void SetContentAsTextBlock(string? content)
    {
        if (_contentPresenter == null)
        {
            return;
        }

        var tb = _contentPresenter.Content as TextBlock;
        if (tb == null)
        {
            _contentPresenter.Content = tb = new TextBlock();
        }
        tb.Text = content ?? ' '.ToString();

        _contentPresenter.ContentTemplate = null;
    }
}
