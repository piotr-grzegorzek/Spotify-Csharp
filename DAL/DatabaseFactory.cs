using System.IO;
using System.Reflection;
using Studio.DAL.Implementations;

namespace Studio {
    static class DatabaseFactory {
        public static Database init() {
            string db_type = File.ReadAllText("Resources/StandardDBs/baza.txt");
            if (db_type == "mysql") {
                return new MySQL();
            }
            else if (db_type == "mongo") {
                return new Mongo();
            }
            else {
                return new MySQL();
            }
        }
    }
}