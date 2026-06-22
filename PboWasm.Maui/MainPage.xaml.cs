namespace PboWasm.Maui;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        try
        {
            InitializeComponent();
            blazorWebView.BlazorWebViewInitialized += (s, e) => System.Diagnostics.Debug.WriteLine("BlazorWebView initialized OK");
            blazorWebView.UrlLoading += (s, e) => System.Diagnostics.Debug.WriteLine($"UrlLoading: {e.Url}");
        }
        catch (Exception ex)
        {
            // InitializeComponent itself failed
            Dispatcher.Dispatch(() =>
            {
                errorLabel.Text = "InitializeComponent failed:\n" + ex.ToString();
                errorPanel.IsVisible = true;
            });
        }
    }

    private void ShowError(string message)
    {
        Dispatcher.Dispatch(() =>
        {
            errorLabel.Text = message;
            errorPanel.IsVisible = true;
        });
    }
}
