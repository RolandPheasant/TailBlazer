﻿using System.ComponentModel;
using MahApps.Metro.Controls;
using TailBlazer.Views.WindowManagement;
using System.Windows;
using System.Windows.Input;
using System;

namespace TailBlazer
{

    public delegate void WindowClosing();

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {

        public MainWindow()
        {
            InitializeComponent();

            Closing += MainWindow_Closing;
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {

            var windowsModel = DataContext as WindowViewModel;
            windowsModel?.OnWindowClosing();
        }


        private void StackPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = false;
        }


        private void pin_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }


    }
}
