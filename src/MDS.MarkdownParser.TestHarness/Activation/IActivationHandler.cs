using System.Threading.Tasks;

namespace MDS.MarkdownParser.TestHarness.Activation;

public interface IActivationHandler
{
    bool CanHandle(object args);

    Task HandleAsync(object args);
}
