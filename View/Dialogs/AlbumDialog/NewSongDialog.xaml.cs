using System.Windows;
using System.Windows.Media.Imaging;
using MahApps.Metro.Controls;
using static Studio.Logic.AppManager;

namespace Studio {
    /// <summary>
    /// Interaction logic for StudioNameDialog.xaml
    /// </summary>
    public partial class NewSongDialog : MetroWindow {
        private BitmapImage? image;
        private byte[]? mp3;
        private string song_album;
        public NewSongDialog(string album) {
            InitializeComponent();
            song_album = album;
        }
        private void btnImage_Click(object sender, RoutedEventArgs e) {
            choose_img(ref image, btnImage);
        }
        private void btnSong_Click(object sender, RoutedEventArgs e) {
            choose_song(ref mp3, btnSong);
        }
        private void okButton_Click(object sender, RoutedEventArgs e) {
            //Add Song to Database
            if (image != null) {
                if (txtName.Text != "") {
                    if (mp3 != null) {
                        Database db = DatabaseFactory.init();
                        if (db.addSong(BufferFromImage(image), txtName.Text, song_album, mp3))
                            DialogResult = true;
                        else
                            MessageBox.Show(db.errMsg);
                    }
                    else
                        MessageBox.Show((string) Application.Current.Resources["err_no_song"]);
                }
                else
                    MessageBox.Show((string) Application.Current.Resources["err_no_name"]);
            }
            else
                MessageBox.Show((string) Application.Current.Resources["err_no_image"]);
        }
        private void cancelButton_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }
    }
}
