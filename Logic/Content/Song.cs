using System.Windows.Media.Imaging;

namespace Studio.Logic {
    public class Song {
        public BitmapImage Image { get; set; }
        public string Name { get; set; }
        public string Album { get; set; }
        public Song(BitmapImage image, string name, string album) {
            Image = image;
            Name = name;
            Album = album;
        }
    }
}