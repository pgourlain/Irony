using System;
using System.Collections.Generic;
using System.Text;

namespace Irony.Parsing
{
  public class JsonParseTree
  {
    public long ParseTimeMilliseconds { get; set; }
    public ParseTreeStatus Status { get; set; }
    public string ErrorMessage { get; set; }
    public JsonParseTreeNode Root { get; set; }
    public JsonLogMessage[] LogMessages { get; internal set; }
    public JsonToken[] Tokens { get; set; }
  }
  public class JsonParseTreeNode
  {
    public string Term { get; set; }
    public string Terminal { get; set; }
    public string AstNodeType { get; set; }
    public string Value { get; set; }
    public JsonParseTreeNode[] ChildNodes { get; set; }
    /// <summary>
    /// contains value to display in IHM
    /// </summary>
    public string DisplayValue { get; set; }
  }

  public class JsonToken
  {
    public string Terminal { get; set; }
    public string KeyTerm { get; set; }
    public string Value { get; set; }
    /// <summary>
    /// contains value to display in IHM
    /// </summary>
    public string DisplayValue { get; set; }
  }
}
