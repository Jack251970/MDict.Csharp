using CommunityToolkit.Mvvm.Input;
using MDict.Csharp.Models;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace MDict.Csharp.Demo.WPF;

public partial class MainWindow : Window
{
    public ObservableCollection<FuzzyWord> FuzzyWords { get; } = [];

    public ThemeHelper ThemeHelper { get; } = new();

    private MdxDict? Dict { get; set; }
    private string? DictDirectory { get; set; }

    private const string VirtualHost = "appassets";

    private readonly Settings _settings;
    private readonly Dictionary<string, string> _webView2PathMapping = [];

    public MainWindow()
    {
        InitializeComponent();
        _settings = Settings.Load();
        if (!string.IsNullOrEmpty(_settings.DictPath))
        {
            DictPath.Text = _settings.DictPath;
            LoadDictionary(_settings.DictPath);
        }
        LightThemeCss.Text = _settings.LightThemeCss;
        DarkThemeCss.Text = _settings.DarkThemeCss;
        ThemeHelper.ThemeChanged += ThemeHelper_ThemeChanged;
    }

    private void ThemeHelper_ThemeChanged(object? sender, ThemeChangedEventArgs e)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            if (FuzzyWordListView.SelectedItem is FuzzyWord word)
            {
                UpdateResultWebView2(word, e.IsDarkTheme);
            }
        });
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
            DictPath.Text = dlg.FileName;
            LoadDictionary(dlg.FileName);
        }
    }

    private void LoadDictionary(string path)
    {
        // Check path exists
        if (string.IsNullOrEmpty(path) || !File.Exists(path)) return;

        // Clear the previous dictionary and fuzzy words
        FuzzyWords.Clear();

        // Close the previous dictionary if it exists
        Dict?.Close();

        // Load the new dictionary
        try
        {
            Dict = new MdxDict(path);
        }
        catch (Exception ex)
        {
            Dict = null;
            MessageBox.Show($"Failed to load dictionary: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        // Load the web view paths
        DictDirectory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(DictDirectory))
        {
            _webView2PathMapping.Clear();
            foreach (var file in Directory.EnumerateFiles(DictDirectory))
            {
                var relatedPath = Path.GetRelativePath(DictDirectory, file);
                if (!string.IsNullOrEmpty(relatedPath))
                {
                    // If related path contains backslashes or forward slashes, we need to add two versions of replacement rules
                    if (relatedPath.Contains('\\'))
                    {
                        var unixRelatedPath = relatedPath.Replace('\\', '/');
                        _webView2PathMapping[relatedPath] = $"https://{VirtualHost}/{unixRelatedPath}";
                        _webView2PathMapping[unixRelatedPath] = $"https://{VirtualHost}/{unixRelatedPath}";
                    }
                    else if (relatedPath.Contains('/'))
                    {
                        var winRelatedPath = relatedPath.Replace('/', '\\');
                        _webView2PathMapping[relatedPath] = $"https://{VirtualHost}/{relatedPath}";
                        _webView2PathMapping[winRelatedPath] = $"https://{VirtualHost}/{relatedPath}";
                    }
                    // Else just add the related path as it is
                    else
                    {
                        _webView2PathMapping[relatedPath] = $"https://{VirtualHost}/{relatedPath}";
                    }
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

    private void FuzzyWordListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (FuzzyWordListView.SelectedItem is not FuzzyWord word) return;

        UpdateResultWebView2(word, ThemeHelper.IsDarkTheme());
    }

    private async void UpdateResultWebView2(FuzzyWord word, bool isDarkTheme)
    {
        if (Dict is null || string.IsNullOrEmpty(DictDirectory) || word is null || ResultWebView2 is null) return;

        var (_, Definition) = Dict.Fetch(word);
        if (Definition is null) return;

        var newDefinition = new StringBuilder(Definition);

        // Replace the light and dark theme CSS based on the system theme
        if (!string.IsNullOrEmpty(_settings.LightThemeCss) && !string.IsNullOrEmpty(_settings.DarkThemeCss))
        {
            if (isDarkTheme)
            {
                newDefinition.Replace(_settings.LightThemeCss, _settings.DarkThemeCss);
            }
            else
            {
                newDefinition.Replace(_settings.DarkThemeCss, _settings.LightThemeCss);
            }
        }

        // Replace relative paths with virtual host urls
        foreach (var kvp in _webView2PathMapping)
        {
            newDefinition.Replace(kvp.Key, kvp.Value);
        }

        await ResultWebView2.EnsureCoreWebView2Async();

        ResultWebView2.CoreWebView2.SetVirtualHostNameToFolderMapping(
            VirtualHost,
            DictDirectory,
            Microsoft.Web.WebView2.Core.CoreWebView2HostResourceAccessKind.Allow
        );

        ResultWebView2.NavigateToString(newDefinition.ToString());
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        ThemeHelper.Dispose();
        Dict?.Close();
    }
}
