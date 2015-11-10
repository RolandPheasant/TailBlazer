using System;

namespace TailBlazer.Domain.FileHandling
{
    [Flags]
    public enum FileNotificationType
    {
        None = 0x0,
        Created = 0x1,
        Changed = 0x2,
        Missing = 0x3,
        Error = 0x4
    }
}