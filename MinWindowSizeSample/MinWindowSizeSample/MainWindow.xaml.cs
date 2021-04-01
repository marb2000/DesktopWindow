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
using WinUI.Desktop;


namespace DesktopWindowSample
{
    public sealed partial class MainWindow : DesktopWindow
    {
        public MainWindow()
        {
            this.InitializeComponent();

            this.Title = "Showcase DesktopWindow API";
            
            //NEW Properties
            this.Closing += MainWindow_Closing;
            this.MaxHeight = 800;
            this.MaxWidth = 1024;
            this.MinHeight = 400;
            this.MinWidth = 400;

            this.Width = 600;
            this.Height = 600;

            this.Icon = "WinUI3.ico";

            SetWindowPlacement(0,0);

            this.Moving += MainWindow_Moving;
            this.Sizing += MainWindow_Sizing;

            debugTBox.Text = $" Size (Height: { this.Height } Width: { this.Width })";

            //Events SizeChanged, and WindowMoved
            //Window Moving?
            //Windoww draggable area
            //Remove Window Borders
            //Full Screen Mode
            //WinPro hook
            //Compact Overlay
            //Restore Window Position?
            //            myWindow.MinimizeWindow();
            //            myWindow.MaximizeWindow();
            //            myWindow.RestoreWindow();
            //            myWindow.HideWindow();`
            //Move and resize window
            //   myWindow.CenterOnScreen();
            //            myWindow.SetWindowPositionAndSize(100, 100, 1024, 768);
            //            Make Window always - on - top
            //    myWindow.SetAlwaysOnTop(true);
            //            Bring window to the top
            //    myWindow.BringToFront();
        }


        private void MainWindow_Sizing(object sender, WindowSizingEventArgs e)
        {
            var windowPosition = GetWindowPosition();
            debugTBox.Text = $"Height: { this.Height } Width: { this.Width }\n " +
                $"Top: {windowPosition.Top} Left:{windowPosition.Left}";
        }

        private void MainWindow_Moving(object sender, WindowMovingEventArgs e)
        {
            //var windowPosition = GetWindowPosition();


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
                //Should this be Cancel?
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
    }
}
