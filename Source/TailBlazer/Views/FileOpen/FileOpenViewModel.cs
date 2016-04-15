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
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace TailBlazer.Views.FileOpen
{
    /// <summary>
    /// View Model for the MaterialDesign based OpenDialog
    /// </summary>
    public class FileOpenViewModel : AbstractNotifyPropertyChanged, IDisposable
    {
        public Guid Id { get; } = Guid.NewGuid();
        private readonly IDisposable _cleanUp;
        private string _selectedTreeViewItemPath;
        public FileInfoWithIcon SelectedItem { get; set; }
        /// <summary>
        /// The content of this list will display on the right side of the Open Dialog
        /// </summary>
        public List<FileInfoWithIcon> FilesAndIcons { get; set; }
        public ICommand SelectedItemChangedCommand { get; set; }    
        public ICommand OpenSelectedItemCommand { get; set; }

        public FileOpenViewModel(Func<IEnumerable<FileInfo>, Task> openFile)
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
        private async void OpenSelectedItemCommandMethod(Func<IEnumerable<FileInfo>, Task> openFile, RoutedCommand closeCommand)
        {

            if (SelectedItem?.FileInfo?.FullName != null)
            {
                await openFile(FileAndDirectoryValidator(SelectedItem.FileInfo.FullName));
                closeCommand?.Execute(null, null);
            }
            else if (!string.IsNullOrEmpty(_selectedTreeViewItemPath))
            {
                await openFile(FileAndDirectoryValidator(_selectedTreeViewItemPath));
                closeCommand?.Execute(null, null);
            }
            else
            {
                //TODO: Notify user to select a folder or file
            }
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
                _selectedTreeViewItemPath = GenerateFilePath(temp);

                FilesAndIcons = new List<FileInfoWithIcon>();
                FilesAndIcons.AddRange(GetDirecotries(_selectedTreeViewItemPath));
                FilesAndIcons.AddRange(GetFiles(_selectedTreeViewItemPath));

                OnPropertyChanged("FilesAndIcons");
            }
        }

        /// <summary>
        /// Gets all files from the given path.
        /// </summary>
        /// <param name="selectedImagePath"></param>
        private List<FileInfoWithIcon> GetFiles(string selectedImagePath)
        {
            List<FileInfoWithIcon> fiwiList = new List<FileInfoWithIcon>();
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
                    
                    fiwiList.Add(fai);
                }
            }
            return fiwiList;
        }

        /// <summary>
        /// Gets all dictionaries from the given path.
        /// </summary>
        /// <param name="selectedImagePath"></param>
        private List<FileInfoWithIcon> GetDirecotries(string selectedImagePath)
        {
            List<FileInfoWithIcon> fiwiList = new List<FileInfoWithIcon>();
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

                fiwiList.Add(fai);
            }
            return fiwiList;
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
                return attr.HasFlag(FileAttributes.Directory) ? GetFiles(fileOrDirectoryPath).Select(t => t.FileInfo).ToArray() : new[] {new FileInfo(fileOrDirectoryPath)};
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
