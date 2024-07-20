using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace Vex
{
    /// <summary>
    /// Interaction logic for ProgressWindow.xaml
    /// </summary>
    public partial class ProgressWindow : Window
    {
        /// <summary>
        /// Whether we cancelled or not
        /// </summary>
        private bool HasCancelled = false;

        /// <summary>
        /// If we are complete or not
        /// </summary>
        private bool IsComplete = false;

        /// <summary>
        /// Initializes Progress Window
        /// </summary>
        public ProgressWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Sets Cancelled to true to update current task
        /// </summary>
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = !HasCancelled;
        }

        /// <summary>
        /// Closes Window on Cancel click
        /// </summary>
        private void CancelClick(object sender, RoutedEventArgs e)
        {
            if (!IsComplete)
            {
                HasCancelled = true;
            }
            else
            {
                Close();
            }
        }

        /// <summary>
        /// Closes Progress Window on Complete
        /// </summary>
        public void Complete()
        {
            ProgressBar.Value = ProgressBar.Maximum;
            HasCancelled = true;
            IsComplete = true;
            Message.Text = "Export Complete";
            OpenButton.Visibility = Visibility.Visible;
            CancelButton.Content = "Close";
            CancelButton.ToolTip = "Close the window";
        }

        public void SetProgressCount(double value)
        {
            // Invoke dispatcher to update UI
            Dispatcher.BeginInvoke(new Action(() =>
            {
                ProgressBar.Maximum = value;
                ProgressBar.Value = 0;
            }));
        }

        public void SetProgressMessage(string value)
        {
            // Invoke dispatcher to update UI
            Dispatcher.BeginInvoke(new Action(() =>
            {
                Message.Text = value;
            }));
        }

        /// <summary>
        /// Update Progress and checks for cancel
        /// </summary>
        public bool IncrementProgress()
        {
            // Invoke dispatcher to update UI
            Dispatcher.BeginInvoke(new Action(() =>
            {
                ProgressBar.Value++;
            }));

            // Return whether we've cancelled or not
            return HasCancelled;
        }

        private void OpenExportFolderClick(object sender, RoutedEventArgs e)
        {
            ProcessStartInfo startInfo =
                new() { Arguments = "exported_files", FileName = "explorer.exe" };

            Process.Start(startInfo);
            Close();
        }
    }
}
