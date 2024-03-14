using System.Windows;
using Microsoft.Xaml.Behaviors;

namespace AccountHelperWpf.Utils;

public class CloseWindowBehavior : Behavior<Window>
{
    public bool CloseTrigger
    {
        get => (bool)GetValue(CloseTriggerProperty);
        set => SetValue(CloseTriggerProperty, value);
    }

    public static readonly DependencyProperty CloseTriggerProperty =
        DependencyProperty.Register(nameof(CloseTrigger), typeof(bool), typeof(CloseWindowBehavior), new PropertyMetadata(false, OnCloseTriggerChanged));

    private static void OnCloseTriggerChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is CloseWindowBehavior behavior)
        {
            behavior.OnCloseTriggerChanged();
        }
    }

    private void OnCloseTriggerChanged()
    {
        // when close trigger is true, close the window
        if (CloseTrigger)
        {
            AssociatedObject.Close();
        }
    }
}
