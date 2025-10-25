// File: AppData.cs
using System;
using System.IO;

namespace MinimalFirewall
{
    internal static class AppData
    {
        private static readonly string _folderPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MinimalFirewall");

        public static string GetPath(string fileName)
        {
            Directory.CreateDirectory(_folderPath);
            return Path.Combine(_folderPath, fileName);
        }
    }
}