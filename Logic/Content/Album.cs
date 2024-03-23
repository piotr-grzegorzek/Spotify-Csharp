using System.Windows;
using System.Windows.Media.Imaging;

namespace Studio.Logic {
    public class Album {
        public BitmapImage Image { get; set; }
        public string Name { get; set; }
        public string Artist { get; set; }
        public string Owner { get; set; }
        public int Price { get; set; }
        public int Selling { get; set; }
        public Visibility BuyVisibility { get; set; } = Visibility.Collapsed;
        public Visibility SellVisibility { get; set; } = Visibility.Collapsed;
        public Visibility CancelSellVisibility { get; set; } = Visibility.Collapsed;
        public Album(BitmapImage image, string name, string artist, string owner, int price, int selling) {
            Image = image;
            Name = name;
            Artist = artist;
            Owner = owner;
            Price = price;
            Selling = selling;
        }
    }
}
