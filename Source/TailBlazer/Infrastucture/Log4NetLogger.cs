using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using TailBlazer.Domain.Infrastructure;

namespace TailBlazer.Infrastucture
{
    public class Log4NetLogger : ILogger
    {
        private readonly ILog _log;

        public string Name => _log.Logger.Name;

        public Log4NetLogger(Type type)
        {
            var name = type.Name;
            var genericArgs = type.GenericTypeArguments;

            if (!genericArgs.Any())
            {
                _log = LogManager.GetLogger(name);
            }
            else
            {

                var startOfGeneric = name.IndexOf("`");
                name = name.Substring(0,startOfGeneric);
                var generics = genericArgs.Select(t=>t.Name).ToDelimited();
                _log = LogManager.GetLogger($"{name}<{generics}>");
            }
        }

        public Log4NetLogger(string name)
        {
            _log = LogManager.GetLogger(name);
        }

        public void Debug(string message, params object[] values)
        {
            if (!_log.IsDebugEnabled) return;
            _log.DebugFormat(message, values);
        }

        public void Info(string message, params object[] values)
        {
            if (!_log.IsInfoEnabled) return;
            _log.InfoFormat(message, values);
        }

        public void Warn(string message, params object[] values)
        {
            if (!_log.IsWarnEnabled) return;
            _log.WarnFormat(message, values);
        }

        public void Error(Exception ex, string message, params object[] values)
        {
            if (!_log.IsErrorEnabled) return;
            _log.Error(string.Format(message, values), ex);
        }

        public void Fatal(string message, params object[] values)
        {
            if (!_log.IsFatalEnabled) return;
            _log.FatalFormat(message, values);
        }
    }
}