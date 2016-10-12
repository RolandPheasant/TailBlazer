using System;

namespace TailBlazer.Domain.FileHandling
{
    [Flags]
    public enum FileNotificationReason
    {
        None ,
        CreatedOrOpened,
        Changed,
        Missing,
        Error
    }
}