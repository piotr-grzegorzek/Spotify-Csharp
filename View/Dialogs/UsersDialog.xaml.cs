using MahApps.Metro.Controls;
using Studio.Logic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static Studio.Logic.AppManager;

namespace Studio {
    /// <summary>
    /// Interaction logic for UsersDialog.xaml
    /// </summary>
    public partial class UsersDialog : MetroWindow {
        private bool IsCellBeingEdited = false;
        string name;
        string initial_name;
        int permission;
        int balance;
        public UsersDialog() {
            InitializeComponent();
            ObservableCollection<User> users = new ObservableCollection<User>();
            
            //Get Users from Database
            Database db = DatabaseFactory.init();
            foreach (Dictionary<string, object> user in db.getUsers()) {
                users.Add(new User(
                    (string) user["username"],
                    (int) user["isAdmin"],
                    (int) user["accountBalance"]
                ));
            }
            userGrid.ItemsSource = users;
        }
        private void DataGrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e) {
            if (IsCellBeingEdited) {
                // Set the Cancel property to true
                e.Cancel = true;
            }
            else {
                IsCellBeingEdited = true;
                //Get the edited user
                User user = (User) e.Row.Item;
                name = user.Name;
                initial_name = name;
                permission = user.Permission;
                balance = user.Money;
            }
        }
        private void userGrid_CellEditEnding(object sender, System.Windows.Controls.DataGridCellEditEndingEventArgs e) {
            //Get editing element
            TextBox txt = (TextBox) e.EditingElement;
            //get editing column name
            string column = e.Column.Header.ToString();
            
            if (column == Application.Current.Resources["Username"]) {
                if (txt.Text == "") {
                    MessageBox.Show((string) Application.Current.Resources["err_no_name"]);
                    e.Cancel = true;
                    (sender as DataGrid).Dispatcher.BeginInvoke((Action) (() => { (sender as DataGrid).SelectedIndex = e.Row.GetIndex(); ((System.Windows.UIElement) e.EditingElement).Focus(); }));
                    return;
                }
                name = txt.Text;
            }
            else if (column == Application.Current.Resources["Permissions"]) {
                if (!int.TryParse(txt.Text, out permission) || (txt.Text != "0" && txt.Text != "1")) {
                    MessageBox.Show((string) Application.Current.Resources["err_invalid_permission"]);
                    e.Cancel = true;
                    (sender as DataGrid).Dispatcher.BeginInvoke((Action) (() => { (sender as DataGrid).SelectedIndex = e.Row.GetIndex(); ((System.Windows.UIElement) e.EditingElement).Focus(); }));
                    return;
                }    
            }
            else if (column == Application.Current.Resources["Account Balance"]) {
                if (!int.TryParse(txt.Text, out balance)) {
                    MessageBox.Show((string) Application.Current.Resources["err_invalid_balance"]);
                    e.Cancel = true;
                    (sender as DataGrid).Dispatcher.BeginInvoke((Action) (() => { (sender as DataGrid).SelectedIndex = e.Row.GetIndex(); ((System.Windows.UIElement) e.EditingElement).Focus(); }));
                    return;
                }
            }
            Database db = DatabaseFactory.init();
            if (!db.editUser(initial_name, name, permission, balance)) {
                MessageBox.Show(db.errMsg);
                e.Cancel = true;
                (sender as DataGrid).Dispatcher.BeginInvoke((Action) (() => { (sender as DataGrid).SelectedIndex = e.Row.GetIndex(); ((System.Windows.UIElement) e.EditingElement).Focus(); }));
                return;
            }
            //Update userInfo if changed user is the current user
            if (initial_name == (string)userInfo["username"]) {
                userInfo = db.userInfo(name);
                if ((int) userInfo["isAdmin"] == 0) {
                    //User is no longer admin, update resources and close the window
                    Application.Current.Resources["adminControlsVisibility"] = Visibility.Collapsed;
                }
            }

            if (e.EditAction == DataGridEditAction.Commit) {
                IsCellBeingEdited = false;
            }
        }
        //Disable tab and enter and esc
        private void userGrid_PreviewKeyDown_1(object sender, System.Windows.Input.KeyEventArgs e) {
            if (e.Key == Key.Tab || e.Key == Key.Enter || e.Key == Key.Escape)
                e.Handled = true;
        }
        private void okButton_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }

        private void userGrid_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if ((Visibility) Application.Current.Resources["adminControlsVisibility"] == Visibility.Collapsed)
                Close();
        }
    }
}