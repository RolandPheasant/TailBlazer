using System;
using System.Collections;
using System.IO;
using System.Linq;
using TailBlazer.Domain.Settings;

namespace TailBlazer.Domain.FileHandling
{
    public class RecentFilesToStateConverter: IConverter<FileInfo[]>
    {
        public FileInfo[] Convert(State state)
        {
            if (state == null || state == State.Empty)
                return new FileInfo[0];

            return state.Value.FromDelimited(s => new FileInfo(s),Environment.NewLine).ToArray();
        }

        public State Convert(FileInfo[] state)
        {
            if (state == null || !state.Any())
                return State.Empty;

            return new State(1, state.Select(fi=>fi.FullName).ToDelimited(Environment.NewLine));
        }

        public FileInfo[] GetDefaultValue()
        {
            return new FileInfo[0];
        }
    }
}