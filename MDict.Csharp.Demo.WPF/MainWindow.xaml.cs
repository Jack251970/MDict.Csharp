using CommunityToolkit.Mvvm.Input;
using MDict.Csharp.Models;
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

    private async void FuzzyWordListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (Dict == null || FuzzyWordListView.SelectedItem is not FuzzyWord word) return;

        var (_, Definition) = Dict.Fetch(word);
        if (Definition is null) return;

        var newDefinition = new StringBuilder(Definition);
        // Replace relative paths with virtual host urls
        foreach (var kvp in _webView2PathMapping)
        {
            newDefinition.Replace(kvp.Key, kvp.Value);
        }
        var newDefinitionString = newDefinition.ToString();

        await ResultWebView2.EnsureCoreWebView2Async();

        ResultWebView2.CoreWebView2.SetVirtualHostNameToFolderMapping(
            "appassets",
            RelatedPath,
            Microsoft.Web.WebView2.Core.CoreWebView2HostResourceAccessKind.Allow
        );
        ResultWebView2.NavigateToString(newDefinitionString);
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        Dict?.Close();
    }
}
