using System;

namespace TailBlazer.Domain.FileHandling
{
    [Flags]
    public enum FileNotificationType
    {
        None ,
        CreatedOrOpened,
        Changed,
        Missing,
        Error
    }
}