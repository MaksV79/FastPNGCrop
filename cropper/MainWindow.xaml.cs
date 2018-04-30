using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.IO;
using System.Windows.Media.Imaging;

namespace cropper
{
    public partial class MainWindow : Window
    {
        List<string> pngFiles = new List<string>();
        int ListIdx=0;
        double MinX, MaxX, MinY, MaxY;
        BitmapImage bitmapImage, next_bitmapImage;
        bool SelectionMode = true;

        int RealX = 0; // mouse position over image in terms of real file resolution
        int RealY = 0;
        int FormX = 0; // mouse position over image in terms of windows form coordinates
        int FormY = 0;
        int PrevWidth = 0, PrevHeight = 0;
        string ImageFile = "";

        public MainWindow()
        {
            InitializeComponent();

            string[] args = Environment.GetCommandLineArgs();
            

            var dlg = new CommonOpenFileDialog();
            dlg.Title = "Select folder with PNG images to crop";
            dlg.IsFolderPicker = true;
            if (args.Length > 1)
            {
                dlg.InitialDirectory = args[1];
                dlg.DefaultDirectory = args[1];
            }
            else
            {
                dlg.InitialDirectory = "c:\\fakes\\";
                dlg.DefaultDirectory = "c:\\fakes\\";
            }
            
            dlg.AddToMostRecentlyUsedList = false;
            dlg.AllowNonFileSystemItems = false;            
            dlg.EnsureFileExists = true;
            dlg.EnsurePathExists = true;
            dlg.EnsureReadOnly = false;
            dlg.EnsureValidNames = true;
            dlg.Multiselect = false;
            dlg.ShowPlacesList = true;

            if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
            {
                var folder = dlg.FileName;
                // Do something with selected folder string
                ProcessFolder(folder);
            }
            else //exit 
            {
                System.Windows.Application.Current.Shutdown(); 
            }
            Height = mainImage.Height;
            Width = mainImage.Width;

            this.PreviewKeyDown += new KeyEventHandler(onKeyboardKeyDown);
            this.MouseMove += mainImage_MouseMove;
            this.MouseLeftButtonDown += mainImage_MouseLeftButtonDown;
            this.MouseLeftButtonUp += mainImage_MouseLeftButtonUp;
        }

        public void ProcessFolder(string fldName)
        {
            string[] fileEntries = Directory.GetFiles(fldName);            
            foreach (string fileName in fileEntries)
                if (fileName.ToLower().Substring(fileName.Length-4,4) == ".png") pngFiles.Add(fileName);
            if (pngFiles.Count > 0) { DisplayImage(); }
            else { MessageBox.Show("No PNG files found"); }
            
        }

        public void DisplayImage()
        {
            try
            {
                if (ListIdx >= pngFiles.Count) { lblStatusBar.Text = "All files processed"; return; }
                ImageFile = pngFiles[ListIdx];
                byte[] image_data = File.ReadAllBytes(ImageFile);
                bitmapImage = new BitmapImage();
                using (MemoryStream memory = new MemoryStream(image_data))
                {
                    bitmapImage.BeginInit();
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.StreamSource = memory;
                    bitmapImage.EndInit();
                    bitmapImage.Freeze();
                    mainImage.Source = bitmapImage;
                }

                SelectionMode = false;
                if ((PrevWidth != (int)bitmapImage.Width) || (PrevHeight != (int)bitmapImage.Height)) //if size of new image changed comparing to previous image
                {
                    PrevWidth = (int)bitmapImage.Width;
                    PrevHeight = (int)bitmapImage.Height;
                    RemoveSelection();
                }

                lblStatusBar.Text = ListIdx.ToString()+"/"+pngFiles.Count.ToString()+" "+ ImageFile + " " + bitmapImage.Width.ToString() + "x" + bitmapImage.Height.ToString();                
            }
            catch (Exception ex)
            {
                lblStatusBar.Text = ex.Message;
            }
        }
        
        private void mainImage_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {                                
                if (SelectionMode)
                {
                    double X = e.GetPosition(mainImage).X;
                    double Y = e.GetPosition(mainImage).Y;
                    double scale = bitmapImage.Width / mainImage.ActualWidth;


                    if (X < 0) { X = 0; }
                    if (Y < 0) { Y = 0; }
                    if (X > mainImage.ActualWidth) { X = mainImage.ActualWidth; }
                    if (Y > mainImage.ActualHeight) { Y = mainImage.ActualHeight; }

                    RealX = (int)(X * scale);
                    RealY = (int)(Y * scale);

                    if (MinX == 0)
                    {
                        imgGrid.RowDefinitions[0].Height = new GridLength(e.GetPosition(MainForm).Y);
                        imgGrid.ColumnDefinitions[0].Width = new GridLength(e.GetPosition(MainForm).X);
                    }
                    else
                    {
                        if (MaxX == 0)
                        {
                            imgGrid.RowDefinitions[2].Height = new GridLength(MainForm.ActualHeight - e.GetPosition(MainForm).Y);
                            imgGrid.ColumnDefinitions[2].Width = new GridLength(MainForm.ActualWidth - e.GetPosition(MainForm).X);
                        }
                    }
                    lblStatusBar.Text = ((Int32)X).ToString() + "x" + ((Int32)Y).ToString() + " scale " + (float)scale + " " + ((Int32)RealX).ToString() + "x" + ((Int32)RealY).ToString();
                }
            }
            catch { }            
        }

        private void mainImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (SelectionMode)
            {
                if (MinX == 0) { MinX = RealX; }
                else { MaxX = RealX; }
                if (MinY == 0) { MinY = RealY; }
                else { MaxY = RealY; }
            }
                    
        }
        private void mainImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (SelectionMode)
            {
                
            }
            else
            {
                RemoveSelection();
            }
        }

        private void RemoveSelection()
        {
            MinX = 0;
            MaxX = 0;
            MinY = 0;
            MaxY = 0;
            imgGrid.RowDefinitions[0].Height = new GridLength(0);
            imgGrid.ColumnDefinitions[0].Width = new GridLength(0);
            imgGrid.RowDefinitions[2].Height = new GridLength(0);
            imgGrid.ColumnDefinitions[2].Width = new GridLength(0);
            SelectionMode = true;
        }

        private void onKeyboardKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.Right) || (e.Key == Key.NumPad6)) //skip this file
            {
                ++ListIdx;
                DisplayImage();
            }
            if (e.Key == Key.Enter)  //crop
            {
                Crop();
            }
            if (e.Key == Key.Escape)  //crop
            {
                RemoveSelection();
            }
            if (e.Key == Key.Delete)  //crop
            {
                File.Delete(pngFiles[ListIdx]);
                ++ListIdx;
                DisplayImage();
            }
        }

        private void Crop()
        {
            if ( (MaxX - MinX <= 0) || (MaxY - MinY <= 0) )
            {
                RemoveSelection();
                return;
            }
            var crb = new CroppedBitmap(bitmapImage, new System.Windows.Int32Rect((Int32)MinX, (Int32)MinY, (Int32)(MaxX-MinX), (Int32)(MaxY-MinY)));
            
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(crb));
            using (var filestream = new FileStream(ImageFile, FileMode.Create))
            {
                encoder.Save(filestream);
            }
            ++ListIdx;
            DisplayImage();
        }
        

    }
}
