using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using FF4_ModTools.FileFormats;

namespace FF4_ModTools
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private void PopulateAsync(string targetFile)
        {
            // Clean up PropertyListView items for later
            PropertyListView.Items.Clear();
            
            // TODO: Don't assume type MASSFile
            MASSFile file = FileHandler.Open<MASSFile>(targetFile);

            // Clear Tree of previous files 
            // TODO: remove this for multi-file support
            FileTree.Items.Clear();

            // Add parent to file tree and save its index
            int parentFileIndex = FileTree.Items.Add(new TreeViewItem
            {
                DataContext = file,
                Header = $"{file.Name} ({file.SubFileCount} files)",
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
            
            int progressPercentage = 0;

            // Loop through all subfiles
            for (int i = 0; i < file.SubFileCount; i++)
            {
                // Recalculate current progress percent
                progressPercentage = Convert.ToInt32(((double)i / file.SubFileCount) * 100);

                // Report Progress
                List<object> progressState = new List<object> { parentFileIndex, i, file.SubFiles[i].FileName };                
                (sender as BackgroundWorker).ReportProgress(progressPercentage, progressState);
                
                // Sleep, sweet summer child
                Thread.Sleep(1);
            }

            e.Result = parentFileIndex;
        }

        void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // Get State parameters
            List<object> userState = new List<object>((IEnumerable<object>)e.UserState);
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

            MASSSubFile _sf = ((MASSFile)((TreeViewItem)item.Parent).DataContext).SubFiles[(int)item.DataContext];

            // TODO: Verify file type, and display in PreviewCanvas

            // Read selected file and try to create a bitmap from it (for png files)
            // ImagePreview.Source = BitmapFrame.Create(new MemoryStream(_sf.FileData, false));

            PropertyListView.Items.Clear();
            PropertyListView.Items.Add(new ListViewItem{
                Content = new KeyValuePair<string, object>("Name", item.Header)});

            PropertyListView.Items.Add(new ListViewItem {
                Content = new KeyValuePair<string, object>("Offset", _sf.Offset)});

            PropertyListView.Items.Add(new ListViewItem {
                Content = new KeyValuePair<string, object>("Size", _sf.Size)});

            PropertyListView.Items.Add(new ListViewItem {
                Content = new KeyValuePair<string, object>("Format", _sf.GetInternalType())});
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void FileTree_Drop(object sender, DragEventArgs e)
        {
            string[] fileDrop;
            
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                fileDrop = (e.Data.GetData(DataFormats.FileDrop) as string[]);
                PopulateAsync(fileDrop[0]);
            }
                
        }
        

        #region File Menu Events

        private void OpenCommand_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            OpenFileDialog dialog = ((App)Application.Current).FileDialog;

            if (dialog.ShowDialog() == true)
            {
                // TODO: Support multiple files
                PopulateAsync(dialog.FileName);
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
