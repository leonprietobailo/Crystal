using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.IO;
using LiveCharts;
using LiveCharts.Wpf;

namespace GameOfLife
{
    public partial class MainWindow : Window
    {
        //Atributos del MainWindow
        int radius, rowinside, columninside;
        bool timerStatus, inside;
        long ticks;
        Rectangle[,] rectangles1, rectangles2;
        Stack<Grid> history = new Stack<Grid>();
        Grid mesh, copy1;
        DispatcherTimer timer = new DispatcherTimer();
        Rules r;
        ChartValues<double> PhaseValues = new ChartValues<double>();
        ChartValues<double> TemperatureValues = new ChartValues<double>();

        //Constructor del MainWindow
        public MainWindow()
        {
            InitializeComponent();
            comboBox2.Items.Add("Constant Phase and Temperature");
            comboBox2.Items.Add("Reflective contour");
            timer.Tick += new EventHandler(dispatcherTimer_Tick);
            timer.Interval = new TimeSpan(Convert.ToInt64(1 / 100e-9));
            mesh = new Grid(0);
            setChartNumbers();

            // Visibilidad
            label5.Visibility = Visibility.Hidden;
            Settings.Visibility = Visibility.Hidden;
            SimControls.Visibility = Visibility.Hidden;
            GridandGraphs.Visibility = Visibility.Hidden;
            Wrongparameters.Visibility = Visibility.Hidden;
        }

        //???
        public SeriesCollection SeriesCollection;

        //???
        private void setChartNumbers()
        {
            SeriesCollection = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Avg.Phase",
                    Values = PhaseValues,
                    ScalesYAt = 0
                },
                new LineSeries
                {
                    Title = "Avg.Temperature",
                    Values = TemperatureValues,
                    ScalesYAt = 1
                }
            };

            Chart1.Series = SeriesCollection;
        }

        //Evento que permite crear un grid con celdas con valor y fase predeterminados
        private void LoadGrid_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Convert.ToInt32(Radius.Text) > 0)
                {
                    radius = Convert.ToInt32(Radius.Text) * 2 + 1;
                    mesh = new Grid(radius);
                    rectangles1 = new Rectangle[radius, radius];
                    rectangles2 = new Rectangle[radius, radius];
                    canvas1.Children.Clear();
                    canvas2.Children.Clear();

                    for (int i = 0; i < radius; i++)
                    {
                        for (int j = 0; j < radius; j++)
                        {
                            // CANVAS 1
                            Rectangle rectangle1 = new Rectangle();
                            rectangle1.Width = canvas1.Width / radius;
                            rectangle1.Height = canvas1.Height / radius;
                            rectangle1.Fill = new SolidColorBrush(Colors.Transparent);
                            rectangle1.StrokeThickness = 0.1;
                            rectangle1.Stroke = Brushes.White;
                            canvas1.Children.Add(rectangle1);

                            Canvas.SetTop(rectangle1, i * rectangle1.Height);
                            Canvas.SetLeft(rectangle1, j * rectangle1.Width);

                            rectangle1.Tag = new Point(i, j);
                            rectangle1.MouseEnter += new MouseEventHandler(rectangle_MouseEnter);
                            rectangle1.MouseLeave += new MouseEventHandler(rectangle_MouseLeave);
                            rectangles1[i, j] = rectangle1;

                            // CANVAS 2
                            Rectangle rectangle2 = new Rectangle();
                            rectangle2.Width = canvas2.Width / radius;
                            rectangle2.Height = canvas2.Height / radius;
                            rectangle2.Fill = new SolidColorBrush(Colors.Transparent);
                            rectangle2.StrokeThickness = 0.1;
                            rectangle2.Stroke = Brushes.White;
                            canvas2.Children.Add(rectangle2);

                            Canvas.SetTop(rectangle2, i * rectangle2.Height);
                            Canvas.SetLeft(rectangle2, j * rectangle2.Width);

                            rectangle2.Tag = new Point(i, j);
                            rectangle2.MouseEnter += new MouseEventHandler(rectangle_MouseEnter);
                            rectangle2.MouseLeave += new MouseEventHandler(rectangle_MouseLeave);
                            rectangles2[i, j] = rectangle2;
                        }
                    }
                    mesh.startCell((radius - 1) / 2, (radius - 1) / 2);
                    updateMesh();

                    history.Clear();
                    PhaseValues.Clear();
                    TemperatureValues.Clear();
                    history.Push(mesh.deepCopy());
                    comboBox2.SelectedIndex = 0;
                    showElements1();

                    Tuple<double, double> averageTempPhase = mesh.getAverageTemperaturePhase();
                    PhaseValues.Add(averageTempPhase.Item2);
                    TemperatureValues.Add(averageTempPhase.Item1);
                    label5.Visibility = Visibility.Hidden;

                    if (mesh.getSize() == 0)
                    {
                        label5.Visibility = Visibility.Visible;
                    }

                    if (TabControl.SelectedIndex == 0)
                    {
                        r = new Rules(Convert.ToDouble(m1.Text), Convert.ToDouble(dt1.Text), Convert.ToDouble(d1.Text), Convert.ToDouble(e1.Text), Convert.ToDouble(b1.Text), Convert.ToDouble(dx1.Text), Convert.ToDouble(dy1.Text));
                        mesh.setRules(r);

                    }
                    else if (TabControl.SelectedIndex == 1)
                    {
                        r = new Rules(Convert.ToDouble(m2.Text), Convert.ToDouble(dt2.Text), Convert.ToDouble(d2.Text), Convert.ToDouble(e2.Text), Convert.ToDouble(b2.Text), Convert.ToDouble(dx2.Text), Convert.ToDouble(dy2.Text));
                        mesh.setRules(r);
                    }
                    else if (TabControl.SelectedIndex == 2)
                    {
                        try
                        {
                            r = new Rules(Convert.ToDouble(m3.Text), Convert.ToDouble(dt3.Text), Convert.ToDouble(d3.Text), Convert.ToDouble(e3.Text), Convert.ToDouble(b3.Text), Convert.ToDouble(dx3.Text), Convert.ToDouble(dy3.Text));
                            mesh.setRules(r);
                        }
                        catch (FormatException)
                        {
                            Wrongparameters.Visibility = Visibility.Visible;
                            Correctparameters.Visibility = Visibility.Hidden;
                        }
                    }

                }
                else
                {
                    label5.Visibility = Visibility.Visible;
                }
            }
            catch (FormatException)
            {
                label5.Visibility = Visibility.Visible;
            }
            catch (OverflowException)
            {
                label5.Visibility = Visibility.Visible;
            }
        }

        //Método que permite visualizar los parámetros de simulación y las condiciones de frontera
        private void showElements1()
        {
            if (mesh.getSize()!= 0)
            {
                Settings.Visibility = Visibility.Visible;
                GridandGraphs.Visibility = Visibility.Visible;
            }
        }

        //Método que permite visualizar los botones que me permiten comenzar la simulacion, llevarla a cabo manualmente o automáticamente así como establecer la velocidad
        private void showElements2()
        {
            buttonStart.Background = Brushes.Green;
            buttonStart.BorderBrush = Brushes.White;
            buttonStart.Foreground = Brushes.White;
            label5.Visibility = Visibility.Hidden;
            Radius.Text = Convert.ToString((mesh.getSize() - 1) / 2);
            SimControls.Visibility = Visibility.Visible;
        }

        //Evento que actualiza y muestra el valor de fase y temperatura de la celda sobre la que se encuentra el ratón el usuario
        private void rectangle_MouseEnter(object sender, MouseEventArgs e)
        {
            Rectangle reg = (Rectangle)sender;
            Point p = (Point)reg.Tag;
            Cellstatus.Visibility = Visibility.Visible;
            CellPhase.Content = Math.Round(mesh.getCellPhase(Convert.ToInt32(p.X), Convert.ToInt32(p.Y)), 5);
            CellTemperature.Content = Math.Round(mesh.getCellTemperature(Convert.ToInt32(p.X), Convert.ToInt32(p.Y)), 5);
            rowinside = Convert.ToInt32(p.X);
            columninside = Convert.ToInt32(p.Y);
            p.X++;
            p.Y++;
            Cellcoordinates1.Content = "(" + p.Y + "," + p.X + ")";
            Cellcoordinates2.Content = "(" + p.Y + "," + p.X + ")";
            inside = true;
        }

        //Evento que oculta el valor de fase y temperatura de la celda cuando el ratón del usuario no se encuentra encima del grid
        private void rectangle_MouseLeave(object sender, MouseEventArgs e)
        {
            Cellstatus.Visibility = Visibility.Hidden;
            inside = false;
        }

        //Método que permite actualizar el valor de fase y temperatura de la celda sobre la que el usuario tiene el ratón cuando este si situa sobre ella y su valor cambia
        public void insideornot()
        {
            if (inside == true)
            {
                CellPhase.Content = Math.Round(mesh.getCellPhase(rowinside, columninside), 5);
                CellTemperature.Content = Math.Round(mesh.getCellTemperature(rowinside, columninside), 5);
            }
        }

        // Método que refleja visualmente los cambios
        private void updateMesh()
        {
            for (int i = 0; i < radius; i++)
            {
                for (int j = 0; j < radius; j++)
                {
                    double a = 255 * Math.Pow(1.0 - mesh.getCellPhase(i, j), 1.0 / 4.0);
                    double b = 255 * Math.Sqrt(mesh.getCellTemperature(i, j) + 1);

                    if (mesh.getCellTemperature(i, j) + 1 < 0)
                    {
                        b = 0;
                    }

                    rectangles1[i, j].Fill = new SolidColorBrush(Color.FromArgb(Convert.ToByte(a), 0, 0, 255));
                    rectangles2[i, j].Fill = new SolidColorBrush(Color.FromArgb(Convert.ToByte(b), 255, 0, 0));
                }
            }
        }

        //Evento que permite establecer los parámetros de simulación
        private void LoadParameters(object sender, RoutedEventArgs e)
        {
            //Standard A
            if (TabControl.SelectedIndex == 0)
            {
                r = new Rules(Convert.ToDouble(m1.Text), Convert.ToDouble(dt1.Text), Convert.ToDouble(d1.Text), Convert.ToDouble(e1.Text), Convert.ToDouble(b1.Text), Convert.ToDouble(dx1.Text), Convert.ToDouble(dy1.Text));
                mesh.setRules(r);
                Correctparameters.Content = "Standard A loaded!";
                Correctparameters.Visibility = Visibility.Visible;
                Wrongparameters.Visibility = Visibility.Hidden;
                showElements2();

            }
            //Standard B
            else if (TabControl.SelectedIndex == 1)
            {
                r = new Rules(Convert.ToDouble(m2.Text), Convert.ToDouble(dt2.Text), Convert.ToDouble(d2.Text), Convert.ToDouble(e2.Text), Convert.ToDouble(b2.Text), Convert.ToDouble(dx2.Text), Convert.ToDouble(dy2.Text));
                mesh.setRules(r);
                Correctparameters.Content = "Standard B loaded!";
                Correctparameters.Visibility = Visibility.Visible;
                Wrongparameters.Visibility = Visibility.Hidden;
                showElements2();

            }
            //Custom
            else if (TabControl.SelectedIndex == 2)
            {
                try
                {
                    r = new Rules(Convert.ToDouble(m3.Text), Convert.ToDouble(dt3.Text), Convert.ToDouble(d3.Text), Convert.ToDouble(e3.Text), Convert.ToDouble(b3.Text), Convert.ToDouble(dx3.Text), Convert.ToDouble(dy3.Text));
                    mesh.setRules(r);
                    Correctparameters.Content = "Custom loaded!";
                    Wrongparameters.Visibility = Visibility.Hidden;
                    Correctparameters.Visibility = Visibility.Visible;
                    showElements2();
                }
                catch (FormatException)
                {
                    Wrongparameters.Visibility = Visibility.Visible;
                    Correctparameters.Visibility = Visibility.Hidden;
                }
            }
        }

        //Evento que permite elegir la condición de las fronteras: contorno reflector o contorno de fase y temperatura constante
        private void comboBox2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            mesh.setBoundaries(comboBox2.SelectedIndex);
        }

        //Evento que permite visualizar la imagen que contiene la explicación de los dos tipos de frontera que pueden establecerse
        private void image2_Click(object sender, MouseButtonEventArgs e)
        {
            Window2 win2 = new Window2();
            win2.ShowDialog();
        }

        //Evento que permite comenzar la simulación de forma automatica 
        private void buttonStart_Click(object sender, RoutedEventArgs e)
        {
            if (!timerStatus)
            {
                buttonStart.Content = "Stop";
                buttonStart.Background = Brushes.Red;
                buttonStart.BorderBrush = Brushes.White;
                buttonStart.Foreground = Brushes.White;
                timer.Start();
                timerStatus = true;

            }
            else
            {
                buttonStart.Content = "Start";
                buttonStart.Background = Brushes.Green;
                buttonStart.BorderBrush = Brushes.White;
                buttonStart.Foreground = Brushes.White;
                timer.Stop();
                timerStatus = false;
            }
        }

        //Evento que permite activar el reloj para ejecutar la simulación automática
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            history.Push(mesh.deepCopy());
            mesh.Iterate();
            updateMesh();

            Tuple<double, double> averageTempPhase = mesh.getAverageTemperaturePhase();
            PhaseValues.Add(averageTempPhase.Item2);
            TemperatureValues.Add(averageTempPhase.Item1);
            
            insideornot();
        }

        //Evento que permite ejecutar la siguiente iteración de la simulación
        private void nextIteration_Click(object sender, RoutedEventArgs e)
        {
            history.Push(mesh.deepCopy());
            mesh.Iterate();

            Tuple<double, double> averageTempPhase = mesh.getAverageTemperaturePhase();
            PhaseValues.Add(averageTempPhase.Item2);
            TemperatureValues.Add(averageTempPhase.Item1);
            
            updateMesh();
            insideornot();
        }

        //Evento que permite ejecutar la anterior iteración de la simulación
        private void previousIteration_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                timer.Stop();
                mesh = history.Pop();
                PhaseValues.RemoveAt(PhaseValues.Count - 1);
                TemperatureValues.RemoveAt(TemperatureValues.Count - 1);
                updateMesh();
                insideornot();
            }
            catch (InvalidOperationException)
            {
            }
        }

        //Evento que permite volver a comenzar la simulación
        private void restart_Click(object sender, RoutedEventArgs e)
        {
            history.Clear();
            timer.Stop();
            mesh.reset();
            mesh.startCell((radius - 1) / 2, (radius - 1) / 2);
            history.Push(mesh.deepCopy());
            PhaseValues.Clear();
            TemperatureValues.Clear();
            PhaseValues.Add(mesh.getAverageTemperaturePhase().Item2);
            TemperatureValues.Add(mesh.getAverageTemperaturePhase().Item1);
            updateMesh();
            insideornot();
        }

        //Evento que permite cambiar la velocidad de simulación
        private void speedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var slider = sender as Slider;
            double time = -9.0 / 1000.0 * (slider.Value * 10.0 - 1000.0 / 9.0); //[s]
            ticks = Convert.ToInt64(time / 100e-9);
            timer.Interval = new TimeSpan(ticks);
        }

        //Evento que permite guardar la simulación
        private void saveSimulation_Click(object sender, RoutedEventArgs e)
        {
            mesh.saveGrid();
        }

        //Evento que permite cargar la simulación
        private void loadSimualtion_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                copy1 = mesh.deepCopy();
                timer.Stop();
                mesh.reset();
                int result = mesh.loadGrid();

                if (mesh == null || result == -1)
                {
                    mesh = copy1.deepCopy();
                    label5.Visibility = Visibility.Visible;
                }
                else
                {
                    int size = new int();
                    size = mesh.getSize();

                    radius = size;

                    rectangles1 = new Rectangle[radius, radius];
                    rectangles2 = new Rectangle[radius, radius];
                    canvas1.Children.Clear();
                    canvas2.Children.Clear();

                    for (int i = 0; i < radius; i++)
                    {
                        for (int j = 0; j < radius; j++)
                        {
                            // CANVAS 1
                            Rectangle rectangle1 = new Rectangle();
                            rectangle1.Width = canvas1.Width / radius;
                            rectangle1.Height = canvas1.Height / radius;
                            rectangle1.Fill = new SolidColorBrush(Colors.Transparent);
                            rectangle1.StrokeThickness = 0.1;
                            rectangle1.Stroke = Brushes.White;
                            canvas1.Children.Add(rectangle1);

                            Canvas.SetTop(rectangle1, i * rectangle1.Height);
                            Canvas.SetLeft(rectangle1, j * rectangle1.Width);

                            rectangle1.Tag = new Point(i, j);
                            rectangles1[i, j] = rectangle1;

                            // CANVAS 2
                            Rectangle rectangle2 = new Rectangle();
                            rectangle2.Width = canvas2.Width / radius;
                            rectangle2.Height = canvas2.Height / radius;
                            rectangle2.Fill = new SolidColorBrush(Colors.Transparent);
                            rectangle2.StrokeThickness = 0.1;
                            rectangle2.Stroke = Brushes.White;
                            canvas2.Children.Add(rectangle2);

                            Canvas.SetTop(rectangle2, i * rectangle2.Height);
                            Canvas.SetLeft(rectangle2, j * rectangle2.Width);

                            rectangle2.Tag = new Point(i, j);
                            rectangles2[i, j] = rectangle2;
                            label5.Visibility = Visibility.Hidden;
                            Radius.Text = Convert.ToString((mesh.getSize()-1)/2);

                        }
                    }

                    history.Clear();
                    history.Push(mesh.deepCopy());
                    comboBox2.SelectedIndex = 1;
                    updateMesh();
                    showElements1();
                }
            }
            catch (FileFormatException)
            {
                label5.Visibility = Visibility.Visible;
            }

            catch (DirectoryNotFoundException)
            {

            }
        }
    }
}
