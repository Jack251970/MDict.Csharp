using CommunityToolkit.Mvvm.Input;
using MDict.Csharp.Models;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace MDict.Csharp.Demo.WPF;

public partial class MainWindow : Window
{
    public ObservableCollection<FuzzyWord> FuzzyWords { get; } = [];

    private MdxDict? Dict { get; set; }
    private string? RelatedPath { get; set; }

    private const string VirtualHost = "appassets";

    private readonly Settings _settings;
    private readonly Dictionary<string, string> _webView2PathMapping = [];

    public MainWindow()
    {
        InitializeComponent();
        _settings = Settings.Load();
        if (!string.IsNullOrEmpty(_settings.DictPath))
        {
            Path.Text = _settings.DictPath;
            LoadDictionary(_settings.DictPath);
        }
        LightThemeCss.Text = _settings.LightThemeCss;
        DarkThemeCss.Text = _settings.DarkThemeCss;
    }

    [RelayCommand]
    private void NavigatePath()
    {
        var dlg = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "MDict Files (*.mdx)|*.mdx"
        };
        if (dlg.ShowDialog() == true && !string.IsNullOrEmpty(dlg.FileName))
        {
            Path.Text = dlg.FileName;
            LoadDictionary(dlg.FileName);
        }
    }

    private void LoadDictionary(string path)
    {
        // Clear the previous dictionary and fuzzy words
        FuzzyWords.Clear();
        // Close the previous dictionary if it exists
        Dict?.Close();
        // Load the new dictionary
        Dict = new MdxDict(path);
        // Load the web view paths
        RelatedPath = System.IO.Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(RelatedPath))
        {
            _webView2PathMapping.Clear();
            foreach (var file in System.IO.Directory.EnumerateFiles(RelatedPath))
            {
                var relatedPath = System.IO.Path.GetRelativePath(RelatedPath, file);
                if (!string.IsNullOrEmpty(relatedPath))
                {
                    _webView2PathMapping[relatedPath] = $"https://{VirtualHost}/{relatedPath}";
                }
            }
        }
        // Search for fuzzy words if the search box is not empty
        if (!string.IsNullOrEmpty(Search.Text))
        {
            Search_TextChanged(Search, new TextChangedEventArgs(TextBoxBase.TextChangedEvent, UndoAction.None));
        }
        // Save the new path to settings
        _settings.DictPath = path;
        _settings.Save();
    }

    private void Search_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (Dict == null || sender is not TextBox textBox) return;

        FuzzyWords.Clear();

        var text = textBox.Text;
        if (!string.IsNullOrWhiteSpace(text))
        {
            var result = Dict.FuzzySearch(text, 99, 5);
            foreach (var word in result)
            {
                FuzzyWords.Add(word);
            }
        }
    }

    private void LightThemeCss_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is not TextBox textBox) return;

        _settings.LightThemeCss = textBox.Text;
        _settings.Save();
    }

    private void DarkThemeCss_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is not TextBox textBox) return;

        _settings.DarkThemeCss = textBox.Text;
        _settings.Save();
    }

    private async void FuzzyWordListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (Dict == null || FuzzyWordListView.SelectedItem is not FuzzyWord word) return;

        var (_, Definition) = Dict.Fetch(word);
        if (Definition is null) return;

        var newDefinition = new StringBuilder(Definition);

        // Replace the light and dark theme CSS based on the system theme
        if (IsSystemDarkTheme())
        {
            newDefinition.Replace(_settings.LightThemeCss, _settings.DarkThemeCss);
        }
        else
        {
            newDefinition.Replace(_settings.DarkThemeCss, _settings.LightThemeCss);
        }

        // Replace relative paths with virtual host urls
        foreach (var kvp in _webView2PathMapping)
        {
            newDefinition.Replace(kvp.Key, kvp.Value);
        }

        await ResultWebView2.EnsureCoreWebView2Async();

        ResultWebView2.CoreWebView2.SetVirtualHostNameToFolderMapping(
            VirtualHost,
            RelatedPath,
            Microsoft.Web.WebView2.Core.CoreWebView2HostResourceAccessKind.Allow
        );
        ResultWebView2.NavigateToString(newDefinition.ToString());
    }

    private static bool IsSystemDarkTheme()
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

    private void Window_Closed(object sender, EventArgs e)
    {
        Dict?.Close();
    }
}
