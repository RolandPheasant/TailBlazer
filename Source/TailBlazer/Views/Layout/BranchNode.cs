namespace TailBlazer.Views.Layout
{
    public class BranchNode
    {
        public string Orientation { get; }
        public double Ratio { get; }

        public BranchNode(double ratio)
        {
            Ratio = ratio;
        }

        public BranchNode(string orientation)
        {
            Orientation = orientation;
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(Orientation))
                return $"Branch {Orientation}";

            return $"Proportion {Ratio}";
        }
    }
}