using System.Reflection;
using System.Windows;
using System.Windows.Input;
using Vex.Pages;

namespace Vex
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Title = $"Vex v{Assembly.GetExecutingAssembly().GetName().Version}";
            MainContentFrame.Navigate(new MainPage());
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Key == Key.P)
            {
                if (MainContentFrame.NavigationService.Content is MainPage mainPage)
                {
                    if (!mainPage.SearchBox.IsFocused)
                        mainPage.OpenPreviewWindow();
                }
            }
        }
    }
}
