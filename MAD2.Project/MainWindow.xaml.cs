using Microsoft.Win32;
using System;
using System.Windows;

namespace MAD2.Project
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly MainViewModel mainViewModel;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = mainViewModel = new MainViewModel();

#if DEBUG
            var args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
                _ = mainViewModel.LoadDatasetAsync(args[1]);
            if(args.Length > 2)
                _ = mainViewModel.LoadNodeInformationAsync(args[2]);
#endif
        }

        private async void LoadDataset(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Multiselect = false,
                CheckPathExists = true,
                InitialDirectory = Environment.CurrentDirectory,
                Filter = "UNICET DL Format files|*.dl",
            };

            if (openFileDialog.ShowDialog() == true)
                await mainViewModel.LoadDatasetAsync(openFileDialog.FileName);
        }

        private async void LoadNodes(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Multiselect = false,
                CheckPathExists = true,
                InitialDirectory = Environment.CurrentDirectory,
            };

            if (openFileDialog.ShowDialog() == true)
                await mainViewModel.LoadNodeInformationAsync(openFileDialog.FileName);
        }

        private void BasicAnalysis(object sender, RoutedEventArgs e)
        {
            mainViewModel.BasicAnalysis();
        }

        private void Test(object sender, RoutedEventArgs e)
        {
            mainViewModel.AverageDegree++;
        }

        private void CalculateModularity(object sender, RoutedEventArgs e)
        {
            mainViewModel.CalculateModularity();
        }
    }
}
