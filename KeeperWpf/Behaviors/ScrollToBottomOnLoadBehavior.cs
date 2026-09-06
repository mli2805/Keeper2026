using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Microsoft.Xaml.Behaviors;


namespace KeeperWpf;

public class ScrollToBottomOnLoadBehavior : Behavior<DataGrid>
{
    protected override void OnAttached()
    {
        AssociatedObject.Loaded += AssociatedObjectOnLoaded;
        AssociatedObject.IsVisibleChanged += AssociatedObjectOnIsVisibleChanged;
    }

    protected override void OnDetaching()
    {
        AssociatedObject.Loaded -= AssociatedObjectOnLoaded;
        AssociatedObject.IsVisibleChanged -= AssociatedObjectOnIsVisibleChanged;
    }

    private void AssociatedObjectOnLoaded(object sender, RoutedEventArgs routedEventArgs)
    {
        ScrollToBottom();
    }

    private void AssociatedObjectOnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is true)
            ScrollToBottom();
    }

    private void ScrollToBottom()
    {
        AssociatedObject.Dispatcher.BeginInvoke(() =>
        {
            if (AssociatedObject.Items.Count > 0)
                AssociatedObject.ScrollIntoView(AssociatedObject.Items[^1]);
        }, DispatcherPriority.Loaded);
    }
}