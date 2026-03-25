using CatalogApp.Client.Views;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace CatalogApp.Client.Services
{
    public class DialogService : IDialogService
    {
        private readonly IServiceProvider _sp;

        public DialogService(IServiceProvider sp)
        {
            _sp = sp;
        }

        public bool ShowAddProductDialog()
        {
            var dlg = (AddProductWindow)_sp.GetRequiredService(typeof(AddProductWindow));
            return dlg.ShowDialog() == true;
        }

        public void ShowRegistrationWindow()
        {
            var dlg = (RegistrationWindow)_sp.GetRequiredService(typeof(RegistrationWindow));
            dlg.Show();
            CloseWindowIfOpen<LoginWindow>();
        }

        public void ShowLoginWindow()
        {
            var dlg = (LoginWindow)_sp.GetRequiredService(typeof(LoginWindow));
            dlg.Show();
            CloseWindowIfOpen<RegistrationWindow>();
        }

        public void ShowMainWindow()
        {
            var dlg = (MainWindow)_sp.GetRequiredService(typeof(MainWindow));
            dlg.Show();
            CloseWindowIfOpen<LoginWindow>();
        }

        private void CloseWindowIfOpen<TWindow>() where TWindow : Window
        {
            foreach (Window w in Application.Current.Windows)
            {
                if (w is TWindow)
                {
                    w.Close();
                    break;
                }
            }
        }
    }
}
