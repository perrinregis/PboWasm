using Android.App;
using Android.Content.PM;
using Android.OS;

namespace PboWasm.Maui;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.M)
        {
            if (CheckSelfPermission(Android.Manifest.Permission.Camera) != Permission.Granted)
            {
                RequestPermissions(new string[] { Android.Manifest.Permission.Camera }, 0);
            }
        }
    }
}
