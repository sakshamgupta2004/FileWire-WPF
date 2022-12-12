using System.IO;
using Microsoft.VisualBasic.FileIO;

namespace WpfApp1
{
    public static class Logging
    {
        public static void createNew(string fileName, string text)
        {
            try
            {
                Directory.CreateDirectory(SpecialDirectories.MyDocuments + "\\FileWire\\Logs");
                var file = File.CreateText(SpecialDirectories.MyDocuments + "\\FileWire\\Logs\\" + fileName +
                                           ".log");
                file.Write(text);
                file.Close();
            }
            catch
            {
                
            }
        }
    }
}