using Studio.Logic;
using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections.Generic;
using System;
using static Studio.Logic.AppManager;

namespace Studio
{
    /// <summary>
    /// Interaction logic for TermsDialog.xaml
    /// </summary>
    public partial class ArtistsDialog : MetroWindow
    {
        public ArtistsDialog()
        {
            InitializeComponent();
            songsControl.ItemsSource = get_artists();
            shortcuts();
        }
        private void shortcuts() {
            //new_artist - ctrl shift n
            RoutedCommand newart = new RoutedCommand();
            newart.InputGestures.Add(new KeyGesture(Key.N, ModifierKeys.Control | ModifierKeys.Shift));
            CommandBindings.Add(new CommandBinding(newart, new_artist));
        }
        private List<Artist> get_artists() {
            List<Artist> artists = new List<Artist>();
            Database db = DatabaseFactory.init();
            var db_artists = db.getArtists();
            for (int i = 0; i < db_artists.Count; i++)
                artists.Add(new Artist(
                    ImageFromBuffer((Byte[]) db_artists[i]["Image"]),
                    (string) db_artists[i]["Name"]));
            return artists;
        }
        private void Card_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e) {
            ((Border) sender).Cursor = Cursors.Hand;
            ((Border) sender).SetResourceReference(BackgroundProperty, "colorCardHover");
        }
        private void Card_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e) {
            ((Border) sender).Cursor = Cursors.Arrow;
            ((Border) sender).SetResourceReference(BackgroundProperty, "colorCard");
        }
        private void edit_artist(object sender, RoutedEventArgs e) {
            var item = ((MenuItem) sender);
            var editArtist_dialog = new EditArtistDialog((Artist) item.DataContext);
            bool? result = editArtist_dialog.ShowDialog();
            if (result == true)
                songsControl.ItemsSource = get_artists();
        }
        private void new_artist(object sender, RoutedEventArgs e) {
            var newArtist_dialog = new NewArtistDialog();
            bool? result = newArtist_dialog.ShowDialog();
            if(result==true)
                songsControl.ItemsSource = get_artists();
        }
        private void delete_artist(object sender, RoutedEventArgs e) {
            var item = ((MenuItem) sender);
            var artist = (Artist) item.DataContext;
            //check if artist has any album
            Database db = DatabaseFactory.init();
            if (db.artistGotAlbum(artist.Name)) {
                if (db.artistsCount() == 1) {
                    MessageBox.Show((string) Application.Current.Resources["err_not_enough_artists"]);
                    return;
                }
                var changeArtist_dialog = new ChangeArtistDialog(artist.Name);
                bool? result = changeArtist_dialog.ShowDialog();
                if (result == true) {
                    //Delete Artist from Database
                    db.deleteArtist(artist.Name);
                    songsControl.ItemsSource = get_artists();
                }
            }
            else {
                //Delete Artist from Database
                db.deleteArtist(artist.Name);
                songsControl.ItemsSource = get_artists();
            }
        }
        private void okButton_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }
    }
}
