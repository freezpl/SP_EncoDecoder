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
        bool isEncrypting;
        public bool IsEncrypting
        {
            get { return isEncrypting; }
            set
            {
                isEncrypting = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsEncrypting)));
            }
        }

        bool isDescripting;
        public bool IsDescripting
        {
            get { return isDescripting; }
            set
            {
                isDescripting = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsDescripting)));
            }
        }

        FileStream fs;
        int startPoint;

        int partSize;
        public int PartSize
        {
            get { return partSize; }
            set
            {
                partSize = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PartSize)));
            }
        }

        string pass;
        public string Pass
        {
            get { return pass; }
            set
            {
                pass = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Pass)));
            }
        }

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
            PartSize = 4096;
            Pass = "abc";
            ProgMax = 100;
            ProgVal = 0;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        //commands
        AppCommand encryptCmd;
        public AppCommand EncryptCmd
        {
            get
            {
                return encryptCmd ?? (encryptCmd = new AppCommand((o) =>
                {
                    if (isEncrypting)
                    {
                        MessageBox.Show("Encripting is alredy running", "Warning!", MessageBoxButton.OK, MessageBoxImage.None);
                        return;
                    }
                    if (isDescripting)
                    {
                        MessageBox.Show("Now running description!\nWait to finish or abort description first!",
                            "Warning!", MessageBoxButton.OK, MessageBoxImage.None);
                        return;
                    }

                    OpenFileDialog ofd = new OpenFileDialog();
                    if (ofd.ShowDialog() != true)
                        return;

                    Task t = new Task(() => {

                        try
                        {
                            using (fs = new FileStream(ofd.FileName, FileMode.Open, FileAccess.ReadWrite))
                            {
                                long size = (fs.Length - startPoint < PartSize) ? fs.Length - startPoint : PartSize;
                                byte[] buffer = new byte[size];
                                fs.Read(buffer, startPoint, buffer.Length);

                                int cursor = 0;
                                for (int i = 0; i < buffer.Length; i++)
                                {
                                    buffer[i] ^= Convert.ToByte(Pass[cursor]);
                                    //cursor++;
                                    //if (cursor >= Pass.Length)
                                    //    cursor = 0;
                                }
                                fs.Seek(startPoint, SeekOrigin.Begin);
                                fs.Write(buffer, startPoint, buffer.Length);
                            }
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    });

                    t.Start();
     
                }));
            }
        }

        void Encrypting()
        {
        }
    }
}