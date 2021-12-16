using Microsoft.WindowsAPICodePack.Shell;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WpfApp1
{
    class Preferences
    {
        private bool? isTimeStampEnabled = null;
        private bool? isTypeSeperationEnabled = null;
        private string receiveLocation = null;
        public string settingsDirectory = null;
        private bool? isTransparencyEnabled = null;
        public Preferences()
        {
            if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\SugarSnooper\\FileWire\\"))
            {
                Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\SugarSnooper\\FileWire\\");
            }
            settingsDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\SugarSnooper\\FileWire\\";
            refreshLocationSettings();
            refreshTransparencySetting();
        }


        public string getReceivingLocation(string fileName, string timeStamp)
        {
            if (IsTypeSeperationEnabled())
            {
                return getReceivingBaseLocation(timeStamp) + System.IO.Path.DirectorySeparatorChar.ToString() + FileWire.MimeType.fileType(fileName);
            }
            else
            {
                return getReceivingBaseLocation(timeStamp);
            }
        }

        public string getReceivingBaseLocation(string timeStamp = null)
        {
            if (timeStamp != null)
            {
                if (IsTimeStampEnabled())
                {
                    return getSaveLocation() + System.IO.Path.DirectorySeparatorChar.ToString() + timeStamp;
                }
                else
                {
                    return getSaveLocation();
                }
            }
            else
            {
                return getSaveLocation();
            }
        }


        public bool IsTypeSeperationEnabled()
        {
            if (isTypeSeperationEnabled == null)
            {
                refreshLocationSettings();
            }
            if (isTypeSeperationEnabled == true)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool IsTimeStampEnabled()
        {
            if (isTimeStampEnabled == null)
            {
                refreshLocationSettings();
            }
            if (isTimeStampEnabled == true)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public string getSaveLocation()
        {
            if (receiveLocation == null)
            {
                refreshLocationSettings();
            }
            return receiveLocation;
        }

        private void refreshLocationSettings()
        {
            receiveLocation = getPreference("receiveLocation", KnownFolders.Downloads.Path + System.IO.Path.DirectorySeparatorChar.ToString() + "FileWire");
            isTimeStampEnabled = bool.Parse(getPreference("isTimeStampEnabled", bool.FalseString));
            isTypeSeperationEnabled = bool.Parse(getPreference("isTypeSeperationEnabled", bool.TrueString));
        }

        public void setLocationSettings(string ReceiveLocation, bool? timeStampEnabled, bool? typeSepEnabled)
        {
            setPreference("receiveLocation", ReceiveLocation);
            setPreference("isTimeStampEnabled", timeStampEnabled.ToString());
            setPreference("isTypeSeperationEnabled", typeSepEnabled.ToString());
            refreshLocationSettings();
        }

        public void setTypeSeperationSetting(bool typeSeperationEnabled)
        {
            setPreference("isTypeSeperationEnabled", typeSeperationEnabled.ToString());
            refreshLocationSettings();
        }
        public void setTimeStampSetting(bool timeStampEnabled)
        {
            setPreference("isTimeStampEnabled", timeStampEnabled.ToString());
            refreshLocationSettings();
        }

        public void setReceiveLocation(string ReceiveLocation)
        {
            setPreference("receiveLocation", ReceiveLocation);
            refreshLocationSettings();
        }




        public bool IsTransparencyAutomatic()
        {
            return !isTransparencyEnabled.HasValue;
        }

        public bool? IsTransparencyEnabled()
        {
            return isTransparencyEnabled;
        }

        private void refreshTransparencySetting()
        {
            string a = getPreference("TransparencySetting", "0");
            int val = int.Parse(a);

            if (val == 0)
            {
                isTransparencyEnabled = null;
            }
            else if (val == 1)
            {
                isTransparencyEnabled = true;
            }
            else if (val == 2)
            {
                isTransparencyEnabled = false;
            }
        }

        public void setAutomaticTransparency()
        {
            isTransparencyEnabled = null;
            setPreference("TransparencySetting", 0.ToString());
        }

        public void setTransparencyTo(bool? transparency)
        {
            int val = 0;
            isTransparencyEnabled = null;
            if (transparency.HasValue)
            {
                if (transparency == true)
                {
                    val = 1;
                    isTransparencyEnabled = true;
                }
                else
                {
                    val = 2;
                    isTransparencyEnabled = false;
                }
            }
            setPreference("TransparencySetting", val.ToString());
        }



        public int getThreads()
        {
            try
            {
                return int.Parse(getPreference("NumThreads", "1"));
            }
            catch
            {
                setPreference("NumThreads", null);
                return 1;
            }
        }
        public void setThreads(int threads)
        {
            setPreference("NumThreads", threads.ToString());
        }


        private string getPreference(string setting, string defaultValue)
        {
            string settingVal = "";
            string settingPath = settingsDirectory + setting;
            if (File.Exists(settingPath))
            {
                byte[] buffer = new byte[4096];
                FileStream fileStream = File.OpenRead(settingPath);
                while (fileStream.Read(buffer, 0, buffer.Length) > 0)
                {
                    settingVal += Encoding.UTF8.GetString(buffer);
                }
                fileStream.Close();
                return settingVal.Substring(settingVal.IndexOf("<set>") + 5, settingVal.IndexOf("</set>") - (settingVal.IndexOf("<set>") + 5));
            }
            else
            {
                return defaultValue;
            }
        }

        private void setPreference(string setting, string val)
        {
            if (val == null)
            {
                File.Delete(settingsDirectory + setting);
                return;
            }
            if (!Directory.Exists(settingsDirectory))
            {
                Directory.CreateDirectory(settingsDirectory);
            }
            string tobewritten = "<set>" + val + "</set>";

            FileStream fs = File.Create(settingsDirectory + setting);
            fs.Write(Encoding.UTF8.GetBytes(tobewritten));
            fs.Close();
        }

        internal void printToLog(string text)
        {
            File.WriteAllText(GetUniqueFilePath(settingsDirectory + "errors.log"), text);
        }
        private string GetUniqueFilePath(string filePath)
        {
            if (File.Exists(filePath))
            {
                string folderPath = Path.GetDirectoryName(filePath);
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                string fileExtension = Path.GetExtension(filePath);
                int number = 1;

                Match regex = Regex.Match(fileName, @"^(.+) \((\d+)\)$");

                if (regex.Success)
                {
                    fileName = regex.Groups[1].Value;
                    number = int.Parse(regex.Groups[2].Value);
                }

                do
                {
                    number++;
                    string newFileName = $"{fileName} ({number}){fileExtension}";
                    filePath = Path.Combine(folderPath, newFileName);
                }
                while (File.Exists(filePath));
            }
            return filePath;
        }
    }
}
