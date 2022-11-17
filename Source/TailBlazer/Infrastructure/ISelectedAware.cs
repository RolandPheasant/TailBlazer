using System;
namespace TailBlazer.Infrastructure;

public interface ISelectedAware
{
    bool IsSelected { get; set; }
}