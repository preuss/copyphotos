﻿using MediaDevices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;

namespace copyphotos
{
    class Program
    {
        public static string localPathRoot;
        public static string [] fileExtensions;
        public static List<string> phoneFileList = new List<string>();
        public static DateTime programStartTime;

        static void Main()
        {
            Console.Title = "Copy Photos v1.0 by Wiktor Kasz(c) 2020";
            programStartTime = DateTime.Now;
            try
            {
                ReadConfigFiles();
                CopyFromPhone();
                CopyFromRemovableStorage();
            }
            catch(Exception ex)
            { Console.WriteLine("Error! " + ex.Message.ToString()); }

            Thread.Sleep(500);
            Console.WriteLine("Closing in 3 seconds...");
            Thread.Sleep(3000);
        }

        static void CopyFromRemovableStorage()
        {

            Console.WriteLine("--------------------------------------------------------------");
            Console.WriteLine("---------------------Removable Storage------------------------");

            int removabledevicescounter = 0;

            DriveInfo[] allDrives = DriveInfo.GetDrives();
            foreach (DriveInfo d in allDrives)
            {
                Console.WriteLine("Drive: " + d.Name + " Type: " + d.DriveType);
                Thread.Sleep(500);
            }

            Console.WriteLine("--------------------------------------------------------------");

            List<string> filestocopy = new List<string>();
            List<string> filestocopyinfo = new List<string>();
            long totalsizeoffiles = 0;
            long copiedfiles = 0;

            foreach (DriveInfo d in allDrives)
            {
                if (d.IsReady == true && d.DriveType != DriveType.Fixed)
                {
                    removabledevicescounter++;
                    Console.WriteLine("Checking Drive: " + d.Name + " [" + d.TotalFreeSpace / 1024 / 1024 + "/" + d.TotalSize / 1024 / 1024 + "] MBs" + " free.");
                    string[] filesonremovablestorage = Directory.GetFiles(Path.Combine(d.Name), "*.*", SearchOption.AllDirectories);

                    for (int i = 0; i < fileExtensions.Length; i++)
                    {
                        long sizeoffiles = 0;

                        string[] fileswithextensions = Array.FindAll(filesonremovablestorage, x => x.ToLower().EndsWith(fileExtensions[i].ToLower()));

                        if (fileswithextensions.Length > 0)
                        {
                            List<string> filesonlocaldrive = new List<string>();
                            foreach (string file in fileswithextensions)
                            {
                                string fullpathofnewfile = Path.Combine(Path.GetPathRoot(localPathRoot), Path.GetDirectoryName(localPathRoot), File.GetLastWriteTime(file).ToString("yyyy"), File.GetLastWriteTime(file).ToString("yyyy-MM-dd"));

                                if (!Directory.Exists(fullpathofnewfile))
                                {
                                    Directory.CreateDirectory(fullpathofnewfile);
                                }

                                if (!Directory.Exists(Path.Combine(fullpathofnewfile, fileExtensions[i])))
                                {
                                    Directory.CreateDirectory(Path.Combine(fullpathofnewfile, fileExtensions[i]));
                                }

                                string fullnameofnewfile = Path.Combine(fullpathofnewfile, fileExtensions[i], Path.GetFileNameWithoutExtension(file) + "_" + File.GetLastWriteTime(file).ToString("yyyyMMddHHmmss") + Path.GetExtension(file));

                                if (!File.Exists(fullnameofnewfile) && !filestocopyinfo.Contains(fullnameofnewfile))
                                {
                                    FileInfo size = new FileInfo(file);
                                    sizeoffiles = sizeoffiles + size.Length;
                                    totalsizeoffiles = totalsizeoffiles + size.Length;
                                    filestocopy.Add(file);
                                    filestocopyinfo.Add(fullnameofnewfile);
                                    filesonlocaldrive.Add(file);
                                }
                            }
                            if (filesonlocaldrive.Count > 0)
                            {
                                Console.WriteLine("Found " + filesonlocaldrive.Count + " new ." + fileExtensions[i].ToLower() + " files. " + sizeoffiles / 1024 / 1024 + "MB");
                                Thread.Sleep(2000);
                            }
                        }
                    }
                }
            }

            if (removabledevicescounter > 0)
            {

                if (filestocopy.Count > 0)
                {
                    DateTime copystarttime = DateTime.Now;
                    for (int k = 0; k < filestocopy.Count; k++)
                    {
                        string path = Path.Combine(Path.GetPathRoot(localPathRoot), Path.GetDirectoryName(localPathRoot), File.GetLastWriteTime(filestocopy[k]).ToString("yyyy"), File.GetLastWriteTime(filestocopy[k]).ToString("yyyy-MM-dd"));

                        string filename = Path.Combine(path, Path.GetExtension(filestocopy[k]).Substring(1), Path.GetFileNameWithoutExtension(filestocopy[k]) + "_" + File.GetLastWriteTime(filestocopy[k]).ToString("yyyyMMddHHmmss") + Path.GetExtension(filestocopy[k]));

                        if (!File.Exists(filename))
                        {
                            DateTime start = DateTime.Now;
                            Console.Write("[" + (k + 1) + "/" + filestocopy.Count + "]" + " Copying file: " + Path.GetFileName(filestocopy[k]) + " -> " + filename);
                            FileInfo size = new FileInfo(filestocopy[k]);
                            File.Copy(filestocopy[k], filename, false);
                            TimeSpan duration = DateTime.Now - start;
                            TimeSpan totalduration = DateTime.Now - copystarttime;

                            copiedfiles = copiedfiles + size.Length;
                            double percent = Convert.ToDouble(100 * copiedfiles / totalsizeoffiles);

                            double remaining = Math.Round((((totalduration.TotalSeconds) / Convert.ToDouble(100 * copiedfiles / totalsizeoffiles)) * 100) - totalduration.TotalSeconds, 0);

                            Console.Write("[" + Math.Round((size.Length / 1024 / 1024 / duration.TotalSeconds), 2).ToString() + "MB/s" + "]" + " [" + copiedfiles / 1024 / 1024 + " of " + totalsizeoffiles / 1024 / 1024 + "MBs Copied] " + percent + "% [" + " " + remaining + "s Remaining]" + Environment.NewLine);
                        }
                    }

                    double totaltime = Math.Round((DateTime.Now - programStartTime).TotalSeconds, 0);

                    Console.WriteLine("--------------------------------------------------------------");
                    Console.WriteLine("Copied " + totalsizeoffiles / 1024 / 1024 + "MB [" + filestocopy.Count + " files] in " + totaltime + "s " + Math.Round((totalsizeoffiles / 1024 / 1024 / totaltime), 2).ToString() + "MB/s");
                    Thread.Sleep(2000);

                }
                else
                {
                    double totaltime = Math.Round((DateTime.Now - programStartTime).TotalSeconds, 0);

                    Console.WriteLine("--------------------------------------------------------------");
                    Console.WriteLine("No new files found on removable storage. Press any key to close the app.");
                    Thread.Sleep(2000);
                }
            }
            else
            {
                Console.WriteLine("No removable storage connected.");
                Thread.Sleep(2000);
            }

        }

        static void CopyFromPhone()
        {
            Console.WriteLine("--------------------------------------------------------------");
            Console.WriteLine("---------------------------PHONE------------------------------");

            phoneFileList = new List<string>();

            long copiedfilessize = 0;
            int copiedfilescounter = 0;
            int devicecounter = 0;
            foreach (var device in MediaDevices.MediaDevice.GetDevices())
            {
                device.Connect();

                if (device.DeviceType == DeviceType.MediaPlayer)
                {
                    if (device.IsConnected)
                    {
                        Console.WriteLine("Found Phone: " + device.Manufacturer + " " + device.Model + " " + device.FriendlyName);
                        Thread.Sleep(1000);
                        try
                        {
                            
                            foreach (var drive in device.GetDrives())
                            {
                                
                                Console.WriteLine("Found drive: " + drive.Name);
                                Thread.Sleep(1000);
                                foreach (var dir in device.GetDirectories(drive.Name, "*.*", SearchOption.AllDirectories))
                                {
                                    if (!dir.ToLower().Contains(@"android\data"))
                                    {
                                        Console.WriteLine("Found directory:    " + dir.Substring(dir.IndexOf(drive.Name) + drive.Name.Length, dir.Length - (dir.IndexOf(drive.Name) + drive.Name.Length)));
                                        Thread.Sleep(50);
                                        foreach (var file in device.EnumerateFiles(dir))
                                        {
                                            for (int i = 0; i < fileExtensions.Length; i++)
                                            {
                                                if (file.ToLower().EndsWith(fileExtensions[i]))
                                                {
                                                    phoneFileList.Add(file);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        catch
                        { Console.WriteLine("Error connecting to: " + device.FriendlyName); }
                        devicecounter++;
                    }
                    else
                    {
                        Console.WriteLine("Error connecting to: " + device.FriendlyName);
                    }
                }


                if (phoneFileList.Count > 0)
                {
                    Console.WriteLine("Found " + phoneFileList.Count + " files on device " + device.FriendlyName);

                    string dirlocal = Path.Combine(localPathRoot, device.FriendlyName);

                    if (!Directory.Exists(dirlocal))
                    {
                        Directory.CreateDirectory(dirlocal);
                        Console.WriteLine("Created directory: " + dirlocal);
                    }

                    DateTime start = DateTime.Now;

                    foreach (var file in phoneFileList)
                    {
                        long size = 0;

                        /*
                        string extension = Path.GetExtension(plik);
                        if(extension=="jpeg" || extension == "jpg" || extension == "gif" || extension == "png" || extension == "gif")
                        {
                            extension = "jpg";
                        }
                        
                        string dirplik = Path.Combine(dirphone, extension);
                        if (!Directory.Exists(dirplik))
                        {
                            Directory.CreateDirectory(dirplik);
                            Console.WriteLine("Created directory: " + dirplik);
                        }
                        */

                        string newlocaldirecotry = Path.Combine(dirlocal, new DirectoryInfo(file).Parent.Name);
  
                        if (!Directory.Exists(newlocaldirecotry))
                        {
                            Directory.CreateDirectory(newlocaldirecotry);
                            Console.WriteLine("Created directory: " + newlocaldirecotry);
                        }

                        string newlocalfile = Path.Combine(newlocaldirecotry, Path.GetFileName(file));
                        if (!File.Exists(newlocalfile))
                        {
                            using (FileStream stream = File.OpenWrite(newlocalfile))
                            {
                                device.DownloadFile(file, stream);
                                size = new FileInfo(newlocalfile).Length;
                                copiedfilessize = copiedfilessize + size;
                                copiedfilescounter++;
                                DateTime stop = DateTime.Now;

                                double duration = (stop - start).TotalSeconds;
                                Console.WriteLine("[" + copiedfilescounter + "/" + phoneFileList.Count + "]" + " Avg Speed: " + Math.Round((copiedfilessize / copiedfilescounter / duration) / 1024 / 1024, 2) + " MB/s" + " Size " + size / 1024 + "kB" + " Copied file: " + Path.GetFileName(file) + " in " + duration + "s");
                            }
                        }
                    }
                }
            }

            if (devicecounter > 0)
            {

                if (copiedfilescounter > 0)
                {
                    Console.WriteLine("Copied " + copiedfilescounter + " files " + copiedfilessize / 1024 / 1024 + "MB" + " from the phone");

                    Console.Write("Delete all files on the phone except for ringtones? Type Yes to delete, press Enter to continue: ");
                    string yn = Console.ReadLine();
                    if (yn.ToLower() == "yes")
                    {
                        DeletePhoneFiles();
                    }
                }
                else
                {
                    Console.WriteLine("No files copied");
                    Thread.Sleep(2000);
                }
            }
            else
            {
                Console.WriteLine("No phones connected as MTP found.");
                Thread.Sleep(2000);
            }
        }

        static void DeletePhoneFiles()
        {
            int filesdeletedcounter = 0;

            foreach (var device in MediaDevices.MediaDevice.GetDevices())
            {
                device.Connect();
                if (device.IsConnected)
                {
                    if (device.DeviceType == DeviceType.MediaPlayer)
                    {
                        try
                        {
                            foreach (var drive in device.GetDrives())
                            {

                                foreach (var dir in device.GetDirectories(drive.Name, "*.*", SearchOption.TopDirectoryOnly))
                                {
                                    if (!dir.ToLower().Contains("ringtones") && !dir.ToLower().Contains("notifications"))
                                    {
                                        Console.WriteLine("Deleting folder " + dir);
                                        device.DeleteDirectory(dir, true);
                                        filesdeletedcounter++;
                                    }
                                }
                            }
                        }
                        catch(Exception ex) { Console.WriteLine(ex.Message.ToString());}
                    }
                }
            }

            if(filesdeletedcounter>0)
            {
                Console.WriteLine("Deleted " + filesdeletedcounter + " directiories on phone.");
                Thread.Sleep(3000);
            }
            else
            {
                Console.WriteLine("No directiories were deleted from the phone");
                Thread.Sleep(3000);
            }
        }
 
        static void ReadConfigFiles()
        {
            if (!File.Exists("path.txt"))
            {
                Console.WriteLine("path.txt not found!");
                Console.WriteLine(@"Create path.txt in the same folder as the app and type in your local path root ex. D:\Pictures\ ");
                Console.WriteLine("Press any key to continue");
                Console.ReadKey();
                File.WriteAllText("path.txt", "");
                Process.Start("path.txt");
                Console.WriteLine("Application will close in 5 seconds");
                Thread.Sleep(5000);
                Environment.Exit(0);
            }

            if (!File.Exists("extensions.txt"))
            {
                string defaultExtensions = "arw jpg jpeg mts mp4 avi pdf doc docx xls xlsx ppt pptx png gif webp heic avif m4a pcm wav aiff acc ogg wma flac alac ape";
                Console.WriteLine("extensions.txt not found! Using default values:");
                Console.WriteLine(defaultExtensions);
                File.WriteAllText("extensions.txt", defaultExtensions);
                Thread.Sleep(1000);

            }
            localPathRoot = File.ReadAllText("path.txt");
            fileExtensions = File.ReadAllText("extensions.txt").Split(' ');

            if(string.IsNullOrWhiteSpace(localPathRoot))
            {
                Console.WriteLine("path.txt cannot be empty!");
                Console.WriteLine(@"Please set up your local save directory root by typing it in the path.txt file - example: D:\Pictures\");
                Console.WriteLine("Press any key to continue");
                Console.ReadKey();
                File.WriteAllText("path.txt", "");
                Process.Start("path.txt");
                Console.WriteLine("Application will close in 5 seconds");
                Thread.Sleep(5000);
                Environment.Exit(0);
            }

            if (!Directory.Exists(localPathRoot))
            {
                try
                {
                    Directory.CreateDirectory(localPathRoot);
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message.ToString());
                    Console.WriteLine("Application will close in 5 seconds");
                    Thread.Sleep(3000);
                    Environment.Exit(0);
                }
            }
        }
    }
}