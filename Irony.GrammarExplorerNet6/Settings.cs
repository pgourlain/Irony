using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Irony.GrammarExplorer.Properties
{
  internal class Settings
  {
    static Lazy<GrammarSettings> _Default = new Lazy<GrammarSettings>(() => new GrammarSettings());

    public static GrammarSettings Default
    {
      get
      {
        return _Default.Value; 
      }
    }
  }

  internal class GrammarSettings
  {
    public string SourceSample { get; set; }
    public string SearchPattern { get; set; }
    public bool EnableTrace { get; set; } = true;
    public bool DisableHili { get; set; }
    public bool AutoRefresh { get; set; }
    public int LanguageIndex { get; set; }
    public string Grammars { get; set; }

    public void Save()
    {

    }
  }
}
