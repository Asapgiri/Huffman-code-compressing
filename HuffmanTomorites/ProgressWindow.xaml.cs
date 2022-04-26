using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace HuffmanTomorites {
    /// <summary>
    /// Interaction logic for ProgressWindow.xaml
    /// </summary>
    public partial class ProgressWindow : Window {
        public Task Progress { get; set; }
        public CancellationTokenSource CancelToken { get; set; }

        public ProgressWindow(bool ki = false) {
            InitializeComponent();
            if (ki) {
                TomoritoLabel.Content = "Kitömörítés folyamatban:";
            }
            Closing += ProgressWindow_Closing;
        }

        private void ProgressWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e) {
            if (Progress != null) {
                CancelToken.Cancel();
            }
        }

        public void SetupProgressBar(double max) {
            Dispatcher.Invoke(() => {
                ProgressBar.Minimum = 0;
                ProgressBar.Maximum = max;
                ProgressBar.Value = 0;
            });
        }

        public void MakeProgress(int progress = 0) {
            Dispatcher.Invoke(() => {
                if (progress > 0) {
                    ProgressBar.Value += progress;
                }
                else {
                    ProgressBar.Value++;
                }
            });
        }
    }
}
