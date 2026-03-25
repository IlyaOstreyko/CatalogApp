using System;
using System.Collections.Generic;
using System.Text;

namespace CatalogApp.Client.Services
{
    public interface IFileDialogService
    {
        string? OpenFileDialog(string filter);
    }
}
