
using Irony.GrammarExplorerExecutor;
using Irony.Parsing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;

var assemblyToLoad = string.Empty;
var grammarTypeName = string.Empty;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();
app.MapPost("/parse", ParseText);
app.MapGet("/langinfo", ExtractLangInformation);


if (args.Length == 3 && int.TryParse(args[0], out var port))
{
  app.Urls.Add($"http://localhost:{port}");
  assemblyToLoad = args[1];
  grammarTypeName = args[2];
}
else
{
  Console.WriteLine("arguments must be [port] [assembly to load] [grammar fullname type]");
  Console.ReadLine();
  return;
}
app.Run();


async Task ParseText(HttpContext context)
{
  if (context.Request.ContentLength > 0)
  {
    var rawRequestBody = await new StreamReader(context.Request.Body).ReadToEndAsync();
    var grammar = CreateGrammar();
    var language = new LanguageData(grammar);
    var parser = new Parser(language);
    var result = parser.Parse(rawRequestBody);
    var options = new JsonSerializerOptions
    {
      PropertyNameCaseInsensitive = true,
    };
    var json = System.Text.Json.JsonSerializer.Serialize(result.ToJson(), options);
    await context.Response.WriteAsync(json);
  }
  else
  {
    await context.Response.WriteAsJsonAsync<string>($"{assemblyToLoad}, {grammarTypeName}");
    context.Response.StatusCode = 400;
  }
}

//
LanguageInformation ExtractLangInformation()
{
  try
  {
    var grammar = CreateGrammar();
    var language = new LanguageData(grammar);
    var parser = new Parser(language);

    var langAttr = LanguageAttribute.GetValue(grammar.GetType());

    var errors = parser.Language.Errors;

    return new LanguageInformation
    {
      CountStates = language.ParserData.States.Count.ToString(),
      ConstructionTime = language.ConstructionTime.ToString(),

      TerminalsText = ParserDataPrinter.PrintTerminals(language),
      NonTerminalsText = ParserDataPrinter.PrintNonTerminals(language),
      ParserStateText = ParserDataPrinter.PrintStateList(language),
      LanguageName = langAttr.LanguageName,
      Version = langAttr.Version,
      Description = langAttr.Description,
      Errors = errors.ToJson(),

    };
  }
  catch (Exception ex)
  {
    return new LanguageInformation { ConstructionTime = "-1", TerminalsText = $"unable to load assembly: {ex.ToString()}" };
  }
}

Grammar CreateGrammar()
{
  var ctx = new GrammarLoaderContext(assemblyToLoad);
  var asm = ctx.LoadFromAssemblyPath(assemblyToLoad);


  var type = asm.GetType(grammarTypeName, true, true);
  var instance = Activator.CreateInstance(type);
  var grammar = instance as Grammar;
  return grammar;
}