using Microsoft.Win32;
using System;
using System.Windows;

namespace FF4_ModTools
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public OpenFileDialog FileDialog;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            FileDialog = new OpenFileDialog
            {
                Multiselect = false,
                Filter = "Dat files (*.dat)|*.dat|All files (*.*)|*.*",
                InitialDirectory = $@"{Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)}\Steam\SteamApps\common\Final Fantasy IV\EXTRACTED_DATA\"
            };
        }
    }
}
