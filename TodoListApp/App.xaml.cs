using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using TodoListApp.Service;
using TodoListApp.ViewModel;

namespace TodoListApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            ItemManager itemManager = new ItemManager();
            MainViewModel mainVM = new MainViewModel(itemManager);
            mainVM.Initialize();

            MainWindow mainWindow = new MainWindow();
            mainWindow.DataContext = mainVM;
            MainWindow = mainWindow;
            mainWindow.Show();
        }
    }
}
