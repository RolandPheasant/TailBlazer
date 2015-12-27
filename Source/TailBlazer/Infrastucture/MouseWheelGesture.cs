using System.Windows.Input;

namespace TailBlazer.Infrastucture
{
    public class MouseWheelGesture : MouseGesture
    {
        public WheelDirection Direction { get; set; }


        public static MouseWheelGesture ControlDown => new MouseWheelGesture(ModifierKeys.Control) { Direction = WheelDirection.Down };
        public static MouseWheelGesture ControlUp => new MouseWheelGesture(ModifierKeys.Control) { Direction = WheelDirection.Up };

        public MouseWheelGesture() : base(MouseAction.WheelClick)
        {
        }

        public MouseWheelGesture(ModifierKeys modifiers) : base(MouseAction.WheelClick, modifiers)
        {
        }



        public override bool Matches(object targetElement, InputEventArgs inputEventArgs)
        {
            if (!base.Matches(targetElement, inputEventArgs)) return false;
            if (!(inputEventArgs is MouseWheelEventArgs)) return false;
            var args = (MouseWheelEventArgs)inputEventArgs;
            switch (Direction)
            {
                case WheelDirection.None:
                    return args.Delta == 0;
                case WheelDirection.Up:
                    return args.Delta > 0;
                case WheelDirection.Down:
                    return args.Delta < 0;
                default:
                    return false;
            }
        }



        public enum WheelDirection
        {
            None,
            Up,
            Down,
        }

    }
}