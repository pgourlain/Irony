using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using Avalonia.Metadata;
using AvaloniaEdit.Document;
using DynamicData;
using HarfBuzzSharp;
using Irony.GrammarExplorerExecutor;
using Irony.GrammarExplorerXaml.Models;
using Irony.GrammarExplorerXaml.Views;
using Irony.Parsing;
using Microsoft.CodeAnalysis;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Reactive;
using System.Reactive.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Irony.GrammarExplorerXaml.ViewModels
{
  public class MainWindowViewModel : ViewModelBase, IDisposable
  {
    public MainWindowViewModel()
    {
      var canParse = Observable.FromEvent<EventHandler<DocumentChangeEventArgs>, bool>(
        onNextHandler => (_, __) =>
        {
          var hasTextToParse = !string.IsNullOrEmpty(this.TextToParse.Text);
          hasTextToParse = hasTextToParse && this.LangInfo != null && _grammarLoader != null;
          onNextHandler(hasTextToParse);
        },
        h => this.TextToParse.Changed += h,
        h => this.TextToParse.Changed -= h);
      var canReleaseGrammar = Observable.FromEvent<PropertyChangedEventHandler, bool>(
         onNextHandler => (_, arg) =>
         {
           var value = string.IsNullOrWhiteSpace(arg.PropertyName) || arg.PropertyName == nameof(LangInfo);
           onNextHandler(value);
         },
        h => this.PropertyChanged += h,
        h => this.PropertyChanged -= h);
      OpenGrammarCommand = ReactiveCommand.CreateFromTask(OnGrammarOpenCommand);
      ParseCommand = ReactiveCommand.CreateFromTask(OnParseCommand, canParse);
      LoadParseCommand = ReactiveCommand.CreateFromTask(OnLoadParseCommand);
      ReleaseGrammarCommand = ReactiveCommand.CreateFromTask(OnReleaseGrammarCommand, canReleaseGrammar);
    }
    public string Greeting => LangInfo != null ? LangInfo.Description : "No Grammar loaded";

    public string TerminalsText => LangInfo?.TerminalsText ?? string.Empty;
    public string NonTerminalsText => LangInfo?.NonTerminalsText ?? string.Empty;

    public string ParserStateText => LangInfo?.ParserStateText ?? string.Empty;

    public string CountStates => LangInfo?.CountStates ?? string.Empty;
    public string ConstructionTime => LangInfo?.ConstructionTime ?? string.Empty;

    public int BottomSelectedIndex { get; set; }

    public ReactiveCommand<Unit, Unit> OpenGrammarCommand { get; }
    public ReactiveCommand<Unit, Unit> ParseCommand { get; }
    public ReactiveCommand<Unit, Unit> LoadParseCommand { get; }
    public ReactiveCommand<Unit, Unit> ReleaseGrammarCommand { get; }
    public LanguageInformation? LangInfo { get; private set; }

    public ObservableCollection<JsonParseTreeNode> ParsingTreeResult { get; } = new ObservableCollection<JsonParseTreeNode>();
    public JsonParseTree? ParsingResult { get; private set; }

    public TextDocument TextToParse { get; private set; } = new TextDocument();


    private GrammarLoader? _grammarLoader;

    public async Task OnReleaseGrammarCommand()
    {

    }


    public async Task OnGrammarOpenCommand()
    {
      if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
      {
        var dlg = new OpenFileDialog();
        dlg.Filters?.Add(new FileDialogFilter() { Name = "Assembly file", Extensions = { "dll", "exe" } });
        dlg.AllowMultiple = false;
        var result = await dlg.ShowAsync(desktop.MainWindow);
        if (result != null && result.Any())
        {
          var location = result.FirstOrDefault();
          if (string.IsNullOrEmpty(location)) return;

          var window = new SelectGrammarWindow
          {
            DataContext = LoadGrammars(location)
          };
          var selectedItems = await window.ShowDialog<IEnumerable<GrammarItem>>(desktop.MainWindow);
          if (selectedItems != null && selectedItems.Any())
          {
            await LoadGrammar(selectedItems.First());
          }
        }
      }
    }

    public async Task OnParseCommand()
    {
      if (_grammarLoader is not null)
      {
        ParsingResult = await _grammarLoader.ParseText(this.TextToParse.Text);
        ParsingTreeResult.Clear();
        if (ParsingResult.Root != null)
        {
          ParsingTreeResult.Add(ParsingResult.Root);
        }
        this.RaisePropertyChanged(nameof(ParsingResult));
      }
    }

    public async Task OnLoadParseCommand()
    {
      if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
      {
        //onpen dialogbox
        var dlg = new OpenFileDialog();
        dlg.Filters?.Add(new FileDialogFilter() { Name = "All files", Extensions = { "*" } });
        dlg.AllowMultiple = false;
        var result = await dlg.ShowAsync(desktop.MainWindow);
        if (result != null && result.Any())
        {
          this.TextToParse.Text = System.IO.File.ReadAllText(result.First());
        }
      }
    }

    private void UnloadGrammar()
    {
      _grammarLoader?.Dispose();
    }

    private async Task LoadGrammar(GrammarItem item)
    {
      UnloadGrammar();
      var grammarItemToLoad = item;
      _grammarLoader = new GrammarLoader(grammarItemToLoad.Location, grammarItemToLoad.TypeName);

      LangInfo = await _grammarLoader.GetLanguageInformation();
      BottomSelectedIndex = (LangInfo.Errors != null && LangInfo.Errors.Length > 0) ? 1 : 0;

      this.RaisePropertyChanged(string.Empty);
    }

    private static GrammarItemList LoadGrammars(string assemblyPath)
    {
      using var gLoader = new GrammarLoader(assemblyPath, string.Empty);
      return gLoader.Grammars;
    }

    public void Dispose()
    {
      _grammarLoader?.Dispose();
    }
  }
}
