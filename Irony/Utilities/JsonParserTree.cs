using System;
using System.Collections.Generic;
using System.Text;

namespace Irony.Parsing
{
  /// <summary>
  /// json model for a tree
  /// </summary>
  public class JsonParseTree
  {
    public ParseTreeStatus Status { get; set; }
    public JsonParseTreeNode Root { get; set; }

  }

  /// <summary>
  /// json model for a TreeNode
  /// </summary>
  public class JsonParseTreeNode
  {
    public string Term { get; set; }
    public string Terminal { get; set; }
    public string AstNodeType { get; set; }
    public string Value { get; set; }
    public JsonParseTreeNode[] ChildNodes { get; set; }
  }
}
