using EncoDecoder.Models;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EncoDecoder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        EncDec ed;
        public MainWindow()
        {
            InitializeComponent();

            ed = new EncDec();
            DataContext = ed;
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (ed.IsEncrypting)
            {
                MessageBoxResult result = MessageBox.Show("Program is coding now!\nIf you close you can continue coding after next launch program. Close now?", 
                    "Warning", MessageBoxButton.YesNo);
                if (result != MessageBoxResult.Yes)
                {
                    e.Cancel = true;
                }
                else
                {
                    ed.ExitAndSave();
                }
            }
            if (ed.IsAborting)
            {
                MessageBoxResult result = MessageBox.Show("Now program aborting changes!\n If you close, your source file will be damagged? Close program?",
                    "Warning", MessageBoxButton.YesNo);
                if (result != MessageBoxResult.Yes)
                {
                    e.Cancel = true;
                }
            }
        }
    }
}
