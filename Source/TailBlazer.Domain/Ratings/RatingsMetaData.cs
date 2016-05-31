namespace TailBlazer.Domain.Settings
{
    public class RatingsMetaData
    {

        public static readonly RatingsMetaData Default = new RatingsMetaData(60,250);

        public int FrameRate { get; }
        public int RefreshRate { get; }

        public RatingsMetaData(int frameRate, int refreshRate)
        {
            FrameRate = frameRate;
            RefreshRate = refreshRate;
        }
    }
}