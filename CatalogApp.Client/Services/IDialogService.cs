using System;
using System.Collections.Generic;
using System.Text;

namespace CatalogApp.Client.Services
{
    public interface IDialogService
    {
        bool ShowAddProductDialog();
        void ShowRegistrationWindow();
        void ShowLoginWindow();
        void ShowMainWindow();
    }
}
