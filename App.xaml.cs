using System.Windows;
using static Studio.Logic.AppManager;

namespace Studio {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        private void Application_Startup(object sender, StartupEventArgs e) {
            Database db = DatabaseFactory.init();
            if(db.not_exists())
                db.create();
            applyPreferences();
        }
    }
}