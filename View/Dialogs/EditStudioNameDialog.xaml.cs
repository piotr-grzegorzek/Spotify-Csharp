using System.Windows;
using MahApps.Metro.Controls;

namespace Studio {
    /// <summary>
    /// Interaction logic for StudioNameDialog.xaml
    /// </summary>
    public partial class EditStudioNameDialog : MetroWindow {
        public EditStudioNameDialog(string studio_name) {
            InitializeComponent();
            txtName.Text = studio_name;
        }
        private void okButton_Click(object sender, RoutedEventArgs e) {
            if (txtName.Text != "") {
                Database db = DatabaseFactory.init();
                db.updateStudioName(txtName.Text);
                DialogResult = true;
            }
        }
        private void cancelButton_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }
    }
}
