using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using WinUIExtensions.Desktop;


namespace DesktopWindowSample
{
    public sealed partial class MainWindow : DesktopWindow
    {
        public MainWindow()
        {
            this.InitializeComponent();

            // MUX.Window features
            this.Title = "Showcase DesktopWindow API";
            this.ExtendsContentIntoTitleBar = true;
            SetTitleBar(myTitleBar);

            //DesktopWindow Features
            this.MaxHeight = 800;
            this.MaxWidth = 1024;
            this.MinHeight = 400;
            this.MinWidth = 400;

            this.Width = 600;
            this.Height = 600;

            this.Icon = "WinUI3.ico";

            SetWindowPlacement(0, 0);

            this.Closing += MainWindow_Closing;
            this.Moving += MainWindow_Moving;
            this.Sizing += MainWindow_Sizing;
            this.DpiChanged += MainWindow_DpiChanged;
            this.OrientationChanged += MainWindow_OrientationChanged;

            debugTBox.Text = $" Size (Height: { this.Height } Width: { this.Width })";


            var list = WinUIExtensions.Desktop.DisplayInformation.GetDisplays();

            foreach (var item in list)
            {
                systemTBox.Text += $"Display ->" +
                    $"\n  Device Name: {item.DeviceName}" +
                    $"\n  Work Area: ({item.WorkArea.top},{item.WorkArea.left},{item.WorkArea.bottom},{item.WorkArea.right})" +
                    $"\n  ScreenHeight: {item.ScreenHeight}" +
                    $"\n  ScreenWidth:{item.ScreenWidth}" +
                    $"\n  Effective Pixels: width({item.ScreenEfectiveWidth}), height({item.ScreenEfectiveHeight})" +
                    $"\n  Availability {item.Availability}\n\n";
            }

            dpiChangedTBox.Text =  $"DPI: {this.Dpi}";
        }

        private void MainWindow_OrientationChanged(object sender, WindowOrientationChangedEventArgs e)
        {
            dpiChangedTBox.Text = $"DPI: {this.Dpi} + Orientation: { e.Orientation.ToString("g")}";
        }

        private void MainWindow_DpiChanged(object sender, WindowDpiChangedEventArgs e)
        {
            dpiChangedTBox.Text = $"DPI Changed:{ e.Dpi} - {this.Dpi} ";
        }

        //public static T FindControl<T>(DependencyObject parent, string ControlName) where T : FrameworkElement
        //{
        //    if (parent == null)
        //        return null;

        //    if (parent.GetType() == typeof(T) && ((T)parent).Name == ControlName)
        //    {
        //        return (T)parent;
        //    }
        //    T result = null;
        //    int count = VisualTreeHelper.GetChildrenCount(parent);
        //    for (int i = 0; i < count; i++)
        //    {
        //        DependencyObject child = VisualTreeHelper.GetChild(parent, i);

        //        if (FindControl<T>(child, ControlName) != null)
        //        {
        //            result = FindControl<T>(child, ControlName);
        //            break;
        //        }
        //    }
        //    return result;
        //}

        private void MainWindow_Sizing(object sender, WindowSizingEventArgs e)
        {
            var windowPosition = GetWindowPosition();
            debugTBox.Text = $"Height: { this.Height } Width: { this.Width }\n " +
                $"Top: {windowPosition.Top} Left:{windowPosition.Left}";
        }

        private void MainWindow_Moving(object sender, WindowMovingEventArgs e)
        {
            debugTBox.Text = $"Height: { this.Height } Width: { this.Width }\n " +
                $"Top: {e.NewPosition.Top} Left:{e.NewPosition.Left}";
        }

        private async void MainWindow_Closing(object sender, WindowClosingEventArgs e)
        {
            ContentDialog contentDialog = new ContentDialog();
            contentDialog.Content = "Close it?";
            contentDialog.XamlRoot = this.Content.XamlRoot;
            contentDialog.PrimaryButtonText = "Yes";
            contentDialog.CloseButtonText = "No";
            contentDialog.IsPrimaryButtonEnabled = true;
            var r = await contentDialog.ShowAsync();
            if (r == ContentDialogResult.Primary)
            {
                e.TryCancel();
            }
        }

        private void OnCenterClick(object sender, RoutedEventArgs e)
        {
            SetWindowPlacement(Placement.Center);
        }

        private void OnTopLeft(object sender, RoutedEventArgs e)
        {
            SetWindowPlacement(Placement.TopLeftCorner);
        }

        private void OnBottomLeft(object sender, RoutedEventArgs e)
        {
            SetWindowPlacement(Placement.BottomLeftCorner);
        }

        private void OnMaximize(object sender, RoutedEventArgs e)
        {
            Maximize();
        }

        private void OnMinimize(object sender, RoutedEventArgs e)
        {
            Minimize();
        }

        private void OnRestore(object sender, RoutedEventArgs e)
        {
            Restore();
        }

        private void OnBringToTop(object sender, RoutedEventArgs e)
        {
            BringToTop();
        }
        
        private void OnClosingApplication(object sender, RoutedEventArgs e)
        {
            Application.Current.Exit();
        }
    }
}
