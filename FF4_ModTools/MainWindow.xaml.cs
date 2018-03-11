﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace FF4_ModTools
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private void OpenFileAsync(object sender, string targetFile)
        {
            // Open file for reading TODO 
            BinaryReader br = new BinaryReader(File.OpenRead(targetFile));

            // Create MASSFile object from BinaryReader stream
            MASSFile file = MASSFile.FromBinaryReader(ref br);

            // Clear Tree of previous files 
            // TODO: remove this for multi-file support
            FileTree.Items.Clear();

            // Add parent to file tree and save its index
            int parentFileIndex = FileTree.Items.Add(new TreeViewItem
            {
                DataContext = file,
                Header = file.Name,
                IsExpanded = true
            });

            // Create a new BackgroundWorker for populating FileTree
            BackgroundWorker worker = new BackgroundWorker { WorkerReportsProgress = true };
            worker.DoWork += Worker_DoWork;
            worker.ProgressChanged += Worker_ProgressChanged;
            worker.RunWorkerCompleted += Worker_RunWorkerCompleted;

            // Execute worker, sending the parentIndex and MASSFile
            worker.RunWorkerAsync(new List<object> {
                parentFileIndex,
                file
            });
        }

        void Worker_DoWork (object sender, DoWorkEventArgs e)
        {
            // Retrieve arguments
            int parentFileIndex = (int)(e.Argument as List<object>)[0];
            MASSFile file = (MASSFile)(e.Argument as List<object>)[1];
            
            // Instantiate progress state variables
            int progressPercentage = 0;
            List<object> progressState = new List<object> { parentFileIndex, null, null};

            // Loop through all subfiles
            for (int i = 0; i < file.SubFileCount; i++)
            {
                // Recalculate current progress percent
                progressPercentage = Convert.ToInt32(((double)i / file.SubFileCount) * 100);

                // Update progressState with the new Item's DataContext and Header values
                progressState[1] = i;
                progressState[2] = file.SubFiles[i].FileName;
                
                // Report Progress
                (sender as BackgroundWorker).ReportProgress(progressPercentage, progressState);
                
                // Sleep, sweet summer child
                Thread.Sleep(1);
            }

            e.Result = progressState[0];
        }

        void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // Get State parameters
            List<object> userState = (List<object>)e.UserState;
            int parentIndex = (int)userState[0];
            int childContext = (int)userState[1];
            string childHeader = (string)userState[2];

            // Add state to FileTree parent as a new TreeViewItem
            (FileTree.Items[parentIndex] as TreeViewItem).Items.Add(new TreeViewItem {
                DataContext = childContext,
                Header = childHeader
            });            
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            foreach (TreeViewItem child in ((TreeViewItem)FileTree.Items[(int)e.Result]).Items)
            {
                child.Selected += SubFile_Selected;
            }
        }

        private void SubFile_Selected(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = (TreeViewItem)sender;
            MASSFile file = (MASSFile)(item.Parent as TreeViewItem).DataContext;

            PropertyFileName.Content = item.Header;
            PropertyOffset.Content = file.SubFiles[(int)item.DataContext].Offset;
            PropertySize.Content = file.SubFiles[(int)item.DataContext].Size;
            PropertyType.Content = file.SubFiles[(int)item.DataContext].GetInternalType();
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void FileTree_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                OpenFileAsync(sender, 
                    (e.Data.GetData(DataFormats.FileDrop) as string[])[0]);
        }




        #region File Menu Events

        private void OpenCommand_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            if (((App)Application.Current).FileDialog.ShowDialog() == true)
            {

            }
        }

        private void FileSave_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void FileExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        
        private void CommandBinding_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }
        
        #endregion
    }
}
