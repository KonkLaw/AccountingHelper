using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace AccountHelperWpf.Views;

partial class UpdateInfoWindow : Window
{
    private readonly Func<Task> downloadDelegate;
    private readonly Func<Task> extractArchive;

    public UpdateInfoWindow(Func<Task> downloadDelegate, Func<Task> extractArchive)
    {
        this.downloadDelegate = downloadDelegate;
        this.extractArchive = extractArchive;

        InitializeComponent();
        Loaded += OnLoaded;
        Activated += OnActivated;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        IntPtr handle = new WindowInteropHelper(this).Handle;
        WindowHelper.Hide(handle);
    }

    private async void OnActivated(object? sender, EventArgs e)
    {
        Label.Content = "Downloading...";
        await downloadDelegate();
        Label.Content = "Extracting...";
        await extractArchive();
        Close();
    }
}

static class WindowHelper
{
    private const int GWL_STYLE = -16;
    private const int WS_SYSMENU = 0x80000;
    [DllImport("user32.dll", SetLastError = true)]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    public static void Hide(IntPtr hwnd)
    {
        SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU);
    }
}