using Irony.Parsing;
using System;
using System.Collections.Generic;
using System.Text;

namespace Irony
{
  public class JsonLogMessage
  {
    public ErrorLevel Level { get; set; }
    public string ParserState { get; set; }
    public JsonSourceLocation Location { get; set; }
    public string Message { get; set; }
  }

  public class JsonSourceLocation
  {
    public JsonSourceLocation(SourceLocation source)
    {
      this.Position = source.Position;
      this.Column = source.Column;
      this.Line = source.Line;
    }
    public JsonSourceLocation()
    {

    }
    public int Position { get; set; }
    /// <summary>Source line number, 0-based.</summary>
    public int Line { get; set; }
    /// <summary>Source column number, 0-based.</summary>
    public int Column { get; set; }
  }
}
