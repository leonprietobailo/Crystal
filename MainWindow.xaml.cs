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

namespace Crystal
{
    public partial class MainWindow : Window
    {
        //Atributos del MainWindow
        int radius, rowInside, columnInside;
        bool timerStatus, inside, firstGrid;
        long ticks;
        Rules r;
        Rectangle[,] phaseRectangles, temperatureRectangles;
        Stack<Grid> history = new Stack<Grid>();
        Grid mesh, temporaryMesh;
        DispatcherTimer timer = new DispatcherTimer();
        //Gráficas
        ChartValues<double> phaseValues = new ChartValues<double>();
        ChartValues<double> temperatureValues = new ChartValues<double>();
        public SeriesCollection SeriesCollection;

        //Constructor del MainWindow
        public MainWindow()
        {
            InitializeComponent();
            //Inicia el evento del reloj para poder realizar la simulación automática y añadimos velocidad predeterminada
            timer.Tick += new EventHandler(dispatcherTimer_Tick);
            ticks = Convert.ToInt64(1 / 100e-9);
            timer.Interval = new TimeSpan(ticks);
            //Establecemos los valores del gráfico
            setChartNumbers();
            //Oculta parte del programa para mostrarlo parcialmente al inicializar
            WIRadius.Visibility = Visibility.Hidden;
            WrongFile.Visibility = Visibility.Hidden;
            SimControls.Visibility = Visibility.Hidden;
            GridandGraphs.Visibility = Visibility.Hidden;
            Wrongparameters.Visibility = Visibility.Hidden;
        }

        //Evento que permite crear un grid con celdas con valor de temperatura y fase predeterminados
        private void LoadGrid_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Convert.ToInt32(Radius.Text) > 0)
                {
                    //Se establece el valor del radio del grid
                    radius = Convert.ToInt32(Radius.Text) * 2 + 1;
                    mesh = new Grid(radius);
                    //Creamos los objetos rectángulos para cada grid (fase y temperatura)
                    phaseRectangles = new Rectangle[radius, radius];
                    temperatureRectangles = new Rectangle[radius, radius];
                    phaseCanvas.Children.Clear();
                    temperatureCanvas.Children.Clear();
                    loadGrid();
                    //Si el radio es 0, aparece la label de "WRONG INPUTS"
                    if (mesh.getSize() == 0)
                    {
                        WIRadius.Visibility = Visibility.Visible;
                    }
                    loadParameters();
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

        //Método que permite cargar un grid con radio previamente determinado
        private void loadGrid() 
        {
            //Creamos los objetos rectángulos para cada grid (fase y temperatura)
            phaseRectangles = new Rectangle[radius, radius];
            temperatureRectangles = new Rectangle[radius, radius];
            phaseCanvas.Children.Clear();
            temperatureCanvas.Children.Clear();
            for (int i = 0; i < radius; i++)
            {
                for (int j = 0; j < radius; j++)
                {
                    //Grid fase
                    Rectangle pRectangle = new Rectangle();
                    //Características de los rectángulos del grid
                    pRectangle.Width = phaseCanvas.Width / radius;
                    pRectangle.Height = phaseCanvas.Height / radius;
                    pRectangle.Fill = new SolidColorBrush(Colors.Transparent);
                    pRectangle.StrokeThickness = 0.1;
                    pRectangle.Stroke = Brushes.White;
                    phaseCanvas.Children.Add(pRectangle);

                    //Posición del rectángulo dentro del grid
                    Canvas.SetTop(pRectangle, i * pRectangle.Height);
                    Canvas.SetLeft(pRectangle, j * pRectangle.Width);

                    pRectangle.Tag = new Point(i, j);
                    //Llamada de evento para saber si he entrado o salido de un rectángulo con el ratón
                    pRectangle.MouseEnter += new MouseEventHandler(rectangle_MouseEnter);
                    pRectangle.MouseLeave += new MouseEventHandler(rectangle_MouseLeave);
                    phaseRectangles[i, j] = pRectangle;

                    //Grid temperatura
                    Rectangle tRectangle = new Rectangle();
                    //Características de los rectángulos del grid
                    tRectangle.Width = temperatureCanvas.Width / radius;
                    tRectangle.Height = temperatureCanvas.Height / radius;
                    tRectangle.Fill = new SolidColorBrush(Colors.Transparent);
                    tRectangle.StrokeThickness = 0.1;
                    tRectangle.Stroke = Brushes.White;
                    temperatureCanvas.Children.Add(tRectangle);

                    //Posición del rectángulo dentro del grid
                    Canvas.SetTop(tRectangle, i * tRectangle.Height);
                    Canvas.SetLeft(tRectangle, j * tRectangle.Width);

                    tRectangle.Tag = new Point(i, j);
                    //Llamada de evento para saber si he entrado o salido de un rectángulo con el ratón
                    tRectangle.MouseEnter += new MouseEventHandler(rectangle_MouseEnter);
                    tRectangle.MouseLeave += new MouseEventHandler(rectangle_MouseLeave);
                    temperatureRectangles[i, j] = tRectangle;
                }
            }
            //Se establece el Contorno Constante como frontera predeterminada
            ContourSelection.SelectedIndex = 0;

            //Se cambia el valor de fase y temperatura de la celda central
            mesh.startCell((radius - 1) / 2, (radius - 1) / 2);

            //Se actualiza la visualización del mesh
            updateMesh();

            //Se limpian los valores guardados de cada uno de los parámetros
            history.Clear();
            phaseValues.Clear();
            temperatureValues.Clear();

            //Se guarda el grid inicial en el historial
            history.Push(mesh.deepCopy());

            //Se visualiza un grupo de componentes del programa para poder establecer los diferentes parámetros de simulación
            showElements1();

            //Se calcula la fase y temperatura medias de todo el grid
            Tuple<double, double> averageTempPhase = mesh.getAverageTemperaturePhase();
            phaseValues.Add(averageTempPhase.Item2);
            temperatureValues.Add(averageTempPhase.Item1);

            //Se oculta la label de "WRONG INPUTS" cuando el radio es correcto
            WIRadius.Visibility = Visibility.Hidden;
            WrongFile.Visibility = Visibility.Hidden;
        }

        //Método que refleja visualmente los cambios de las valores de fase y temperatura de cada celda
        private void updateMesh()
        {
            for (int i = 0; i < radius; i++)
            {
                for (int j = 0; j < radius; j++)
                {
                    //Debido a que hay ciertos casos en los que la fase y temperatura se salen de sus valores físicos, aplicamos una simple corrección para el color.
                    double correctedTemperature;
                    double correctedPhase;

                    //Correcciones de temperatura
                    if (mesh.getCellTemperature(i, j) < -1)
                    {
                        correctedTemperature = -1;
                    }

                    else if (mesh.getCellTemperature(i,j) > 0)
                    {
                        correctedTemperature = 0;
                    }

                    else
                    {
                        correctedTemperature = mesh.getCellTemperature(i, j);
                    }
                    double aasd = mesh.getCellPhase(i, j);
                    //Correciones de fase
                    if (mesh.getCellPhase(i, j) > 1)
                    {
                        correctedPhase = 1;
                    }

                    else if (mesh.getCellPhase(i, j) < 0)
                    {
                        correctedPhase = 0;
                    }

                    else
                    {
                        correctedPhase = mesh.getCellPhase(i, j);
                    }

                    //Se calcula la distribución de colores de acuerdo a una función exponencial
                    double a = 255 * Math.Pow(1.0 - correctedPhase, 1.0 / 4.0);
                    double b = 255 * Math.Sqrt(correctedTemperature + 1);

                    //Se rellenan los rectángulos con color
                    phaseRectangles[i, j].Fill = new SolidColorBrush(Color.FromArgb(Convert.ToByte(a), 0, 0, 255));
                    temperatureRectangles[i, j].Fill = new SolidColorBrush(Color.FromArgb(Convert.ToByte(b), 255, 0, 0));
                }
            }
        }

        //Método que permite establecer los parámetros de la gráfica
        private void setChartNumbers()
        {
            SeriesCollection = new SeriesCollection
            {
                //Gráfica de fase, con sus propiedades
                new LineSeries
                {
                    Values = phaseValues,
                    ScalesYAt = 0,
                    Stroke = new SolidColorBrush(Color.FromRgb(75, 75, 255)),
                    Fill = Brushes.Transparent,
                    Title = "Avg.Phase"
                }
            };
            Chart1.Series = SeriesCollection;
            SeriesCollection = new SeriesCollection
            {
                //Gráfica de temperatura, con sus propiedades
                new LineSeries
                {
                Values = temperatureValues,
                ScalesYAt = 0,
                Stroke = new SolidColorBrush(Color.FromRgb(255, 75, 75)),
                Fill = Brushes.Transparent,
                Title = "Avg.Temperature"
                }
            };
            
            Chart1_Copy.Series = SeriesCollection;
        }

        //Método que permite visualizar los parámetros de simulación, las condiciones de frontera y los botones que permiten interactuar con la simulación
        private void showElements1()
        {
            //Si el valor del radio introducido es diferente de 0
            if (mesh.getSize()!= 0)
            {
                SimControls.Visibility = Visibility.Visible;
                GridandGraphs.Visibility = Visibility.Visible;
                WIRadius.Visibility = Visibility.Hidden;
                Radius.Text = Convert.ToString((mesh.getSize() - 1) / 2);
            }
        }

        //Evento que permite establecer los parámetros de simulación
        private void LoadParameters(object sender, RoutedEventArgs e)
        {
            loadParameters();
            WrongFile.Visibility = Visibility.Hidden;
        }

        //Evento que permite cargar los parametros del grid
        private void loadParameters() 
        {
            //Standard A
            if (TabControl.SelectedIndex == 0)
            {
                r = new Rules(Convert.ToDouble(m1.Text), Convert.ToDouble(dt1.Text), Convert.ToDouble(d1.Text), Convert.ToDouble(e1.Text), Convert.ToDouble(b1.Text), Convert.ToDouble(dx1.Text), Convert.ToDouble(dy1.Text));
                mesh.setRules(r);
                Correctparameters.Content = "Standard A loaded!";
                Correctparameters.Visibility = Visibility.Visible;
                Wrongparameters.Visibility = Visibility.Hidden;
            }

            //Standard B
            else if (TabControl.SelectedIndex == 1)
            {
                r = new Rules(Convert.ToDouble(m2.Text), Convert.ToDouble(dt2.Text), Convert.ToDouble(d2.Text), Convert.ToDouble(e2.Text), Convert.ToDouble(b2.Text), Convert.ToDouble(dx2.Text), Convert.ToDouble(dy2.Text));
                mesh.setRules(r);
                Correctparameters.Content = "Standard B loaded!";
                Correctparameters.Visibility = Visibility.Visible;
                Wrongparameters.Visibility = Visibility.Hidden;
            }

            //Custom
            else if (TabControl.SelectedIndex == 2)
            {
                //Si los parámetros introducidos son correctos
                try
                {
                    r = new Rules(Convert.ToDouble(m3.Text), Convert.ToDouble(dt3.Text), Convert.ToDouble(d3.Text), Convert.ToDouble(e3.Text), Convert.ToDouble(b3.Text), Convert.ToDouble(dx3.Text), Convert.ToDouble(dy3.Text));
                    mesh.setRules(r);
                    Correctparameters.Content = "Custom loaded!";
                    Wrongparameters.Visibility = Visibility.Hidden;
                    Correctparameters.Visibility = Visibility.Visible;
                }
                //Si los paráetros introducidos no son correctos
                catch (FormatException)
                {
                    Wrongparameters.Visibility = Visibility.Visible;
                    Correctparameters.Visibility = Visibility.Hidden;
                }
            }
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

        //Evento que permite cambiar el botón Start/Stop a la configuración de Start (Color verde)
        private void showStartButton()
        {
            if (timerStatus)
            {
                buttonStart.Content = "Start";
                buttonStart.Background = Brushes.Green;
                buttonStart.BorderBrush = Brushes.White;
                buttonStart.Foreground = Brushes.White;
                timer.Stop();
                timerStatus = false;
            }
        }

        //Evento que permite cmbiar el botón Start/Stop a la configuración de Stop (Color rojo)
        private void startStop()
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
                showStartButton();
            }

        }

        //Evento que permite comenzar la simulación de forma automatica 
        private void buttonStart_Click(object sender, RoutedEventArgs e)
        {
            if (Wrongparameters.Visibility==Visibility.Hidden)
            {
                WrongFile.Visibility = Visibility.Hidden;
                startStop();
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
            phaseValues.Add(averageTempPhase.Item2);
            temperatureValues.Add(averageTempPhase.Item1);
            //Actualiza los valores de las labels en las que se informa de la fase y temperatura de la celda sobre la que el usuario tiene el ratón
            insideornot();
        }

        //Evento que permite ejecutar la siguiente iteración de la simulación
        private void nextIteration_Click(object sender, RoutedEventArgs e)
        {
            if (Wrongparameters.Visibility == Visibility.Hidden)
            {
                //Detenemos el timer.
                showStartButton();
                WrongFile.Visibility = Visibility.Hidden;
                //Se añaden los valores al historial
                history.Push(mesh.deepCopy());
                //Se realiza el calculo de la próxima fase y temperatura 
                mesh.Iterate();
                //Cálculo de la fase y temperatura medias en cada iteración
                Tuple<double, double> averageTempPhase = mesh.getAverageTemperaturePhase();
                phaseValues.Add(averageTempPhase.Item2);
                temperatureValues.Add(averageTempPhase.Item1);
                //Se actulizan los cambios visualmente
                updateMesh();
                //Actualiza los valores de las labels en las que se informa de la fase y temperatura de la celda sobre la que el usuario tiene el ratón
                insideornot();
            }
        }

        //Evento que permite ejecutar la anterior iteración de la simulación
        private void previousIteration_Click(object sender, RoutedEventArgs e)
        {
            if (Wrongparameters.Visibility == Visibility.Hidden)
            {
                WrongFile.Visibility = Visibility.Hidden;
                try
                {
                    //Se detiene reloj.
                    showStartButton();
                    //Se quita el último elemento del historial
                    if (history.Count > 1)
                    {
                        mesh = history.Pop();
                    }

                    //Se quitan los ultimos valores de fase y temperaturas de la gráfica
                    if (phaseValues.Count != 1)
                    {
                        phaseValues.RemoveAt(phaseValues.Count - 1);
                        temperatureValues.RemoveAt(temperatureValues.Count - 1);
                    }
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
        }

        //Evento que permite volver a comenzar la simulación
        private void restart_Click(object sender, RoutedEventArgs e)
        {
            WrongFile.Visibility = Visibility.Hidden;
            //Se limpia el historial
            history.Clear();
            //Se para el reloj
            showStartButton();
            //Se resetear el grid
            mesh.reset();
            mesh.startCell(mesh.getSize()/2, mesh.getSize()/2);
            //Añadimos una copia del grid al historial
            history.Push(mesh.deepCopy());
            //Se eliminan todos los valors de fase y temperaturas medias de la gráfica
            phaseValues.Clear();
            temperatureValues.Clear();
            //Cálculo de la fase y temperatura medias en cada iteración
            Tuple<double, double> averageTempPhase = mesh.getAverageTemperaturePhase();
            phaseValues.Add(averageTempPhase.Item2);
            temperatureValues.Add(averageTempPhase.Item1);
            //Se actulizan los cambios visualmente
            updateMesh();
            //Actualiza los valores de las labels en las que se informa de la fase y temperatura de la celda sobre la que el usuario tiene el ratón
            insideornot();
        }

        //Evento que actualiza y muestra el valor de fase y temperatura de la celda sobre la que se encuentra el ratón del usuario
        private void rectangle_MouseEnter(object sender, MouseEventArgs e)
        {
            //Obtnemos la ubicación del puntero
            Rectangle reg = (Rectangle)sender;
            Point p = (Point)reg.Tag;
            //Mostramos las etiquetas correspondientes a los estados de las celdas
            Cellstatus.Visibility = Visibility.Visible;
            //Actualizamos los valores de fase y temperatura
            CellPhase.Content = Math.Round(mesh.getCellPhase(Convert.ToInt32(p.X), Convert.ToInt32(p.Y)), 5);
            CellTemperature.Content = Math.Round(mesh.getCellTemperature(Convert.ToInt32(p.X), Convert.ToInt32(p.Y)), 5);
            //Actualizamos casilla seleccionada
            rowInside = Convert.ToInt32(p.X);
            columnInside = Convert.ToInt32(p.Y);
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
                CellPhase.Content = Math.Round(mesh.getCellPhase(rowInside, columnInside), 5);
                CellTemperature.Content = Math.Round(mesh.getCellTemperature(rowInside, columnInside), 5);
            }
        }

        //Evento que permite guardar la simulación
        private void saveSimulation_Click(object sender, RoutedEventArgs e)
        {
            mesh.saveGrid();
            showStartButton();
            WrongFile.Visibility = Visibility.Hidden;
        }

        //Evento que permite cargar la simulación
        private void loadSimualtion_Click(object sender, RoutedEventArgs e)
        {
            firstGrid = false;
            //Detenemos el timer
            showStartButton();
            //Si no hay malla creada
            if (mesh == null)
            {
                mesh = new Grid(0);
                //Ejecutamos la función loadGrid() y guardamos el resultado
                int result = mesh.loadGrid();
                if (result == 0)
                {
                    firstGrid = true;
                    //Se establace el radio del grid
                    int size = mesh.getSize() + 2;
                    radius = size - 2;
                    loadGrid();
                    loadParameters();
                }
                //Si la carga del fichero no ha sido satisfactoria
                else
                {
                    mesh = null;
                    if (result == -1)
                    {
                        WrongFile.Visibility = Visibility.Visible;
                    }
                }
            }

            //Si ya hay una malla creada y no es la primera vez que cargamos fichero
            if (mesh != null && !firstGrid)
            {
                temporaryMesh = mesh.deepCopy();
                int result = mesh.loadGrid();
                if (result == 0)
                {
                    //Se establace el radio del grid
                    int size = new int();
                    size = mesh.getSize() + 2;
                    radius = size - 2;
                    loadGrid();
                }
                else
                {
                    //Si la carga del fichero no ha sido satisfactoria
                    if (result == -1)
                    {
                        mesh = temporaryMesh.deepCopy();
                        WrongFile.Visibility = Visibility.Visible;
                    }
                }
            }
        }
    }
}
