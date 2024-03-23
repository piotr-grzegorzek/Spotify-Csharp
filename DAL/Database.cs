using System.Collections.Generic;
using System;

namespace Studio {
    internal interface Database {
        string? errMsg { get; set; }
        bool not_exists();
        void create();
        bool adminExists();
        bool createUser(string username, string password, string lang, string layout, int isAdmin, int accountBalance);
        bool loginUser(string username, string password);
        Dictionary<string, object> userInfo(string username);
        Byte[] getLogo();
        void setLogo(Byte[] img);
        string getStudioName();
        void updateStudioName(string text);
        void updateUserPreferences(string lang_choice, string layout_choice, string username);
        List<Dictionary<string, object>> getUsers();
        bool editUser(string initial_name, string username, int permission, int balance);
        List<Dictionary<string, object>> getAlbums();
        List<Dictionary<string, object>> getUserAlbums(string username);
        List<Dictionary<string, object>> getSellingAlbums(string username);
        bool editAlbum(string initial_name, byte[] image, string name, string artist, string owner, int price);
        bool addAlbum(byte[] image, string name, string artist, string owner, int price);
        void deleteAlbum(string name);
        void buyAlbum(string user, string name);
        void setAlbumSelling(string name, int v);
        List<Dictionary<string, object>> getArtists();
        bool editArtist(string initial_name, Byte[] image, string new_name);
        bool addArtist(byte[] Image, string Name);
        bool artistGotAlbum(string name);
        void updateAlbumsArtist(string artist_name, string new_artist_name);
        void deleteArtist(string name);
        List<Dictionary<string, object>> getSongs(string album);
        Byte[] getSong(string song);
        bool editSong(string initial_name, byte[] image, string new_name, string album, byte[] song);
        bool addSong(byte[] image, string name, string album, byte[] song);
        void deleteSong(string name);
        int artistsCount();
    }
}