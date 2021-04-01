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


namespace MinWindowSizeSample
{
    public sealed partial class MainWindow : ExtendedWindow
    {
        public MainWindow()
        {
            this.InitializeComponent();

            this.Title = "Testing Extended Window apps";
            
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

            debugTBox.Text = $" Size (Height: { this.Height } Width: { this.Width })";


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

        private void MainWindow_Moving(object sender, WindowMovingEventArgs e)
        {
            debugTBox.Text = $"Height: { this.Height } Width: { this.Width }\n Top: {e.Top} Left:{e.Left}";
            
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
            SetWindowPlacement(Placement.Center); ;
        }
    }
}
