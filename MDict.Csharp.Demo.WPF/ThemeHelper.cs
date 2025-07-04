using Microsoft.Win32;

namespace MDict.Csharp.Demo.WPF;

public class ThemeHelper : IDisposable
{
    public EventHandler<ThemeChangedEventArgs>? ThemeChanged;

    public ThemeHelper()
    {
        SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;
    }

    public void Dispose()
    {
        SystemEvents.UserPreferenceChanged -= SystemEvents_UserPreferenceChanged;
        GC.SuppressFinalize(this);
    }

    public static bool IsDarkTheme()
    {
        using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
        var registryValueObject = key?.GetValue("AppsUseLightTheme");
        if (registryValueObject is null)
        {
            return false; // Default to light theme if the registry value is not found
        }

        if (registryValueObject is int registryValueInt)
        {
            return registryValueInt <= 0; // 0 means dark theme, 1 means light theme
        }

        if (registryValueObject is string registryValueString && int.TryParse(registryValueString, out var parsedValue))
        {
            return parsedValue == 0; // 0 means dark theme, 1 means light theme
        }

        return false; // Default to light theme if the value is not an int or string
    }

    private void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
    {
        switch (e.Category)
        {
            case UserPreferenceCategory.General:
                ThemeChanged?.Invoke(this, new ThemeChangedEventArgs(IsDarkTheme()));
                break;
        }
    }
}

public class ThemeChangedEventArgs(bool isDarkTheme) : EventArgs
{
    public bool IsDarkTheme { get; } = isDarkTheme;
}
