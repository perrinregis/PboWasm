using Android.App;
using Android.Content.PM;
using Android.OS;

namespace PboWasm.Maui;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        Android.Webkit.WebView.SetWebContentsDebuggingEnabled(true);
        base.OnCreate(savedInstanceState);
    }
}
