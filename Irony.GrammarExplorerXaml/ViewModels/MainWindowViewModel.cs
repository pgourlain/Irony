using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using HarfBuzzSharp;
using Irony.GrammarExplorerExecutor;
using Irony.GrammarExplorerXaml.Models;
using Irony.GrammarExplorerXaml.Views;
using Irony.Parsing;
using Microsoft.CodeAnalysis;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Irony.GrammarExplorerXaml.ViewModels
{
  public class MainWindowViewModel : ViewModelBase, IDisposable
  {
    public MainWindowViewModel()
    {
      OpenCommand = ReactiveCommand.CreateFromTask(OnOpenCommand);
    }
    public string Greeting => LangInfo != null ? LangInfo.Description : "No Grammar loaded";

    public string TerminalsText => LangInfo?.TerminalsText ?? string.Empty;
    public string NonTerminalsText => LangInfo?.NonTerminalsText ?? string.Empty;

    public string ParserStateText => LangInfo?.ParserStateText ?? string.Empty;

    public string CountStates => LangInfo?.CountStates ?? string.Empty;
    public string ConstructionTime => LangInfo?.ConstructionTime ?? string.Empty;

    public int BottomSelectedIndex { get; set; }

    public ReactiveCommand<Unit, Unit> OpenCommand { get; }
    public LanguageInformation? LangInfo { get; private set; }

    private GrammarLoader? _grammarLoader;

    public async Task OnOpenCommand()
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

          var window = new SelectGrammarWindow();
          window.DataContext = LoadGrammars(location);
          var selectedItems = await window.ShowDialog<IEnumerable<GrammarItem>>(desktop.MainWindow);
          if (selectedItems != null && selectedItems.Any())
          {
            await LoadGrammar(selectedItems.First());
          }
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
      _grammarLoader = new GrammarLoader(grammarItemToLoad.Location);
      //var grammar = gl.LoadGrammar(grammarItemToLoad.TypeName);
      //CreateParser(grammar);
      //ShowLanguageInfo(grammar);
      //refresh all
      //grammar = null;

      LangInfo = await _grammarLoader.GetLanguageInformation(grammarItemToLoad.TypeName);
      BottomSelectedIndex = (LangInfo.Errors != null && LangInfo.Errors.Length > 0) ? 1 : 0;
      //CountStates = info.CountStates;
      //ConstructionTime = info.ConstructionTime;
      //TerminalsText = info.TerminalsText;
      //NonTerminalsText = info.NonTerminalsText;
      //ParserStateText = info.ParserStateText;


      this.RaisePropertyChanged(string.Empty);
    }

    //private void CreateParser(Grammar grammar)
    //{
    //  var language = new LanguageData(grammar);
    //  var parser = new Parser(language);

    //  CountStates = language.ParserData.States.Count.ToString();
    //  ConstructionTime = language.ConstructionTime.ToString();

    //  TerminalsText = ParserDataPrinter.PrintTerminals(language);
    //  NonTerminalsText = ParserDataPrinter.PrintNonTerminals(language);
    //  ParserStateText = ParserDataPrinter.PrintStateList(language);
    //}

    //private void ShowLanguageInfo(Grammar grammar)
    //{
    //  if (grammar == null) return;
    //  var langAttr = LanguageAttribute.GetValue(grammar.GetType());
    //  LangInfo = new LanguageAttribute(langAttr.LanguageName, langAttr.Version, langAttr.Description);
    //}

    private GrammarItemList LoadGrammars(string assemblyPath)
    {
      using var gLoader = new GrammarLoader(assemblyPath);
      return gLoader.Grammars;
    }

    public void Dispose()
    {
      _grammarLoader?.Dispose();
    }
  }
}
