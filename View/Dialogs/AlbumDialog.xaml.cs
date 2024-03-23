using Studio.Logic;
using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls;
using System.Windows.Input;
using System.Collections.Generic;
using System;
using static Studio.Logic.AppManager;
using System.Windows.Documents;

namespace Studio
{
    /// <summary>
    /// Interaction logic for TermsDialog.xaml
    /// </summary>
    public partial class AlbumDialog : MetroWindow
    {
        public AlbumDialog(Album album)
        {
            InitializeComponent();
            Title = album.Name;

            albumImage.ImageSource = album.Image;
            albumName.Text = album.Name;
            albumArtist.Inlines.Add((string)Application.Current.Resources["MadeBy"]+" ");
            albumArtist.Inlines.Add(new Run(album.Artist) { FontStyle = FontStyles.Italic });
            albumOwner.Inlines.Add((string) Application.Current.Resources["OwnedBy"]+" ");
            albumOwner.Inlines.Add(new Run(album.Owner) { FontStyle = FontStyles.Italic });
            albumPrice.Inlines.Add((string) Application.Current.Resources["PricedAt"] + " ");
            albumPrice.Inlines.Add(new Run($"{album.Price} PLN") { FontStyle = FontStyles.Italic });

            songsControl.ItemsSource = get_songs_for(album.Name);
            shortcuts();
        }
        private void shortcuts() {
            if ((int) userInfo["isAdmin"] == 1) {
                //new_song - ctrl shift n
                RoutedCommand new_son = new RoutedCommand();
                new_son.InputGestures.Add(new KeyGesture(Key.N, ModifierKeys.Control | ModifierKeys.Shift));
                CommandBindings.Add(new CommandBinding(new_son, new_song));
            }
        }
        private List<Song> get_songs_for(string album) {
            List<Song> songs = new List<Song>();
            Database db = DatabaseFactory.init();
            var db_songs = db.getSongs(album);
            for (int i = 0; i < db_songs.Count; i++)
                songs.Add(new Song(
                    ImageFromBuffer((Byte[]) db_songs[i]["Image"]),
                    (string) db_songs[i]["Name"],
                    (string) db_songs[i]["Album"]));
            return songs;
        }
        private void Card_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e) {
            ((Border) sender).Cursor = Cursors.Hand;
            ((Border) sender).SetResourceReference(BackgroundProperty, "colorCardHover");
        }
        private void Card_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e) {
            ((Border) sender).Cursor = Cursors.Arrow;
            ((Border) sender).SetResourceReference(BackgroundProperty, "colorCard");
        }
        private void open_song(object sender, MouseButtonEventArgs e) {
            var item = ((Border) sender);
            Song song = (Song) item.DataContext;
            var song_dialog = new SongDialog(song);
            bool? result = song_dialog.ShowDialog();
        }
        private void edit_song(object sender, RoutedEventArgs e) {
            var item = ((MenuItem) sender);
            Song song = (Song) item.DataContext;
            var editSong_dialog = new EditSongDialog(song);
            bool? result = editSong_dialog.ShowDialog();
            if (result == true)
                songsControl.ItemsSource = get_songs_for(albumName.Text);
        }
        private void new_song(object sender, RoutedEventArgs e) {
            var newSong_dialog = new NewSongDialog(albumName.Text);
            bool? result = newSong_dialog.ShowDialog();
            if (result == true)
                songsControl.ItemsSource = get_songs_for(albumName.Text);
        }
        private void delete_song(object sender, RoutedEventArgs e) {
            var item = ((MenuItem) sender);
            string name = ((Song) item.DataContext).Name;
            Database db = DatabaseFactory.init();
            db.deleteSong(name);
            songsControl.ItemsSource = get_songs_for(albumName.Text);
        }
        private void okButton_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }
    }
}
