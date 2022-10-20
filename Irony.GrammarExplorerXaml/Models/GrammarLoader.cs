using Avalonia.Media;
using Irony.GrammarExplorerExecutor;
using Irony.Parsing;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Irony.GrammarExplorerXaml.Models
{
  /// <summary>
  /// responsible to lauch webserver
  /// </summary>
  internal class GrammarLoader : IDisposable
  {
    private bool disposedValue;
    GrammarLoaderContext? _context;
    Process? _process;
    readonly string _grammarTypeName;
    //readonly IHttpClientFactory _httpClientFactory;

    private static readonly HttpClient _httpClient;

    static GrammarLoader()
    {
      var socketsHandler = new SocketsHttpHandler
      {
        PooledConnectionLifetime = TimeSpan.FromMinutes(2)
      };

      _httpClient = new HttpClient(socketsHandler);
    }

    GrammarItemList? _Grammars = null;
    public GrammarItemList Grammars
    {
      get
      {
        if (_Grammars == null)
        {
          LoadGrammars();
        }
        return _Grammars ?? new GrammarItemList();
      }
    }

    public GrammarLoader(string assemblyPath, string typeName)
    {
      _context = new GrammarLoaderContext(assemblyPath, true);
      _grammarTypeName = typeName;
    }

    protected virtual void Dispose(bool disposing)
    {
      if (!disposedValue)
      {
        if (_process is not null && !_process.HasExited)
        {
          //_process.StandardInput.WriteLine("\x3");
          _process.Kill();
          _process.Dispose();
        }
        if (disposing)
        {
          if (_context != null)
          {
            WeakReference testAlcWeakRef = new(_context);
            _context.Unload();
            _context = null;
            for (int i = 0; testAlcWeakRef.IsAlive && (i < 10); i++)
            {
              GC.Collect();
              GC.WaitForPendingFinalizers();
            }
          }

          // TODO: free unmanaged resources (unmanaged objects) and override finalizer
          // TODO: set large fields to null
          disposedValue = true;
        }
      }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~GrammarLoader()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
      // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
      Dispose(disposing: true);
      GC.SuppressFinalize(this);
    }

    private void LoadGrammars()
    {
      if (_context is null) return;
      //ne need to launch webserver for listing grammars inside an assembly
      Assembly asm;
      Type[] types;
      try
      {
        asm = _context.LoadFromAssemblyPath(_context.AssemblyPath);
        types = asm.GetTypes();
      }
      catch (Exception ex)
      {
        Trace.TraceError($"Unable to load '{_context.AssemblyPath}', {ex}");
        return;
      }

      var grammars = new GrammarItemList();

      foreach (Type t in types)
      {
        if (t.IsAbstract) continue;

        if (!t.IsSubclassOf(typeof(Grammar))) continue;
        grammars.Add(new GrammarItem(t, _context.AssemblyPath));
      }
      if (grammars.Count == 0)
      {
        _Grammars = new GrammarItemList();
      }
      _Grammars = grammars;
    }

    private async Task CheckWebServerIsStarted(string assemblyPath, string typeName)
    {
      if (_process is null || _process.HasExited)
      {
        _process?.Dispose();
        var path = typeof(LanguageInformation).Assembly.Location;

        if (File.Exists(path))
        {
          var psi = new ProcessStartInfo
          {
            WorkingDirectory = Path.GetDirectoryName(path),
            FileName = "dotnet",
            UseShellExecute = false
          };
          psi.ArgumentList.Add(path);
          psi.WindowStyle = ProcessWindowStyle.Minimized;
          //TODO: find a port that is not used
          psi.ArgumentList.Add("1234");
          psi.ArgumentList.Add(assemblyPath);
          psi.ArgumentList.Add(typeName);
          _process = Process.Start(psi);
          await Task.Delay(1000);
        }
      }
    }

    internal async Task<LanguageInformation> GetLanguageInformation()
    {
      //because create instane of grammar lock dll inside this executable
      //webserver should be start on each need

      //var selectedGrammarAssembly = _context.LoadFromAssemblyPath(_context.AssemblyPath);
      //var type = selectedGrammarAssembly.GetType(typeName, true, true);
      //if (type == null) return null;

      //return Activator.CreateInstance(type) as Grammar;
      
      if (_context is null) return new LanguageInformation { TerminalsText = "GrammarLoaderContext is null" };
      await CheckWebServerIsStarted(_context.AssemblyPath, _grammarTypeName);

      try
      {
        var result = await _httpClient.GetFromJsonAsync<LanguageInformation>("http://localhost:1234/langinfo") ?? new LanguageInformation { TerminalsText = "unable to get language information" };
        return result;
      }
      catch (Exception ex)
      {
        return new LanguageInformation { TerminalsText = ex.ToString() };
      }
    }

    internal async Task<JsonParseTree> ParseText(string text)
    {
      if (_context is null) return new JsonParseTree { Status = ParseTreeStatus.Error, ErrorMessage = "GrammarLoaderContext is not set to an instance" } ;
      await CheckWebServerIsStarted(_context.AssemblyPath, _grammarTypeName);
      try
      {
        using var responseMessage = await _httpClient.PostAsync("http://localhost:1234/parse", new StringContent(text, Encoding.UTF8));
        if (responseMessage.IsSuccessStatusCode)
        {
          var parsedJson = await responseMessage.Content.ReadAsStringAsync();
          //var result = await responseMessage.Content.ReadFromJsonAsync<JsonParseTree>();
          var options = new JsonSerializerOptions
          {
            PropertyNameCaseInsensitive= true,
          };
          var result = System.Text.Json.JsonSerializer.Deserialize<JsonParseTree>(parsedJson, options);
          return result;
        }

        return new JsonParseTree { Status = ParseTreeStatus.Error, ErrorMessage ="unable to parse from webserver" };        
      }
      catch (Exception ex)
      {
        return new JsonParseTree { Status = ParseTreeStatus.Error, ErrorMessage = ex.ToString() };
      }
    }
  }
}
