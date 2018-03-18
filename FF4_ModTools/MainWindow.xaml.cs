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
        OpenFileDialog openDialog;

        private void PopulateAsync(string targetFile)
        {
            // Clean up PropertyListView items for later
            PropertyListView.Items.Clear();


            //FileHandler.GetFormat(targetFile);
            //XBNFile s = FileHandler.Open<XBNFile>(targetFile);
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

            ((TreeViewItem)FileTree.Items[parentFileIndex]).Selected += TreeViewFile_Selected;
            ((TreeViewItem)FileTree.Items[parentFileIndex]).ContextMenu = (ContextMenu)this.Resources["TreeViewItemContextMenu"];

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

        void Worker_DoWork(object sender, DoWorkEventArgs e)
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
                List<object> progressState = new List<object> { parentFileIndex, i, file.SubFiles[i].Header.Name };
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
            (FileTree.Items[parentIndex] as TreeViewItem).Items.Add(new TreeViewItem
            {
                DataContext = childContext,
                Header = childHeader
            });
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var parent = ((TreeViewItem)FileTree.Items[(int)e.Result]);
            foreach (TreeViewItem child in parent.Items)
            {
                child.Selected += SubFile_Selected;
                child.ContextMenu = (ContextMenu)this.Resources["SubFileContextMenu"];
                child.ContextMenuOpening += SubFile_ContextMenuOpening;
            }

            // Initialize PropertyViewItemList with the parent file's data
            GeneratePropertyViewItems(new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("Name", parent.Header),
                new KeyValuePair<string, object>("File Count", ((MASSFile)parent.DataContext).SubFileCount),
                new KeyValuePair<string, object>("First File", ((MASSFile)parent.DataContext).FileOffset)
            });
        }

        private void SubFile_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            ((TreeViewItem)sender).IsSelected = true;
        }

        private void TreeViewFile_Selected(object sender, RoutedEventArgs e)
        {
            var item = (TreeViewItem)sender;
            var context = item.DataContext;

            if (context.GetType() == typeof(MASSFile))
            {
                GeneratePropertyViewItems(new List<KeyValuePair<string, object>>
                {
                    new KeyValuePair<string, object>("Name", item.Header),
                    new KeyValuePair<string, object>("File Count", ((MASSFile)context).SubFileCount),
                    new KeyValuePair<string, object>("First File", ((MASSFile)context).FileOffset)
                });
            }
        }

        private void SubFile_Selected(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = (TreeViewItem)sender;

            MASSSubFile _sf = ((MASSFile)((TreeViewItem)item.Parent).DataContext).SubFiles[(int)item.DataContext];

            // TODO: Verify file type, and display in PreviewCanvas

            // Read selected file and try to create a bitmap from it (for png files)
            // ImagePreview.Source = BitmapFrame.Create(new MemoryStream(_sf.FileData, false));


            GeneratePropertyViewItems(new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("Name", item.Header),
                new KeyValuePair<string, object>("Relative Offset", _sf.Header.Offset),
                new KeyValuePair<string, object>("Size", _sf.Header.Size),
                new KeyValuePair<string, object>("Format", _sf.GetInternalType()),
            });
            e.Handled = true;
        }

        private void GeneratePropertyViewItems(List<KeyValuePair<string, object>> listViewItems)
        {
            PropertyListView.Items.Clear();

            foreach (KeyValuePair<string, object> k in listViewItems)
            {
                PropertyListView.Items.Add(new ListViewItem
                {
                    Content = k
                });
            }

            foreach (GridViewColumn c in (PropertyListView.View as GridView).Columns)
            {
                c.Width = c.ActualWidth;    // First set to a different value
                c.Width = Double.NaN;       // Then set it back to NaN for auto resizing
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            
            openDialog = new OpenFileDialog
            {
                Multiselect = false,
                Filter = "Dat files (*.dat)|*.dat|All files (*.*)|*.*",
                InitialDirectory = $@"{Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)}\Steam\SteamApps\common\Final Fantasy IV\EXTRACTED_DATA\"
            };
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
            if (openDialog.ShowDialog() == true)
            {
                // TODO: Support multiple files
                PopulateAsync(openDialog.FileName);
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

        private void SubFileContextExport_Click(object sender, RoutedEventArgs e)
        {
            var item = (TreeViewItem)((ContextMenu)((MenuItem)sender).Parent).PlacementTarget;

            int index = (int)item.DataContext;
            var parent = (TreeViewItem)item.Parent;

            var subFile = ((MASSFile)parent.DataContext).SubFiles[index];

            SaveFileDialog saveDialog = new SaveFileDialog
            {
                Filter = $"{subFile.Name} | *.{subFile.Extension}",
                FileName = subFile.Name
            };

            if (saveDialog.ShowDialog() == true)
            {
                FileHandler.Save(saveDialog.FileName, subFile.Data);
            }
        }

        private void ParentFileRepack_Click(object sender, RoutedEventArgs e)
        {
            var parent = (TreeViewItem)FileTree.Items[0];

            MASSFile context = (MASSFile)parent.DataContext;

            SaveFileDialog saveDialog = new SaveFileDialog
            {
                Filter = $"{context.Name} | *.*",
                FileName = context.Name
            };

            if (saveDialog.ShowDialog() == true)
            {
                FileHandler.Save(saveDialog.FileName, context.GetData());
            }

            // FileHandler.Save(); ;
        }

        private void SubFileContextReplace(object sender, RoutedEventArgs e)
        {
            var item = (TreeViewItem)((ContextMenu)((MenuItem)sender).Parent).PlacementTarget;

            int index = (int)item.DataContext;
            var context = (MASSFile)((TreeViewItem)item.Parent).DataContext;

            ref var subFiles = ref context.SubFiles;

            openDialog.Filter = "All files (*.*)|*.*";

            if (openDialog.ShowDialog() == true)
            {
                subFiles[index].ReplaceData(FileHandler.Open<MASSSubFile>(openDialog.FileName));
                context.SetDirty(index);
            }
        }
    }
}
