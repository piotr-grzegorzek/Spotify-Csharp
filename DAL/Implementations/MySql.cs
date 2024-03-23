using MySqlConnector;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace Studio.DAL.Implementations {
    internal class MySQL : Database {
        private string server_conn_info;
        private string db_conn_info;
        public string? errMsg { get; set; }
        public MySQL() {
            db_conn_info = File.ReadAllText("Resources/Secret/mysql.txt");
            //server_conn_info is db_conn_info without database name
            server_conn_info = db_conn_info.Substring(0, db_conn_info.IndexOf("DATABASE=") + 9) + db_conn_info.Substring(db_conn_info.IndexOf("UID=") - 1);
        }
        public bool not_exists() {
            bool not_exists = true;
            string query = "SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = 'mysql_studio'";
            MySqlConnection conn = new MySqlConnection(server_conn_info);
            conn.Open();
            MySqlCommand command = new MySqlCommand(query, conn);
            MySqlDataReader reader = command.ExecuteReader();
            if (reader.Read())
                not_exists = false;
            conn.Close();
            return not_exists;
        }
        public void create() {
            MySqlConnection server_conn = new MySqlConnection(server_conn_info);
            server_conn.Open();
            MySqlCommand command = new MySqlCommand("CREATE DATABASE mysql_studio", server_conn);
            command.ExecuteNonQuery();
            server_conn.Close();

            string constring = db_conn_info;
            string file = "Resources/StandardDBs/standard_mysql.sql";
            using (MySqlConnection conn = new MySqlConnection(constring)) {
                using (MySqlCommand cmd = new MySqlCommand()) {
                    using (MySqlBackup mb = new MySqlBackup(cmd)) {
                        cmd.Connection = conn;
                        conn.Open();
                        mb.ImportFromFile(file);
                        conn.Close();
                    }
                }
            }
        }
        public bool adminExists() {
            if (read("SELECT isAdmin FROM `users` WHERE isAdmin=1").Any())
                return true;
            return false;
        }
        public bool createUser(string username, string password, string lang, string layout, int isAdmin, int accountBalance) {
            try {
                exec($"INSERT INTO `users` " +
                    $"(`username`, `password`, `language`, `layout`, `isAdmin`, `accountBalance`) VALUES " +
                    $"('{username}', '{password}', '{lang}', '{layout}', '{isAdmin}', '{accountBalance}')");
                return true;
            }
            catch (MySqlException ex) {
                if (ex.Number == 1062)
                    errMsg = (string?) Application.Current.Resources["err_duplicate_name"];
                else
                    errMsg = $"{ex.Message}";
                return false;
            }
        }
        public bool loginUser(string username, string password) {
            if (read($"SELECT * FROM `users` WHERE username='{username}' AND password='{password}'").Any())
                return true;
            return false;
        }
        public Dictionary<string, object> userInfo(string username) {
            return read($"SELECT username, language, layout, isAdmin, accountBalance FROM `users` WHERE username='{username}'")[0];
        }
        public Byte[] getLogo() {
            return (Byte[]) read("SELECT logo FROM `config`")[0]["logo"];
        }
        public void setLogo(Byte[] img) {
            exec($"UPDATE `config` SET logo=0x{Convert.ToHexString(img)}");
        }
        public string getStudioName() {
            return (string) read("SELECT studioName FROM `config`")[0]["studioName"];
        }
        public void updateStudioName(string text) {
            exec($"UPDATE `config` SET studioName='{text}'");
        }
        public void updateUserPreferences(string lang_choice, string layout_choice, string username) {
            exec($"UPDATE `users` SET language='{lang_choice}', layout='{layout_choice}' WHERE username='{username}'");
        }
        public List<Dictionary<string, object>> getUsers() {
            return read("SELECT username, isAdmin, accountBalance FROM `users`");
        }
        public bool editUser(string initial_name, string username, int permission, int balance) {
            //Return errmsg and false when there user is the last admin left, and the admin permission is being removed
            if (permission == 0 && read($"SELECT isAdmin FROM `users` WHERE isAdmin=1 AND username='{initial_name}'").Any() && !read($"SELECT isAdmin FROM `users` WHERE isAdmin=1 AND username!='{initial_name}'").Any()) {
                errMsg = (string?) Application.Current.Resources["err_last_admin"];
                return false;
            }
            try {
                exec($"UPDATE `users` SET username='{username}', isAdmin={permission}, accountBalance={balance} WHERE username='{initial_name}'");
                return true;
            }
            catch (MySqlException ex) {
                if (ex.Number == 1062)
                    errMsg = (string?) Application.Current.Resources["err_duplicate_name"];
                else
                    errMsg = $"{ex.Message}";
                return false;
            }
        }
        public List<Dictionary<string, object>> getAlbums() {
            return read("SELECT Image, Name, Artist, Owner, Price, Selling FROM `albums`");
        }
        public List<Dictionary<string, object>> getUserAlbums(string username) {
            return read($"SELECT Image, Name, Artist, Owner, Price, Selling FROM `albums` WHERE Owner='{username}'");
        }
        public List<Dictionary<string, object>> getSellingAlbums(string username) {
            //Get all albums that are for sale but not owned by the user
            return read($"SELECT Image, Name, Artist, Owner, Price, Selling FROM `albums` WHERE Owner!='{username}' AND Selling=1");
        }
        public bool editAlbum(string initial_name, byte[] image, string name, string artist, string owner, int price) {
            try {
                exec($"UPDATE `albums` SET Image=0x{Convert.ToHexString(image)}, Name='{name}', Artist='{artist}', Owner='{owner}', Price='{price}' WHERE Name='{initial_name}'");
                return true;
            }
            catch (MySqlException ex) {
                if (ex.Number == 1062)
                    errMsg = (string?) Application.Current.Resources["err_duplicate_name"];
                else
                    errMsg = $"{ex.Message}";
                return false;
            }
        }
        public bool addAlbum(byte[] image, string name, string artist, string owner, int price) {
            try {
                exec($"INSERT INTO `albums` " +
                    $"(`Image`, `Name`, `Artist`, `Owner`, `Price`, `Selling`) VALUES " +
                    $"(0x{Convert.ToHexString(image)}, '{name}', '{artist}', '{owner}', '{price}', 0)");
                return true;
            }
            catch (MySqlException ex) {
                if (ex.Number == 1062)
                    errMsg = (string?) Application.Current.Resources["err_duplicate_name"];
                else
                    errMsg = $"{ex.Message}";
                return false;
            }
        }
        public void deleteAlbum(string name) {
            exec($"DELETE FROM `albums` WHERE Name='{name}'");
            exec($"DELETE FROM `songs` WHERE Album='{name}'");
        }
        public void buyAlbum(string user, string name) {
            //get old owner
            string old_owner = (string) read($"SELECT Owner FROM `albums` WHERE Name='{name}'")[0]["Owner"];
            exec($"UPDATE `albums` SET Owner='{user}', Selling=0 WHERE Name='{name}'");
            //get the price of the album
            int price = (int) read($"SELECT Price FROM `albums` WHERE Name='{name}'")[0]["Price"];
            //reduce the price from the user's account balance
            exec($"UPDATE `users` SET accountBalance=accountBalance-{price} WHERE username='{user}'");
            //add the price to the old owner's account balance
            exec($"UPDATE `users` SET accountBalance=accountBalance+{price} WHERE username='{old_owner}'");
        }
        public void setAlbumSelling(string name, int v) {
            exec($"UPDATE `albums` SET Selling={v} WHERE Name='{name}'");
        }
        public List<Dictionary<string, object>> getArtists() {
            return read("SELECT Image, Name FROM `artists`");
        }
        public bool editArtist(string initial_name, Byte[] image, string new_name) {
            try {
                exec($"UPDATE `artists` SET Image=0x{Convert.ToHexString(image)}, Name='{new_name}' WHERE Name='{initial_name}'");
                return true;
            }
            catch (MySqlException ex) {
                if (ex.Number == 1062)
                    errMsg = (string?) Application.Current.Resources["err_duplicate_name"];
                else
                    errMsg = $"{ex.Message}";
                return false;
            }
        }
        public bool addArtist(byte[] Image, string Name) {
            try {
                exec($"INSERT INTO `artists` (`Image`, `Name`) VALUES (0x{Convert.ToHexString(Image)}, '{Name}')");
                return true;
            }
            catch (MySqlException ex) {
                if (ex.Number == 1062)
                    errMsg = (string?) Application.Current.Resources["err_duplicate_name"];
                else
                    errMsg = $"{ex.Message}";
                return false;
            }
        }
        public bool artistGotAlbum(string name) {
            return read($"SELECT Artist FROM `albums` WHERE Artist='{name}'").Any();
        }
        public void updateAlbumsArtist(string artist_name, string new_artist_name) {
            exec($"UPDATE `albums` SET Artist='{new_artist_name}' WHERE Artist='{artist_name}'");
        }
        public void deleteArtist(string name) {
            exec($"DELETE FROM `artists` WHERE Name='{name}'");
        }
        public List<Dictionary<string, object>> getSongs(string album) {
            return read($"SELECT Image, Name, Album FROM `songs` WHERE Album='{album}'");
        }
        public Byte[] getSong(string song) {
            return (Byte[]) read($"SELECT Song FROM `songs` WHERE Name=\"{song}\"")[0]["Song"];
        }
        public bool editSong(string initial_name, byte[] image, string new_name, string album, byte[] song) {
            try {
                exec($"UPDATE `songs` SET Image=0x{Convert.ToHexString(image)}, Name='{new_name}', Album='{album}', Song=0x{Convert.ToHexString(song)} WHERE Name='{initial_name}'");
                return true;
            }
            catch (MySqlException ex) {
                if (ex.Number == 1062)
                    errMsg = (string?) Application.Current.Resources["err_duplicate_name"];
                else
                    errMsg = $"{ex.Message}";
                return false;
            }
        }
        public bool addSong(byte[] image, string name, string album, byte[] song) {
            try {
                exec($"INSERT INTO `songs` (`Image`, `Name`, `Album`, `Song`) VALUES (0x{Convert.ToHexString(image)}, '{name}', '{album}', 0x{Convert.ToHexString(song)})");
                return true;
            }
            catch (MySqlException ex) {
                if (ex.Number == 1062)
                    errMsg = (string?) Application.Current.Resources["err_duplicate_name"];
                else
                    errMsg = $"{ex.Message}";
                return false;
            }
        }
        public void deleteSong(string name) {
            exec($"DELETE FROM `songs` WHERE Name='{name}'");
        }
        private void exec(string query) {
            MySqlConnection conn = new MySqlConnection(db_conn_info);
            conn.Open();
            MySqlCommand command = new MySqlCommand(query, conn);
            command.ExecuteNonQuery();
            conn.Close();
        }
        private List<Dictionary<string, object>> read(string query) {
            List<Dictionary<string, object>> table = new List<Dictionary<string, object>>();
            Dictionary<string, object> row;
            MySqlConnection conn = new MySqlConnection(db_conn_info);
            conn.Open();
            MySqlCommand command = new MySqlCommand(query, conn);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read()) {
                row = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; i++)
                    row[reader.GetName(i)] = reader.GetValue(i);
                table.Add(row);
            }
            conn.Close();
            return table;
        }
        public int artistsCount() {
            return Convert.ToInt32(read("SELECT COUNT(*) FROM `artists`")[0]["COUNT(*)"]);
        }
    }
}
