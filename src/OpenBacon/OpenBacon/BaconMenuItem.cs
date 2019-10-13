namespace OpenBacon
{
    public class BaconMenuItem
    {
        public string Name { get; set; }
        public string Detail { get; set; }
        public string ImageSrc { get; set; }

        public BaconMenuItem(string name, string detail, string iconResource)
        {
            Name = name;
            Detail = detail;
            ImageSrc = iconResource;
        }

        public BaconMenuItem() { }
    }
}
