using System.IO;
using System.Text.Json;

namespace MDict.Csharp.Demo.WPF;

public class Settings
{
    public string DictPath { get; set; } = string.Empty;

    private static readonly string SettingsPath = Path.Combine(AppContext.BaseDirectory, "MDict.Csharp.Demo.WPF.json");

    public static Settings Load()
    {
        if (!File.Exists(SettingsPath))
        {
            return new Settings();
        }
        var json = File.ReadAllText(SettingsPath);
        return JsonSerializer.Deserialize<Settings>(json) ?? new Settings();
    }

    public void Save()
    {
        var json = JsonSerializer.Serialize(this);
        File.WriteAllText(SettingsPath, json);
    }
}
