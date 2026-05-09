using System.Reflection;  
using Xunit;  
  
namespace MDS.Tool.Tests;  
  
public class BasicTests  
{  
    [Fact]  
    public void ToolAssembly_ShouldLoadAndHaveEntryPoint()  
    {  
        var assembly = Assembly.LoadFrom("MDS.Tool.dll");  
        Assert.NotNull(assembly);  
  
        var entryPoint = assembly.EntryPoint;  
        Assert.NotNull(entryPoint);  
        Assert.Equal("<Main>$", entryPoint.Name);  
    }  
} 
