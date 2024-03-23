using System.Windows;
using System.Windows.Media.Imaging;
using MahApps.Metro.Controls;
using Studio.Logic;
using static Studio.Logic.AppManager;

namespace Studio {
    /// <summary>
    /// Interaction logic for StudioNameDialog.xaml
    /// </summary>
    public partial class EditArtistDialog : MetroWindow {
        private BitmapImage image;
        private string initial_name;
        public EditArtistDialog(Artist artist) {
            InitializeComponent();
            image = artist.Image;
            txtName.Text = artist.Name;
            initial_name = artist.Name;
        }
        private void btnImage_Click(object sender, RoutedEventArgs e) {
            choose_img(ref image, btnImage);
        }
        private void okButton_Click(object sender, RoutedEventArgs e) {
            //Edit Artist in Database
            if (txtName.Text != "") {
                Database db = DatabaseFactory.init();
                if (db.editArtist(initial_name, BufferFromImage(image), txtName.Text))
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
