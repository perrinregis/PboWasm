namespace PboWasm.Maui;

public partial class App : Application
{
    public App()
    {
        System.Diagnostics.Debugger.Break();
        InitializeComponent();

        MainPage = new AppShell();
    }
}
