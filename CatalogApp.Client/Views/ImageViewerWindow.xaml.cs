using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CatalogApp.Client.Views
{
    public partial class ImageViewerWindow : Window
    {
        private readonly ImageSource _image;

        public ImageViewerWindow(ImageSource image)
        {
            InitializeComponent();

            _image = image ?? throw new ArgumentNullException(nameof(image));
            FullImage.Source = _image;

            // Сделаем владельцем главное окно (если не задано извне)
            if (Owner == null && Application.Current?.MainWindow != this)
            {
                Owner = Application.Current?.MainWindow;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Рабочая область экрана (без панели задач)
            var work = SystemParameters.WorkArea;

            // Отступы, чтобы окно не прилегало вплотную к краям экрана
            const double margin = 40.0;
            var maxAvailableWidth = Math.Max(200, work.Width - margin);
            var maxAvailableHeight = Math.Max(200, work.Height - margin);

            // Попробуем получить натуральный размер изображения (в пикселях)
            double imgPixelWidth = 0;
            double imgPixelHeight = 0;

            if (_image is BitmapSource bmp)
            {
                imgPixelWidth = bmp.PixelWidth;
                imgPixelHeight = bmp.PixelHeight;

                // Учитываем DPI: если DPI != 96, приводим к логическим единицам WPF
                var dpiX = bmp.DpiX > 0 ? bmp.DpiX : 96.0;
                var dpiY = bmp.DpiY > 0 ? bmp.DpiY : 96.0;
                imgPixelWidth = imgPixelWidth * (96.0 / dpiX);
                imgPixelHeight = imgPixelHeight * (96.0 / dpiY);
            }
            else
            {
                // Если не BitmapSource, используем текущ отображаемый размер как запасной вариант
                imgPixelWidth = FullImage.ActualWidth;
                imgPixelHeight = FullImage.ActualHeight;
            }

            // Если размеры не определены — используем разумные дефолты
            if (double.IsNaN(imgPixelWidth) || imgPixelWidth <= 0) imgPixelWidth = Math.Min(800, maxAvailableWidth);
            if (double.IsNaN(imgPixelHeight) || imgPixelHeight <= 0) imgPixelHeight = Math.Min(600, maxAvailableHeight);

            // Вычисляем коэффициент масштабирования, чтобы изображение поместилось в доступную область
            var scale = Math.Min(maxAvailableWidth / imgPixelWidth, maxAvailableHeight / imgPixelHeight);

            // Если изображение меньше доступной области, не увеличиваем его (scale <= 1)
            if (scale > 1.0) scale = 1.0;

            // Итоговые размеры окна (учитываем отступы рамки и padding)
            // Оценим дополнительные размеры окна (хром, рамки). Здесь даём небольшой запас.
            const double chromeWidthEstimate = 32;  // приблизительная ширина рамки + отступы
            const double chromeHeightEstimate = 48; // приблизительная высота заголовка + рамки + отступы

            var desiredContentWidth = Math.Max(200, imgPixelWidth * scale);
            var desiredContentHeight = Math.Max(200, imgPixelHeight * scale);

            // Устанавливаем размеры окна так, чтобы оно не превышало рабочую область
            this.Width = Math.Min(desiredContentWidth + chromeWidthEstimate, work.Width - 20);
            this.Height = Math.Min(desiredContentHeight + chromeHeightEstimate, work.Height - 20);

            // Если после установки размеров ScrollViewer показывает скроллы — это нормально.
            // Центрируем относительно владельца
            if (Owner != null)
            {
                this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                this.Left = Owner.Left + (Owner.Width - this.Width) / 2;
                this.Top = Owner.Top + (Owner.Height - this.Height) / 2;
            }
            else
            {
                this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }

            // Убедимся, что ScrollViewer не блокирует взаимодействие, если изображение помещается
            PART_ScrollViewer.HorizontalScrollBarVisibility = (imgPixelWidth * scale > this.Width - chromeWidthEstimate) ? System.Windows.Controls.ScrollBarVisibility.Auto : System.Windows.Controls.ScrollBarVisibility.Disabled;
            PART_ScrollViewer.VerticalScrollBarVisibility = (imgPixelHeight * scale > this.Height - chromeHeightEstimate) ? System.Windows.Controls.ScrollBarVisibility.Auto : System.Windows.Controls.ScrollBarVisibility.Disabled;
        }

        // Закрыть по Esc
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) Close();
        }

        // Закрыть по правому клику
        private void Window_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            Close();
        }
    }
}
