using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MsftILogger = Microsoft.Extensions.Logging.ILogger;

public class DebugLoggerProvider : ILoggerProvider
{
    public MsftILogger CreateLogger(string categoryName)
    {
        return new DebugLogger();
    }

    public void Dispose()
    {
        // empty
    }
}

public class DebugLogger : MsftILogger, IDisposable
{
    public void Dispose()
    {
        // TODO
    }

    IDisposable MsftILogger.BeginScope<TState>(TState state)
    {
        return this;
    }
    bool MsftILogger.IsEnabled(LogLevel logLevel)
    {
        return true;
    }
    void MsftILogger.Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        if (exception != null)
        {
            Debug.LogError(exception);
        }
    }
}
