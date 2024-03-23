using System.Windows;
using System.Windows.Input;
using MahApps.Metro.Controls;
using static Studio.Logic.AppManager;

namespace Studio {
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : MetroWindow {
        public LoginWindow() {
            InitializeComponent();
            shortcuts();
        }
        private void shortcuts() {
            //btnRegister_Click - alt r
            RoutedCommand register = new RoutedCommand();
            register.InputGestures.Add(new KeyGesture(Key.R, ModifierKeys.Alt));
            CommandBindings.Add(new CommandBinding(register, BtnRegister_Click));
        }
        private void BtnLogin_Click(object sender, RoutedEventArgs e) {
            string username = txtUsername.Text;
            string password = SHA1(SHA1(txtPassword.Password));
            Visibility adminControlsVisibility;
            Database db = DatabaseFactory.init();
            if (db.loginUser(username, password)) {
                userInfo = db.userInfo(username);
                viewMode = "";
                applyPreferences((string) userInfo["language"], (string) userInfo["layout"]);
                if ((int)userInfo["isAdmin"] == 1)
                    adminControlsVisibility = Visibility.Visible;
                else
                    adminControlsVisibility = Visibility.Collapsed;
                Application.Current.Resources["adminControlsVisibility"] = adminControlsVisibility;
                MainWindow main_window = new MainWindow();
                Close();
                main_window.Show();
            }
            else
                MessageBox.Show((string) Application.Current.Resources["bad_username_or_password"]);
        }
        private void BtnRegister_Click(object sender, RoutedEventArgs e) {
            RegisterWindow register_window = new RegisterWindow();
            Close();
            register_window.Show();
        }
    }
}