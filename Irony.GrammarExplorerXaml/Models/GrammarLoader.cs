using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Irony.GrammarExplorerXaml.Models
{
  internal class GrammarLoader
  {
    private static readonly HashSet<string> _probingPaths = new ();
    private static readonly HashSet<Assembly> _loadedAssemblies = new ();
    private static readonly Dictionary<string, Assembly> _loadedAssembliesByNames = new ();

    static GrammarLoader()
    {
      AppDomain.CurrentDomain.AssemblyLoad += (sender, args) => _loadedAssembliesByNames[args.LoadedAssembly.FullName] = args.LoadedAssembly;
      AppDomain.CurrentDomain.AssemblyResolve += (sender, args) => FindAssembly(args.Name);
    }

    static Assembly FindAssembly(string assemblyName)
    {
      if (_loadedAssembliesByNames.ContainsKey(assemblyName))
        return _loadedAssembliesByNames[assemblyName];
      // ignore resource assemblies
      if (assemblyName.ToLower().Contains(".resources, version="))
        return _loadedAssembliesByNames[assemblyName] = null;
      // use probing paths to look for dependency assemblies
      var fileName = assemblyName.Split(',').First() + ".dll";
      foreach (var path in _probingPaths)
      {
        var fullName = Path.Combine(path, fileName);
        if (File.Exists(fullName))
        {
          try
          {
            return LoadAssembly(fullName);
          }
          catch
          {
            // the file seems to be bad, let's try to find another one
          }
        }
      }
      
      // assembly not found, don't search for it again
      return _loadedAssembliesByNames[assemblyName] = null;
    }

    public static Assembly LoadAssembly(string fileName)
    {
      // normalize the filename
      fileName = new FileInfo(fileName).FullName;
      // save assembly path for dependent assemblies probing
      var path = Path.GetDirectoryName(fileName);
      _probingPaths.Add(path);
      // try to load assembly using the standard policy
      var assembly = Assembly.LoadFrom(fileName);
      // if the standard policy returned the old version, force reload
      if (_loadedAssemblies.Contains(assembly))
      {
        assembly = Assembly.Load(File.ReadAllBytes(fileName));
      }
      // cache the loaded assembly by its location
      _loadedAssemblies.Add(assembly);
      return assembly;
    }
  }
}
