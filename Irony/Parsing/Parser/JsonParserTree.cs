using System;
using System.Collections.Generic;
using System.Text;

namespace Irony.Parsing
{
  public class JsonParseTree
  {
    public ParseTreeStatus Status { get; set; }
    public JsonParseTreeNode Root { get; set; }

  }
  public class JsonParseTreeNode
  {
    public string Term { get; set; }
    public string Terminal { get; set; }
    public string AstNodeType { get; set; }
    public string Value { get; set; }
    public JsonParseTreeNode[] ChildNodes { get; set; }
  }
}
