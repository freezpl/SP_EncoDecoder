﻿using Microsoft.Win32;
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
        string path;
        public string Path
        {
            get { return path; }
            set
            {
                path = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Path)));
            }
        }

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

        bool isAborting;
        public bool IsAborting
        {
            get { return isAborting; }
            set
            {
                isAborting = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsAborting)));
            }
        }

        FileStream fs;
        Task t;
        long startPoint;
        CancellationTokenSource cts;


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
            Path = "";
            PartSize = 10;
            Pass = "abc";
            ProgMax = 4096;
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

                    OpenFileDialog ofd = new OpenFileDialog();
                    if (ofd.ShowDialog() != true)
                        return;

                    Path = ofd.FileName;
                    cts = new CancellationTokenSource();
                    t = new Task(() =>
                    {
                        try
                        {
                            using (fs = new FileStream(path, FileMode.Open, FileAccess.ReadWrite))
                            {
                                IsEncrypting = true;
                                ProgMax = fs.Length;
                                while (startPoint < fs.Length)
                                {
                                    if (cts.Token.IsCancellationRequested)
                                    {
                                        fs = null;
                                        MessageBox.Show(startPoint.ToString() );
                                        return;
                                    }

                                    long size = (fs.Length - startPoint < PartSize) ? fs.Length - startPoint : PartSize;
                                    byte[] buffer = new byte[size];
                                    fs.Read(buffer, 0, buffer.Length);

                                    int cursor = 0;
                                    for (int i = 0; i < buffer.Length; i++)
                                    {
                                        buffer[i] ^= Convert.ToByte(Pass[cursor]);
                                        cursor++;
                                        if (cursor >= Pass.Length)
                                            cursor = 0;
                                    }
                                    fs.Seek(startPoint, SeekOrigin.Begin);
                                    fs.Write(buffer, 0, buffer.Length);
                                    startPoint = fs.Position;
                                    ProgVal = startPoint;
                                    
                                }
                                IsEncrypting = false;
                                startPoint = 0;
                                Path = string.Empty;
                            }
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.Message + "\n" + e.StackTrace, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }, cts.Token);

                    t.Start();

                }));
            }
        }

        AppCommand abortCmd;
        public AppCommand AbortCmd
        {
            get
            {
                return abortCmd ?? (abortCmd = new AppCommand((o) =>
                {
                    if (!isEncrypting || path == string.Empty || cts == null || startPoint == 0)
                    {
                        MessageBox.Show("Encrypting not running!", "Warning!", MessageBoxButton.OK, MessageBoxImage.None);
                        return;
                    }

                    MessageBoxResult res = MessageBox.Show("Are you sure? \n Abort operation?", "Attention!",
                        MessageBoxButton.YesNo, MessageBoxImage.Exclamation);

                    if (res != MessageBoxResult.Yes)
                        return;

                    cts.Cancel();
                    t.Wait();

                    t = new Task(() =>
                    {
                        try
                        {
                            using (fs = new FileStream(Path, FileMode.Open, FileAccess.ReadWrite))
                            {
                                long length = startPoint;
                                ProgMax = fs.Length;
                                startPoint = 0;
                                while (startPoint < length)
                                {
                                    long size = (length - startPoint < PartSize) ? length - startPoint : PartSize;
                                    byte[] buffer = new byte[size];
                                    fs.Read(buffer, 0, buffer.Length);

                                    int cursor = 0;
                                    for (int i = 0; i < buffer.Length; i++)
                                    {
                                        buffer[i] ^= Convert.ToByte(Pass[cursor]);
                                        cursor++;
                                        if (cursor >= Pass.Length)
                                            cursor = 0;
                                    }
                                    fs.Seek(startPoint, SeekOrigin.Begin);
                                    fs.Write(buffer, 0, buffer.Length);
                                    startPoint = fs.Position;
                                    ProgVal = length - startPoint;
                                }
                                startPoint = 0;
                                MessageBox.Show("Restore complete!");
                            }
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.Message + "\n" + e.StackTrace, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    });
                    t.Start();
                }));
            }
        }

        public void ExitAndSave()
        {
            RegistryKey curUserKey = Registry.CurrentUser;
            RegistryKey encDeskKey = curUserKey.CreateSubKey("EncrDescr");
            encDeskKey.SetValue("path", path);
            encDeskKey.SetValue("start", startPoint);
        }

        void Encrypting()
        {
            t = new Task(() =>
            {

                try
                {
                    using (fs = new FileStream(Path, FileMode.Open, FileAccess.ReadWrite))
                    {
                        long length = startPoint;
                        ProgMax = length;
                        startPoint = 0;
                        while (startPoint < length)
                        {
                            long size = (length - startPoint < PartSize) ? length - startPoint : PartSize;
                            byte[] buffer = new byte[size];
                            fs.Read(buffer, 0, buffer.Length);

                            int cursor = 0;
                            for (int i = 0; i < buffer.Length; i++)
                            {
                                buffer[i] ^= Convert.ToByte(Pass[cursor]);
                                cursor++;
                                if (cursor >= Pass.Length)
                                    cursor = 0;
                            }
                            fs.Seek(startPoint, SeekOrigin.Begin);
                            fs.Write(buffer, 0, buffer.Length);
                            startPoint = fs.Position;
                            ProgVal = startPoint;
                        }
                        startPoint = 0;
                        MessageBox.Show("Restore complete!");
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message + "\n" + e.StackTrace, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            });

            t.Start();
        }


    }
}