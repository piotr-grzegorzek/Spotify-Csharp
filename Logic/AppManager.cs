using System.Collections.Generic;
using System.Windows;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using System.Globalization;
using Microsoft.Win32;
using ControlzEx.Theming;
using System.IO;
using System.Windows.Media.Imaging;
using System;
using System.Windows.Controls;
using NAudio.Wave;

namespace Studio.Logic {
    static class AppManager {
        public static Dictionary<string, object>? userInfo;
        public static string viewMode = "";
        public static string SHA1(string input) {
            var hash = new SHA1Managed().ComputeHash(Encoding.UTF8.GetBytes(input));
            return string.Concat(hash.Select(b => b.ToString("x2")));
        }
        public static void applyPreferences(string? lang = null, string? layout = null) {
            if (lang == null)
                lang = getSystemLang();
            if (layout == null)
                layout = getSystemLayout();
            if (layout == (string) Application.Current.Resources["tagDark"])
                ThemeManager.Current.ChangeTheme(Application.Current, "Dark.Emerald");
            else
                ThemeManager.Current.ChangeTheme(Application.Current, "Light.Emerald");

            ResourceDictionary lang_res = new ResourceDictionary();
            lang_res.Source = new System.Uri($"Resources/Lang/{lang}.xaml", System.UriKind.Relative);
            Application.Current.Resources.MergedDictionaries[1] = lang_res;
            ResourceDictionary layout_res = new ResourceDictionary();
            layout_res.Source = new System.Uri($"Resources/Styles/{layout}.xaml", System.UriKind.Relative);
            Application.Current.Resources.MergedDictionaries[2] = layout_res;
        }
        public static string getSystemLang() {
            string system_lang = CultureInfo.InstalledUICulture.TwoLetterISOLanguageName;
            if (system_lang == "pl")
                return (string) Application.Current.Resources["tagPL"];
            return (string) Application.Current.Resources["tagENG"];
        }
        public static string getSystemLayout() {
            int theme = 1;
            try {
                theme = (int) Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize", "AppsUseLightTheme", -1);
            }
            catch { }
            if (theme == 0)
                return (string) Application.Current.Resources["tagDark"];
            return (string) Application.Current.Resources["tagLight"];
        }
        public static BitmapImage ImageFromBuffer(Byte[] bytes) {
            MemoryStream stream = new MemoryStream(bytes);
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.StreamSource = stream;
            image.EndInit();
            return image;
        }
        public static Byte[] BufferFromImage(BitmapImage imageSource) {
            byte[] data;
            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(imageSource));
            using (MemoryStream ms = new MemoryStream()) {
                encoder.Save(ms);
                data = ms.ToArray();
            }
            return data;
        }
        public static void choose_img(ref BitmapImage? image, Button? btnImage = null) {
            var dialog = new OpenFileDialog();
            dialog.Filter = "Pictures (*.jpeg;*.jpg;*.png;*.bmp;*.gif)|*.jpeg;*.jpg;*.png;*.bmp;*.gif";
            if (dialog.ShowDialog() == true) {
                image = new BitmapImage(new Uri(dialog.FileName));
                if (btnImage != null)
                    btnImage.Content = dialog.SafeFileName;
            }
        }
        public static void choose_song(ref byte[]? song, Button? btnSong = null) {
            var dialog = new OpenFileDialog();
            dialog.Filter = "Audio files (*.mp3)|*.mp3";
            if (dialog.ShowDialog() == true) {
                string path = new Uri(dialog.FileName, UriKind.Absolute).ToString();
                //cut file:///
                path = path.Substring(8);
                song = File.ReadAllBytes(path);
                if (btnSong != null)
                    btnSong.Content = dialog.SafeFileName;
            }
        }
        public static List<string> getArtistsFromDb() {
            Database db = DatabaseFactory.init();
            var db_artists = db.getArtists();
            List<string> artists = new List<string>();
            foreach (var artist in db_artists)
                artists.Add((string) artist["Name"]);
            return artists;
        }
        public static List<string> getUsersFromDb() {
            Database db = DatabaseFactory.init();
            var db_users = db.getUsers();
            List<string> users = new List<string>();
            foreach (var user in db_users)
                users.Add((string) user["username"]);
            return users;
        }
        public static List<string> getAlbumsFromDb() {
            Database db = DatabaseFactory.init();
            var db_albums = db.getAlbums();
            List<string> albums = new List<string>();
            foreach (var album in db_albums)
                albums.Add((string) album["Name"]);
            return albums;
        }
    }
}