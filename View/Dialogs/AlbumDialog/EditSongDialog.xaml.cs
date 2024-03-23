using System.Windows;
using System.Windows.Media.Imaging;
using MahApps.Metro.Controls;
using Studio.Logic;
using static Studio.Logic.AppManager;

namespace Studio {
    /// <summary>
    /// Interaction logic for StudioNameDialog.xaml
    /// </summary>
    public partial class EditSongDialog : MetroWindow {
        private BitmapImage image;
        private string initial_name;
        private byte[] mp3;
        public EditSongDialog(Song song) {
            InitializeComponent();
            image = song.Image;
            
            txtName.Text = song.Name;
            initial_name = song.Name;
            
            //Load Albums to cmbAlbum
            foreach (string album in getAlbumsFromDb())
                cmbAlbum.Items.Add(album);
            //Set Album in cmbAlbum
            cmbAlbum.SelectedValue = song.Album;
            
            Database db = DatabaseFactory.init();
            mp3 = db.getSong(song.Name);
        }
        private void btnImage_Click(object sender, RoutedEventArgs e) {
            choose_img(ref image, btnImage);
        }
        private void btnSong_Click(object sender, RoutedEventArgs e) {
            choose_song(ref mp3, btnSong);
        }
        private void okButton_Click(object sender, RoutedEventArgs e) {
            //Edit Song in Database
            if (txtName.Text != "") {
                Database db = DatabaseFactory.init();
                if (db.editSong(initial_name, BufferFromImage(image), txtName.Text, cmbAlbum.SelectedValue.ToString(), mp3))
                    DialogResult = true;
                else
                    MessageBox.Show(db.errMsg);
            }
            else
                MessageBox.Show((string) Application.Current.Resources["err_no_name"]);
        }
        private void cancelButton_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }
    }
}
