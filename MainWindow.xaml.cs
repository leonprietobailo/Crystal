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
        public SeriesCollection SeriesCollection;

        //Constructor del MainWindow
        public MainWindow()
        {
            InitializeComponent();
            //Inicia el evento del reloj para poder realizar la simulación automática
            timer.Tick += new EventHandler(dispatcherTimer_Tick);
            //Define el intervalo de tiempo entre cada iteración
            //timer.Interval = new TimeSpan(Convert.ToInt64(1 / 100e-9)); //Se puede eliminar???
            //mesh = new Grid(0); lo ponemos donde lo necesitamos que esen el load simulation???
            //Establecemos los valores del gráfico
            setChartNumbers();
            //Establece de forma predeterminada las condiciones del contorno: contorno constante (0) o reflector (1)
            //ContourSelection.SelectedIndex = 0;??? 

            //Oculta parte del programa para mostrarlo parcialmente al inicializar
            WIRadius.Visibility = Visibility.Hidden;
            Settings.Visibility = Visibility.Hidden;
            SimControls.Visibility = Visibility.Hidden;
            GridandGraphs.Visibility = Visibility.Hidden;
            Wrongparameters.Visibility = Visibility.Hidden;
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
                    //Creamos los objetos rectángulos para cada grid (fase y temperatura)
                    rectangles1 = new Rectangle[radius, radius];
                    rectangles2 = new Rectangle[radius, radius];
                    canvas1.Children.Clear();
                    canvas2.Children.Clear();

                    for (int i = 0; i < radius; i++)
                    {
                        for (int j = 0; j < radius; j++)
                        {
                            //Grid fase
                            Rectangle rectangle1 = new Rectangle();
                            //Características de los rectángulos del grid
                            rectangle1.Width = canvas1.Width / radius;
                            rectangle1.Height = canvas1.Height / radius;
                            rectangle1.Fill = new SolidColorBrush(Colors.Transparent);
                            rectangle1.StrokeThickness = 0.1;
                            rectangle1.Stroke = Brushes.White;
                            canvas1.Children.Add(rectangle1);

                            //Posición del rectángulo dentro del grid
                            Canvas.SetTop(rectangle1, i * rectangle1.Height);
                            Canvas.SetLeft(rectangle1, j * rectangle1.Width);

                            rectangle1.Tag = new Point(i, j);
                            //Llamada de evento para saber si he entrado o salido de un rectángulo con el ratón
                            rectangle1.MouseEnter += new MouseEventHandler(rectangle_MouseEnter);
                            rectangle1.MouseLeave += new MouseEventHandler(rectangle_MouseLeave);
                            rectangles1[i, j] = rectangle1;

                            //Grid temperatura
                            Rectangle rectangle2 = new Rectangle();
                            //Características de los rectángulos del grid
                            rectangle2.Width = canvas2.Width / radius;
                            rectangle2.Height = canvas2.Height / radius;
                            rectangle2.Fill = new SolidColorBrush(Colors.Transparent);
                            rectangle2.StrokeThickness = 0.1;
                            rectangle2.Stroke = Brushes.White;
                            canvas2.Children.Add(rectangle2);

                            //Posición del rectángulo dentro del grid
                            Canvas.SetTop(rectangle2, i * rectangle2.Height);
                            Canvas.SetLeft(rectangle2, j * rectangle2.Width);

                            rectangle2.Tag = new Point(i, j);
                            //Llamada de evento para saber si he entrado o salido de un rectángulo con el ratón
                            rectangle2.MouseEnter += new MouseEventHandler(rectangle_MouseEnter);
                            rectangle2.MouseLeave += new MouseEventHandler(rectangle_MouseLeave);
                            rectangles2[i, j] = rectangle2;
                        }
                    }

                    ContourSelection.SelectedIndex = 0;

                    //Se cambia el valor de fase y temperatura de la celda central
                    mesh.startCell((radius - 1) / 2, (radius - 1) / 2);

                    //Se actualiza la visualización del mesh
                    updateMesh();

                    //Se limpian los valores guardados de cada uno de los parámetros
                    history.Clear();
                    PhaseValues.Clear();
                    TemperatureValues.Clear();

                    //Se guarda el grid inicial en el historial
                    history.Push(mesh.deepCopy());

                    //Se visualiza un grupo de componentes del programa para poder establecer los diferentes parámetros de simulación
                    showElements1();

                    //Se calcula la fase y temperatura medias de todo el grid
                    Tuple<double, double> averageTempPhase = mesh.getAverageTemperaturePhase();
                    PhaseValues.Add(averageTempPhase.Item2);
                    TemperatureValues.Add(averageTempPhase.Item1);

                    //Se oculta la label de "WRONG INPUTS" cuando el radio es correcto
                    WIRadius.Visibility = Visibility.Hidden;

                    //Si el radio es 0, aparece la label de "WRONG INPUTS"
                    if (mesh.getSize() == 0)
                    {
                        WIRadius.Visibility = Visibility.Visible;
                    }

                    //Se establecen las reglas en función del conjunto de parámetros seleccionados ???????????????? pondria una label poniendo que nada ha sido cargado y quitaria todo este codigo o no???
                    //Standard A
                    if (TabControl.SelectedIndex == 0)
                    {
                        r = new Rules(Convert.ToDouble(m1.Text), Convert.ToDouble(dt1.Text), Convert.ToDouble(d1.Text), Convert.ToDouble(e1.Text), Convert.ToDouble(b1.Text), Convert.ToDouble(dx1.Text), Convert.ToDouble(dy1.Text));
                        mesh.setRules(r);

                    }
                    //Standard B
                    else if (TabControl.SelectedIndex == 1)
                    {
                        r = new Rules(Convert.ToDouble(m2.Text), Convert.ToDouble(dt2.Text), Convert.ToDouble(d2.Text), Convert.ToDouble(e2.Text), Convert.ToDouble(b2.Text), Convert.ToDouble(dx2.Text), Convert.ToDouble(dy2.Text));
                        mesh.setRules(r);
                    }
                    //Custom
                    else if (TabControl.SelectedIndex == 2)
                    {
                        try
                        {
                            r = new Rules(Convert.ToDouble(m3.Text), Convert.ToDouble(dt3.Text), Convert.ToDouble(d3.Text), Convert.ToDouble(e3.Text), Convert.ToDouble(b3.Text), Convert.ToDouble(dx3.Text), Convert.ToDouble(dy3.Text));
                            mesh.setRules(r);
                        }
                        //Caso en el que los parámetros introducidos no sean validos
                        catch (FormatException)
                        {
                            Wrongparameters.Visibility = Visibility.Visible;
                            Correctparameters.Visibility = Visibility.Hidden;
                        }
                    }
                }
                else
                {
                    WIRadius.Visibility = Visibility.Visible;
                }
            }
            //Caso en el que el radio introducido no sea valido
            catch (FormatException)
            {
                WIRadius.Visibility = Visibility.Visible;
            }
            //Caso en el que el radio introducido no sea valido
            catch (OverflowException)
            {
                WIRadius.Visibility = Visibility.Visible;
            }
        }

        //Método que refleja visualmente los cambios de las valores de fase y temperatura de cada celda
        private void updateMesh()
        {
            for (int i = 0; i < radius; i++)
            {
                for (int j = 0; j < radius; j++)
                {
                    //Se calcula la distribución de colores de acuerdo a una función exponencial
                    double a = 255 * Math.Pow(1.0 - mesh.getCellPhase(i, j), 1.0 / 4.0);
                    double b = 255 * Math.Sqrt(mesh.getCellTemperature(i, j) + 1);

                    if (mesh.getCellTemperature(i, j) + 1 < 0)
                    {
                        b = 0;
                    }
                    //Se rellenan los rectángulos con color
                    rectangles1[i, j].Fill = new SolidColorBrush(Color.FromArgb(Convert.ToByte(a), 0, 0, 255));
                    rectangles2[i, j].Fill = new SolidColorBrush(Color.FromArgb(Convert.ToByte(b), 255, 0, 0));
                }
            }
        }

        //Método que permite establecer los parámetros de la gráfica
        private void setChartNumbers()
        {
            SeriesCollection = new SeriesCollection
            {
                //Gráfica de fase
                new LineSeries
                {
                    Values = PhaseValues,
                    ScalesYAt = 0,
                    Stroke = new SolidColorBrush(Color.FromRgb(75, 75, 255)),
                    Fill = Brushes.Transparent,
                    Title = "Avg.Phase"
                }
            };
            Chart1.Series = SeriesCollection;

            SeriesCollection = new SeriesCollection

            {
                //Gráfica de temperatura
                new LineSeries
                {
                Values = TemperatureValues,
                ScalesYAt = 0,
                Stroke = new SolidColorBrush(Color.FromRgb(255, 75, 75)),
                Fill = Brushes.Transparent,
                Title = "Avg.Temperature"
                }
            };
            
            Chart1_Copy.Series = SeriesCollection;
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

        //Método que permite visualizar los botones para poder comenzar la simulacion, llevarla a cabo manualmente o automáticamente así como establecer la velocidad
        private void showElements2()
        {
            buttonStart.Background = Brushes.Green;
            buttonStart.BorderBrush = Brushes.White;
            buttonStart.Foreground = Brushes.White;
            WIRadius.Visibility = Visibility.Hidden;
            Radius.Text = Convert.ToString((mesh.getSize() - 1) / 2);
            SimControls.Visibility = Visibility.Visible;
        }
        
        //Evento que permite visualizar la imagen que contiene la explicación de los dos tipos de frontera que pueden establecerse
        private void ContourInfo_Click(object sender, MouseButtonEventArgs e)
        {
            ContourInfo win2 = new ContourInfo();
            win2.ShowDialog();
        }

        //Evento que permite elegir la condición de las fronteras: contorno reflector o contorno de fase y temperatura constante
        private void ContourSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            mesh.setBoundaries(ContourSelection.SelectedIndex);
        }

        //Evento que permite cambiar la velocidad de simulación con una barra deslizadora
        private void speedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var slider = sender as Slider;
            //Cálculo del tiempo entre iteraciones establecido con la barra deslizadora
            double time = -3.0 / 400.0 * (slider.Value * 10.0 - 400.0 / 3.0); //[s]
            //Cálculo del número de ticks que se deben realizar en un tiempo de 100e-9 s
            ticks = Convert.ToInt64(time / 100e-9);
            //Se establce el intervalo de tiempo entre iteraciones??
            timer.Interval = new TimeSpan(ticks);
        }

        //Evento que permite comenzar la simulación de forma automatica 
        private void buttonStart_Click(object sender, RoutedEventArgs e)
        {
            //Si ha comenzado la simulación de forma automatica
            if (!timerStatus)
            {
                buttonStart.Content = "Stop";
                buttonStart.Background = Brushes.Red;
                buttonStart.BorderBrush = Brushes.White;
                buttonStart.Foreground = Brushes.White;
                timer.Start();
                timerStatus = true;

            }
            //Si no ha comenzado la simulación de forma automatica
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
            //Se añaden los valores al historial
            history.Push(mesh.deepCopy());
            //Se realiza el calculo de la próxima fase y temperatura 
            mesh.Iterate();
            //Se actulizan los cambios visualmente
            updateMesh();
            //Cálculo de la fase y temperatura medias en cada iteración
            Tuple<double, double> averageTempPhase = mesh.getAverageTemperaturePhase();
            PhaseValues.Add(averageTempPhase.Item2);
            TemperatureValues.Add(averageTempPhase.Item1);
            //Actualiza los valores de las labels en las que se informa de la fase y temperatura de la celda sobre la que el usuario tiene el ratón
            insideornot();
        }

        //Evento que permite ejecutar la siguiente iteración de la simulación
        private void nextIteration_Click(object sender, RoutedEventArgs e)
        {
            //Se añaden los valores al historial
            history.Push(mesh.deepCopy());
            //Se realiza el calculo de la próxima fase y temperatura 
            mesh.Iterate();
            //Cálculo de la fase y temperatura medias en cada iteración
            Tuple<double, double> averageTempPhase = mesh.getAverageTemperaturePhase();
            PhaseValues.Add(averageTempPhase.Item2);
            TemperatureValues.Add(averageTempPhase.Item1);
            //Se actulizan los cambios visualmente
            updateMesh();
            //Actualiza los valores de las labels en las que se informa de la fase y temperatura de la celda sobre la que el usuario tiene el ratón
            insideornot();
        }

        //Evento que permite ejecutar la anterior iteración de la simulación
        private void previousIteration_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //Se para el reloj
                timer.Stop();
                //Se quita el último elemento del historial
                mesh = history.Pop();
                //Se quitan los ultimos valores de fase y temperaturas de la gráfica
                PhaseValues.RemoveAt(PhaseValues.Count - 1);
                TemperatureValues.RemoveAt(TemperatureValues.Count - 1);
                //Se actulizan los cambios visualmente
                updateMesh();
                //Actualiza los valores de las labels en las que se informa de la fase y temperatura de la celda sobre la que el usuario tiene el ratón
                insideornot();
            }
            //Caso en el que ya no hay más elementos en el historial
            catch (InvalidOperationException)
            {
            }
        }

        //Evento que permite volver a comenzar la simulación
        private void restart_Click(object sender, RoutedEventArgs e)
        {
            //Se limpia el historial
            history.Clear();
            //Se para el reloj
            timer.Stop();
            //Se resetear el grid
            mesh.reset();
            mesh.startCell(mesh.getSize()/2, mesh.getSize()/2);
            history.Push(mesh.deepCopy());//?????????????
            //Se eliminan todos los valors de fase y temperaturas medias de la gráfica
            PhaseValues.Clear();
            TemperatureValues.Clear();
            //Cálculo de la fase y temperatura medias en cada iteración
            Tuple<double, double> averageTempPhase = mesh.getAverageTemperaturePhase();
            PhaseValues.Add(averageTempPhase.Item2);
            TemperatureValues.Add(averageTempPhase.Item1);
            //Se actulizan los cambios visualmente
            updateMesh();
            //Actualiza los valores de las labels en las que se informa de la fase y temperatura de la celda sobre la que el usuario tiene el ratón
            insideornot();
        }

        //Evento que actualiza y muestra el valor de fase y temperatura de la celda sobre la que se encuentra el ratón del usuario
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
                //Se copia el grid
                //copy1 = mesh.deepCopy();??paara?
                //Se para el reloj
                timer.Stop();
                mesh = new Grid(0);
                //Se resetea el grid
                //mesh.reset();//??para?

                //Se obtiene el parámetro que indica si el archivo se ha cargado o no
                int result = mesh.loadGrid();
                //Si el archivo no se ha podido cargar
                if (mesh == null || result == -1)
                {
                    mesh=mesh.deepCopy();
                    //mesh = copy1.deepCopy();
                    WIRadius.Visibility = Visibility.Visible;
                }
                //Si el archivo se ha podido cargar
                else
                {
                    //Se establace el radio del grid
                    int size = new int();
                    size = mesh.getSize();
                    radius = size;

                    //Creamos los objetos rectángulos para cada grid (fase y temperatura)
                    rectangles1 = new Rectangle[radius, radius];
                    rectangles2 = new Rectangle[radius, radius];
                    canvas1.Children.Clear();
                    canvas2.Children.Clear();

                    for (int i = 0; i < radius; i++)
                    {
                        for (int j = 0; j < radius; j++)
                        {
                            //Grid fase
                            Rectangle rectangle1 = new Rectangle();
                            //Características de los rectángulos del grid
                            rectangle1.Width = canvas1.Width / radius;
                            rectangle1.Height = canvas1.Height / radius;
                            rectangle1.Fill = new SolidColorBrush(Colors.Transparent);
                            rectangle1.StrokeThickness = 0.1;
                            rectangle1.Stroke = Brushes.White;
                            canvas1.Children.Add(rectangle1);

                            //Posición del rectángulo dentro del grid
                            Canvas.SetTop(rectangle1, i * rectangle1.Height);
                            Canvas.SetLeft(rectangle1, j * rectangle1.Width);

                            rectangle1.Tag = new Point(i, j);
                            rectangles1[i, j] = rectangle1;


                            //Grid temperature
                            Rectangle rectangle2 = new Rectangle();
                            //Características de los rectángulos del grid
                            rectangle2.Width = canvas2.Width / radius;
                            rectangle2.Height = canvas2.Height / radius;
                            rectangle2.Fill = new SolidColorBrush(Colors.Transparent);
                            rectangle2.StrokeThickness = 0.1;
                            rectangle2.Stroke = Brushes.White;
                            canvas2.Children.Add(rectangle2);

                            //Posición del rectángulo dentro del grid
                            Canvas.SetTop(rectangle2, i * rectangle2.Height);
                            Canvas.SetLeft(rectangle2, j * rectangle2.Width);

                            rectangle2.Tag = new Point(i, j);
                            rectangles2[i, j] = rectangle2;

                            WIRadius.Visibility = Visibility.Hidden;
                            Radius.Text = Convert.ToString((mesh.getSize()-1)/2);
                        }
                    }

                    //Se limpian los valores guardados de cada uno de los parámetros
                    history.Clear();
                    history.Push(mesh.deepCopy());

                    //Establece de forma predeterminada las condiciones del contorno: contorno constante (0) o reflector (1)
                    ContourSelection.SelectedIndex = 0;


                    //Se actulizan los cambios visualmente
                    updateMesh();

                    //Se visualiza un grupo de componentes del programa para poder establecer los diferentes parámetros de simulación
                    showElements1();
                }
            }
            //Caso en el que el formato del archivo no sea correcto
            catch (FileFormatException)
            {
                WIRadius.Visibility = Visibility.Visible;
            }
            //Caso en el que no se haya podido encontrar el archivo
            catch (DirectoryNotFoundException)
            {

            }
        }
    }
}
