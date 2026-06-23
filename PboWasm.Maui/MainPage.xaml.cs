namespace PboWasm.Maui;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        // Global C# Unhandled Exceptions
        AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
        {
            var exception = args.ExceptionObject as Exception;
            ShowError("Unhandled C# Exception:\n" + (exception?.ToString() ?? "Unknown exception"));
        };

        TaskScheduler.UnobservedTaskException += (sender, args) =>
        {
            ShowError("Unobserved Task Exception:\n" + args.Exception?.ToString());
            args.SetObserved();
        };

        try
        {
            InitializeComponent();
            blazorWebView.BlazorWebViewInitialized += (s, e) => System.Diagnostics.Debug.WriteLine("BlazorWebView initialized OK");
            blazorWebView.UrlLoading += (s, e) => System.Diagnostics.Debug.WriteLine($"UrlLoading: {e.Url}");

#if ANDROID
            Platforms.Android.PermissionWebChromeClient.OnConsoleError += (message) =>
            {
                ShowError(message);
            };
#endif
        }
        catch (Exception ex)
        {
            // InitializeComponent itself failed
            ShowError("InitializeComponent failed:\n" + ex.ToString());
        }
    }

    private void ShowError(string message)
    {
        Dispatcher.Dispatch(() =>
        {
            if (string.IsNullOrEmpty(errorLabel.Text))
            {
                errorLabel.Text = message;
            }
            else
            {
                // Concatenate multiple errors if they happen
                errorLabel.Text += "\n\n-------------------\n\n" + message;
            }
            errorPanel.IsVisible = true;
        });
    }
}
