using System;
using System.Windows;
using System.Windows.Threading;
using TailBlazer.Domain.Infrastructure;

namespace TailBlazer.Infrastructure;

public class UhandledExceptionHandler
{
    private readonly ILogger _logger;

    public UhandledExceptionHandler(ILogger logger)
    {
        _logger = logger;

        Application.Current.DispatcherUnhandledException += CurrentDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomainUnhandledException;
    }

    private void CurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var ex = (Exception)e.ExceptionObject;
        _logger.Error(ex, ex.Message);
    }

    private void CurrentDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        var ex = e.Exception;
        _logger.Error(ex, ex.Message);
        e.Handled = true;
    }

}