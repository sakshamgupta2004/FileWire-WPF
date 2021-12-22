using Microsoft.WindowsAPICodePack.Dialogs;
using Microsoft.WindowsAPICodePack.Shell;
using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace WpfApp1
{
    public partial class MainWindow
    {
        private bool initialised = false;

        private void openSettingsClick(object sender, EventArgs e)
        {
            if (!initialised)
            {
                initListeners();
                initialised = true;
            }
            initSettings();
            
            SettingsView.Visibility = Visibility.Visible;
            DoubleAnimation doubleAnimation = new DoubleAnimation()
            {
                From = 1.0,
                To = 0.0,
                Duration = new Duration(TimeSpan.FromMilliseconds(500))
            };
            doubleAnimation.Completed += (s, e) => QRCodeViewPanel.Visibility = Visibility.Hidden;
            QRCodeViewPanel.BeginAnimation(ScrollViewerEx.OpacityProperty, doubleAnimation);
        }


        private void backFromSettingsToHomeClick(object sender, EventArgs e)
        {
            DoubleAnimation doubleAnimation = new DoubleAnimation()
            {
                From = 0.0,
                To = 1.0,
                Duration = new Duration(TimeSpan.FromMilliseconds(500))
            };
            doubleAnimation.Completed += (s, e) => SettingsView.Visibility = Visibility.Hidden;
            QRCodeViewPanel.Visibility = Visibility.Visible;
            QRCodeViewPanel.BeginAnimation(ScrollViewerEx.OpacityProperty, doubleAnimation);
        }

        private void initListeners()
        {

            SaveLocationSelectorButton.Click += (s, e) =>
            {
                CommonOpenFileDialog dialog = new CommonOpenFileDialog();
                dialog.IsFolderPicker = true;
                dialog.Multiselect = false;
                dialog.Title = "Select Receiving Location";
                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    preferences.setReceiveLocation(dialog.FileName);
                    updatePathTextBox();
                }
            };
            SaveLocationSelectorComboBox.SelectionChanged += (s, e1) =>
            {
                if (SaveLocationSelectorComboBox.SelectedIndex == 1)
                {
                    SaveLocationSelectorButton.Visibility = Visibility.Visible;
                }
                else
                {
                    preferences.setReceiveLocation(null);
                    updatePathTextBox();
                    SaveLocationSelectorButton.Visibility = Visibility.Collapsed;
                }
            };
            TimeStampSettingRadioButtons.SelectionChanged += (s, e1) =>
            {
                if (TimeStampSettingRadioButtons.SelectedIndex == 1)
                {
                    preferences.setTimeStampSetting(false);
                    updatePathTextBox();
                }
                else
                {
                    preferences.setTimeStampSetting(true);
                    updatePathTextBox();
                }
            };
            TypeSplitSettingRadioButtons.SelectionChanged += (s, e1) =>
            {
                if (TypeSplitSettingRadioButtons.SelectedIndex == 1)
                {
                    preferences.setTypeSeperationSetting(false);
                    updatePathTextBox();
                }
                else
                {
                    preferences.setTypeSeperationSetting(true);
                    updatePathTextBox();
                }
            };



            AppTransparencySettingsRadioButtons.SelectionChanged += (s, e1) =>
            {
                if (AppTransparencySettingsRadioButtons.SelectedIndex == 0)
                {
                    preferences.setAutomaticTransparency();
                }
                else if (AppTransparencySettingsRadioButtons.SelectedIndex == 1)
                {
                    preferences.setTransparencyTo(true);
                }
                else if (AppTransparencySettingsRadioButtons.SelectedIndex == 2)
                {
                    preferences.setTransparencyTo(false);
                }
                autoSetTransparency();
            };

            NumThreadsSelector.SelectionChanged += (_, __) =>
            {
                preferences.setThreads(NumThreadsSelector.SelectedIndex + 1);
                if (NumThreadsSelector.SelectedIndex > 0)
                {
                    MultithreadWarning.Visibility = Visibility.Visible;
                }
                else
                {
                    MultithreadWarning.Visibility = Visibility.Collapsed;
                }
            };
        }

        private void initSettings()
        {

            if (File.Exists(preferences.settingsDirectory + "receiveLocation"))
            {
                SaveLocationSelectorComboBox.SelectedIndex = 1;
                SaveLocationSelectorButton.Visibility = Visibility.Visible;
            }
            else
            {
                SaveLocationSelectorComboBox.SelectedIndex = 0;
                SaveLocationSelectorButton.Visibility = Visibility.Collapsed;
            }

            if (preferences.IsTimeStampEnabled())
            {
                TimeStampSettingRadioButtons.SelectedIndex = 0;
            }
            else
            {
                TimeStampSettingRadioButtons.SelectedIndex = 1;
            }

            if (preferences.IsTypeSeperationEnabled())
            {
                TypeSplitSettingRadioButtons.SelectedIndex = 0;
            }
            else
            {
                TypeSplitSettingRadioButtons.SelectedIndex = 1;
            }

            if (System.Diagnostics.Process.GetProcessesByName("FileWire").Length > 2)
            {
                SettingsDisableCover.Visibility = Visibility.Visible;
            }
            else
            {
                SettingsDisableCover.Visibility = Visibility.Collapsed;
            }
            updatePathTextBox();



            if (preferences.IsTransparencyAutomatic())
            {
                AppTransparencySettingsRadioButtons.SelectedIndex = 0;
            }
            else
            {
                if (preferences.IsTransparencyEnabled() == true)
                {
                    AppTransparencySettingsRadioButtons.SelectedIndex = 1;
                }
                else
                {
                    AppTransparencySettingsRadioButtons.SelectedIndex = 2;
                }
            }


            NumThreadsSelector.SelectedIndex = preferences.getThreads() - 1;
            if (NumThreadsSelector.SelectedIndex > 0)
            {
                MultithreadWarning.Visibility = Visibility.Visible;
            }
            else
            {
                MultithreadWarning.Visibility = Visibility.Collapsed;
            }
        }
        private void updatePathTextBox()
        {
            this.SizeChanged += MainWindow_SizeChanged;
            string path = preferences.getReceivingBaseLocation(null);
            if (!path.EndsWith("\\"))
            {
                path += "\\";
            }
            if (preferences.IsTimeStampEnabled())
            {
                path += "<TimeStamp>\\";
            }
            if (preferences.IsTypeSeperationEnabled())
            {
                path += "<FileType>\\";
            }

            PathSettingsTextBox.Text = path;
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Trace.WriteLine(this.Width.ToString() + "            " + this.Height.ToString());
        }

        private void openReceivingBaseLocation(object s, EventArgs e)
        {
            Process.Start("explorer.exe", preferences.getReceivingBaseLocation());
        }
    }
}
