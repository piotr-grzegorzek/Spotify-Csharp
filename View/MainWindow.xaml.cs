using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using MahApps.Metro.Controls;
using Studio.Logic;
using static Studio.Logic.AppManager;

namespace Studio {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private BitmapImage image;
        public MainWindow()
        {
            InitializeComponent();
            image = getLogo();
            logoImg.ImageSource = image;
            studioName.Text = get_studio_name();
            var albums = get_albums();
            if (albums.Count > 0)
                albumControl.ItemsSource = albums;
            refresh_acc_balance();
            shortcuts();
        }
        void shortcuts() {
            //open_library - alt l
            RoutedCommand lib = new RoutedCommand();
            lib.InputGestures.Add(new KeyGesture(Key.L, ModifierKeys.Alt));
            CommandBindings.Add(new CommandBinding(lib, open_library));
            //open_shop - alt s
            RoutedCommand shop = new RoutedCommand();
            shop.InputGestures.Add(new KeyGesture(Key.S, ModifierKeys.Alt));
            CommandBindings.Add(new CommandBinding(shop, open_shop));
            //open_settings - ctrl s
            RoutedCommand settings = new RoutedCommand();
            settings.InputGestures.Add(new KeyGesture(Key.S, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(settings, open_settings));
            //open_terms - alt t
            RoutedCommand terms = new RoutedCommand();
            terms.InputGestures.Add(new KeyGesture(Key.T, ModifierKeys.Alt));
            CommandBindings.Add(new CommandBinding(terms, open_terms));
            //logout - ctrl shift l
            RoutedCommand logou = new RoutedCommand();
            logou.InputGestures.Add(new KeyGesture(Key.L, ModifierKeys.Control | ModifierKeys.Shift));
            CommandBindings.Add(new CommandBinding(logou, logout));

            if ((int) userInfo["isAdmin"] == 1) {
                //show_artists - ctrl a
                RoutedCommand artists = new RoutedCommand();
                artists.InputGestures.Add(new KeyGesture(Key.A, ModifierKeys.Control));
                CommandBindings.Add(new CommandBinding(artists, show_artists));
                //show_users - ctrl u
                RoutedCommand users = new RoutedCommand();
                users.InputGestures.Add(new KeyGesture(Key.U, ModifierKeys.Control));
                CommandBindings.Add(new CommandBinding(users, show_users));
                //edit logo - ctrl l
                RoutedCommand edit_log = new RoutedCommand();
                edit_log.InputGestures.Add(new KeyGesture(Key.L, ModifierKeys.Control));
                CommandBindings.Add(new CommandBinding(edit_log, edit_logo));
                //edit_studio_name - ctrl n
                RoutedCommand edit_studio_nam = new RoutedCommand();
                edit_studio_nam.InputGestures.Add(new KeyGesture(Key.N, ModifierKeys.Control));
                CommandBindings.Add(new CommandBinding(edit_studio_nam, edit_studio_name));
                //new_album - ctrl shift n
                RoutedCommand new_albu = new RoutedCommand();
                new_albu.InputGestures.Add(new KeyGesture(Key.N, ModifierKeys.Control | ModifierKeys.Shift));
                CommandBindings.Add(new CommandBinding(new_albu, new_album));
            }
        }
        private void open_library(object sender, RoutedEventArgs e) {
            if (viewMode == "") {
                viewMode = "lib";
                menuLibrary.SetResourceReference(MenuItem.HeaderProperty, "Main Page");
            }
            else if (viewMode == "lib") {
                viewMode = "";
                menuLibrary.SetResourceReference(MenuItem.HeaderProperty, "Library");
            }
            else if (viewMode == "shop"){
                viewMode = "lib";
                menuLibrary.SetResourceReference(MenuItem.HeaderProperty, "Main Page");
                menuShop.SetResourceReference(MenuItem.HeaderProperty, "Shop");
            }
            albumControl.ItemsSource = get_albums();
        }
        private void open_shop(object sender, RoutedEventArgs e) {
            if (viewMode == "") {
                viewMode = "shop";
                menuShop.SetResourceReference(MenuItem.HeaderProperty, "Main Page");
            }
            else if (viewMode == "shop") {
                viewMode = "";
                menuShop.SetResourceReference(MenuItem.HeaderProperty, "Shop");
            }
            else if (viewMode == "lib") {
                viewMode = "shop";
                menuShop.SetResourceReference(MenuItem.HeaderProperty, "Main Page");
                menuLibrary.SetResourceReference(MenuItem.HeaderProperty, "Library");
            }
            albumControl.ItemsSource = get_albums();
        }
        private void show_artists(object sender, RoutedEventArgs e) {
            var artists_dialog = new ArtistsDialog();
            bool? result = artists_dialog.ShowDialog();
            if (result == true)
                albumControl.ItemsSource = get_albums();
        }
        private void show_users(object sender, RoutedEventArgs e) {
            var users_dialog = new UsersDialog();
            users_dialog.ShowDialog();
            refresh_acc_balance();
        }
        private void open_settings(object sender, RoutedEventArgs e) {
            var settings_dialog = new SettingsDialog();
            bool? result = settings_dialog.ShowDialog();
        }
        private void open_terms(object sender, RoutedEventArgs e) {
            var terms_dialog = new TermsDialog();
            terms_dialog.ShowDialog();
        }
        private void logout(object sender, RoutedEventArgs e) {
            userInfo.Clear();
            applyPreferences();
            LoginWindow login_window = new LoginWindow();
            Close();
            login_window.Show();
        }
        private BitmapImage getLogo() {
            Database db = DatabaseFactory.init();
            Byte[] img = db.getLogo();
            return ImageFromBuffer(img);
        }
        private void edit_logo(object sender, RoutedEventArgs e) {
            choose_img(ref image);
            Byte[] new_logo = BufferFromImage(image);
            Database db = DatabaseFactory.init();
            db.setLogo(new_logo);
            logoImg.ImageSource = image;
        }
        private string get_studio_name() {
            Database db = DatabaseFactory.init();
            return db.getStudioName();
        }
        private void edit_studio_name(object sender, RoutedEventArgs e) {
            var name_dialog = new EditStudioNameDialog(studioName.Text);
            bool? result = name_dialog.ShowDialog();
            if (result == true)
                studioName.Text = get_studio_name();
        }
        private void refresh_acc_balance() {
            string money = ": " + ((int) userInfo["accountBalance"]).ToString() + " PLN";
            string base_text = (string) Application.Current.Resources["Account Balance"];
            //join base_text and money
            acc_balance.Text = base_text + money;
        }
        private List<Album> get_albums() {
            List<Album> albums = new List<Album>();
            Database db = DatabaseFactory.init();
            var db_albums = new List<Dictionary<string, object>>();
            if (viewMode == "lib")
                db_albums = db.getUserAlbums((string) userInfo["username"]);
            else if (viewMode == "shop")
                db_albums = db.getSellingAlbums((string) userInfo["username"]);
            else
                db_albums = db.getAlbums();
            for (int i = 0; i < db_albums.Count; i++)
                albums.Add(new Album(
                    ImageFromBuffer((Byte[]) db_albums[i]["Image"]),
                    (string) db_albums[i]["Name"],
                    (string) db_albums[i]["Artist"],
                    (string) db_albums[i]["Owner"],
                    (int) db_albums[i]["Price"],
                    (int) db_albums[i]["Selling"]));

            //Set buy visibility to true if album is not owned by user and selling property is set to true
            foreach (var album in albums) {
                if (album.Owner != (string) userInfo["username"] && album.Selling == 1)
                    album.BuyVisibility = Visibility.Visible;
            }
            //set sell visibility to true if album is owned by user and selling property is set to false
            foreach (var album in albums) {
                if (album.Owner == (string) userInfo["username"] && album.Selling == 0)
                    album.SellVisibility = Visibility.Visible;
            }
            //set cancel sell visibility to true if album is owned by user and selling property is set to true
            foreach (var album in albums) {
                if (album.Owner == (string) userInfo["username"] && album.Selling == 1)
                    album.CancelSellVisibility = Visibility.Visible;
            }
            return albums;
        }
        private void Card_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e) {
            ((Border) sender).Cursor = Cursors.Hand;
            ((Border) sender).SetResourceReference(BackgroundProperty, "colorCardHover");
        }
        private void Card_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e) {
            ((Border) sender).Cursor = Cursors.Arrow;
            ((Border) sender).SetResourceReference(BackgroundProperty, "colorCard");
        }
        private void open_album(object sender, RoutedEventArgs e) {
            var item = ((Border) sender);
            Album album = (Album) item.DataContext;
            var album_dialog = new AlbumDialog(album);
            bool? result = album_dialog.ShowDialog();
        }
        private void edit_album(object sender, RoutedEventArgs e) {
            var item = ((MenuItem) sender);
            var editAlbum_dialog = new EditAlbumDialog((Album) item.DataContext);
            bool? result = editAlbum_dialog.ShowDialog();
            if (result == true)
                albumControl.ItemsSource = get_albums();
        }
        private void new_album(object sender, RoutedEventArgs e) {
            Database db = DatabaseFactory.init();
            if (db.artistsCount() == 0){
                MessageBox.Show((string) Application.Current.Resources["err_not_enough_artists"]);
                return;
            }
            var newAlbum_dialog = new NewAlbumDialog();
            bool? result = newAlbum_dialog.ShowDialog();
            if (result == true)
                albumControl.ItemsSource = get_albums();
        }
        private void delete_album(object sender, RoutedEventArgs e) {
            //Delete Album from Database
            var item = ((MenuItem) sender);
            Album album = (Album) item.DataContext;
            Database db = DatabaseFactory.init();
            db.deleteAlbum(album.Name);
            albumControl.ItemsSource = get_albums();
        }
        private void buy(object sender, RoutedEventArgs e) {
            //check if user can afford to buy album
            var item = ((MenuItem) sender);
            Album album = (Album) item.DataContext;
            if ((int) userInfo["accountBalance"] < album.Price) {
                MessageBox.Show((string) Application.Current.Resources["no_money"]);
                return;
            }
            //buy album
            Database db = DatabaseFactory.init();
            db.buyAlbum((string) userInfo["username"], album.Name);
            //update user info
            userInfo = db.userInfo((string) userInfo["username"]);
            //update albums
            albumControl.ItemsSource = get_albums();
            refresh_acc_balance();
        }
        private void sell(object sender, RoutedEventArgs e) {
            //Set album selling parameter to 1 in database
            var item = ((MenuItem) sender);
            Album album = (Album) item.DataContext;
            Database db = DatabaseFactory.init();
            db.setAlbumSelling(album.Name, 1);
            albumControl.ItemsSource = get_albums();
        }
        private void cancel_sell(object sender, RoutedEventArgs e) {
            //Set album selling parameter to 0 in database
            var item = ((MenuItem) sender);
            Album album = (Album) item.DataContext;
            Database db = DatabaseFactory.init();
            db.setAlbumSelling(album.Name, 0);
            albumControl.ItemsSource = get_albums();
        }
    }
}