using System.Windows;
using MahApps.Metro.Controls;
using static Studio.Logic.AppManager;

namespace Studio {
    /// <summary>
    /// Interaction logic for StudioNameDialog.xaml
    /// </summary>
    public partial class ChangeArtistDialog : MetroWindow {
        private string artist_name;
        public ChangeArtistDialog(string artist_name) {
            InitializeComponent();
            //Load Artists to cmbArtist
            foreach (string artist in getArtistsFromDb())
                if (artist != artist_name)
                    cmbArtist.Items.Add(artist);
            //select first element
            cmbArtist.SelectedIndex = 0;
            this.artist_name = artist_name;
        }
        private void okButton_Click(object sender, RoutedEventArgs e) {
            //Update all albums belonging to artist_name to cmbArtist.SelectedValue.ToString()
            Database db = DatabaseFactory.init();
            db.updateAlbumsArtist(artist_name, cmbArtist.SelectedValue.ToString());
            DialogResult = true;
        }
        private void cancelButton_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }
    }
}
