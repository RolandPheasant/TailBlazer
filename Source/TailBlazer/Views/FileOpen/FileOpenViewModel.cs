using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Input;
using DynamicData.Binding;
using System.Reactive.Disposables;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Expression.Interactivity.Core;
using System.Drawing;
using System.Linq;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace TailBlazer.Views.FileOpen
{
    /// <summary>
    /// View Model for the MaterialDesign based OpenDialog
    /// </summary>
    public class FileOpenViewModel : AbstractNotifyPropertyChanged, IDisposable
    {
        private readonly IDisposable _cleanUp;
        public FileInfoWithIcon SelectedItem { get; set; }
        /// <summary>
        /// The content of this list will display on the right side of the Open Dialog
        /// </summary>
        public List<FileInfoWithIcon> FilesAndIcons { get; set; }
        public ICommand SelectedItemChangedCommand { get; set; }    
        public ICommand OpenSelectedItemCommand { get; set; }

        public FileOpenViewModel(Action<IEnumerable<FileInfo>> openFile)
        {
            SelectedItemChangedCommand = new ActionCommand(treeview => SelectedItemChangedCommandMethod(treeview as TreeView));
            OpenSelectedItemCommand = new ActionCommand(closeCommand => OpenSelectedItemCommandMethod(openFile, closeCommand as RoutedCommand));
            _cleanUp = new CompositeDisposable();
        }

        /// <summary>
        /// Calls openFile with a FileInfo array then closes the OpenFile dialog window.
        /// </summary>
        /// <param name="openFile"></param>
        /// <param name="closeCommand"></param>
        private void OpenSelectedItemCommandMethod(Action<IEnumerable<FileInfo>> openFile, RoutedCommand closeCommand)
        {
            openFile(FileAndDirectoryValidator(SelectedItem.FileInfo.FullName));
            closeCommand?.Execute(null, null);
        }

        /// <summary>
        /// Fills a FileInfoWithIcon with all the files and directories on the path that generated from a TreeView
        /// </summary>
        /// <param name="tree"></param>
        private void SelectedItemChangedCommandMethod(TreeView tree)
        {
            var temp = ((TreeViewItem) tree.SelectedItem);
            if (temp != null)
            {
                var selectedImagePath = GenerateFilePath(temp);

                FilesAndIcons = new List<FileInfoWithIcon>();
                GetDirecotries(selectedImagePath);
                GetFiles(selectedImagePath);

                OnPropertyChanged("FilesAndIcons");
            }
        }

        /// <summary>
        /// Gets all files from the given path.
        /// </summary>
        /// <param name="selectedImagePath"></param>
        private void GetFiles(string selectedImagePath)
        {
            foreach (var fileName in new DirectoryInfo(selectedImagePath)
                .GetFiles()
                // Skipping hidden and system files. 
                .Where(f => !f.Attributes.HasFlag(FileAttributes.Hidden | FileAttributes.System))
                .Select(x => x.FullName).ToList())
            {
                var extractAssociatedIcon = Icon.ExtractAssociatedIcon(fileName);

                // Making FileInfoWithIcon with all the files and their icons.
                if (extractAssociatedIcon != null)
                {
                    var fai = new FileInfoWithIcon
                    {
                        FileInfo = new FileInfo(fileName),
                        ImageSource = Imaging.CreateBitmapSourceFromHIcon(
                            extractAssociatedIcon.Handle,
                            Int32Rect.Empty,
                            BitmapSizeOptions.FromEmptyOptions())
                    };
                    
                    FilesAndIcons.Add(fai);
                }
            }
        }

        /// <summary>
        /// Gets all dictionaries from the given path.
        /// </summary>
        /// <param name="selectedImagePath"></param>
        private void GetDirecotries(string selectedImagePath)
        {
            foreach (var fileName in Directory.GetDirectories(selectedImagePath)
                .Where(d => !new DirectoryInfo(d).Attributes.HasFlag(FileAttributes.Hidden | FileAttributes.System)))
            {
                // Dictionaries do not have associated icon.
                // Other solution: Using the native SHGetFileInfo function.
                var uri = new Uri("pack://application:,,,/Resources/folder.png");
                var fai = new FileInfoWithIcon
                {
                    FileInfo = new FileInfo(fileName),
                    ImageSource = new BitmapImage(uri)
                };

                FilesAndIcons.Add(fai);
            }
        }

        /// <summary>
        /// Generates file path from the given TreeViewItem.
        /// </summary>
        /// <param name="treeviewitem">asd</param>
        /// <returns></returns>
        private static string GenerateFilePath(TreeViewItem treeviewitem)
        {
            string selectedImagePath = "";
            string temp1 = "";

            // recursively iterate through from the treeviewitem to the root and generates the full path
            while (true)
            {
                var temp2 = treeviewitem.Header.ToString();

                if (temp2.Contains(@"\"))
                {
                    temp1 = "";
                }

                selectedImagePath = temp2 + temp1 + selectedImagePath;

                if (treeviewitem.Parent.GetType() == typeof (TreeView))
                {
                    break;
                }

                treeviewitem = ((TreeViewItem) treeviewitem.Parent);
                temp1 = @"\";
            }
            return selectedImagePath;
        }

        /// <summary>
        /// Validates the given path and convert it to FileInfo array
        /// </summary>
        /// <param name="fileOrDirectoryPath"></param>
        /// <returns></returns>
        public FileInfo[] FileAndDirectoryValidator(string fileOrDirectoryPath)
        {
            try
            {
                var attr = File.GetAttributes(fileOrDirectoryPath);
                return attr.HasFlag(FileAttributes.Directory) ? new DirectoryInfo(fileOrDirectoryPath).GetFiles() : new[] {new FileInfo(fileOrDirectoryPath)};
            }
            catch (Exception ex)
            {
                // returns null if file is cannot open or access denied
                if (ex is FileNotFoundException
                    || ex is DirectoryNotFoundException
                    || ex is IOException
                    || ex is UnauthorizedAccessException)
                {
                    return null;
                }

                throw;
            }
        }

        void IDisposable.Dispose()
        {
            _cleanUp.Dispose();
        }
    }
}
