using Irony.Parsing;

namespace Irony.GrammarExplorerExecutor
{
  public class LanguageInformation
  {
    public string LanguageName { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public JsonGrammarError[] Errors { get; set; } = Array.Empty<JsonGrammarError>();
    public string TerminalsText { get; set; } = string.Empty;
    public string NonTerminalsText { get; set; } = string.Empty;

    public string ParserStateText { get; set; } = string.Empty;

    public string CountStates { get; set; } = string.Empty;
    public string ConstructionTime { get; set; } = string.Empty;
  }
}
