using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace CatalogApp.Client.Resources
{
    public static class PasswordBoxHelper
    {
        public static readonly DependencyProperty BoundPasswordProperty =
            DependencyProperty.RegisterAttached(
                "BoundPassword",
                typeof(string),
                typeof(PasswordBoxHelper),
                new FrameworkPropertyMetadata(string.Empty,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnBoundPasswordChanged));

        public static string GetBoundPassword(DependencyObject obj)
            => (string)obj.GetValue(BoundPasswordProperty);

        public static void SetBoundPassword(DependencyObject obj, string value)
            => obj.SetValue(BoundPasswordProperty, value);



        private static void OnBoundPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PasswordBox pb)
            {
                // временно отключаем обработчик, чтобы избежать рекурсии
                pb.PasswordChanged -= HandlePasswordChanged;

                var newPassword = (string)e.NewValue ?? string.Empty;
                if (pb.Password != newPassword)
                    pb.Password = newPassword;

                // обновляем флаг HasText
                SetHasText(pb, pb.SecurePassword.Length > 0);

                pb.PasswordChanged += HandlePasswordChanged;
            }
        }

        private static void HandlePasswordChanged(object sender, RoutedEventArgs e)
        {
            var pb = (PasswordBox)sender;
            SetBoundPassword(pb, pb.Password);
            SetHasText(pb, pb.SecurePassword.Length > 0);
        }
        public static readonly DependencyProperty HasTextProperty =
            DependencyProperty.RegisterAttached(
                "HasText",
                typeof(bool),
                typeof(PasswordBoxHelper),
                new FrameworkPropertyMetadata(false));

        public static bool GetHasText(DependencyObject obj) =>
            (bool)obj.GetValue(HasTextProperty);

        public static void SetHasText(DependencyObject obj, bool value) =>
            obj.SetValue(HasTextProperty, value);

        public static readonly DependencyProperty AttachProperty =
            DependencyProperty.RegisterAttached(
                "Attach",
                typeof(bool),
                typeof(PasswordBoxHelper),
                new PropertyMetadata(false, OnAttach));

        public static bool GetAttach(DependencyObject obj) =>
            (bool)obj.GetValue(AttachProperty);

        public static void SetAttach(DependencyObject obj, bool value) =>
            obj.SetValue(AttachProperty, value);

        private static void OnAttach(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PasswordBox passwordBox)
            {
                if ((bool)e.NewValue)
                {
                    passwordBox.PasswordChanged += HandlePasswordChanged;
                    // инициализация
                    SetHasText(passwordBox, passwordBox.SecurePassword.Length > 0);
                    SetBoundPassword(passwordBox, passwordBox.Password);
                }
                else
                {
                    passwordBox.PasswordChanged -= HandlePasswordChanged;
                }
            }
        }
    }
}
