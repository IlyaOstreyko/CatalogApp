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
    public partial class RegistrationWindow : Window
    {
        public RegistrationWindow(RegistrationViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
    }
}
