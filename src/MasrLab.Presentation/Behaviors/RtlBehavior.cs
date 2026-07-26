using System.Windows;

namespace MasrLab.Presentation.Behaviors;

/// <summary>
/// سلوك RTL (متطلب 1): يضبط اتجاه التدفق من اليمين لليسار على أي عنصر واجهة.
/// يُطبَّق تصريحاً في XAML عبر خاصية مرفقة:
///     &lt;Window behaviors:RtlBehavior.IsEnabled="True" /&gt;
/// هيكل أساسي — منطق الواجهة التفصيلي يُستكمل في مرحلة تصميم الشاشات.
/// </summary>
public static class RtlBehavior
{
    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached(
            "IsEnabled",
            typeof(bool),
            typeof(RtlBehavior),
            new PropertyMetadata(false, OnIsEnabledChanged));

    public static bool GetIsEnabled(DependencyObject element)
    {
        ArgumentNullException.ThrowIfNull(element);
        return (bool)element.GetValue(IsEnabledProperty);
    }

    public static void SetIsEnabled(DependencyObject element, bool value)
    {
        ArgumentNullException.ThrowIfNull(element);
        element.SetValue(IsEnabledProperty, value);
    }

    private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FrameworkElement element)
        {
            return;
        }

        element.FlowDirection = e.NewValue is true
            ? FlowDirection.RightToLeft
            : FlowDirection.LeftToRight;
    }
}
