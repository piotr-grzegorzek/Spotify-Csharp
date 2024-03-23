using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using MahApps.Metro.Controls;
using Studio.Logic;
using static Studio.Logic.AppManager;

namespace Studio {
    /// <summary>
    /// Interaction logic for StudioNameDialog.xaml
    /// </summary>
    public partial class EditAlbumDialog : MetroWindow {
        private BitmapImage image;
        private string initial_name;
        public EditAlbumDialog(Album album) {
            InitializeComponent();
            image = album.Image;
            txtName.Text = album.Name;
            initial_name = album.Name;
            //Load Artists to cmbArtist
            foreach (string artist in getArtistsFromDb())
                cmbArtist.Items.Add(artist);
            //Set Artist in cmbArtist         
            cmbArtist.SelectedValue = album.Artist;
            //Load users to cmbOwner
            foreach (string user in getUsersFromDb())
                cmbOwner.Items.Add(user);
            //Set owner in cmbOwner
            cmbOwner.SelectedValue = album.Owner;
            intPrice.Text = album.Price.ToString();
        }
        private void btnImage_Click(object sender, RoutedEventArgs e) {
            choose_img(ref image, btnImage);
        }
        private void okButton_Click(object sender, RoutedEventArgs e) {
            //Edit Album in Database
            if (txtName.Text != "") {
                //Make sure intPrice is not empty and is a number
                if (intPrice.Text != "" && int.TryParse(intPrice.Text, out int price)) {
                    Database db = DatabaseFactory.init();
                    if (db.editAlbum(initial_name, BufferFromImage(image), txtName.Text, cmbArtist.SelectedValue.ToString(), cmbOwner.SelectedValue.ToString(), price))
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
        private void cancelButton_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }
    }
}
