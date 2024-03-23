using Studio.Logic;
using System.IO;
using System.Windows;
using MahApps.Metro.Controls;

namespace Studio
{
    /// <summary>
    /// Interaction logic for TermsDialog.xaml
    /// </summary>
    public partial class TermsDialog : MetroWindow
    {
        public TermsDialog()
        {
            InitializeComponent();
            string path = new System.Uri($"Resources/Rules/{AppManager.userInfo["language"]}.txt", System.UriKind.Relative).ToString();
            if (File.Exists(path))
                rules.Text = File.ReadAllText(path);
            else
                rules.Text = (string?) Application.Current.Resources["err_rules_not_found"];
        }
        private void okButton_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }
    }
}
