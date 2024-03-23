using System.Windows.Media.Imaging;

namespace Studio.Logic {
    public class Artist {
        public BitmapImage Image { get; set; }
        public string Name { get; set; }
        public Artist(BitmapImage image, string name) {
            Image = image;
            Name = name;
        }
    }
}
