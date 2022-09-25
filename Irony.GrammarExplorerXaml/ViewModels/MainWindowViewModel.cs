using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using HarfBuzzSharp;
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
  public class MainWindowViewModel : ViewModelBase
  {
    public MainWindowViewModel()
    {
      OpenCommand = ReactiveCommand.CreateFromTask(OnOpenCommand);
    }
    public string Greeting => LangInfo != null ? LangInfo.Description : "No Grammar loaded";

    public string TerminalsText { get; private set; }
    public string NonTerminalsText {get; private set; }

    public string ParserStateText {get; private set; }

    public string CountStates { get; private set; }
    public string ConstructionTime { get; private set; }

    public ReactiveCommand<Unit, Unit> OpenCommand { get; }

    public Grammar? CurrentGrammar { get; private set; }
    public LanguageAttribute? LangInfo { get; private set; }
    LanguageData _language;
    Parser _parser;
    ParseTree _parseTree;

    public async Task OnOpenCommand()
    {
      if (Avalonia.Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
      {
        var dlg = new OpenFileDialog();
        dlg.Filters.Add(new FileDialogFilter() { Name = "Assembly file", Extensions = { "dll", "exe" } });
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
            LoadGrammar(selectedItems.First());
          }
        }
      }
    }

    private void LoadGrammar(GrammarItem item)
    {
      var grammarItemToLoad = item;
      var selectedGrammarAssembly = GrammarLoader.LoadAssembly(grammarItemToLoad.Location);
      var type = selectedGrammarAssembly.GetType(grammarItemToLoad.TypeName, true, true);
      CurrentGrammar = Activator.CreateInstance(type) as Grammar;
      CreateParser();
      ShowLanguageInfo();
      //refresh all
      this.RaisePropertyChanged(string.Empty);
    }

    private void CreateParser()
    {
      _language = new LanguageData(CurrentGrammar);
      _parser = new Parser(_language);

      CountStates= _language.ParserData.States.Count.ToString();
      ConstructionTime = _language.ConstructionTime.ToString();

      TerminalsText = ParserDataPrinter.PrintTerminals(_language);
      NonTerminalsText = ParserDataPrinter.PrintNonTerminals(_language);
      ParserStateText = ParserDataPrinter.PrintStateList(_language);
    }

    private void ShowLanguageInfo()
    {
      if (CurrentGrammar == null) return;
      var langAttr = LanguageAttribute.GetValue(CurrentGrammar.GetType());
      LangInfo = langAttr;
    }

    private static GrammarItemList LoadGrammars(string assemblyPath)
    {
      Assembly asm = null;
      try
      {
        asm = GrammarLoader.LoadAssembly(assemblyPath);
      }
      catch (Exception ex)
      {
        //MessageBox.Show("Failed to load assembly: " + ex.Message);
        return null;
      }
      var types = asm.GetTypes();
      var grammars = new GrammarItemList();
      foreach (Type t in types)
      {
        if (t.IsAbstract) continue;
        if (!t.IsSubclassOf(typeof(Grammar))) continue;
        grammars.Add(new GrammarItem(t, assemblyPath));
      }
      if (grammars.Count == 0)
      {
        //MessageBox.Show("No classes derived from Irony.Grammar were found in the assembly.");
        //return null;
      }
      return grammars;
    }
  }
}
