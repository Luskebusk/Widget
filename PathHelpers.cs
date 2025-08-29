using System;
using System.IO;

namespace MobitSystemInfoWidget
{
    public static class PathHelpers
    {
        public static string BaseDir => AppContext.BaseDirectory;

        public static string Asset(string relative) =>
            Path.Combine(BaseDir, relative);

        public static string LocalDataDir
        {
            get
            {
                var dir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "MobitSystemInfoWidget");
                Directory.CreateDirectory(dir);
                return dir;
            }
        }

        public static string LocalData(string relative) =>
            Path.Combine(LocalDataDir, relative);
    }
}
