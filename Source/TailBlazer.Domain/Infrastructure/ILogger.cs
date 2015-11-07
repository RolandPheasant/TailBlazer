using System;

namespace Trader.Domain.Infrastucture
{
    public interface ILogger
    {
        void Debug(string message, params object[] values);
        void Info(string message, params object[] values);
        void Warn(string message, params object[] values);
        void Error(Exception ex, string message, params object[] values);
        void Fatal(string message, params object[] values);
    }
}