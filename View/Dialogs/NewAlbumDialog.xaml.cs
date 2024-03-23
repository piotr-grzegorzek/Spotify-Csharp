using System.Windows;
using System.Windows.Media.Imaging;
using MahApps.Metro.Controls;
using Studio.Logic;
using static Studio.Logic.AppManager;

namespace Studio {
    /// <summary>
    /// Interaction logic for StudioNameDialog.xaml
    /// </summary>
    public partial class NewAlbumDialog : MetroWindow {
        private BitmapImage? image;
        public NewAlbumDialog() {
            InitializeComponent();
            //Load Artists to cmbArtist
            foreach (string artist in getArtistsFromDb())
                cmbArtist.Items.Add(artist);
            cmbArtist.SelectedIndex = 0;
            //Load users to cmbOwner
            foreach (string user in getUsersFromDb())
                cmbOwner.Items.Add(user);
            cmbOwner.SelectedIndex = 0;
        }
        private void btnImage_Click(object sender, RoutedEventArgs e) {
            choose_img(ref image, btnImage);
        }
        private void okButton_Click(object sender, RoutedEventArgs e) {
            //Add Album to Database
            if (image != null) {
                if (txtName.Text != "") {
                    //Make sure intPrice is not empty and is a number
                    if (intPrice.Text != "" && int.TryParse(intPrice.Text, out int price)) {
                        Database db = DatabaseFactory.init();
                        if (db.addAlbum(BufferFromImage(image), txtName.Text, cmbArtist.SelectedValue.ToString(), cmbOwner.SelectedValue.ToString(), price))
                            DialogResult = true;
                        else
                            MessageBox.Show(db.errMsg);
                    }
                    else
                        MessageBox.Show((string) Application.Current.Resources["err_invalid_price"]);
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
