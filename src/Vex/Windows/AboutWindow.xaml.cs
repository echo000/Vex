using System;
using System.Diagnostics;
using System.Windows;

namespace Vex
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
        }

        private void GithubButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://github.com/echo000/Vex") { UseShellExecute = true });
        }

        protected override void OnClosed(EventArgs e)
        {
            GithubButton.Click -= GithubButton_Click;
            base.OnClosed(e);
        }

        private void OkayButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
