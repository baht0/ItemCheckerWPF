﻿using ItemChecker.MVVM.ViewModel;
using System.Windows;
using System.Windows.Input;

namespace ItemChecker.MVVM.View
{
    /// <summary>
    /// Interaction logic for ImportParserWindow.xaml
    /// </summary>
    public partial class ImportParserWindow : Window
    {
        public string ReturnValue { get; set; } = string.Empty;
        public ImportParserWindow()
        {
            InitializeComponent();
            this.DataContext = new ImportParserViewModel();
        }
        private void Window_DragMove(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
        private void Button_Close(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            object item = filesGrid.CurrentItem;
            if (!filesGrid.Items.IsEmpty && item != null)
            {
                this.ReturnValue = ((Model.ImportFile)item).Path;
                this.Close();
            }
        }
    }
}