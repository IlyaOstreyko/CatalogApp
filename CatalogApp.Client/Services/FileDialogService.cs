using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Text;

namespace CatalogApp.Client.Services
{
    public class FileDialogService : IFileDialogService
    {
        public string? OpenFileDialog(string filter)
        {
            var dlg = new OpenFileDialog { Filter = filter };
            return dlg.ShowDialog() == true ? dlg.FileName : null;
        }
    }
}
