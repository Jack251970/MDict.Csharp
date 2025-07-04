using Microsoft.Win32;

namespace MDict.Csharp.Demo.WPF;

public static class ThemeHelper
{
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
}
