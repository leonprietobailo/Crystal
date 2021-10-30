using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Win32;


namespace Crystal
{
    class Grid
    {
        //Atributos del grid
        int radius, boundaries;
        Cell[,] array;
        Rules r;

        //Constructor del grid (desde otro grid).
        public Grid(Grid grid)
        {
            radius = grid.radius;
            r = grid.r;
            array = new Cell[radius, radius];
            for (int n = 0; n < radius; n++)
            {
                for (int s = 0; s < radius; s++)
                {
                    array[n, s] = new Cell(grid.array[n, s].getTemperature(), grid.array[n, s].getPhase());
                }
            }
        }

        //Constructor del grid (con un radio definido)
        public Grid(int iIn)
        {
            //Se añaden las fronteras a las dimensiones introducidas
            radius = iIn + 2;
            array = new Cell[this.radius, this.radius];
            //Se rellena la malla con un estado por defecto
            for (int n = 0; n < this.radius; n++)
            {
                for (int s = 0; s < this.radius; s++)
                {
                    array[n, s] = new Cell(-1, 1);
                }
            }
            r = new Rules();
        }

        //Método que cambia el valor de fase y temperatura de la célula central del grid: valor fase (0) / valor temperatura (0)
        public void startCell(int n, int s)
        {
            array[n + 1, s + 1] = new Cell(0, 0);
        }

        //Método que retorna el tamaño del grid visible (sin contorno)
        public int getSize()
        {
            int size = radius - 2;
            return size;
        }

        //Método que retorna la temperatura de una celda del grid
        public double getCellTemperature(int n, int s)
        {
            return array[n + 1, s + 1].getTemperature();
        }

        //Método que retorna la fase de una celda del grid
        public double getCellPhase(int n, int s)
        {
            return array[n + 1, s + 1].getPhase();
        }

        //Método que retorna el valor de la temperatura y fase media de todas las celdas del grid
        public Tuple<double, double> getAverageTemperaturePhase()
        {
            int counter = 0;
            double totalTemperature = 0;
            double totalPhase = 0;
            //Recorre el grid y va calculando la suma de los valores de fase y temperatura de cada celda
            for (int n = 1; n < this.radius - 1; n++)
            {
                for (int s = 1; s < this.radius - 1; s++)
                {
                    totalTemperature += array[n, s].getTemperature();
                    totalPhase += array[n, s].getPhase();
                    counter++;
                }
            }
            //Se retornan las respectivas medias
            return Tuple.Create(totalTemperature / counter, totalPhase / counter);
        }

        //Método que permite establecer las reglas
        public void setRules(Rules rules)
        {
            r = rules;
        }

        //Método que establece las fronteras del grid: contorno reflector o constante
        public void setBoundaries(int b)
        {
            boundaries = b;
            setBoundaryLayer();
        }

        // Método que establece los valores de fase y temperatura de las fronteras del grid
        public void setBoundaryLayer()
        {
            //Contorno reflector
            if (boundaries == 1)
            {
                //Se establecen los valores de las esquinas de las fronteras
                array[0, 0] = array[2, 2];
                array[0, radius - 1] = array[2, radius - 3];
                array[radius - 1, 0] = array[radius - 3, 2];
                array[radius - 1, radius - 1] = array[radius - 3, radius - 3];

                //Se establecen el resto de valores de las fronteras
                for (int n = 1; n < radius - 1; n++)
                {
                    array[n, 0] = array[n, 2];
                    array[n, radius - 1] = array[n, radius - 3];
                    array[0, n] = array[2, n];
                    array[radius - 1, n] = array[radius - 3, n];
                }
            }
            //Contorno constante
            else
            {
                for (int n = 0; n < radius; n++)
                {
                    array[n, 0] = new Cell(-1, 1);
                    array[n, radius - 1] = new Cell(-1, 1);
                    array[0, n] = new Cell(-1, 1);
                    array[radius - 1, n] = new Cell(-1, 1);
                }
            }
        }

        //Método que permite realizar una iteración calculando el próximo estado de la celda y actualizándolo como estado actual
        public void Iterate()
        {
            //Se calcula el próximo valor de fase y temperatura de cada celda
            for (int n = 1; n < this.radius - 1; n++)
            {
                for (int s = 1; s < this.radius - 1; s++)
                {
                    //Se establecen los vectores que contendrán los valores de fase y temperatura de las celdas vecinas
                    double[] uN = new double[4];
                    double[] pN = new double[4];

                    //Se obtiene el valor de temperatura de cada celda vecina
                    uN[0] = array[n, s + 1].getTemperature();
                    uN[1] = array[n - 1, s].getTemperature();
                    uN[2] = array[n + 1, s].getTemperature();
                    uN[3] = array[n, s - 1].getTemperature();

                    //Se obtiene el valor de fase de cada celda vecina
                    pN[0] = array[n, s + 1].getPhase();
                    pN[1] = array[n - 1, s].getPhase();
                    pN[2] = array[n + 1, s].getPhase();
                    pN[3] = array[n, s - 1].getPhase();

                    array[n, s].getNextStatus(r, uN, pN);
                }
            }
            //Se establece el próximo estado de fase y temperatura de cada celda anteriormente calculado como estado actual
            for (int n = 1; n < this.radius - 1; n++)
            {
                for (int s = 1; s < this.radius - 1; s++)
                {
                    array[n, s].setNextStatus();
                }
            }
        }

        //Método que resetea el grid asociando a cada celda un valor de fase y termperatura predeterminados: valor fase (1) / valor temperatura (-1)
        //En la celda central del grid se asocian: valor fase (0) / valor temperatura (0)
        public void reset()
        {
            for (int n = 1; n < radius - 1; n++)
            {
                for (int s = 1; s < radius - 1; s++)
                {
                    array[n, s] = new Cell(-1, 1);
                }
            }
        }

        //Método que realiza una copia del grid y la retorna
        public Grid deepCopy()
        {
            Grid deepCopyGrid = new Grid(this);
            return deepCopyGrid;
        }

        //Método que permite guardar el grid con todos los valores de fase y temperatura de cada celda en un archivo de texto
        public void saveGrid()
        {
            //Abre el explorador de archivos y permite guardar la simulación
            SaveFileDialog dig = new SaveFileDialog();
            //Obtiene la cadena de filtro que determina qué tipos de archivos se muestran desde SaveFileDialog
            dig.Filter = "(*.txt)|*.*";
            //Especifica la cadena de la extensión predeterminada que se va a usar para filtrar la lista de archivos que se muestran
            dig.DefaultExt = "txt";

            //Si se ha seleccionado guardar la simulación
            if (dig.ShowDialog() == true)
            {
                //Crea el archivo txt donde se guardará la simulación
                FileStream emptyFile = File.Create(dig.FileName);
                //Cierra el archivo
                emptyFile.Close();

                //Recorre el grid de la simulación que se quiere guardar
                for (int n = 0; n < this.radius; n++)
                {
                    for (int s = 0; s < this.radius; s++)
                    {
                        //Escribe en el archivo los valores de temperatura y fase por "|"
                        File.AppendAllText(dig.FileName, Convert.ToString(array[n, s].getTemperature()));
                        File.AppendAllText(dig.FileName, "|");
                        File.AppendAllText(dig.FileName, Convert.ToString(array[n, s].getPhase()));
                        //Separa cada valor de celda con " "
                        if (s != radius - 1)
                        {
                            File.AppendAllText(dig.FileName, " ");
                        }
                    }
                    //Hace un salto de linea cada vez que estamos en una fila diferente del grid
                    if (n != radius - 1)
                    {
                        File.AppendAllText(dig.FileName, "\n");
                    }
                }
            }
        }

        //Método que permite cargar un archivo de texto con una simulación
        public int loadGrid()
        {
            try
            {
                //Abre el explorador de archivos y permite especificar un nombre de archivo 
                OpenFileDialog dig = new OpenFileDialog();
                //Impide cargar varios archivos a la vez
                dig.Multiselect = false;
                //Obtiene la cadena de filtro que determina qué tipos de archivos se muestran desde OpenFileDialog
                dig.Filter = "(*.txt*)|*.*";
                //Especifica la cadena de la extensión predeterminada que se va a usar para filtrar la lista de archivos que se muestran
                dig.DefaultExt = ".txt";

                //Si se ha seleccionado el archivo
                if (dig.ShowDialog() == true)
                {
                    //Lee el archivo seleccionado
                    StreamReader readFile = new StreamReader(dig.FileName);
                    string strReadline = readFile.ReadLine();

                    //Cuenta el número de columnas que tiene el archivo y actualiza el atributo "i" de la clase Grid
                    radius = strReadline.Split('|').Length - 1;

                    //Carga un grid cuadrado de dimension igual al numero de columnas
                    Grid loadedGrid = new Grid(radius);

                    //Recorre el grid creado y establece cada valor de fase y temperatura de cada celda con la información del archivo que estamos leyendo
                    int s = 0;
                    string[] temperaturePhase;
                    while (strReadline != null)
                    {
                        //Se separa para obtener la información individual de cada celda
                        string[] subs = strReadline.Split(' ');
                        for (int n = 0; n < subs.Length; n++)
                        {
                            //Se separa para obtener el valor de fase y temperatura individualmente
                            temperaturePhase = subs[n].Split('|');
                            //Se asocian los valores del archivo leido a nuevas celdas
                            loadedGrid.array[s, n] = new Cell(Convert.ToDouble(temperaturePhase[0]), Convert.ToDouble(temperaturePhase[1]));
                        }
                        strReadline = readFile.ReadLine();
                        s++;
                    }

                    //Cerramos el archivo que se estaba leyendo
                    readFile.Close();

                    //Actualizamos el valor del atributo "array" de la clase Grid
                    array = loadedGrid.array;
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            //Caso en el que el formato del archivo que se quiere cargar no sea correcto
            catch (FileFormatException)
            {
                return -1;
            }

            //Caso en el que el contenido del archivo que se quiere cargar no sea correcto
            catch (FormatException)
            {
                return -1;
            }
        }
    }
}
