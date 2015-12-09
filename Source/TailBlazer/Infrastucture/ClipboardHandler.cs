using System;
using System.Collections.Generic;
using System.Windows;
using TailBlazer.Domain.Annotations;
using TailBlazer.Domain.Infrastructure;

namespace TailBlazer.Infrastucture
{
    public class ClipboardHandler : IClipboardHandler
    {
        private readonly ILogger _logger;

        public ClipboardHandler(ILogger logger)
        {
            _logger = logger;
        }

        public void WriteToClipboard(string text)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            try
            {
                _logger.Info($"Attempting to copy {Environment.NewLine}{text}{Environment.NewLine} to the clipboard");
                Clipboard.SetText(text, TextDataFormat.UnicodeText);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Problem copying items to the clipboard");
            }
        }

        public void WriteToClipboard([NotNull] IEnumerable<string> items)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            try
            {
                var array = items.AsArray();
                _logger.Info($"Attempting to copy {array.Length} lines to the clipboard");
                Clipboard.SetText(array.ToDelimited(Environment.NewLine), TextDataFormat.UnicodeText);
            }
            catch (Exception ex)
            {
                _logger.Error(ex,"Problem copying items to the clipboard");
            }


        }
    }
}