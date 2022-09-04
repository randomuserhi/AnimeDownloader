using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace Updater
{
    internal class Program
    {
        static void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
        {
            // Get information about the source directory
            var dir = new DirectoryInfo(sourceDir);

            // Check if the source directory exists
            if (!dir.Exists)
                throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

            // Cache directories before we start copying
            DirectoryInfo[] dirs = dir.GetDirectories();

            // Create the destination directory
            Directory.CreateDirectory(destinationDir);

            // Get the files in the source directory and copy to the destination directory
            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                int attempts = 0;
                for (; attempts < 10; attempts++)
                {
                    try
                    {
                        file.CopyTo(targetFilePath, true);

                        break;
                    }
                    catch (Exception err)
                    {
                        Console.WriteLine("[Updater] Failed to copy " + targetFilePath);
                        Console.WriteLine("[Updater] Trying again... ");
                        Console.WriteLine("[Updater : WARNING] " + err);
                        Task.Delay(1000).Wait();
                    }
                }
                if (attempts == 10) throw new Exception("Failed to move file after 10 attempts.");
                else Console.WriteLine("[Updater] Moved " + targetFilePath);
            }

            // If recursive and copying subdirectories, recursively call this method
            if (recursive)
            {
                foreach (DirectoryInfo subDir in dirs)
                {
                    string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                    CopyDirectory(subDir.FullName, newDestinationDir, true);
                }
            }
        }

        static void Main(string[] args)
        {
            ProcessStartInfo updater = new ProcessStartInfo("AutoDownloader.exe");

            try
            {
                Console.WriteLine("[Updater] Moving files...");

                string directoryName = args[0];
                DirectoryInfo dirInfo = new DirectoryInfo(directoryName);
                if (dirInfo.Exists == false)
                    Directory.CreateDirectory(directoryName);

                DirectoryInfo updateDirInfo = new DirectoryInfo(Path.Combine(directoryName, "update"));
                CopyDirectory(Path.Combine(directoryName, "update"), directoryName, true);
                Directory.Delete(updateDirInfo.FullName, true);

                Process.Start(updater);
            }
            catch(Exception err)
            {
                Console.WriteLine("[Updater] Failed to push updates, this is a fatal error and may result in your program being corrupted. Please reinstall from https://github.com/randomuserhi/AnimeDownloader/releases");
                Console.WriteLine("[Updater : FATAL ERROR] " + err);
                Console.WriteLine("Press enter to continue...");
                Console.Read();
            }
        }
    }
}
