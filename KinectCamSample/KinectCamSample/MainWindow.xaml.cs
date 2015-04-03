using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.Kinect;


namespace KinectCamSample
{
    
    public partial class MainWindow : Window
    {
        private DispatcherTimer intervalo = new DispatcherTimer();
        Skeleton[] totalSkeleton = new Skeleton[6];
        private KinectSensor sensor;
        private byte[] pixelData;
       

        public MainWindow()
        {
            InitializeComponent();
        }

        
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (this.sensor != null && this.sensor.IsRunning)
                this.saveImage();
           
        }
        public void saveImage()
        {
            int segundos = int.Parse(DateTime.Now.Second.ToString());
            string photoname = "Mi foto con Kinect"+segundos+".jpg";
            if (File.Exists(photoname))
                File.Delete(photoname);
            using (FileStream saveimagen = new FileStream(photoname, FileMode.CreateNew))
            {
                BitmapSource imagen = (BitmapSource)VideoControl.Source;
                JpegBitmapEncoder jpg = new JpegBitmapEncoder();
                jpg.QualityLevel = 70;
                jpg.Frames.Add(BitmapFrame.Create(imagen));
                jpg.Save(saveimagen);
                saveimagen.Close();
                txtestatus.Content = "Estatus: "+photoname;

            }
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            this.sensor.Stop();
            this.Close();
        }

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            if (this.sensor != null && this.sensor.IsRunning)
                this.sensor.Stop();
        }

        private void BtnBgn_Click(object sender, RoutedEventArgs e)
        {
            if (this.sensor != null && this.sensor.IsRunning)
                this.sensor.Start();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (KinectSensor.KinectSensors.Count > 0)
            {
                this.sensor = KinectSensor.KinectSensors.FirstOrDefault(sensorItem => sensorItem.Status == KinectStatus.Connected);
                this.sensor.Start();
                this.sensor.ColorStream.Enable();
                this.sensor.ColorFrameReady += this.sensor_ColorFrameReady;

                if (!this.sensor.SkeletonStream.IsEnabled)
                {
                    this.sensor.SkeletonStream.Enable();
                    this.sensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;
                    this.sensor.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(sensor_SkeletonFrameReady);
                }

            }
            else
            {
                MessageBox.Show("No esta conectado el Kinect a su ordenador");
                this.Close();
            }
        }

        private void sensor_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                // Verifica que se encontró un esqueleto.
                if (skeletonFrame == null)
                {
                    return;
                }

                //copia la información del frame en la colección
                skeletonFrame.CopySkeletonDataTo(totalSkeleton);

                //Obtiene el primer esqueleto
                Skeleton firstSkeleton = (from trackskeleton in totalSkeleton
                                          where trackskeleton.TrackingState == SkeletonTrackingState.Tracked
                                          select trackskeleton).FirstOrDefault();


                //Aqui verificamos si el primer esqueleto regresa nulo, es decir no encontrado
                if (firstSkeleton == null)
                {
                    return;
                }

                //Aqui es lo divertido, es donde detectamos la mano derecha, llamamos al metodo mapping.
                if (firstSkeleton.Joints[JointType.HandRight].TrackingState == JointTrackingState.Tracked)
                {

                    //this.MapJointsWithUIElement(firstSkeleton);
                    this.saveImage();
                    this.sensor.Stop();

                }
            }
        }

      

        private void sensor_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (ColorImageFrame imageFrame = e.OpenColorImageFrame())
            {
                if (imageFrame == null)
                    return;
                else
                {
                    this.pixelData = new byte[imageFrame.PixelDataLength];
                    imageFrame.CopyPixelDataTo(this.pixelData);
                    int stride = imageFrame.Width * imageFrame.BytesPerPixel;
                    this.VideoControl.Source = BitmapSource.Create(imageFrame.Width, imageFrame.Height, 96, 96, PixelFormats.Bgr32, 
                    null, pixelData, stride);

                }
            }
        }
        public void starTimer()
        {
            this.intervalo.Interval = new TimeSpan(0, 0, 10);
            this.intervalo.Start();
          this.intervalo.Tick +=intervalo_Tick;
        }

        public void StopTimer()
    {
        this.intervalo.Stop();
        this.intervalo.Tick -= this.intervalo_Tick;
    }
        private void intervalo_Tick(object sender, EventArgs e)
        {
            if (this.sensor != null && this.sensor.IsRunning)
                this.saveImage();
        }

        private void TomaAutomatica_Checked(object sender, RoutedEventArgs e)
        {
            if (TomaAutomatica.IsChecked == true)
            {
                this.starTimer();
            }
            else
                this.StopTimer();
        }

        

     


      
    }
}
