﻿using System;
using System.Threading.Tasks;

using MDS.MarkdownParser.TestHarness.Contracts.Services;
using MDS.MarkdownParser.TestHarness.ViewModels;

using Microsoft.UI.Xaml;

namespace MDS.MarkdownParser.TestHarness.Activation;

public class DefaultActivationHandler : ActivationHandler<LaunchActivatedEventArgs>
{
    private readonly INavigationService _navigationService;

    public DefaultActivationHandler(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }

    protected override bool CanHandleInternal(LaunchActivatedEventArgs args)
        // None of the ActivationHandlers has handled the activation.
        => _navigationService.Frame.Content == null;

    protected async override Task HandleInternalAsync(LaunchActivatedEventArgs args)
    {
        _navigationService.NavigateTo(typeof(SyntaxTreeViewModel).FullName, args.Arguments);

        await Task.CompletedTask;
    }
}
