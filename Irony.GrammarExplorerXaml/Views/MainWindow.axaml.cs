using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.Threading.Tasks;

namespace Irony.GrammarExplorerXaml.Views
{
  public partial class MainWindow : Window
  {
    public MainWindow()
    {
      InitializeComponent();
      this.Closing += MainWindow_Closing;
    }

    private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
      if (this.DataContext is IDisposable toDispose)
      {
        //cloe the web server if it's running.
        toDispose.Dispose();
      }
    }
  }
}
