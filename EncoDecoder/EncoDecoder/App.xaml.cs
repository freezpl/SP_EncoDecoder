using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using QuickConverter;

namespace EncoDecoder
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            EquationTokenizer.AddNamespace(typeof(object));
            EquationTokenizer.AddNamespace(typeof(System.Windows.Visibility));

            //bool existed;
            //string id = Marshal.GetTypeLibGuidForAssembly(Assembly.GetExecutingAssembly()).ToString();
            //MessageBox.Show(id);
            //Mutex mutex = new Mutex(true, id, out existed);
            //if (existed == false)
            //{
            //    MessageBox.Show("App is already opened");
            //    Application.Current.Shutdown();
            //    return;
            //}

            base.OnStartup(e);
        }
    }
}
