using System;
using System.Collections.Generic;
using System.Text;

namespace Irony.Parsing
{
  public class JsonGrammarError
  {
    public GrammarErrorLevel Level { get; set; }
    public string Message { get; set; }
    public string ParserState { get; set; }
  }
}
