
using System;
using System.Windows;

namespace TailBlazer.Views.Layout;

public class ShellSettings :  IEquatable<ShellSettings>
{
    public double Top { get; }
    public double Left { get; }

    public double Width { get; }

    public double Height { get; }

    public WindowState State { get; }

    public ShellSettings(double top, double left, double width, double height, WindowState state)
    {
        Top = top;
        Left = left;
        Width = width;
        Height = height;
        State = state;
    }

    #region Equality

    public bool Equals(ShellSettings other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Top.Equals(other.Top) && Left.Equals(other.Left) && Width.Equals(other.Width) && Height.Equals(other.Height) && State == other.State;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((ShellSettings) obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = Top.GetHashCode();
            hashCode = (hashCode*397) ^ Left.GetHashCode();
            hashCode = (hashCode*397) ^ Width.GetHashCode();
            hashCode = (hashCode*397) ^ Height.GetHashCode();
            hashCode = (hashCode*397) ^ (int) State;
            return hashCode;
        }
    }

    public static bool operator ==(ShellSettings left, ShellSettings right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(ShellSettings left, ShellSettings right)
    {
        return !Equals(left, right);
    }

    #endregion

    public override string ToString()
    {
        return $"Location: ({Top},{Left}).  Size({Width},{Height}). State: {State}";
    }
}