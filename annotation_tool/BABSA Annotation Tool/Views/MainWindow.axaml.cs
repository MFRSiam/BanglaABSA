// --- START OF FILE MainWindow.axaml.cs ---
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Styling;
using BABSA_Annotation_Tool.ViewModels;

namespace BABSA_Annotation_Tool.Views
{
    public partial class MainWindow : Window
    {
        private bool _isDarkTheme = true;

        public MainWindow()
        {
            InitializeComponent();

            // Set initial theme based on system preference or default to dark
            UpdateTheme();
            if (DataContext == null && this.StorageProvider != null)
            {
                DataContext = new MainWindowViewModel(this.StorageProvider);
            }
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);

            // Set DataContext here when the window is attached to visual tree
            // and StorageProvider is guaranteed to be available
            if (DataContext == null && this.StorageProvider != null)
            {
                DataContext = new MainWindowViewModel(this.StorageProvider);
            }
        }

        private void OnThemeToggleClick(object? sender, RoutedEventArgs e)
        {
            _isDarkTheme = !_isDarkTheme;
            UpdateTheme();

            // Update button text
            if (sender is Button button)
            {
                button.Content = _isDarkTheme ? "🌙" : "☀️";
            }
        }

        private void UpdateTheme()
        {
            if (Application.Current != null)
            {
                Application.Current.RequestedThemeVariant = _isDarkTheme
                    ? ThemeVariant.Dark
                    : ThemeVariant.Light;
            }
        }
    }
}
// --- END OF FILE MainWindow.axaml.cs ---