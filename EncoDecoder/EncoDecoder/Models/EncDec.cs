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
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsWork)));
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
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsWork)));
            }
        }

        public bool IsWork
        {
            get { return (isEncrypting || IsAborting) ? true : false; }
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

        //register data
        const string PROGRAM_NAME = "Encodecoder";
        RegistryKey userKey;
        RegistryKey programKey;

        public EncDec()
        {
            Path = "Encript/Descriptor";
            PartSize = 100;
            Pass = "querty";
            ProgMax = 4096;
            ProgVal = 0;


            userKey = Registry.CurrentUser;
            if(userKey != null)
            {
                programKey =  userKey.OpenSubKey(PROGRAM_NAME);
                if (programKey != null)
                {
                    MessageBoxResult  res = MessageBox.Show("You have unfinished task! \nFile: " + 
                        programKey.GetValue(nameof(Path)) + "Continue?", "Unfinished task", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if(res == MessageBoxResult.Yes)
                    {
                        Path = programKey.GetValue(nameof(Path)).ToString();
                        startPoint = Convert.ToInt64(programKey.GetValue(nameof(startPoint)));

                        IsEncrypting = true;
                        Encrypting();
                    }
                programKey.Close();
                }
            }
        }

        public void ExitAndSave()
        {
            if (userKey == null)
                return;

            if (programKey == null)
                programKey = userKey.CreateSubKey(PROGRAM_NAME);

            programKey.SetValue(nameof(path), path);
            programKey.SetValue(nameof(startPoint), startPoint);
        }


        DateTime startTime;
        public string ProgTip
        {
            get
            {
                if(isEncrypting || IsAborting)
                {
                    TimeSpan tp = DateTime.Now - startTime; 
                    return (Convert.ToInt32(startPoint / tp.Seconds/1000)).ToString() + " Kbit/s";
                }
                else
                return "Not work";
            }
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
                    if (ofd.ShowDialog() == true)
                    {
                        Path = ofd.FileName;
                        startPoint = 0;
                        IsEncrypting = true;
                        Encrypting();
                    }
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
                    
                   
                    isAborting = true;
                    Encrypting();
                }));
            }
        }

        void Encrypting()
        {
            cts = new CancellationTokenSource();
            t = new Task(() =>
            {
                try
                {
                    using (fs = new FileStream(path, FileMode.Open, FileAccess.ReadWrite))
                    {
                        ProgMax = fs.Length;
                        long codeLenght;
                        startTime = DateTime.Now;
                        if (IsEncrypting)
                        {
                            codeLenght = fs.Length;
                        }
                        else
                        {
                            codeLenght = startPoint;
                            startPoint = 0;
                        }
                        while (startPoint < codeLenght)
                        {
                            if (cts.Token.IsCancellationRequested)
                            {
                                fs = null;
                                if (IsEncrypting)
                                    IsEncrypting = false;
                                return;
                            }

                            long size = (codeLenght - startPoint < PartSize) ? codeLenght - startPoint : PartSize;
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
                            ProgVal = (isEncrypting) ? startPoint : codeLenght - startPoint;
                            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ProgTip)));
                        }

                        MessageBox.Show("The work is complete!");
                        Path = "Encript/Descriptor";


                        if (userKey != null && programKey != null)
                            userKey.DeleteSubKey(PROGRAM_NAME, false);

                        if (IsEncrypting)
                            IsEncrypting = false;
                        else
                            IsAborting = false;

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
        }
        
        AppCommand speedCmd;
        public AppCommand SpeedCmd
        {
            get
            {
                return speedCmd ?? (speedCmd = new AppCommand((o) =>
                {
                   // MessageBox.Show("Speed");
                }));
            }
        }
    }
}