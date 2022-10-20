using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Irony.GrammarExplorerXaml.ViewModels;
using Irony.GrammarExplorerXaml.Views;
using Microsoft.Extensions.Hosting.Internal;

namespace Irony.GrammarExplorerXaml
{
  public partial class App : Application
  {
    public override void Initialize()
    {
      AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
      if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
      {        
        desktop.MainWindow = new MainWindow
        {
          DataContext = new MainWindowViewModel(),
        };
      }

      base.OnFrameworkInitializationCompleted();
    }

    private void OnAbout_Click(object sender, EventArgs args)
    {

    }
  }
}
