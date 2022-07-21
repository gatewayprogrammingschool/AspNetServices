using System.Threading.Tasks;

namespace MDS.MarkdownParser.TestHarness.Contracts.Services;

public interface IActivationService
{
    Task ActivateAsync(object activationArgs);
}
