using System.Reflection;
using System.Runtime.Loader;

namespace Irony.GrammarExplorerExecutor
{
  public class GrammarLoaderContext : AssemblyLoadContext
  {
    string _assemblyPath;
    AssemblyDependencyResolver _resolver;
    public string AssemblyPath => _assemblyPath;
    public GrammarLoaderContext(string assemblyPath, bool isCollectible) : base(isCollectible)
    {
      _assemblyPath = assemblyPath;
      _resolver = new AssemblyDependencyResolver(assemblyPath);
    }
    public GrammarLoaderContext(string assemblyPath) : this(assemblyPath, false)
    {

    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
      string? assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
      if (assemblyPath != null && assemblyName.Name != "Irony")
      {        
        //Irony should not be loaded in this Context, because of type comparison failed (Grammar,...)
        //so it's the Irony hosted in default context that should be used
        return LoadFromAssemblyPath(assemblyPath);
      }
      return null;
    }
  }
}
