using CommunityToolkit.Mvvm.Input;
using MDict.Csharp.Models;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace MDict.Csharp.Demo.WPF;

public partial class MainWindow : Window
{
    public ObservableCollection<FuzzyWord> FuzzyWords { get; } = [];

    private MdxDict? Dict { get; set; }

    private readonly Settings _settings;

    public MainWindow()
    {
        InitializeComponent();
        _settings = Settings.Load();
        Path.Text = _settings.DictPath ?? string.Empty;
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
            // Clear the previous dictionary and fuzzy words
            FuzzyWords.Clear();
            // Close the previous dictionary if it exists
            Dict?.Close();
            // Load the new dictionary
            Dict = new MdxDict(dlg.FileName);
            // Search for fuzzy words if the search box is not empty
            if (!string.IsNullOrEmpty(Search.Text))
            {
                Search_TextChanged(Search, new TextChangedEventArgs(TextBoxBase.TextChangedEvent, UndoAction.None));
            }
            // Save the new path to settings
            _settings.DictPath = dlg.FileName;
            _settings.Save();
        }
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

        await ResultWebView2.EnsureCoreWebView2Async();
        ResultWebView2.NavigateToString(Definition);
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        Dict?.Close();
    }
}
