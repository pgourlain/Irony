using Avalonia.Controls;
using Avalonia.Interactivity;
using Irony.GrammarExplorerXaml.Models;
using Irony.Parsing;
using System;
using System.Linq;

namespace Irony.GrammarExplorerXaml.Views
{
  public partial class SelectGrammarWindow : Window
  {
    public SelectGrammarWindow()
    {
      InitializeComponent();
    }

    public void OkButton_Click(object sender, RoutedEventArgs args)
    {
      var result = (GrammarItem?)lbGrammars.SelectedItem;
      //Close("OK Clicked!");
      if (result is null)
      {
        Close(Enumerable.Empty<GrammarItem>());
      }
      else
      {
        Close(new GrammarItem[] {result });
      }
    }
  }
}
