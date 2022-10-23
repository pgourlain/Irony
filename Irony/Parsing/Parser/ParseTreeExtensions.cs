#region License
/* **********************************************************************************
 * Copyright (c) Roman Ivantsov
 * This source code is subject to terms and conditions of the MIT License
 * for Irony. A copy of the license can be found in the License.txt file
 * at the root of this distribution. 
 * By using this source code in any fashion, you are agreeing to be bound by the terms of the 
 * MIT License.
 * You must not remove this notice from this software.
 * **********************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;

namespace Irony.Parsing
{

  public static class ParseTreeExtensions
  {

    public static string ToXml(this ParseTree parseTree)
    {
      if (parseTree == null || parseTree.Root == null) return string.Empty;
      var xdoc = ToXmlDocument(parseTree);
      StringWriter sw = new StringWriter();
      XmlTextWriter xw = new XmlTextWriter(sw)
      {
        Formatting = Formatting.Indented
      };
      xdoc.WriteTo(xw);
      xw.Flush();
      return sw.ToString();
    }

    public static XmlDocument ToXmlDocument(this ParseTree parseTree)
    {
      var xdoc = new XmlDocument();
      if (parseTree == null || parseTree.Root == null) return xdoc;
      var xTree = xdoc.CreateElement("ParseTree");
      xdoc.AppendChild(xTree);
      var xRoot = parseTree.Root.ToXmlElement(xdoc);
      xTree.AppendChild(xRoot);
      return xdoc;
    }

    public static XmlElement ToXmlElement(this ParseTreeNode node, XmlDocument ownerDocument)
    {
      var xElem = ownerDocument.CreateElement("Node");
      xElem.SetAttribute("Term", node.Term.Name);
      var term = node.Term;
      if (term.HasAstConfig() && term.AstConfig.NodeType != null)
        xElem.SetAttribute("AstNodeType", term.AstConfig.NodeType.Name);
      if (node.Token != null)
      {
        xElem.SetAttribute("Terminal", node.Term.GetType().Name);
        //xElem.SetAttribute("Text", node.Token.Text);
        if (node.Token.Value != null)
          xElem.SetAttribute("Value", node.Token.Value.ToString());
      }
      else
        foreach (var child in node.ChildNodes)
        {
          var xChild = child.ToXmlElement(ownerDocument);
          xElem.AppendChild(xChild);
        }
      return xElem;
    }//method

    public static JsonParseTree ToJson(this ParseTree parseTree)
    {
      var result = new JsonParseTree();
      if (parseTree != null)
      {
        result.LogMessages = parseTree.ParserMessages.ToJson();
        result.ParseTimeMilliseconds = parseTree.ParseTimeMilliseconds;
        result.Tokens = parseTree.Tokens.ToJson();
      }
      if (parseTree == null || parseTree.Root == null) return result;
      result.Root = parseTree.Root.ToJson();
      return result;
    }

    public static JsonParseTreeNode ToJson(this ParseTreeNode node)
    {
      var result = new JsonParseTreeNode
      {
        Term = node.Term.Name,
        DisplayValue = node.ToString()
      };
      var term = node.Term;
      if (term.HasAstConfig() && term.AstConfig.NodeType != null)
        result.AstNodeType = term.AstConfig.NodeType.Name;
      if (node.Token != null)
      {
        result.Terminal = node.Term.GetType().Name;
        //xElem.SetAttribute("Text", node.Token.Text);
        if (node.Token.Value != null)
          result.Value = node.Token.Value.ToString();
      }
      else
      {
        result.ChildNodes = node.ChildNodes.Select(x => x.ToJson()).ToArray();
      }
      return result;
    }

    public static JsonLogMessage[] ToJson(this LogMessageList logMessages)
    {
      if (logMessages != null)
      {
        return logMessages.Select(x => x.ToJson()).ToArray();
      }
      return Array.Empty<JsonLogMessage>();
    }

    public static JsonLogMessage ToJson(this LogMessage logMessage)
    {
      return new JsonLogMessage()
      {
        Level= logMessage.Level,
        Location= new JsonSourceLocation(logMessage.Location),  
        Message= logMessage.Message,
        ParserState = logMessage.ParserState.ToString(),
      };
    }

    public static JsonToken[] ToJson(this TokenList tokens)
    {
      if (tokens != null)
      {
        return tokens.Select(x => x.ToJson()).ToArray();
      }
      return Array.Empty<JsonToken>();
    }

    public static JsonToken ToJson(this Token token)
    {
      return new JsonToken()
      {
        Terminal= token.Terminal?.ToString(),
        KeyTerm = token.KeyTerm?.ToString(),
        Value = token.Value?.ToString(),
        DisplayValue = token.ToString(),
      };
    }

  }//class
}//namespace
