using System;
using System.Diagnostics;
using System.IO;

namespace SpeechlyTouch.Helpers
{
    public static class FileDirectoryHelper
    {
        /// <summary>
        /// Creates parent directory where wav file will created if the directory does not exist
        /// </summary>
        public static void CreateDirectoryIfNotExists(string filePath)
        {
            try
            {
                var dirName = Path.GetDirectoryName(filePath);
                var directoryInfo = new DirectoryInfo(dirName);
                if (!directoryInfo.Exists)
                {
                    // Create Directory
                    Directory.CreateDirectory(dirName);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"File not created: {e.Message}");
                throw e;
            }
        }

        public static void CreateFileIfNotExists(string filePath)
        {
            try
            {
                CreateDirectoryIfNotExists(filePath);
                if (!File.Exists(filePath))
                {
                    File.Create(filePath);
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }
}
