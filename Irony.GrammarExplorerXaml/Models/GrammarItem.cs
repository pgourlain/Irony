using Irony.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Irony.GrammarExplorerXaml.Models
{
  public class GrammarItem
  {
    public readonly string Caption;
    public readonly string LongCaption;
    public readonly string Location; //location of assembly containing the grammar
    public readonly string TypeName; //full type name
    internal bool _loading;
    public GrammarItem(string caption, string location, string typeName)
    {
      Caption = caption;
      Location = location;
      TypeName = typeName;
      LongCaption = string.Empty;
    }
    public GrammarItem(Type grammarClass, string assemblyLocation)
    {
      _loading = true;
      Location = assemblyLocation;
      TypeName = grammarClass.FullName ?? string.Empty;
      //Get language name from Language attribute
      Caption = grammarClass.Name; //default caption
      LongCaption = Caption;
      var langAttr = LanguageAttribute.GetValue(grammarClass);
      if (langAttr != null)
      {
        Caption = langAttr.LanguageName;
        if (!string.IsNullOrEmpty(langAttr.Version))
          Caption += ", version " + langAttr.Version;
        LongCaption = Caption;
        if (!string.IsNullOrEmpty(langAttr.Description))
          LongCaption += ": " + langAttr.Description;
      }
    }
    public GrammarItem(XmlElement element)
    {
      Caption = element.GetAttribute("Caption");
      Location = element.GetAttribute("Location");
      TypeName = element.GetAttribute("TypeName");
      LongCaption = string.Empty;
    }
    public void Save(XmlElement toElement)
    {
      toElement.SetAttribute("Caption", Caption);
      toElement.SetAttribute("Location", Location);
      toElement.SetAttribute("TypeName", TypeName);
    }
    public override string ToString()
    {
      return _loading ? LongCaption : Caption;
    }    

  }//class
}
