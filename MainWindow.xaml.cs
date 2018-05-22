using System;
using System.Collections.Generic;
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
using LiveCharts;
using LiveCharts.Wpf;
using LiveCharts.Defaults;
using LiveCharts.Configurations;
using System.ComponentModel;
using System.IO;
using System.IO.Ports;
using System.Globalization;
using System.Threading;
using Microsoft.Maps.MapControl.WPF;
using System.Windows.Media.Media3D;

namespace GUI_v1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {

        public List<SeriesCollection> Chartseries { get; set; }
        public List<Func<double, string>> YFormatter { get; set; }
        public List<double> CValues;
        public int index = 0;
        public int comboFlag = 0;
        public int num;
        public int n=0;
        public int b;
        public int a;
        public List<CartesianChart> Charts;
        public List<Axis> axis;
        public LineSeries myseries;
        public int num1 = 0, num2 = 0, num3 = 0;
        public List<Label> labels;
        public Random random;
        int x = 0;
        int y = 0;
        int z = 0;
        public double val1;
        public double val2;
        public MapPolyline line;
        public Location loc;
        public string datastring;
        public String[] data;

        SerialPort serial;

        public MainWindow()
        {
            InitializeComponent();

            random = new Random();
            //3D plot
            TheCamera = new PerspectiveCamera();
            //Charts
            Chartseries = new List<SeriesCollection>();
            Charts = new List<CartesianChart> { C0, C1, C2, C3 };
            axis = new List<Axis> { a0, a1, a2, a3 };
            YFormatter = new List<Func<double, string>>();            
            CValues = new List<double> { 0, 0, 0, 0 };
            //labels
            labels = new List<Label> { lb1, lb2, lb3, lb4, lb5, lb6, lb7, lb8, lb9, lb10, lb11, lb12, lb13, lb14, lb15, lb16, lb17, lb18, lb19 };
            //Bing map
            line = new MapPolyline();
            //loc = new Location();
            line.Stroke = new SolidColorBrush(Colors.Red);
            line.StrokeThickness = 1;
            line.Locations = new LocationCollection();
            bingmap.Children.Add(line);
            datastring = "";
            /*loc.Latitude = 0;
            loc.Longitude = 0;
            loc.Altitude = 0;*/

            //initialize serial port
            serial = new SerialPort();
            serial.BaudRate = 38400;
            serial.PortName = "COM9";
            serial.DataReceived += new SerialDataReceivedEventHandler(datareceivedhandler);
            serial.Open();

            
            //Adding series to the series collection
            for (int i = 0; i < 4; i++)
            {
                myseries = new LineSeries();
                myseries.Values = new ChartValues<double>();
                SeriesCollection tempseries = new SeriesCollection();
                tempseries.Add(myseries);
                Chartseries.Add(tempseries);
            }

            //Adding charts and axes to the series collection
            for (int i = 0; i < 4; i++)
            {
                YFormatter.Add(Value => Value + " ");
                Charts[i].Series = Chartseries[i];
                //Charts[i].AxisY.Add(axis[i]);
                axis[i].LabelFormatter = YFormatter[i];
            }

            //Defining label format for the axes
            YFormatter[0] = value => value.ToString("0") + "°C";
            axis[0].LabelFormatter = YFormatter[0];
            YFormatter[1] = value => value.ToString("0") + "°C";
            axis[1].LabelFormatter = YFormatter[1];
            YFormatter[2] = value => value.ToString("0") + "Pa";
            axis[2].LabelFormatter = YFormatter[2];
            YFormatter[3] = value => value.ToString("0") + "m";
            axis[3].LabelFormatter = YFormatter[3];

            //initialize timer
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;
            timer.Start();

            //initialize background worker for mission time
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += worker_DoWork;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerAsync(a);

            //initialize background worker for temperature
            BackgroundWorker worker1 = new BackgroundWorker();
            worker1.WorkerReportsProgress = true;
            worker1.DoWork += worker1_DoWork;
            worker1.ProgressChanged += worker1_ProgressChanged;
            worker1.RunWorkerAsync(b);

            DataContext = this;
        }

        private void datareceivedhandler(object sender, SerialDataReceivedEventArgs e)
        {

            /*Thread.Sleep(100);
            SerialPort serial = (SerialPort)sender;
            //serial.ReadTo("\n");
            //datastring = serial.ReadExisting();
            //serial.ReadTo("\n");
            Console.WriteLine("START");
            int a = serial.BytesToRead;
            Console.WriteLine(a);
            //datastring = serial.ReadLine();
            Console.WriteLine(Convert.ToString(serial.ReadExisting()));
            //datastring = Convert.ToString(serial.ReadChar());
            Console.WriteLine("STOP");
            a = serial.BytesToRead;
            //Console.WriteLine(a);
            //Console.Write(datastring);
            char[] datasplit = { '2', '6', '1', '0' };
            //datastring = datastring.Split(datasplit)[1];
            //Console.WriteLine(datastring.Length);*/

            this.Dispatcher.Invoke(() =>
            {

                Thread.Sleep(500);
                SerialPort serial = (SerialPort)sender;
                //Console.WriteLine("START");
                int a = serial.BytesToRead;
                Console.WriteLine(a);

                datastring = serial.ReadExisting();
                char[] datasplit = { '2', '6', '1', '0' };
                data = datastring.Split('P');  // start bit is P, hence splitting it from P and taking the right part
                data = data[1].Split('X'); // Stop bit is X, Splitting from X and taking the left part
                if (data.Length > 1)
                {
                    data = data[0].Split(',');  // again splitting data
                }
                Console.Write("\n----------------------------start---------------------------------------\n");
                for (int i = 1; i < data.Length; i++) // starting from as 1st data is 0
                {
                    Console.WriteLine(data[i]);
                    try
                    {
                       
                            labels[i-1].Content = data[i];
                        
                    }
                    catch (Exception ex)
                    {
                        labels[i - 1].Content = 0; //labels[i-1].Content; //Keeping label content same as previous
                        Console.WriteLine("exception" + ex);
                    }
                }
                Console.Write("\n----------------------------end---------------------------------------\n");
                Console.WriteLine(datastring.Length);
                //Console.WriteLine("STOP");
                a = serial.BytesToRead;
                Console.WriteLine(a);
            });

        }

        void timer_Tick(object sender, EventArgs e)
        {
            //Labels (14-19)
            for(int i=0; i<5;i++)
            {
                labels[i+14].Content = labels[i].Content;
            }
          

            //Cartesian Charts
            for (int i = 0; i < 4; i++)
            {
                //CValues[i] = random.Next(10, 200);
                try
                {
                    CValues[i] = Convert.ToDouble(data[6 - i]);
                    CValues[0] = Convert.ToDouble(data[5 - index]);
                }
                catch(Exception ex)
                {
                    CValues[i] = 0;
                    Console.WriteLine("Exception Occured" + ex);
                }
                (Chartseries[i])[0].Values.Add(CValues[i]);
                if ((Chartseries[i])[0].Values.Count > 30)
                    (Chartseries[i])[0].Values.RemoveAt(0);                 
            }

            //Bing map
            loc = new Location(); 
            /*num1 = random.Next(-100, 100);
            num2 = random.Next(-50, 50);
            num3 = random.Next(0, 1000);
            loc.Latitude = num1;
            loc.Longitude = num2;
            loc.Altitude = num3;*/
            try
            {
                loc.Latitude = Convert.ToDouble(data[8]);
                loc.Longitude = Convert.ToDouble(data[9]);
                loc.Altitude = Convert.ToDouble(data[10]);
            }
            catch(Exception f)
            {
                Console.WriteLine("exception" + f);
            }
  
            line.Locations.Add(loc);
            if (line.Locations.Count > 20)
                line.Locations.RemoveAt(0);

            //Angular Gauge
            //num = new Random().Next(0, 5);
            //V1.Value = num;
            try
            {
                V1.Value = Convert.ToDouble(data[6]);
            }
            catch(Exception ex1)
            {
                V1.Value = 0;
                Console.WriteLine("Exception occured" + ex1);
            }


            //Mission Time Progress Bar
            try
            {
                n = (int)Convert.ToDouble(data[3]);
            }
            catch (Exception ex2)
            {
                V1.Value = 0;
                Console.WriteLine("Exception occured" + ex2);
            } 
            int maxheight = 600; 
            a = (maxheight - n);
            a = (a / maxheight)*100;

            //Temperature progress bar
            try
            {
                b = (int)Convert.ToDouble(data[5]);
            }
            catch (Exception ex3)
            {
                V1.Value = 0;
                Console.WriteLine("Exception occured" + ex3);
            }
            

            //3D Plot
            //val1 = new Random().Next(10, 100);
            //val2 = new Random().Next(20, 100);
            try
            {
                val1 = Convert.ToDouble(data[13]);
                val2 = Convert.ToDouble(data[14]);
            }
            catch (Exception ex4)
            {
                V1.Value = 0;
                Console.WriteLine("Exception occured" + ex4);
            }

            CameraTheta = val1;
            CameraPhi = val2; 
            PositionCamera();
        }

        private void cmb_SelectionChanged(object sender, SelectionChangedEventArgs e) //Combobox for the charts
        {
            switch (Select.SelectedIndex)
            {
                case 0:                 //temperature
                    index = 0;
                    if (comboFlag != 0)
                    {
                        (Chartseries[0])[0].Values.Clear();
                        YFormatter[0] = value => value.ToString("0") + "°C";
                        axis[0].LabelFormatter = YFormatter[0];
                        comboFlag = 0;
                        axis[0].Title = "Temperature";
                    }
                    break;
                case 1:                 //pressure
                    index = 1;
                    if (comboFlag != 1)
                    {
                        (Chartseries[0])[0].Values.Clear();
                        YFormatter[0] = value => value.ToString("0") + "Pa";
                        axis[0].LabelFormatter = YFormatter[0];
                        axis[0].Title = "Pressure";
                        comboFlag = 1;
                    }
                    break;
                case 2:                 //altitude
                    index = 2;
                    if (comboFlag != 2)
                    {
                        (Chartseries[0])[0].Values.Clear();
                        YFormatter[0] = value => value.ToString("0") + "m";
                        axis[0].LabelFormatter = YFormatter[0];
                        axis[0].Title = "Altitude";
                        comboFlag = 2;
                    }
                    break;
            }
        }

        void worker_DoWork(object sender, DoWorkEventArgs e)    //background worker for progress bar of mission time
        {
            int t = (int)e.Argument;
            for (int i = 0; i < 100; i = i++)
            {
                (sender as BackgroundWorker).ReportProgress(a);
                Thread.Sleep(1000);
            }
        }

        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)  //reprting progress to progress bar
        {
            Pbstatus.Value = e.ProgressPercentage;
        }

        void worker1_DoWork(object sender, DoWorkEventArgs e)    //background worker for progress bar of temperature
        {
            int t = (int)e.Argument;
            for (int i = 0; i < 100; i = i++)
            {
                (sender as BackgroundWorker).ReportProgress(b);
                Thread.Sleep(1000);
            }
        }

        void worker1_ProgressChanged(object sender, ProgressChangedEventArgs e)  //reprting progress to progress bar
        {
            Temp.Value = e.ProgressPercentage;
        }

        // The main object model group.
        private Model3DGroup MainModel3Dgroup = new Model3DGroup();

        // The camera.
        public PerspectiveCamera TheCamera;

        // The camera's current location.
        private double CameraPhi = Math.PI / 2;       // angle in y-z plane or angle with y axis
        private double CameraTheta = Math.PI / 2;     // angle in x-y plane or angle with x axis
        private double CameraR = 8.0;

        // The change in CameraPhi when you press the up and down arrows.
        private const double CameraDPhi = 0.02;

        // The change in CameraTheta when you press the left and right arrows.
        private const double CameraDTheta = 0.02;

        // The change in CameraR when you press + or -.
        private const double CameraDR = 0.1;

        // Create the scene.
        // MainViewport is the Viewport3D defined
        // in the XAML code that displays everything.
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Give the camera its initial position.
            TheCamera = new PerspectiveCamera();
            TheCamera.FieldOfView = 60;
            MainViewport.Camera = TheCamera;
            PositionCamera();

            // Define lights.
            DefineLights();

            // Create the model.
            DefineModel(MainModel3Dgroup);

            // Add the group of models to a ModelVisual3D.
            ModelVisual3D model_visual = new ModelVisual3D();
            model_visual.Content = MainModel3Dgroup;

            // Display the main visual to the viewportt.
            MainViewport.Children.Add(model_visual);
        }

        // Define the lights.
        private void DefineLights()
        {
            AmbientLight ambient_light = new AmbientLight(Colors.Gray);
            DirectionalLight directional_light =
                new DirectionalLight(Colors.Gray, new Vector3D(-1.0, -3.0, -2.0));
            MainModel3Dgroup.Children.Add(ambient_light);
            MainModel3Dgroup.Children.Add(directional_light);
        }

        // Add the model to the Model3DGroup.
        private void DefineModel(Model3DGroup model_group)
        {
            // Make a cylinder along the Y axis.
            MeshGeometry3D mesh1 = new MeshGeometry3D();
            AddCylinder(mesh1, new Point3D(0, 1, -1),
                new Vector3D(0, 2, 2), 1, 100);
            SolidColorBrush brush1 = Brushes.LightGreen;
            DiffuseMaterial material1 = new DiffuseMaterial(brush1);
            GeometryModel3D model1 = new GeometryModel3D(mesh1, material1);
            model_group.Children.Add(model1);

            /* // Make a cylinder along the X axis.
             MeshGeometry3D mesh2 = new MeshGeometry3D();
             AddCylinder(mesh2, new Point3D(-1, 0, 0),
                 new Vector3D(2, 0, 0), 0.375, 8);
             SolidColorBrush brush2 = Brushes.Red;
             DiffuseMaterial material2 = new DiffuseMaterial(brush2);
             GeometryModel3D model2 = new GeometryModel3D(mesh2, material2);
             model_group.Children.Add(model2);

             // Make a cylinder along the X axis.
             MeshGeometry3D mesh3 = new MeshGeometry3D();
             AddCylinder(mesh3, new Point3D(0, 0, 1),
                 new Vector3D(0, 0, -2), 0.5, 20);
             SolidColorBrush brush3 = Brushes.LightBlue;
             DiffuseMaterial material3 = new DiffuseMaterial(brush3);
             GeometryModel3D model3 = new GeometryModel3D(mesh3, material3);
             model_group.Children.Add(model3);*/

            Console.WriteLine(
                mesh1.Positions.Count +
                /*mesh2.Positions.Count +
                mesh3.Positions.Count +*/
                " points");
            Console.WriteLine(
                (mesh1.TriangleIndices.Count +
                 /*mesh2.TriangleIndices.Count +
                 mesh3.TriangleIndices.Count) / 3 +*/ " triangles"));
            Console.WriteLine();
        }

        // Add a triangle to the indicated mesh.
        // Do not reuse points so triangles don't share normals.
        private void AddTriangle(MeshGeometry3D mesh, Point3D point1, Point3D point2, Point3D point3)
        {
            // Create the points.
            int index1 = mesh.Positions.Count;
            mesh.Positions.Add(point1);
            mesh.Positions.Add(point2);
            mesh.Positions.Add(point3);

            // Create the triangle.
            mesh.TriangleIndices.Add(index1++);
            mesh.TriangleIndices.Add(index1++);
            mesh.TriangleIndices.Add(index1);
        }

        // Make a thin rectangular prism between the two points.
        // If extend is true, extend the segment by half the
        // thickness so segments with the same end points meet nicely.
        private void AddSegment(MeshGeometry3D mesh,
            Point3D point1, Point3D point2, Vector3D up)
        {
            AddSegment(mesh, point1, point2, up, false);
        }
        private void AddSegment(MeshGeometry3D mesh,
            Point3D point1, Point3D point2, Vector3D up,
            bool extend)
        {
            const double thickness = 0.25;

            // Get the segment's vector.
            Vector3D v = point2 - point1;

            if (extend)
            {
                // Increase the segment's length on both ends by thickness / 2.
                Vector3D n = ScaleVector(v, thickness / 2.0);
                point1 -= n;
                point2 += n;
            }

            // Get the scaled up vector.
            Vector3D n1 = ScaleVector(up, thickness / 2.0);

            // Get another scaled perpendicular vector.
            Vector3D n2 = Vector3D.CrossProduct(v, n1);
            n2 = ScaleVector(n2, thickness / 2.0);

            // Make a skinny box.
            // p1pm means point1 PLUS n1 MINUS n2.
            Point3D p1pp = point1 + n1 + n2;
            Point3D p1mp = point1 - n1 + n2;
            Point3D p1pm = point1 + n1 - n2;
            Point3D p1mm = point1 - n1 - n2;
            Point3D p2pp = point2 + n1 + n2;
            Point3D p2mp = point2 - n1 + n2;
            Point3D p2pm = point2 + n1 - n2;
            Point3D p2mm = point2 - n1 - n2;

            // Sides.
            AddTriangle(mesh, p1pp, p1mp, p2mp);
            AddTriangle(mesh, p1pp, p2mp, p2pp);

            AddTriangle(mesh, p1pp, p2pp, p2pm);
            AddTriangle(mesh, p1pp, p2pm, p1pm);

            AddTriangle(mesh, p1pm, p2pm, p2mm);
            AddTriangle(mesh, p1pm, p2mm, p1mm);

            AddTriangle(mesh, p1mm, p2mm, p2mp);
            AddTriangle(mesh, p1mm, p2mp, p1mp);

            // Ends.
            AddTriangle(mesh, p1pp, p1pm, p1mm);
            AddTriangle(mesh, p1pp, p1mm, p1mp);

            AddTriangle(mesh, p2pp, p2mp, p2mm);
            AddTriangle(mesh, p2pp, p2mm, p2pm);
        }

        // Add a cage.
        private void AddCage(MeshGeometry3D mesh)
        {
            // Top.
            Vector3D up = new Vector3D(0, 1, 0);
            AddSegment(mesh, new Point3D(1, 1, 1), new Point3D(1, 1, -1), up, true);
            AddSegment(mesh, new Point3D(1, 1, -1), new Point3D(-1, 1, -1), up, true);
            AddSegment(mesh, new Point3D(-1, 1, -1), new Point3D(-1, 1, 1), up, true);
            AddSegment(mesh, new Point3D(-1, 1, 1), new Point3D(1, 1, 1), up, true);

            // Bottom.
            AddSegment(mesh, new Point3D(1, -1, 1), new Point3D(1, -1, -1), up, true);
            AddSegment(mesh, new Point3D(1, -1, -1), new Point3D(-1, -1, -1), up, true);
            AddSegment(mesh, new Point3D(-1, -1, -1), new Point3D(-1, -1, 1), up, true);
            AddSegment(mesh, new Point3D(-1, -1, 1), new Point3D(1, -1, 1), up, true);

            // Sides.
            Vector3D right = new Vector3D(1, 0, 0);
            AddSegment(mesh, new Point3D(1, -1, 1), new Point3D(1, 1, 1), right);
            AddSegment(mesh, new Point3D(1, -1, -1), new Point3D(1, 1, -1), right);
            AddSegment(mesh, new Point3D(-1, -1, 1), new Point3D(-1, 1, 1), right);
            AddSegment(mesh, new Point3D(-1, -1, -1), new Point3D(-1, 1, -1), right);
        }

        // Set the vector's length.
        private Vector3D ScaleVector(Vector3D vector, double length)
        {
            double scale = length / vector.Length;
            return new Vector3D(
                vector.X * scale,
                vector.Y * scale,
                vector.Z * scale);
        }

        // Adjust the camera's position.
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                    CameraPhi += CameraDPhi;
                    if (CameraPhi > Math.PI / 2.0) CameraPhi = Math.PI / 2.0;
                    break;
                case Key.Down:
                    CameraPhi -= CameraDPhi;
                    if (CameraPhi < -Math.PI / 2.0) CameraPhi = -Math.PI / 2.0;
                    break;
                case Key.Left:
                    CameraTheta += CameraDTheta;
                    //textBox.Text = Convert.ToString(CameraTheta);
                    break;
                case Key.Right:
                    CameraTheta -= CameraDTheta;
                    break;
                case Key.Add:
                case Key.OemPlus:
                    CameraR -= CameraDR;
                    if (CameraR < CameraDR) CameraR = CameraDR;
                    break;
                case Key.Subtract:
                case Key.OemMinus:
                    CameraR += CameraDR;
                    break;
            }

            // Update the camera's position.
            PositionCamera();
        }

        // Position the camera.
        private void PositionCamera()
        {
            // Calculate the camera's position in Cartesian coordinates.
            double y = CameraR * Math.Sin(CameraPhi);
            double hyp = CameraR * Math.Cos(CameraPhi);
            double x = hyp * Math.Cos(CameraTheta);
            double z = hyp * Math.Sin(CameraTheta);
            TheCamera.Position = new Point3D(x, y, z);

            // Look toward the origin.
            TheCamera.LookDirection = new Vector3D(-x, -y, -z);

            // Set the Up direction.
            TheCamera.UpDirection = new Vector3D(0, 1, 0);

            // Console.WriteLine("Camera.Position: (" + x + ", " + y + ", " + z + ")");
        }

        // Add a cylinder.
        private void AddCylinder(MeshGeometry3D mesh, Point3D end_point, Vector3D axis, double radius, int num_sides)
        {
            // Get two vectors perpendicular to the axis.
            Vector3D v1;
            if ((axis.Z < -0.01) || (axis.Z > 0.01))
                v1 = new Vector3D(axis.Z, axis.Z, -axis.X - axis.Y);
            else
                v1 = new Vector3D(-axis.Y - axis.Z, axis.X, axis.X);
            Vector3D v2 = Vector3D.CrossProduct(v1, axis);

            // Make the vectors have length radius.
            v1 *= (radius / v1.Length);
            v2 *= (radius / v2.Length);

            // Make the top end cap.
            double theta = 0;
            double dtheta = 2 * Math.PI / num_sides;
            for (int i = 0; i < num_sides; i++)
            {
                Point3D p1 = end_point +
                    Math.Cos(theta) * v1 +
                    Math.Sin(theta) * v2;
                theta += dtheta;
                Point3D p2 = end_point +
                    Math.Cos(theta) * v1 +
                    Math.Sin(theta) * v2;
                AddTriangle(mesh, end_point, p1, p2);
            }

            // Make the bottom end cap.
            Point3D end_point2 = end_point + axis;
            theta = 0;
            for (int i = 0; i < num_sides; i++)
            {
                Point3D p1 = end_point2 +
                    Math.Cos(theta) * v1 +
                    Math.Sin(theta) * v2;
                theta += dtheta;
                Point3D p2 = end_point2 +
                    Math.Cos(theta) * v1 +
                    Math.Sin(theta) * v2;
                AddTriangle(mesh, end_point2, p2, p1);
            }

            // Make the sides.
            theta = 0;
            for (int i = 0; i < num_sides; i++)
            {
                Point3D p1 = end_point +
                    Math.Cos(theta) * v1 +
                    Math.Sin(theta) * v2;
                theta += dtheta;
                Point3D p2 = end_point +
                    Math.Cos(theta) * v1 +
                    Math.Sin(theta) * v2;

                Point3D p3 = p1 + axis;
                Point3D p4 = p2 + axis;

                AddTriangle(mesh, p1, p3, p2);
                AddTriangle(mesh, p2, p3, p4);
            }

        }
    }
}
