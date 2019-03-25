using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace EncoDecoder.Models
{
    public class EncDec : INotifyPropertyChanged
    {
        public string Msg { get; set; }

        long progMax;
        public long ProgMax
        {
            get
            {
                return progMax;
            }

            set
            {
                progMax = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ProgMax)));
            }
        }
        long progVal;
        public long ProgVal
        {
            get
            {
                return progVal;
            }
            set
            {
                progVal = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ProgVal)));
            }
        }

        public EncDec()
        {
            OpenFileDialog od = new OpenFileDialog();
            if (od.ShowDialog() == true)
            {

                FileStream fs = File.OpenRead(od.FileName);

                byte[] arr = new byte[fs.Length];
                ProgMax = fs.Length;

                byte[] buffer = new byte[1];
                Thread th = new Thread(() =>
                {
                    for (int i = 0; i < fs.Length; i++)
                    {
                        fs.Read(buffer, 0, 1);
                        arr[i] = buffer[0];
                        ProgVal = i;
                    }

                    //string str = "";
                    //foreach (var i in arr)
                    //{
                    //    str += i.ToString() + " ";
                    //}
                    MessageBox.Show("ok!");
                });
                th.IsBackground = true;
                th.Start();

                //fs.Read(arr, 0, arr.Length);

                //string str = "";
                //foreach (var i in arr)
                //{
                //    str += i.ToString() + " ";
                //}
                //MessageBox.Show(str);
            }

            //algorythm
            //char ch = ' ';
            //char ch2 = 'q';
            //char res = Convert.ToChar(ch ^ ch2);
            //char ch3 = Convert.ToChar(res ^ ch2);   
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
