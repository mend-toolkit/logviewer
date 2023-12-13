using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogViewerConsole.Helpers
{
    public static class FileHelper
    {
        public static void WriteFile(string filename, string data)
        {
            if (string.IsNullOrEmpty(filename))
                Console.WriteLine(data);
            else
            {
                File.AppendAllText(filename, data + "\r\n");
            }
        }

        public static void DeleteFile(string filename)
        {
            if (File.Exists(filename))
            {
                File.Delete(filename);
            }
        }
    }
}
