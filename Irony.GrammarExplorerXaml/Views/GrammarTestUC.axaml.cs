using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using System;
using System.Collections.Generic;
using System.Resources;

namespace Irony.GrammarExplorerXaml.Views
{
  public partial class GrammarTestUC : UserControl
  {
    private readonly TextEditor _textEditor;
    public GrammarTestUC()
    {
      InitializeComponent();
      _textEditor = this.FindControl<TextEditor>("Editor");
      _textEditor.HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Visible;
      //_textEditor.Background = Brushes.Transparent;
      //_textEditor.ShowLineNumbers = true;
      //_textEditor.ContextMenu = new ContextMenu
      //{
      //  Items = new List<MenuItem>
      //          {
      //              new MenuItem { Header = "Copy", InputGesture = new KeyGesture(Key.C, KeyModifiers.Control) },
      //              new MenuItem { Header = "Paste", InputGesture = new KeyGesture(Key.V, KeyModifiers.Control) },
      //              new MenuItem { Header = "Cut", InputGesture = new KeyGesture(Key.X, KeyModifiers.Control) }
      //          }
      //};
      //_textEditor.TextArea.Background = this.Background;
      _textEditor.Document = new TextDocument("coucou");
    }
  }
}
