
using System.Data.SQLite;
using System.Collections.Generic;
using System.IO;

namespace Studio.DAL.Implementations {
    internal class SQLite : Database {
        public string? errMsg { get; set; }
        public bool not_exists() {
            return (!File.Exists("sqlite.db") ? true : false);
        }
        public void create() {
            File.Move("Resources/StandardDBs/standard_sqlite.db", "sqlite.db");
        }
        private void exec(string query) {
            SQLiteConnection conn = new SQLiteConnection("URI=file:sqlite.db");
            conn.Open();
            SQLiteCommand command = new SQLiteCommand(query, conn);
            command.ExecuteNonQuery();
            conn.Close();
        }
        private List<Dictionary<string, object>> read(string query) {
            List<Dictionary<string, object>> table = new List<Dictionary<string, object>>();
            Dictionary<string, object> row;
            SQLiteConnection conn = new SQLiteConnection("URI=file:sqlite.db");
            conn.Open();
            SQLiteCommand command = new SQLiteCommand(query, conn);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read()) {
                row = new Dictionary<string, object>();
                for(int i = 0; i < reader.FieldCount; i++)
                    row[reader.GetName(i)] = reader.GetValue(i);
                table.Add(row);
            }
            conn.Close();
            return table;
        }

        public bool adminExists() {
            throw new System.NotImplementedException();
        }

        public bool createUser(string username, string password, string lang, string layout, int isAdmin, int accountBalance) {
            throw new System.NotImplementedException();
        }

        public bool loginUser(string username, string password) {
            throw new System.NotImplementedException();
        }

        public Dictionary<string, object> userInfo(string username) {
            throw new System.NotImplementedException();
        }

        public byte[] getLogo() {
            throw new System.NotImplementedException();
        }

        public void setLogo(byte[] img) {
            throw new System.NotImplementedException();
        }

        public string getStudioName() {
            throw new System.NotImplementedException();
        }

        public void updateStudioName(string text) {
            throw new System.NotImplementedException();
        }

        public void updateUserPreferences(string lang_choice, string layout_choice, string username) {
            throw new System.NotImplementedException();
        }

        public List<Dictionary<string, object>> getUsers() {
            throw new System.NotImplementedException();
        }

        public bool editUser(string initial_name, string username, int permission, int balance) {
            throw new System.NotImplementedException();
        }

        public List<Dictionary<string, object>> getAlbums() {
            throw new System.NotImplementedException();
        }

        public List<Dictionary<string, object>> getUserAlbums(string username) {
            throw new System.NotImplementedException();
        }

        public List<Dictionary<string, object>> getSellingAlbums(string username) {
            throw new System.NotImplementedException();
        }

        public bool editAlbum(string initial_name, byte[] image, string name, string artist, string owner, int price) {
            throw new System.NotImplementedException();
        }

        public bool addAlbum(byte[] image, string name, string artist, string owner, int price) {
            throw new System.NotImplementedException();
        }

        public void deleteAlbum(string name) {
            throw new System.NotImplementedException();
        }

        public void buyAlbum(string user, string name) {
            throw new System.NotImplementedException();
        }

        public void setAlbumSelling(string name, int v) {
            throw new System.NotImplementedException();
        }

        public List<Dictionary<string, object>> getArtists() {
            throw new System.NotImplementedException();
        }

        public bool editArtist(string initial_name, byte[] image, string new_name) {
            throw new System.NotImplementedException();
        }

        public bool addArtist(byte[] Image, string Name) {
            throw new System.NotImplementedException();
        }

        public bool artistGotAlbum(string name) {
            throw new System.NotImplementedException();
        }

        public void updateAlbumsArtist(string artist_name, string new_artist_name) {
            throw new System.NotImplementedException();
        }

        public void deleteArtist(string name) {
            throw new System.NotImplementedException();
        }

        public List<Dictionary<string, object>> getSongs(string album) {
            throw new System.NotImplementedException();
        }

        public byte[] getSong(string song) {
            throw new System.NotImplementedException();
        }

        public bool editSong(string initial_name, byte[] image, string new_name, string album, byte[] song) {
            throw new System.NotImplementedException();
        }

        public bool addSong(byte[] image, string name, string album, byte[] song) {
            throw new System.NotImplementedException();
        }

        public void deleteSong(string name) {
            throw new System.NotImplementedException();
        }

        public int artistsCount() {
            throw new System.NotImplementedException();
        }
    }
}