using CatalogApp.Client.Services;
using CatalogApp.Client.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CatalogApp.Client.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow(MainViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
        private void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // Получаем ProductViewModel из DataContext элемента
            if (sender is Image img && img.DataContext is ProductViewModel pvm)
            {
                var source = pvm.ImageSource; // или свойство, которое у вас хранит ImageSource
                if (source == null)
                {
                    MessageBox.Show("Изображение ещё не загружено.", "Просмотр", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var viewer = new ImageViewerWindow(source)
                {
                    Owner = this
                };

                // Показываем модально, чтобы пользователь мог закрыть и вернуться
                viewer.ShowDialog();
            }
        }
    }
}
