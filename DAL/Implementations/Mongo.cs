using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Windows;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace Studio.DAL.Implementations {
    internal class Mongo : Database {
        private string _connectionString;
        public string? errMsg { get; set; }
        public Mongo() {
            _connectionString = File.ReadAllText("Resources/Secret/mongo.txt");
        }
        public bool not_exists() {
            MongoClient client = new MongoClient(_connectionString);
            return !client.ListDatabaseNames().ToList().Contains("mongo_studio");
        }
        public void create() {
            // Import the config collection from file Resources/StandardDBs/mongo/config.json
            MongoClient client = new MongoClient(_connectionString);
            IMongoDatabase db = client.GetDatabase("mongo_studio");
            IMongoCollection<BsonDocument> collection = db.GetCollection<BsonDocument>("config");
            string json = File.ReadAllText("Resources/StandardDBs/mongo/config.json");
            BsonDocument bson = BsonSerializer.Deserialize<BsonDocument>(json);
            collection.InsertOne(bson);

            // Create the artists and users and songs collections
            db.CreateCollection("artists");
            db.CreateCollection("users");
            db.CreateCollection("songs");

            // Import the artists $jsonSchema from file Resources/StandardDBs/mongo/artists_schema.json
            json = File.ReadAllText("Resources/StandardDBs/mongo/artists_schema.json");
            bson = BsonSerializer.Deserialize<BsonDocument>(json);
            db.RunCommand<BsonDocument>(new BsonDocument {
                 { "collMod", "artists" },
                 { "validator", bson }
            });

            // Import the users $jsonSchema from file Resources/StandardDBs/mongo/users_schema.json
            json = File.ReadAllText("Resources/StandardDBs/mongo/users_schema.json");
            bson = BsonSerializer.Deserialize<BsonDocument>(json);
            db.RunCommand<BsonDocument>(new BsonDocument {
                { "collMod", "users" },
                { "validator", bson }
            });

            // Import the config $jsonSchema from file Resources/StandardDBs/mongo/config_schema.json
            json = File.ReadAllText("Resources/StandardDBs/mongo/config_schema.json");
            bson = BsonSerializer.Deserialize<BsonDocument>(json);
            db.RunCommand<BsonDocument>(new BsonDocument {
                { "collMod", "config" },
                { "validator", bson }
            });

            // Create a unique index on the Name field in the artists collection
            db.GetCollection<BsonDocument>("artists").Indexes.CreateOne(
                new CreateIndexModel<BsonDocument>(new BsonDocument {
                    { "Name", 1 }
                }, new CreateIndexOptions {
                    Unique = true
                }));

            // Create a unique index on the username field in the users collection
            db.GetCollection<BsonDocument>("users").Indexes.CreateOne(
                new CreateIndexModel<BsonDocument>(
                    Builders<BsonDocument>.IndexKeys.Ascending("username"),
                    new CreateIndexOptions { Unique = true }));
            // Create a unique and sparse index on the albums.Name field in the users collection (doesnt work when belonging to same user)
            db.GetCollection<BsonDocument>("users").Indexes.CreateOne(
                new CreateIndexModel<BsonDocument>(
                    Builders<BsonDocument>.IndexKeys.Ascending("albums.Name"),
                    new CreateIndexOptions { Unique = true, Sparse = true }));
            // Create a unique and sparse index on the albums.songs.Name field in the users collection (doesnt work when belonging to same user)
            db.GetCollection<BsonDocument>("users").Indexes.CreateOne(
                new CreateIndexModel<BsonDocument>(
                    Builders<BsonDocument>.IndexKeys.Ascending("albums.songs.Name"),
                    new CreateIndexOptions { Unique = true, Sparse = true }));
            //https://stackoverflow.com/questions/4435637/mongodb-unique-key-in-embedded-document
        }
        public bool adminExists() {
            MongoClient client = new MongoClient(_connectionString);
            IMongoDatabase db = client.GetDatabase("mongo_studio");
            IMongoCollection<BsonDocument> collection = db.GetCollection<BsonDocument>("users");

            return collection.CountDocuments(new BsonDocument { { "isAdmin", true } }) != 0;
        }
        public bool createUser(string username, string password, string lang, string layout, int isAdmin, int accountBalance) {
            var user = new BsonDocument
            {
                {"username", username},
                {"password", password},
                {"language", lang},
                {"layout", layout},
                {"isAdmin", Convert.ToBoolean(isAdmin)},
                {"accountBalance", accountBalance},
                {"albums", new BsonArray()}
            };

            MongoClient client = new MongoClient(_connectionString);
            IMongoDatabase db = client.GetDatabase("mongo_studio");
            IMongoCollection<BsonDocument> collection = db.GetCollection<BsonDocument>("users");

            try {
                collection.InsertOne(user);
                return true;
            }
            catch (MongoWriteException e) {
                if (e.WriteError.Category == ServerErrorCategory.DuplicateKey)
                    errMsg = (string?) Application.Current.Resources["err_duplicate_name"];
                else
                    errMsg = $"{e.Message}";
                return false;
            }
        }
        public bool loginUser(string username, string password) {
            var client = new MongoClient(_connectionString);
            var db = client.GetDatabase("mongo_studio");
            var collection = db.GetCollection<BsonDocument>("users");
            var filter = Builders<BsonDocument>.Filter.Eq("username", username);
            var projection = Builders<BsonDocument>.Projection.Include("password");
            var result = collection.Find(filter).Project(projection).FirstOrDefault();
            return result != null && result["password"] == password;
        }
        public Dictionary<string, object> userInfo(string username) {
            MongoClient client = new MongoClient(_connectionString);
            IMongoDatabase db = client.GetDatabase("mongo_studio");
            IMongoCollection<BsonDocument> collection = db.GetCollection<BsonDocument>("users");

            var filter = Builders<BsonDocument>.Filter.Eq("username", username);
            var projection = Builders<BsonDocument>.Projection
                .Include("username")
                .Include("language")
                .Include("layout")
                .Include("isAdmin")
                .Include("accountBalance");

            var user = collection.Find(filter).Project<BsonDocument>(projection).FirstOrDefault();

            return new Dictionary<string, object>
            {
                {"username", user["username"].AsString},
                {"language", user["language"].AsString},
                {"layout", user["layout"].AsString},
                {"isAdmin", Convert.ToInt32(user["isAdmin"].AsBoolean)},
                {"accountBalance", user["accountBalance"].AsInt32}
            };
        }
        public byte[] getLogo() {
            MongoClient client = new MongoClient(_connectionString);
            IMongoDatabase db = client.GetDatabase("mongo_studio");
            IMongoCollection<BsonDocument> collection = db.GetCollection<BsonDocument>("config");
            var filter = Builders<BsonDocument>.Filter.Empty;
            var projection = Builders<BsonDocument>.Projection.Include("logo").Exclude("_id");

            var config = collection.Find(filter).Project(projection).FirstOrDefault();
            return config["logo"].AsByteArray;
        }
        public string getStudioName() {
            MongoClient client = new MongoClient(_connectionString);
            IMongoDatabase db = client.GetDatabase("mongo_studio");
            IMongoCollection<BsonDocument> collection = db.GetCollection<BsonDocument>("config");
            return collection.Find(Builders<BsonDocument>.Filter.Empty)
            .Project(Builders<BsonDocument>.Projection.Include("studioName"))
            .FirstOrDefault()["studioName"].AsString;
        }
        public List<Dictionary<string, object>> getAlbums() {
            var client = new MongoClient(_connectionString);
            var db = client.GetDatabase("mongo_studio");
            var userCollection = db.GetCollection<BsonDocument>("users");
            var artistCollection = db.GetCollection<BsonDocument>("artists");

            // Project the needed fields from user and artist collections
            var projection = Builders<BsonDocument>.Projection.Include("username")
                .Include("albums.Image")
                .Include("albums.Name")
                .Include("albums.Price")
                .Include("albums.Selling")
                .Include("albums.Artist");
            var users = userCollection.Aggregate().Project(projection).ToList();
            var artists = artistCollection.Aggregate().Project(Builders<BsonDocument>.Projection
                .Include("_id").Include("Name")).ToList();

            var albums = new List<Dictionary<string, object>>();
            foreach (var user in users) {
                foreach (var album in user["albums"].AsBsonArray) {
                    var albumInfo = new Dictionary<string, object>();
                    albumInfo.Add("Image", album["Image"].AsByteArray);
                    albumInfo.Add("Name", album["Name"].AsString);

                    // Find the artist by id
                    var artist = artists.FirstOrDefault(a => a["_id"] == album["Artist"].AsObjectId);
                    albumInfo.Add("Artist", artist["Name"].AsString);
                    albumInfo.Add("Owner", user["username"].AsString);
                    albumInfo.Add("Price", album["Price"].AsInt32);
                    albumInfo.Add("Selling", Convert.ToInt32(album["Selling"].AsBoolean));
                    albums.Add(albumInfo);
                }
            }
            return albums;
        }
        public void setLogo(byte[] img) {
            MongoClient client = new MongoClient(_connectionString);
            IMongoDatabase db = client.GetDatabase("mongo_studio");
            IMongoCollection<BsonDocument> collection = db.GetCollection<BsonDocument>("config");
            var filter = new BsonDocument();
            var update = Builders<BsonDocument>.Update.Set("logo", img);
            collection.UpdateOne(filter, update);
        }
        public void updateStudioName(string text) {
            MongoClient client = new MongoClient(_connectionString);
            IMongoDatabase db = client.GetDatabase("mongo_studio");
            IMongoCollection<BsonDocument> collection = db.GetCollection<BsonDocument>("config");

            var filter = new BsonDocument();
            var update = Builders<BsonDocument>.Update.Set("studioName", text);
            collection.UpdateOne(filter, update);
        }
        public void updateUserPreferences(string lang_choice, string layout_choice, string username) {
            MongoClient client = new MongoClient(_connectionString);
            IMongoDatabase db = client.GetDatabase("mongo_studio");
            IMongoCollection<BsonDocument> collection = db.GetCollection<BsonDocument>("users");
            var filter = Builders<BsonDocument>.Filter.Eq("username", username);
            var update = Builders<BsonDocument>.Update
                .Set("language", lang_choice)
                .Set("layout", layout_choice);

            collection.UpdateOne(filter, update);
        }
        public List<Dictionary<string, object>> getUsers() {
            MongoClient client = new MongoClient(_connectionString);
            IMongoDatabase db = client.GetDatabase("mongo_studio");
            IMongoCollection<BsonDocument> collection = db.GetCollection<BsonDocument>("users");

            var projection = Builders<BsonDocument>.Projection
                .Include("username")
                .Include("isAdmin")
                .Include("accountBalance");

            var users = collection.Find(new BsonDocument()).Project(projection).ToList();
            List<Dictionary<string, object>> usersList = new List<Dictionary<string, object>>();

            foreach (var user in users) {
                Dictionary<string, object> userInfo = new Dictionary<string, object>();
                userInfo.Add("username", user["username"].AsString);
                userInfo.Add("isAdmin", user["isAdmin"].AsBoolean ? 1 : 0);
                userInfo.Add("accountBalance", user["accountBalance"].AsInt32);
                usersList.Add(userInfo);
            }

            return usersList;
        }
        public bool editUser(string initialName, string newUsername, int permission, int balance) {
            if (permission == 0 && getAdminCount() == 1 && isAdmin(initialName)) {
                errMsg = Application.Current.Resources["err_last_admin"] as string;
                return false;
            }
            try {
                MongoClient client = new MongoClient(_connectionString);
                IMongoDatabase db = client.GetDatabase("mongo_studio");
                IMongoCollection<BsonDocument> collection = db.GetCollection<BsonDocument>("users");

                var filter = Builders<BsonDocument>.Filter.Eq("username", initialName);
                var update = Builders<BsonDocument>.Update.Set("username", newUsername)
                                                          .Set("isAdmin", permission == 1)
                                                          .Set("accountBalance", balance);

                collection.UpdateOne(filter, update);
                return true;
            }
            catch (MongoWriteException e) {
                if (e.WriteError.Category == ServerErrorCategory.DuplicateKey)
                    errMsg = Application.Current.Resources["err_duplicate_name"] as string;
                else
                    errMsg = e.Message;

                return false;
            }
        }
        private long getAdminCount() {
            MongoClient client = new MongoClient(_connectionString);
            IMongoDatabase db = client.GetDatabase("mongo_studio");
            IMongoCollection<BsonDocument> collection = db.GetCollection<BsonDocument>("users");
            var filter = Builders<BsonDocument>.Filter.Eq("isAdmin", true);
            return collection.CountDocuments(filter);
        }
        private bool isAdmin(string username) {
            var client = new MongoClient(_connectionString);
            var db = client.GetDatabase("mongo_studio");
            var collection = db.GetCollection<BsonDocument>("users");
            var filter = Builders<BsonDocument>.Filter.Eq("username", username);
            var projection = Builders<BsonDocument>.Projection.Include("isAdmin");
            var user = collection.Find(filter).Project<BsonDocument>(projection).FirstOrDefault();
            return user?["isAdmin"].AsBoolean ?? false;
        }
        public List<Dictionary<string, object>> getUserAlbums(string username) {
            var client = new MongoClient(_connectionString);
            var db = client.GetDatabase("mongo_studio");
            var usersCollection = db.GetCollection<BsonDocument>("users");
            var artistsCollection = db.GetCollection<BsonDocument>("artists");

            var filter = Builders<BsonDocument>.Filter.Eq("username", username);
            var projection = Builders<BsonDocument>.Projection.Include("username")
                .Include("albums.Image")
                .Include("albums.Name")
                .Include("albums.Price")
                .Include("albums.Selling")
                .Include("albums.Artist");
            var user = usersCollection.Find(filter)
                                      .Project<BsonDocument>(projection).FirstOrDefault();

            var artistIds = user["albums"].AsBsonArray
                                          .Select(album => album["Artist"].AsObjectId)
                                          .ToList();

            var artistFilter = Builders<BsonDocument>.Filter.In("_id", artistIds);
            var artistProjection = Builders<BsonDocument>.Projection.Include("Name");
            var artists = artistsCollection.Find(artistFilter)
                                           .Project(artistProjection)
                                           .ToList();

            var artistMap = artists.ToDictionary(artist => artist["_id"].AsObjectId,
                                                 artist => artist["Name"].AsString);

            var albums = new List<Dictionary<string, object>>();
            foreach (var album in user["albums"].AsBsonArray) {
                var albumInfo = new Dictionary<string, object> {
                    ["Image"] = album["Image"].AsByteArray,
                    ["Name"] = album["Name"].AsString,
                    ["Owner"] = user["username"].AsString,
                    ["Price"] = album["Price"].AsInt32,
                    ["Selling"] = Convert.ToInt32(album["Selling"].AsBoolean)
                };

                albumInfo["Artist"] = artistMap[album["Artist"].AsObjectId];

                albums.Add(albumInfo);
            }

            return albums;
        }
        public List<Dictionary<string, object>> getSellingAlbums(string username) {
            var client = new MongoClient(_connectionString);
            var db = client.GetDatabase("mongo_studio");
            var usersCollection = db.GetCollection<BsonDocument>("users");
            var filter = Builders<BsonDocument>.Filter.And(
                Builders<BsonDocument>.Filter.Eq("albums.Selling", true),
                Builders<BsonDocument>.Filter.Ne("username", username));
            var projection = Builders<BsonDocument>.Projection.Include("username")
                .Include("albums.Image")
                .Include("albums.Name")
                .Include("albums.Price")
                .Include("albums.Selling")
                .Include("albums.Artist");
            var users = usersCollection.Find(filter).Project(projection).ToList();
            var albums = new List<Dictionary<string, object>>();

            var artistIds = new List<ObjectId>();
            foreach (var user in users) {
                var albumArray = user["albums"].AsBsonArray;
                foreach (var album in albumArray) {
                    var albumInfo = new Dictionary<string, object>
                    {
                    {"Image", album["Image"].AsByteArray},
                    {"Name", album["Name"].AsString},
                    {"Owner", user["username"].AsString},
                    {"Price", album["Price"].AsInt32},
                    {"Selling", Convert.ToInt32(album["Selling"].AsBoolean)},
                    {"ArtistId", album["Artist"].AsObjectId}
                };

                    albums.Add(albumInfo);
                    artistIds.Add(album["Artist"].AsObjectId);
                }
            }
            var artistCollection = db.GetCollection<BsonDocument>("artists");
            var artistFilter = Builders<BsonDocument>.Filter.In("_id", artistIds);
            var artistProjection = Builders<BsonDocument>.Projection.Include("Name");
            var artists = artistCollection.Find(artistFilter).Project(artistProjection).ToList();
            var artistDict = new Dictionary<ObjectId, string>();
            foreach (var artist in artists) {
                artistDict[artist["_id"].AsObjectId] = artist["Name"].AsString;
            }
            foreach (var album in albums) {
                album["Artist"] = artistDict[(ObjectId) album["ArtistId"]];
                album.Remove("ArtistId");
            }
            return albums;
        }
        public bool editAlbum(string initialName, byte[] image, string name, string artist, string newOwner, int price) {
            var client = new MongoClient(_connectionString);
            var db = client.GetDatabase("mongo_studio");
            // Check if the newOwner already has an album with the same name (index doesnt)
            IMongoCollection<BsonDocument> collection = db.GetCollection<BsonDocument>("users");
            var filter = Builders<BsonDocument>.Filter.And(
                Builders<BsonDocument>.Filter.Eq("username", newOwner),
                Builders<BsonDocument>.Filter.ElemMatch("albums", Builders<BsonDocument>.Filter.Eq("Name", name))
            );
            var projection = Builders<BsonDocument>.Projection.Include("albums.Name");
            var albumExists = collection.Find(filter).Project(projection).Any();

            if (albumExists && initialName != name) {
                errMsg = (string?) Application.Current.Resources["err_duplicate_name"];
                return false;
            }
            var artistsCollection = db.GetCollection<BsonDocument>("artists");
            var artistFilter = Builders<BsonDocument>.Filter.Eq("Name", artist);
            var artistProjection = Builders<BsonDocument>.Projection.Include("_id").Include("Name");
            var artistt = artistsCollection.Find(artistFilter).Project(artistProjection).FirstOrDefault();
            var artistId = artistt["_id"].AsObjectId;

            var usersCollection = db.GetCollection<BsonDocument>("users");
            var ownerFilter = Builders<BsonDocument>.Filter.Eq("albums.Name", initialName);
            var ownerProjection = Builders<BsonDocument>.Projection.Include("username").Include("albums.$");
            var owner = usersCollection.Find(ownerFilter).Project(ownerProjection).FirstOrDefault();
            var oldAlbum = owner["albums"].AsBsonArray[0];
            bool albumDeleted = false;

            try {
                if (owner["username"].AsString == newOwner) {
                    var update = Builders<BsonDocument>.Update
                        .Set("albums.$[elem].Image", image)
                        .Set("albums.$[elem].Name", name)
                        .Set("albums.$[elem].Artist", artistId)
                        .Set("albums.$[elem].Price", price);
                    var options = new UpdateOptions {
                        ArrayFilters = new List<ArrayFilterDefinition> { new BsonDocumentArrayFilterDefinition<BsonDocument>(new BsonDocument("elem.Name", initialName)) }
                    };
                    usersCollection.UpdateOne(ownerFilter, update, options);
                }
                else {
                    var deleteUpdate = Builders<BsonDocument>.Update.Pull("albums", new BsonDocument { { "Name", initialName } });
                    usersCollection.UpdateOne(ownerFilter, deleteUpdate);
                    albumDeleted = true;

                    var newOwnerFilter = Builders<BsonDocument>.Filter.Eq("username", newOwner);
                    var addUpdate = Builders<BsonDocument>.Update.Push("albums", new BsonDocument { { "Image", image }, { "Name", name }, { "Artist", artistId }, { "Price", price }, { "Selling", oldAlbum["Selling"] }, { "songs", oldAlbum["songs"] } });
                    usersCollection.UpdateOne(newOwnerFilter, addUpdate);
                }

                return true;
            }
            catch (MongoWriteException e) {
                if (e.WriteError.Category == ServerErrorCategory.DuplicateKey) {
                    errMsg = (string?) Application.Current.Resources["err_duplicate_name"];
                }
                else
                    errMsg = $"{e.Message}";
                if (albumDeleted) {
                    var oldOwnerFilter = Builders<BsonDocument>.Filter.Eq("username", owner["username"].AsString);
                    var addBackUpdate = Builders<BsonDocument>.Update.Push("albums", oldAlbum);
                    usersCollection.UpdateOne(oldOwnerFilter, addBackUpdate);
                }
                return false;
            }
        }
        public bool addAlbum(byte[] image, string name, string artist, string owner, int price) {
            //try to add the album to the user's albums, if it fails return false
            try {
                MongoClient client = new MongoClient(_connectionString);
                IMongoDatabase db = client.GetDatabase("mongo_studio");
                // Check if the owner already has an album with the same name (index doesnt)
                IMongoCollection<BsonDocument> collection = db.GetCollection<BsonDocument>("users");
                var filter = Builders<BsonDocument>.Filter.And(
                    Builders<BsonDocument>.Filter.Eq("username", owner),
                    Builders<BsonDocument>.Filter.ElemMatch("albums", Builders<BsonDocument>.Filter.Eq("Name", name))
                );
                var projection = Builders<BsonDocument>.Projection.Include("albums.Name");
                var albumExists = collection.Find(filter).Project(projection).Any();
                if (albumExists) {
                    errMsg = (string?) Application.Current.Resources["err_duplicate_name"];
                    return false;
                }

                // Get artist object id from artists collection
                collection = db.GetCollection<BsonDocument>("artists");
                filter = Builders<BsonDocument>.Filter.Eq("Name", artist);
                var artistProjection = Builders<BsonDocument>.Projection.Include("_id");
                var artistt = collection.Find(filter).Project(artistProjection).FirstOrDefault();
                var artistId = artistt["_id"].AsObjectId;


                // Add album to the owner's albums
                collection = db.GetCollection<BsonDocument>("users");
                filter = Builders<BsonDocument>.Filter.Eq("username", owner);
                var album = new BsonDocument {
                    {"Image", new BsonBinaryData(image)},
                    {"Name", name},
                    {"Artist", artistId},
                    {"Price", price},
                    {"Selling", false},
                    {"songs", new BsonArray() }
                };
                var update = Builders<BsonDocument>.Update.Push("albums", album);
                collection.UpdateOne(filter, update);
                return true;
            }
            catch (MongoWriteException e) {
                if (e.WriteError.Category == ServerErrorCategory.DuplicateKey)
                    errMsg = (string?) Application.Current.Resources["err_duplicate_name"];
                else
                    errMsg = $"{e.Message}";
                return false;
            }
        }
        public void deleteAlbum(string name) {
            MongoClient client = new MongoClient(_connectionString);
            IMongoDatabase db = client.GetDatabase("mongo_studio");
            IMongoCollection<BsonDocument> collection = db.GetCollection<BsonDocument>("users");

            var albumFilter = Builders<BsonDocument>.Filter.ElemMatch("albums", Builders<BsonDocument>.Filter.Eq("Name", name));
            var projection = Builders<BsonDocument>.Projection.Include("albums.songs.Name").Include("albums.Name");
            var document = collection.Find(albumFilter).Project(projection).ToList();

            var album = document[0]["albums"].AsBsonArray.FirstOrDefault(x => x["Name"].AsString == name);
            var songs = album["songs"].AsBsonArray;
            for (int i = 0; i < songs.Count; i++) {
                var song = songs[i].AsBsonDocument;
                var songName = song["Name"].AsString;
                deleteSong(songName);
            }

            var update = Builders<BsonDocument>.Update.Pull("albums", new BsonDocument { { "Name", name } });
            collection.UpdateOne(albumFilter, update);
        }
        public void buyAlbum(string user, string name) {
            MongoClient client = new MongoClient(_connectionString);
            IMongoDatabase db = client.GetDatabase("mongo_studio");
            IMongoCollection<BsonDocument> collection = db.GetCollection<BsonDocument>("users");

            // Get old owner
            var oldOwnerFilter = Builders<BsonDocument>.Filter.Eq("albums.Name", name);
            var oldOwner = collection.Find(oldOwnerFilter)
                         .Project(Builders<BsonDocument>.Projection.Include("username").Include("albums"))
                         .FirstOrDefault();

            // Get album
            var album = oldOwner["albums"].AsBsonArray.FirstOrDefault(x => x["Name"].AsString == name);

            // Delete album from old owner's albums
            var oldOwnerUpdate = Builders<BsonDocument>.Update.Pull("albums", album);
            collection.UpdateOne(oldOwnerFilter, oldOwnerUpdate);

            // Add album to new owner's albums
            var newOwnerFilter = Builders<BsonDocument>.Filter.Eq("username", user);
            var newOwnerUpdate = Builders<BsonDocument>.Update.Push("albums", album);
            collection.UpdateOne(newOwnerFilter, newOwnerUpdate);

            // Set selling parameter to false
            var sellingFilter = Builders<BsonDocument>.Filter.And(
                Builders<BsonDocument>.Filter.Eq("username", user),
                Builders<BsonDocument>.Filter.Eq("albums.Name", name)
            );
            var sellingUpdate = Builders<BsonDocument>.Update.Set("albums.$.Selling", false);
            var updateResult = collection.UpdateOne(sellingFilter, sellingUpdate);

            // Get the price of the album
            var price = album["Price"].AsInt32;

            // Reduce the price from the new owner's balance
            var reduceBalanceFilter = Builders<BsonDocument>.Filter.Eq("username", user);
            var reduceBalanceUpdate = Builders<BsonDocument>.Update.Inc("accountBalance", -price);
            collection.UpdateOne(reduceBalanceFilter, reduceBalanceUpdate);

            // Add the price to the old owner's balance
            var addBalanceFilter = Builders<BsonDocument>.Filter.Eq("username", oldOwner["username"].AsString);
            var addBalanceUpdate = Builders<BsonDocument>.Update.Inc("accountBalance", price);
            collection.UpdateOne(addBalanceFilter, addBalanceUpdate);
        }
        public void setAlbumSelling(string name, int v) {
            MongoClient client = new MongoClient(_connectionString);
            IMongoDatabase db = client.GetDatabase("mongo_studio");
            IMongoCollection<BsonDocument> collection = db.GetCollection<BsonDocument>("users");
            var filter = Builders<BsonDocument>.Filter.Eq("albums.Name", name);
            var update = Builders<BsonDocument>.Update.Set("albums.$[elem].Selling", Convert.ToBoolean(v));
            var options = new UpdateOptions { ArrayFilters = new List<ArrayFilterDefinition> { new BsonDocumentArrayFilterDefinition<BsonDocument>(new BsonDocument("elem.Name", name)) } };
            collection.UpdateOne(filter, update, options);
        }
        public List<Dictionary<string, object>> getArtists() {
            var client = new MongoClient(_connectionString);
            var db = client.GetDatabase("mongo_studio");
            var collection = db.GetCollection<BsonDocument>("artists");
            var artists = new List<Dictionary<string, object>>();

            var projection = Builders<BsonDocument>.Projection.Include("Image").Include("Name");

            foreach (var artist in collection.Find(new BsonDocument()).Project(projection).ToList()) {
                var artistInfo = new Dictionary<string, object>
                {
                    { "Image", artist["Image"].AsByteArray },
                    { "Name", artist["Name"].AsString }
                };

                artists.Add(artistInfo);
            }

            return artists;
        }
        public bool editArtist(string initialName, byte[] image, string newName) {
            try {
                MongoClient client = new MongoClient(_connectionString);
                IMongoDatabase db = client.GetDatabase("mongo_studio");
                IMongoCollection<BsonDocument> collection = db.GetCollection<BsonDocument>("artists");
                var filter = Builders<BsonDocument>.Filter.Eq("Name", initialName);
                var update = Builders<BsonDocument>.Update.Combine(
                    Builders<BsonDocument>.Update.Set("Image", new BsonBinaryData(image)),
                    Builders<BsonDocument>.Update.Set("Name", newName)
                );
                collection.UpdateOne(filter, update);
                return true;
            }
            catch (MongoWriteException e) {
                if (e.WriteError.Category == ServerErrorCategory.DuplicateKey)
                    errMsg = (string?) Application.Current.Resources["err_duplicate_name"];
                else
                    errMsg = $"{e.Message}";
                return false;
            }
        }
        public bool addArtist(byte[] image, string name) {
            try {
                MongoClient client = new MongoClient(_connectionString);
                IMongoDatabase db = client.GetDatabase("mongo_studio");
                IMongoCollection<BsonDocument> collection = db.GetCollection<BsonDocument>("artists");

                var document = new BsonDocument
                {
                    { "Image", new BsonBinaryData(image) },
                    { "Name", name }
                };

                collection.InsertOne(document);
                return true;
            }
            catch (MongoWriteException e) {
                if (e.WriteError.Category == ServerErrorCategory.DuplicateKey) {
                    errMsg = (string?) Application.Current.Resources["err_duplicate_name"];
                }
                else {
                    errMsg = $"{e.Message}";
                }

                return false;
            }
        }
        public bool artistGotAlbum(string name) {
            var client = new MongoClient(_connectionString);
            var db = client.GetDatabase("mongo_studio");
            var artistsCollection = db.GetCollection<BsonDocument>("artists");
            var usersCollection = db.GetCollection<BsonDocument>("users");
            var artistFilter = Builders<BsonDocument>.Filter.Eq("Name", name);
            var artist = artistsCollection.Find(artistFilter).Project(Builders<BsonDocument>.Projection.Include("_id")).FirstOrDefault();
            if (artist == null)
                return false;

            var artistId = artist["_id"].AsObjectId;
            var albumFilter = Builders<BsonDocument>.Filter.ElemMatch("albums", Builders<BsonDocument>.Filter.Eq("Artist", artistId));
            var artistAlbums = usersCollection.Find(albumFilter).Project(Builders<BsonDocument>.Projection.Exclude("_id")).Limit(1).FirstOrDefault();

            return artistAlbums != null;
        }
        public void updateAlbumsArtist(string artistName, string newArtistName) {
            MongoClient client = new MongoClient(_connectionString);
            IMongoDatabase db = client.GetDatabase("mongo_studio");
            IMongoCollection<BsonDocument> collection = db.GetCollection<BsonDocument>("artists");
            var artistFilter = Builders<BsonDocument>.Filter.Eq("Name", artistName);
            var artist = collection.Find(artistFilter).FirstOrDefault();
            var artistId = artist["_id"].AsObjectId;

            artistFilter = Builders<BsonDocument>.Filter.Eq("Name", newArtistName);
            artist = collection.Find(artistFilter).FirstOrDefault();
            var newArtistId = artist["_id"].AsObjectId;

            collection = db.GetCollection<BsonDocument>("users");
            var albumFilter = Builders<BsonDocument>.Filter.Eq("albums.Artist", artistId);
            var update = Builders<BsonDocument>.Update.Set("albums.$[elem].Artist", newArtistId);
            var options = new UpdateOptions { ArrayFilters = new List<ArrayFilterDefinition> { new BsonDocumentArrayFilterDefinition<BsonDocument>(new BsonDocument("elem.Artist", artistId)) } };
            collection.UpdateMany(albumFilter, update, options);
        }
        public void deleteArtist(string name) {
            MongoClient client = new MongoClient(_connectionString);
            IMongoDatabase db = client.GetDatabase("mongo_studio");
            IMongoCollection<BsonDocument> collection = db.GetCollection<BsonDocument>("artists");
            var filter = Builders<BsonDocument>.Filter.Eq("Name", name);
            collection.DeleteOne(filter);
        }
        public List<Dictionary<string, object>> getSongs(string album) {
            var client = new MongoClient(_connectionString);
            var db = client.GetDatabase("mongo_studio");
            var collection = db.GetCollection<BsonDocument>("users");

            var filter = Builders<BsonDocument>.Filter.ElemMatch("albums", Builders<BsonDocument>.Filter.Eq("Name", album));
            var projection = Builders<BsonDocument>.Projection.Include("albums.Name").Include("albums.songs.Image").Include("albums.songs.Name");

            var user = collection.Find(filter).Project(projection).FirstOrDefault();

            var albumObj = user["albums"].AsBsonArray.First(x => x["Name"].AsString == album);
            var songs = albumObj["songs"].AsBsonArray;

            var songsList = new List<Dictionary<string, object>>();
            foreach (var song in songs) {
                var songDict = new Dictionary<string, object>
                {
                    {"Image", song["Image"].AsByteArray},
                    {"Name", song["Name"].AsString},
                    {"Album", album}
                };
                songsList.Add(songDict);
            }

            return songsList;
        }
        public byte[] getSong(string song) {
            var client = new MongoClient(_connectionString);
            var database = client.GetDatabase("mongo_studio");
            var collection = database.GetCollection<BsonDocument>("users");

            var filter = Builders<BsonDocument>.Filter.ElemMatch("albums.songs", Builders<BsonDocument>.Filter.Eq("Name", song));
            var projection = Builders<BsonDocument>.Projection.Include("albums.songs.Name").Include("albums.songs.Song");

            var user = collection.Find(filter).Project(projection).FirstOrDefault();

            var albums = user["albums"].AsBsonArray;
            var album = albums.FirstOrDefault(x => x["songs"].AsBsonArray.Any(y => y["Name"].AsString == song));

            var songs = album["songs"].AsBsonArray;
            var songData = songs.FirstOrDefault(x => x["Name"].AsString == song);
            var songId = songData["Song"].AsObjectId;

            collection = database.GetCollection<BsonDocument>("songs");
            filter = Builders<BsonDocument>.Filter.Eq("_id", songId);
            projection = Builders<BsonDocument>.Projection.Include("Song");
            var song_bin = collection.Find(filter).Project(projection).FirstOrDefault();

            return song_bin["Song"].AsByteArray;
        }
        public bool addSong(byte[] image, string name, string album, byte[] song) {
            var client = new MongoClient(_connectionString);
            var db = client.GetDatabase("mongo_studio");
            var collection = db.GetCollection<BsonDocument>("users");
            var filter = Builders<BsonDocument>.Filter.ElemMatch("albums.songs", Builders<BsonDocument>.Filter.Eq("Name", name));
            var songExists = collection.Find(filter).Any();
            if (songExists) {
                errMsg = (string?) Application.Current.Resources["err_duplicate_name"];
                return false;
            }

            var id = ObjectId.GenerateNewId();
            filter = Builders<BsonDocument>.Filter.Eq("albums.Name", album);
            var update = Builders<BsonDocument>.Update.Push("albums.$[elem].songs", new BsonDocument
            {
                { "Image", new BsonBinaryData(image) },
                { "Name", name },
                { "Song", id }
            });
            var options = new UpdateOptions { ArrayFilters = new List<ArrayFilterDefinition> { new BsonDocumentArrayFilterDefinition<BsonDocument>(new BsonDocument("elem.Name", album)) } };

            try {
                collection.UpdateOne(filter, update, options);
                var songDocument = new BsonDocument
                {
                    { "_id", id },
                    { "Song", new BsonBinaryData(song)}
                };
                var songsCollection = db.GetCollection<BsonDocument>("songs");
                songsCollection.InsertOne(songDocument);
                return true;
            }
            catch (MongoWriteException e) {
                if (e.WriteError.Category == ServerErrorCategory.DuplicateKey) {
                    errMsg = (string?) Application.Current.Resources["err_duplicate_name"];
                }
                else {
                    errMsg = $"{e.Message}";
                }

                return false;
            }
        }
        public bool editSong(string initialName, byte[] image, string newName, string newAlbum, byte[] song) {
            bool songDeleted = false;

            var client = new MongoClient(_connectionString);
            var db = client.GetDatabase("mongo_studio");
            var usersCollection = db.GetCollection<BsonDocument>("users");

            var albumFilter = Builders<BsonDocument>.Filter.ElemMatch("albums", Builders<BsonDocument>.Filter.ElemMatch("songs", Builders<BsonDocument>.Filter.Eq("Name", initialName)));
            var projection = Builders<BsonDocument>.Projection.Include("albums").Include("username");
            var owner = usersCollection.Find(albumFilter).Project<BsonDocument>(projection).FirstOrDefault();

            var songExistsFilter = Builders<BsonDocument>.Filter.And(
                Builders<BsonDocument>.Filter.Eq("username", owner["username"].AsString),
                Builders<BsonDocument>.Filter.ElemMatch("albums.songs", Builders<BsonDocument>.Filter.Eq("Name", newName))
            );

            if (usersCollection.Find(songExistsFilter).Any() && initialName != newName) {
                errMsg = (string?) Application.Current.Resources["err_duplicate_name"];
                return false;
            }

            var album = owner["albums"].AsBsonArray.FirstOrDefault(x => x["songs"].AsBsonArray.Any(y => y["Name"].AsString == initialName));
            var songDoc = album["songs"].AsBsonArray.FirstOrDefault(x => x["Name"].AsString == initialName);
            string oldAlbumName = album["Name"].AsString;
            var id = ObjectId.GenerateNewId();

            try {
                if (oldAlbumName != newAlbum) {
                    var deleteFilter = Builders<BsonDocument>.Filter.ElemMatch("albums.songs", Builders<BsonDocument>.Filter.Eq("Name", initialName));
                    var pullUpdate = Builders<BsonDocument>.Update.PullFilter("albums.$[].songs", Builders<BsonDocument>.Filter.Eq("Name", initialName));
                    usersCollection.UpdateOne(deleteFilter, pullUpdate);
                    songDeleted = true;

                    var insertFilter = Builders<BsonDocument>.Filter.Eq("albums.Name", newAlbum);
                    var pushUpdate = Builders<BsonDocument>.Update.Push("albums.$[elem].songs", new BsonDocument
                    {
                        { "Image", new BsonBinaryData(image) },
                        { "Name", newName },
                        { "Song", id }
                    });
                    var options = new UpdateOptions { ArrayFilters = new List<ArrayFilterDefinition> { new BsonDocumentArrayFilterDefinition<BsonDocument>(new BsonDocument("elem.Name", newAlbum)) } };
                    usersCollection.UpdateOne(insertFilter, pushUpdate, options);
                }
                else {
                    var update = Builders<BsonDocument>.Update
                    .Set("albums.$[album].songs.$[song].Image", image)
                    .Set("albums.$[album].songs.$[song].Name", newName)
                    .Set("albums.$[album].songs.$[song].Song", id);

                    var options = new UpdateOptions {
                        ArrayFilters = new List<ArrayFilterDefinition>{
                            new BsonDocumentArrayFilterDefinition<BsonDocument>(new BsonDocument("album.Name", album["Name"].AsString)),
                            new BsonDocumentArrayFilterDefinition<BsonDocument>(new BsonDocument("song.Name", initialName))
                        }
                    };

                    usersCollection.UpdateOne(albumFilter, update, options);
                }

                var songDocument = new BsonDocument
                {
                    { "_id", id },
                    { "Song", new BsonBinaryData(song)}
                };
                var songsCollection = db.GetCollection<BsonDocument>("songs");
                songsCollection.InsertOne(songDocument);

                var oldSongId = songDoc["Song"];
                var filter = Builders<BsonDocument>.Filter.Eq("_id", oldSongId);
                songsCollection.DeleteOne(filter);

                return true;
            }
            catch (MongoWriteException e) {
                if (e.WriteError.Category == ServerErrorCategory.DuplicateKey) {
                    errMsg = (string?) Application.Current.Resources["err_duplicate_name"];
                }
                else {
                    errMsg = $"{e.Message}";
                }
                if (songDeleted) {
                    var insertFilter = Builders<BsonDocument>.Filter.Eq("albums.Name", oldAlbumName);
                    var pushUpdate = Builders<BsonDocument>.Update.Push("albums.$[].songs", songDoc);
                    usersCollection.UpdateOne(insertFilter, pushUpdate);
                }

                return false;
            }
        }
        public void deleteSong(string name) {
            MongoClient client = new MongoClient(_connectionString);
            IMongoDatabase db = client.GetDatabase("mongo_studio");
            IMongoCollection<BsonDocument> collection = db.GetCollection<BsonDocument>("users");

            var albumFilter = Builders<BsonDocument>.Filter.ElemMatch("albums", Builders<BsonDocument>.Filter.ElemMatch("songs", Builders<BsonDocument>.Filter.Eq("Name", name)));
            var projection = Builders<BsonDocument>.Projection.Include("albums.songs.Song").Include("albums.songs.Name");
            var document = collection.Find(albumFilter).Project(projection).FirstOrDefault();
            var album = document["albums"].AsBsonArray.FirstOrDefault(x => x["songs"].AsBsonArray.Any(y => y["Name"].AsString == name));
            var songDoc = album["songs"].AsBsonArray.FirstOrDefault(x => x["Name"].AsString == name);
            var songId = songDoc["Song"].AsObjectId;

            var update = Builders<BsonDocument>.Update.PullFilter("albums.$[].songs", Builders<BsonDocument>.Filter.Eq("Name", name));
            collection.UpdateOne(albumFilter, update);

            collection = db.GetCollection<BsonDocument>("songs");
            albumFilter = Builders<BsonDocument>.Filter.Eq("_id", songId);
            collection.DeleteOne(albumFilter);
        }
        public int artistsCount() {
            var client = new MongoClient(_connectionString);
            var db = client.GetDatabase("mongo_studio");
            var collection = db.GetCollection<BsonDocument>("artists");
            return (int) collection.CountDocuments(new BsonDocument());
        }
    }
}