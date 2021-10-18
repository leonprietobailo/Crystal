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

        int nRows, nColumns;
        Rectangle[,] rectangles1, rectangles2;
        Stack<Grid> history = new Stack<Grid>();
        Grid mesh, copy1;
        DispatcherTimer timer = new DispatcherTimer();
        bool timerStatus;
        long ticks;
        Rules r;
        ChartValues<double> PhaseValues = new ChartValues<double>();
        ChartValues<double> TemperatureValues = new ChartValues<double>();

        public MainWindow()
        {
            InitializeComponent();
            comboBox2.Items.Add("Constant Phase and Temperature");
            comboBox2.Items.Add("Reflective contour");
            timer.Tick += new EventHandler(dispatcherTimer_Tick);
            timer.Interval = new TimeSpan(Convert.ToInt64(1 / 100e-9));
            mesh = new Grid(0, 0);
            setChartNumbers();
        }

        public SeriesCollection SeriesCollection { get; set; }

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

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Convert.ToInt32(textBox1.Text) > 1 && Convert.ToInt32(textBox1.Text) > 1)
                {
                    nRows = Convert.ToInt32(textBox1.Text);
                    nColumns = Convert.ToInt32(textBox2.Text);
                    mesh = new Grid(nRows, nColumns);
                    rectangles1 = new Rectangle[nRows, nColumns];
                    rectangles2 = new Rectangle[nRows, nColumns];
                    canvas1.Children.Clear();
                    canvas2.Children.Clear();

                    for (int i = 0; i < nRows; i++)
                    {
                        for (int j = 0; j < nColumns; j++)
                        {
                            // CANVAS 1
                            Rectangle rectangle1 = new Rectangle();
                            rectangle1.Width = canvas1.Width / nColumns;
                            rectangle1.Height = canvas1.Height / nRows;
                            rectangle1.Fill = new SolidColorBrush(Colors.Transparent);
                            rectangle1.StrokeThickness = 1;
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
                            rectangle2.Width = canvas2.Width / nColumns;
                            rectangle2.Height = canvas2.Height / nRows;
                            rectangle2.Fill = new SolidColorBrush(Colors.Transparent);
                            rectangle2.StrokeThickness = 1;
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

                    mesh.startCell((nRows-1) / 2, (nColumns - 1) / 2);
                    updateMesh();

                    history.Clear();
                    history.Push(mesh.deepCopy());
                    comboBox2.SelectedIndex = 0;
                    showElements1();
                    PhaseValues.Add(mesh.getAveragePhase());
                    TemperatureValues.Add(mesh.getAverageTemperature());
                    if (mesh.getSize()[0] == 0 || mesh.getSize()[1] == 0)
                    {
                        label5.Visibility = Visibility.Visible;
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

        private void showElements1()
        {
            if (mesh.getSize()[0] != 0 && mesh.getSize()[1] != 0)
            {
                Settings.Visibility = Visibility.Visible;
                GridandGraphs.Visibility= Visibility.Visible;
            }
        }

        private void showElements2()
        {
            buttonStart.Background = Brushes.SpringGreen;
            buttonStart.BorderBrush = Brushes.White;
            buttonStart.Foreground = Brushes.White;
            label5.Visibility = Visibility.Hidden;
            textBox1.Text = Convert.ToString(mesh.getSize()[0]);
            textBox2.Text = Convert.ToString(mesh.getSize()[1]);
            SimControls.Visibility = Visibility.Visible;
        }

        private void rectangle_MouseEnter(object sender, MouseEventArgs e)
        {
            Rectangle reg = (Rectangle)sender;
            Point p = (Point)reg.Tag;
            Cellstatus.Visibility = Visibility.Visible;
            CellPhase.Content = Math.Round(mesh.getCellPhase(Convert.ToInt32(p.X), Convert.ToInt32(p.Y)),5);
            CellTemperature.Content = Math.Round(mesh.getCellTemperature(Convert.ToInt32(p.X), Convert.ToInt32(p.Y)),5);
            p.X++;
            p.Y++;
            Cellcoordinates1.Content = "("+ p.Y +","+ p.X +")";
            Cellcoordinates2.Content = "("+ p.Y +","+ p.X +")";

        }

        private void rectangle_MouseLeave(object sender, MouseEventArgs e)
        {
            Cellstatus.Visibility = Visibility.Hidden;
        }

        // Reflejar visualmente los cambios.
        private void updateMesh()
        {
            for (int i = 0; i < nRows; i++)
            {
                for (int j = 0; j < nColumns; j++)
                {
                    double a = 255*Math.Pow(1.0-mesh.getCellPhase(i, j),1.0/4.0);
                    double b = 255*Math.Sqrt(mesh.getCellTemperature(i, j) + 1);

                    if (mesh.getCellTemperature(i, j) + 1 < 0)
                    {
                        b = 0;
                    }

                    rectangles1[i, j].Fill = new SolidColorBrush(Color.FromArgb(Convert.ToByte(a), 0, 0, 255));
                    rectangles2[i, j].Fill = new SolidColorBrush(Color.FromArgb(Convert.ToByte(b), 255, 0, 0));
                }
            }
        }

        private void buttonStart_Click(object sender, RoutedEventArgs e)
        {
            if (!timerStatus)
            {
                buttonStart.Content = "Stop";
                timer.Start();
                timerStatus = true;
                
            }
            else
            {
                buttonStart.Content = "Start";
                buttonStart.Background = Brushes.SpringGreen;
                buttonStart.BorderBrush = Brushes.White;
                buttonStart.Foreground = Brushes.White;
                timer.Stop();
                timerStatus = false;


            }
        }

        private void nextIteration_Click(object sender, RoutedEventArgs e)
        {
            history.Push(mesh.deepCopy());
            mesh.Iterate();
            PhaseValues.Add(mesh.getAveragePhase());
            TemperatureValues.Add(mesh.getAverageTemperature());
            updateMesh();
        }

        private void previousIteration_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                timer.Stop();
                mesh = history.Pop();
                PhaseValues.RemoveAt(PhaseValues.Count - 1);
                TemperatureValues.RemoveAt(TemperatureValues.Count - 1);
                updateMesh();
            }
            catch (InvalidOperationException)
            {
            }
        }
        private void restart_Click(object sender, RoutedEventArgs e)
        {
            history.Clear();
            timer.Stop();
            mesh.reset();
            history.Push(mesh.deepCopy());
            PhaseValues.Clear();
            TemperatureValues.Clear();
            updateMesh();

        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            history.Push(mesh.deepCopy());
            mesh.Iterate();
            updateMesh();
            PhaseValues.Add(mesh.getAveragePhase());
            TemperatureValues.Add(mesh.getAverageTemperature());
        }

        private void speedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var slider = sender as Slider;
            double time = -3.0 / 400.0 * (slider.Value * 10.0 - 400.0 / 3.0); //[s]
            ticks = Convert.ToInt64(time / 100e-9);
            timer.Interval = new TimeSpan(ticks);
        }

        private void saveSimulation_Click(object sender, RoutedEventArgs e)
        {
            mesh.saveGrid();
        }

        private void LoadParameters(object sender, RoutedEventArgs e)
        {
            if (TabControl.SelectedIndex == 0)
            {
                r = new Rules(Convert.ToDouble(m1.Text), Convert.ToDouble(dt1.Text), Convert.ToDouble(d1.Text), Convert.ToDouble(e1.Text), Convert.ToDouble(b1.Text), Convert.ToDouble(dx1.Text), Convert.ToDouble(dy1.Text));
                mesh.setRules(r);
                Correctparameters.Content = "Standard one loaded!";
                Correctparameters.Visibility = Visibility.Visible;
                Wrongparameters.Visibility = Visibility.Hidden;
                showElements2();

            }
            if (TabControl.SelectedIndex == 1)
            {
                r = new Rules(Convert.ToDouble(m2.Text), Convert.ToDouble(dt2.Text), Convert.ToDouble(d2.Text), Convert.ToDouble(e2.Text), Convert.ToDouble(b2.Text), Convert.ToDouble(dx2.Text), Convert.ToDouble(dy2.Text));
                mesh.setRules(r);
                Correctparameters.Content = "Standard two loaded!";
                Correctparameters.Visibility = Visibility.Visible;
                Wrongparameters.Visibility = Visibility.Hidden;
                showElements2();

            }
            if (TabControl.SelectedIndex == 2)
            {
                try
                {
                    r = new Rules(Convert.ToDouble(m3.Text), Convert.ToDouble(dt3.Text), Convert.ToDouble(d3.Text), Convert.ToDouble(e3.Text), Convert.ToDouble(b3.Text), Convert.ToDouble(dx3.Text), Convert.ToDouble(dy3.Text));
                    mesh.setRules(r);
                    Correctparameters.Content = "Chosen values loaded!";
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

        private void loadSimualtion_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                copy1 = mesh.deepCopy();
                timer.Stop();
                mesh.reset();
                mesh.loadGrid();

                if (mesh == null)
                {
                    mesh = copy1.deepCopy();
                }

                int[] size = new int[2];
                size = mesh.getSize();

                nRows = size[0];
                nColumns = size[1];

                rectangles1 = new Rectangle[nRows, nColumns];
                rectangles2 = new Rectangle[nRows, nColumns];
                canvas1.Children.Clear();
                canvas2.Children.Clear();

                for (int i = 0; i < nRows; i++)
                {
                    for (int j = 0; j < nColumns; j++)
                    {
                        // CANVAS 1
                        Rectangle rectangle1 = new Rectangle();
                        rectangle1.Width = canvas1.Width / nColumns;
                        rectangle1.Height = canvas1.Height / nRows;
                        rectangle1.Fill = new SolidColorBrush(Colors.Transparent);
                        rectangle1.StrokeThickness = 1;
                        rectangle1.Stroke = Brushes.White;
                        canvas1.Children.Add(rectangle1);

                        Canvas.SetTop(rectangle1, i * rectangle1.Height);
                        Canvas.SetLeft(rectangle1, j * rectangle1.Width);

                        rectangle1.Tag = new Point(i, j);
                        rectangles1[i, j] = rectangle1;

                        // CANVAS 2
                        Rectangle rectangle2 = new Rectangle();
                        rectangle2.Width = canvas2.Width / nColumns;
                        rectangle2.Height = canvas2.Height / nRows;
                        rectangle2.Fill = new SolidColorBrush(Colors.Transparent);
                        rectangle2.StrokeThickness = 1;
                        rectangle2.Stroke = Brushes.White;
                        canvas2.Children.Add(rectangle2);

                        Canvas.SetTop(rectangle2, i * rectangle2.Height);
                        Canvas.SetLeft(rectangle2, j * rectangle2.Width);

                        rectangle2.Tag = new Point(i, j);
                        rectangles2[i, j] = rectangle2;


                    }
                }

                history.Clear();
                history.Push(mesh.deepCopy());
                comboBox2.SelectedIndex = 1;
                updateMesh();
                showElements1();
            }
            catch (FileFormatException)
            {
                label5.Visibility = Visibility.Visible;
            }
        }

        private void comboBox2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            mesh.setBoundaries(comboBox2.SelectedIndex);
        }

        private void image2_Click(object sender, MouseButtonEventArgs e)
        {
            Window2 win2 = new Window2();
            win2.ShowDialog();
        }
    }
}
