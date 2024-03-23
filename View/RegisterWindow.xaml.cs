using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MahApps.Metro.Controls;
using static Studio.Logic.AppManager;

namespace Studio {
    /// <summary>
    /// Interaction logic for RegisterWindow.xaml
    /// </summary>
    public partial class RegisterWindow : MetroWindow {
        public RegisterWindow() {
            InitializeComponent();
            foreach (ComboBoxItem lang in cmbLang.Items) {
                if (lang.Tag.ToString() == getSystemLang()) {
                    cmbLang.SelectedItem = lang;
                    break;
                }
            }
            foreach (ComboBoxItem layout in cmbLayout.Items) {
                if (layout.Tag.ToString() == getSystemLayout()) {
                    cmbLayout.SelectedItem = layout;
                    break;
                }
            }
            shortcuts();
        }
        private void shortcuts() {
            //btnLogin_Click - alt l
            RoutedCommand login = new RoutedCommand();
            login.InputGestures.Add(new KeyGesture(Key.L, ModifierKeys.Alt));
            CommandBindings.Add(new CommandBinding(login, BtnLogin_Click));
        }
        private void BtnSubmit_Click(object sender, RoutedEventArgs e) {
            string username = txtUsername.Text;
            string password = txtPassword.Password;
            string re_password = txtRePassword.Password;
            string lang = ((ComboBoxItem) cmbLang.SelectedItem).Tag.ToString();
            string layout = ((ComboBoxItem) cmbLayout.SelectedItem).Tag.ToString();
            if (valid_username(username)) {
                if (valid_password(password)) {
                    if (same(password, re_password)) {
                        password = SHA1(SHA1(password));
                        int isAdmin = 0;
                        int accountBalance = 100;
                        Database db = DatabaseFactory.init();
                        if (!db.adminExists())
                            isAdmin = 1;
                        if (db.createUser(username, password, lang, layout, isAdmin, accountBalance)) {
                            LoginWindow login_window = new LoginWindow();
                            Close();
                            login_window.Show();
                        }
                        else
                            MessageBox.Show(db.errMsg);
                    }
                    else
                        MessageBox.Show((string) Application.Current.Resources["diff_register_password"]);
                }
                else
                    MessageBox.Show((string) Application.Current.Resources["bad_register_password"]);
            }
            else
                MessageBox.Show((string) Application.Current.Resources["bad_register_username"]);
        }
        private bool valid_username(string username) => 
            (username.Length > 1 && username.Length < 33) ? true : false;
        private bool valid_password(string password) => 
            (password.Length > 7) ? true : false;
        private bool same(string password, string re_password) =>
            (password == re_password) ? true : false;
        private void BtnLogin_Click(object sender, RoutedEventArgs e) {
            LoginWindow login_window = new LoginWindow();
            Close();
            login_window.Show();
        }
    }
}