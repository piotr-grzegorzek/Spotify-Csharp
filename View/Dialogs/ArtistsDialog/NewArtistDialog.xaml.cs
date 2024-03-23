using System.Windows;
using System.Windows.Media.Imaging;
using MahApps.Metro.Controls;
using static Studio.Logic.AppManager;

namespace Studio {
    /// <summary>
    /// Interaction logic for StudioNameDialog.xaml
    /// </summary>
    public partial class NewArtistDialog : MetroWindow {
        private BitmapImage? image;
        public NewArtistDialog() {
            InitializeComponent();
        }
        private void btnImage_Click(object sender, RoutedEventArgs e) {
            choose_img(ref image, btnImage);
        }
        private void okButton_Click(object sender, RoutedEventArgs e) {
            //Add Artist to Database
            if (image != null) {
                if (txtName.Text != "") {
                    Database db = DatabaseFactory.init();
                    if (db.addArtist(BufferFromImage(image), txtName.Text))
                        DialogResult = true;
                    else
                        MessageBox.Show(db.errMsg);
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
