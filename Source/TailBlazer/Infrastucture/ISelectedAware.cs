using System;
namespace TailBlazer.Infrastucture
{
    public interface ISelectedAware
    {
        bool IsSelected { get; set; }
    }
}
