using System;
using System.Collections.Generic;
//using System.Linq;
//using System.Text;
using System.Windows;
using System.Windows.Controls;
//using System.Windows.Data;
//using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
//using System.Windows.Media.Imaging;
//using System.Windows.Navigation;
using System.Windows.Shapes;
//using System.Collections;
using System.Windows.Threading;
using System.IO;
using Microsoft.Win32;

namespace GameOfLife
{

    public partial class MainWindow : Window
    {

        int nRows, nColumns;
        Rectangle[,] rectangles1, rectangles2;
        //Stack<Grid> history = new Stack<Grid>();
        Grid mesh, copy1;
        DispatcherTimer timer = new DispatcherTimer();
        Boolean timerStatus = false;
        long ticks;
        Rules r;
        bool[] infected;
        bool[] healed;
        bool modified;
        string name;

        public MainWindow()
        {
            InitializeComponent();
            comboBox2.Items.Add("Dead boundaries");
            comboBox2.Items.Add("Reflective boundaries");
            timer.Tick += new EventHandler(dispatcherTimer_Tick);
            timer.Interval = new TimeSpan(Convert.ToInt64(1 / 100e-9));
            mesh = new Grid(0, 0);
            r = new Rules();
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
                            rectangle1.MouseDown += new MouseButtonEventHandler(rectangle_MouseDown);
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
                            rectangle2.MouseDown += new MouseButtonEventHandler(rectangle_MouseDown);
                            rectangles2[i, j] = rectangle2;


                        }
                    }


                    //history.Push(mesh.deepCopy()); ESTO HAY QUE DARLE BIEN
                    comboBox2.SelectedIndex = 0;
                    showElements();

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

        private void showElements()
        {
            if (mesh.getSize()[0] != 0 && mesh.getSize()[1] != 0)
            {
                comboBox2.Visibility = Visibility.Visible;
                label8.Visibility = Visibility.Visible;
                image2.Visibility = Visibility.Visible;
                speedLabel.Visibility = Visibility.Visible;
                speedSlider.Visibility = Visibility.Visible;
                buttonStart.Visibility = Visibility.Visible;
                buttonStart.Background = Brushes.SpringGreen;
                buttonStart.BorderBrush = Brushes.White;
                buttonStart.Foreground = Brushes.White;
                label5.Visibility = Visibility.Hidden;
                textBox1.Text = Convert.ToString(mesh.getSize()[0]);
                textBox2.Text = Convert.ToString(mesh.getSize()[1]);
                restart.Visibility = Visibility.Visible;
                saveSimulation.Visibility = Visibility.Visible;
                previousIteration.Visibility = Visibility.Visible;
                nextIteration.Visibility = Visibility.Visible;
            }
        }
        private void rectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Rectangle reg = (Rectangle)sender;
            Point p = (Point)reg.Tag;
            p.X++;
            p.Y++;
            mesh.clickedCell(Convert.ToInt32(p.X), Convert.ToInt32(p.Y));
            updateMesh();
        }

        // Reflejar visualmente los cambios.
        private void updateMesh()
        {
            for (int i = 0; i < nRows; i++)
            {
                for (int j = 0; j < nColumns; j++)
                {
                    rectangles1[i, j].Fill = new SolidColorBrush(Color.FromArgb(Convert.ToByte(255 * mesh.getCellPhase(i, j)), 0, 0, 255));
                    rectangles2[i, j].Fill = new SolidColorBrush(Color.FromArgb(Convert.ToByte(-255 * mesh.getCellTemperature(i, j)), 255, 0, 0));
                }
            }

           


            //for (int i = 0; i < nRows; i++)
            //{
            //    for (int j = 0; j < nColumns; j++)
            //    {
            //        if (mesh.getCellStatus(i + 1, j + 1))
            //        {
            //            if (comboBox1.SelectedIndex == 0)
            //            {
            //                rectangles1[i, j].Fill = new SolidColorBrush(Colors.Aqua);
            //            }
            //            else if (comboBox1.SelectedIndex == 1)
            //            {
            //                rectangles1[i, j].Fill = new SolidColorBrush(Colors.Red);
            //            }
            //            else if (comboBox1.SelectedIndex == 2)
            //            {
            //                rectangles1[i, j].Fill = new SolidColorBrush(Colors.Lime);
            //            }
            //        }
            //        else
            //        {
            //            rectangles1[i, j].Fill = new SolidColorBrush(Colors.Transparent);
            //        }
            //    }
            //}

            //if (mesh.isLastIteration())
            //{
            //    timer.Stop();
            //    buttonStart.Content = "Start";
            //    buttonStart.Background = Brushes.SpringGreen;
            //    buttonStart.BorderBrush = Brushes.White;
            //    buttonStart.Foreground = Brushes.White;
            //    textStatus.Text = "Status: Stable";
            //    textStatus.Foreground = new SolidColorBrush(Colors.Green);
            //    timerStatus = false;
            //}
            //else
            //{
            //    textStatus.Text = "Status: Unstable";
            //    textStatus.Foreground = new SolidColorBrush(Colors.Red);
            //}
        }

        private void buttonStart_Click(object sender, RoutedEventArgs e)
        {
            if (!timerStatus)
            {
                //if (!mesh.isLastIteration())
                //{
                    
                //    buttonStart.Background = Brushes.Red;
                //    buttonStart.BorderBrush = Brushes.White;
                //    buttonStart.Foreground = Brushes.White;


                    
                //}
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
            //if (!mesh.isLastIteration())
            //{
                
            //}
            //history.Push(mesh.deepCopy());
            mesh.Iterate();
            //mesh.setBoundaries(comboBox2.SelectedIndex);
            updateMesh();
        }



        private void previousIteration_Click(object sender, RoutedEventArgs e)
        {
            //try
            //{
            //    mesh = history.Pop();
            //    updateMesh();
            //}
            //catch (InvalidOperationException)
            //{
            //}
        }

        private void restart_Click(object sender, RoutedEventArgs e)
        {
            //history.Clear();
            timer.Stop();
            mesh.reset();
            //history.Push(mesh.deepCopy());
            updateMesh();

        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            //history.Push(mesh.deepCopy());
            mesh.Iterate();
            //mesh.setBoundaries(comboBox2.SelectedIndex);
            updateMesh();
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
            //mesh.saveGrid();
        }

        private void loadSimualtion_Click(object sender, RoutedEventArgs e)
        {
            //try
            //{
            //    copy1 = mesh.deepCopy();
            //    timer.Stop();
            //    mesh.reset();
            //    mesh.loadGrid();

            //    if (mesh.isClean() || mesh == null)
            //    {
            //        mesh = copy1.deepCopy();
            //    }

            //    int[] size = new int[2];
            //    size = mesh.getSize();

            //    nRows = size[0];
            //    nColumns = size[1];
            //    rectangles1 = new Rectangle[nRows, nColumns];
            //    canvas1.Children.Clear();
            //    for (int i = 0; i < nRows; i++)
            //    {
            //        for (int j = 0; j < nColumns; j++)
            //        {
            //            Rectangle r = new Rectangle();
            //            r.Width = canvas1.Width / nColumns;
            //            r.Height = canvas1.Height / nRows;
            //            r.StrokeThickness = 1;
            //            r.Stroke = Brushes.White;
            //            canvas1.Children.Add(r);

            //            Canvas.SetTop(r, i * r.Height);
            //            Canvas.SetLeft(r, j * r.Width);

            //            r.Tag = new Point(i, j);
            //            r.MouseDown += new MouseButtonEventHandler(rectangle_MouseDown);
            //            rectangles1[i, j] = r;
            //        }
            //    }
            //    history.Clear();
            //    history.Push(mesh.deepCopy());
            //    updateMesh();
            //    comboBox1.SelectedIndex = 0;
            //    comboBox2.SelectedIndex = 0;

            //    showElements();
            //}
            //catch (FileFormatException)
            //{
            //    label5.Visibility = Visibility.Visible;
            //}
        }

        //private void comboBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    if (comboBox1.SelectedIndex == 0)
        //    {
        //        r.setConway();
        //        mesh.setRules(r);
        //    }
        //    else if (comboBox1.SelectedIndex == 1)
        //    {
        //        r.setCOVID19();
        //        mesh.setRules(r);
        //    }
        //    else if (comboBox1.SelectedIndex == 2)
        //    {
        //        r.setNewVirus(this.infected, this.healed);
        //        mesh.setRules(r);
        //    }
        //    updateMesh();
        //}

        private void comboBox2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            mesh.setBoundaries(comboBox2.SelectedIndex);
        }

        //private void image1_Click(object sender, MouseButtonEventArgs e)
        //{
        //    Window1 win1 = new Window1();
        //    win1.ShowDialog();
        //}

        private void image2_Click(object sender, MouseButtonEventArgs e)
        {
            Window2 win2 = new Window2();
            win2.ShowDialog();
        }

        //private void AddVirus_Click(object sender, RoutedEventArgs e)
        //{
        //    if (!modified)
        //    {
        //        Window3 win3 = new Window3();
        //        win3.ShowDialog();
        //        if (win3.addedvirus())
        //        {
        //            infected = win3.getINextStatus();
        //            healed = win3.getHNextStatus();
        //            name = win3.getvirusname();
        //            comboBox1.Items.Add(name);
        //            AddVirus.Content = "Modify";
        //            modified = true;
        //        }
        //    }
        //    else if (modified)
        //    {
        //        Window3 win4 = new Window3();
        //        win4.setNextStatus(infected, healed);
        //        win4.setvirusname(name);
        //        comboBox1.Items.Remove(2);
        //        win4.ShowDialog();    
        //    }
        //}
    }
}