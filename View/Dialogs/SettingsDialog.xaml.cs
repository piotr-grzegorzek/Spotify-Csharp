using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls;
using static Studio.Logic.AppManager;

namespace Studio {
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsDialog : MetroWindow {
        public SettingsDialog() {
            InitializeComponent();
            foreach(ComboBoxItem lang in cmbLang.Items) {
                if (lang.Tag.ToString() == userInfo["language"] as string) {
                    cmbLang.SelectedItem = lang;
                    break;
                }
            }
            foreach (ComboBoxItem layout in cmbLayout.Items) {
                if (layout.Tag.ToString() == userInfo["layout"] as string) {
                    cmbLayout.SelectedItem = layout;
                    break;
                }
            }
        }
        private void okButton_Click(object sender, RoutedEventArgs e) {
            string lang_choice = (cmbLang.SelectedItem as ComboBoxItem).Tag.ToString();
            string layout_choice = (cmbLayout.SelectedItem as ComboBoxItem).Tag.ToString();
            if (lang_choice != userInfo["language"] as string || layout_choice != userInfo["layout"] as string) {
                Database db = DatabaseFactory.init();
                db.updateUserPreferences(lang_choice, layout_choice, userInfo["username"] as string);
                userInfo["language"] = lang_choice;
                userInfo["layout"] = layout_choice;
                applyPreferences(lang_choice, layout_choice);
                DialogResult = true;
            }
            else
                DialogResult = false;
        }
        private void cancelButton_Click(object sender, RoutedEventArgs e) {
            DialogResult= false;
        }
    }
}