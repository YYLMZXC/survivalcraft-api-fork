namespace Game
{
    public class CommunityContentEntry
    {
        public ExternalContentType Type;

        public string Name;

        public string Address;

        public string UserId;

        public long Size;

        public string ExtraText;

        public float RatingsAverage;

        public string IconSrc;

        public Engine.Graphics.Texture2D Icon;

        public RectangleWidget IconInstance = null;
    }
}
