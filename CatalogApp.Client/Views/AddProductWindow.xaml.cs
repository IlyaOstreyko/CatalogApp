using CatalogApp.Client.Services;
using CatalogApp.Client.ViewModels;
using Microsoft.Extensions.DependencyInjection;
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
    public partial class AddProductWindow : Window
    {
        public AddProductWindow(AddProductViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
    }
}
