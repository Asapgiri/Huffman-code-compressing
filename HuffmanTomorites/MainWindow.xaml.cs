using Microsoft.Win32;
using System.Windows;
using System.IO;
using System;
using System.Windows.Controls;
using System.Threading.Tasks;
using System.Diagnostics;

namespace HuffmanTomorites {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private const string FILE_EXTENSION = "hfzip";

        private string[] filesToZip;
        private string fileToUnzip;

        public MainWindow() {
            InitializeComponent();
        }

        private void Button_SelectFilesToZipClick(object sender, RoutedEventArgs e) {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;

            if (true == openFileDialog.ShowDialog()) {
                filesToZip = openFileDialog.FileNames;

                SelectedFilesTozip.Items.Clear();
                for (int i = 0; i < filesToZip.Length; i++) {
                    SelectedFilesTozip.Items.Add(filesToZip[i]);
                }
            }
        }

        private void Button_SelectFileToUnzipClick(object sender, RoutedEventArgs e) {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = $"{FILE_EXTENSION} files (*.{FILE_EXTENSION})|*.{FILE_EXTENSION}|All files (*.*)|*.*";

            if (true == openFileDialog.ShowDialog()) {
                if (!SelectedFileToUnzip.Items.Contains(openFileDialog.FileName)) {
                    SelectedFileToUnzip.Items.Add(openFileDialog.FileName);
                    fileToUnzip = openFileDialog.FileName;
                }
                SelectedFileToUnzip.SelectedItem = openFileDialog.FileName;
            }
        }

        private void Button_SelectNewZipFileClick(object sender, RoutedEventArgs e) {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = $"{FILE_EXTENSION} files (*.{FILE_EXTENSION})|*.{FILE_EXTENSION}|All files (*.*)|*.*";
            saveFileDialog.FileName = "tömmfile." + FILE_EXTENSION;

            if (true == saveFileDialog.ShowDialog()) {
                FileToZip.Text = saveFileDialog.FileName;
            }
        }

        private void Button_SelectFolderToUnzipClick(object sender, RoutedEventArgs e) {
            OpenFileDialog folderBrowser = new OpenFileDialog();
            folderBrowser.ValidateNames = false;
            folderBrowser.CheckFileExists = false;
            folderBrowser.CheckPathExists = true;
            folderBrowser.FileName = "Folder Selection.";

            if (true == folderBrowser.ShowDialog()) {
                string? folderPath = Path.GetDirectoryName(folderBrowser.FileName);
                if (null != folderPath) {
                    FolderToUnzip.Text = folderPath;
                }
            }
        }

        private void Button_TomoritesClick(object sender, RoutedEventArgs e) {
            if (SelectedFilesTozip.Items.Count == 0) {
                MessageBox.Show("Kérem válasszon ki tömörítendő fájlokat!");
                return;
            }
            if (string.Empty == FileToZip.Text) {
                MessageBox.Show("Kérem válasszon ki fájlt a tömörítéshez!");
                return;
            }

            var pw = new ProgressWindow();
            pw.Show();
            string newFilename = FileToZip.Text;
            pw.CancelToken = new System.Threading.CancellationTokenSource();
            pw.Progress = Task.Factory.StartNew(() => {
                if (Coder.CompressFiles(filesToZip, newFilename, pw)) {
                    MessageBox.Show("Sikeres tömörítés!");
                }
                else {
                    MessageBox.Show("A tömörítés sikertelen!");
                }
            }).ContinueWith((t) => {
                pw.Close();
                Process.Start("explorer.exe", $"/select, {FileToZip.Text}");
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void Button_KitomoritesClick(object sender, RoutedEventArgs e) {
            if (null == fileToUnzip || string.Empty == fileToUnzip) {
                MessageBox.Show("Kérem válasszon ki kitömörítendő fájlt!");
                return;
            }
            if (string.Empty == FolderToUnzip.Text) {
                MessageBox.Show("Kérem válasszon ki mappát ahova kitömörít!");
                return;
            }

            var pw = new ProgressWindow(true);
            pw.Show();
            string folderName = FolderToUnzip.Text;
            pw.CancelToken = new System.Threading.CancellationTokenSource();
            pw.Progress = Task.Factory.StartNew(() => {
                if (Coder.DeCompressFiles(fileToUnzip, folderName, pw)) {
                    MessageBox.Show("Sikeres kitömörítés!");
                }
                else {
                    MessageBox.Show("A kitömörítés sikertelen!");
                }
            }).ContinueWith((t) => {
                pw.Close();
                Process.Start("explorer.exe", FolderToUnzip.Text);
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void SelectedFileToUnzip_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) {
            fileToUnzip = (string)SelectedFileToUnzip.SelectedItem;
        }
    }
}
