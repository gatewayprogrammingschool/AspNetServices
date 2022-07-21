using System;

namespace MDS.MarkdownParser.TestHarness.Contracts.Services;

public interface IPageService
{
    Type GetPageType(string key);
}
