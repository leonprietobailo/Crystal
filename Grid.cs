using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Win32;


namespace GameOfLife
{
    class Grid
    {
        //ATRIBUTOS
        int i, j;
        Cell[,] array;
        int boundaries;
        Rules r;

        //CONSTRUCTOR
        public Grid(Grid grid)
        {
            i = grid.i;
            j = grid.j;
            r = grid.r;
            array = new Cell[i, j];
            for (int n = 0; n < i; n++)
            {
                for(int s = 0; s < j; s++)
                {
                    array[n, s] = new Cell(grid.array[n,s].getTemperature(), grid.array[n, s].getPhase());
                }
            }
        }

        public Grid(int iIn, int jIn)
        {
            i = iIn + 2;
            j = jIn + 2;
            array = new Cell[this.i, this.j];
            for (int n = 0; n < this.i; n++)
            {
                for (int s = 0; s < this.j; s++)
                {
                    array[n, s] = new Cell(-1, 1);
                }
            }
            r = new Rules();
        }

        public void setRules(Rules rules)
        {
            r = rules;
        }

        public void setBoundaries(int b)
        {
            boundaries = b;
            setBoundaryLayer();
        }

        public int[] getSize()
        {
            int[] size = new int[2];
            size[0] = i - 2;
            size[1] = j - 2;
            return size;
        }
        public void reset()
        {
            for (int n = 1; n < i - 1; n++)
            {
                for (int s = 1; s < j - 1; s++)
                {
                    array[n, s] = new Cell(-1, 1);
                }
            }
        }

        public void clickedCell(int x, int y)
        {
            if (array[x, y].getTemperature() == 0 && array[x, y].getPhase() == 0)
            {
                array[x, y] = new Cell(-1, 1);
            }
            else
            {
                array[x, y] = new Cell(0, 0);
            }
        }

        public double getCellPhase(int n, int s)
        {
            return array[n + 1, s + 1].getPhase();
        }

        public double getCellTemperature(int n, int s)
        {
            return array[n + 1, s + 1].getTemperature();
        }

        // RETOCAR
        public void setBoundaryLayer()
        {
            if (boundaries == 1)
            {
                array[0, 0] = array[2, 2];
                array[0, j - 1] = array[2, j - 3];
                array[i - 1, 0] = array[i - 3, 2];
                array[i - 1, j - 1] = array[i - 3, j - 3];

                for (int n = 1; n < i - 1; n++)
                {
                    array[n, 0] = array[n, 2];
                    array[n, j - 1] = array[n, j - 3];
                }

                for (int s = 1; s < i - 1; s++)
                {
                    array[0, s] = array[2, s];
                    array[i - 1, s] = array[i - 3, s];
                }
            }
            else
            {
                for (int n = 0; n < i; n++)
                {
                    array[n, 0] = new Cell(-1, 1);
                    array[n, j - 1] = new Cell(-1, 1);
                }

                for (int s = 0; s < i; s++)
                {
                    array[0, s] = new Cell(-1, 1);
                    array[i - 1, s] = new Cell(-1, 1);
                }
            }
        }

        public Grid deepCopy()
        {
            Grid deepCopyGrid = new Grid(this);
            return deepCopyGrid;
        }

        public void Iterate()
        {
            for (int n = 1; n < this.i - 1; n++)
            {
                for (int s = 1; s < this.j - 1; s++)
                {
                    double[] uN = new double[4];
                    double[] pN = new double[4];

                    uN[0] = array[n, s + 1].getTemperature();
                    uN[1] = array[n - 1, s].getTemperature();
                    uN[2] = array[n + 1, s].getTemperature();
                    uN[3] = array[n, s - 1].getTemperature();

                    pN[0] = array[n, s + 1].getPhase();
                    pN[1] = array[n - 1, s].getPhase();
                    pN[2] = array[n + 1, s].getPhase();
                    pN[3] = array[n, s - 1].getPhase();

                    array[n, s].getNextStatus(r, uN, pN);
                }
            }

            for (int n = 1; n < this.i - 1; n++)
            {
                for (int s = 1; s < this.j - 1; s++)
                {
                    array[n, s].setNextStatus();
                }
            }
        }

        public void saveGrid()
        {
            SaveFileDialog dig = new SaveFileDialog();
            dig.Filter = "(*.txt)|*.*";
            dig.DefaultExt = "txt";


            if (dig.ShowDialog() == true)
            {
                FileStream emptyFile = File.Create(dig.FileName);
                emptyFile.Close();
                for (int n = 0; n < this.i; n++)
                {
                    for (int s = 0; s < this.j; s++)
                    {
                        File.AppendAllText(dig.FileName, Convert.ToString(array[n, s].getTemperature()));
                        File.AppendAllText(dig.FileName, "|");
                        File.AppendAllText(dig.FileName, Convert.ToString(array[n, s].getPhase()));
                        if (s != j - 1)
                        {
                            File.AppendAllText(dig.FileName, " ");
                        }
                    }
                    if (n != i - 1)
                    {
                        File.AppendAllText(dig.FileName, "\n");
                    }
                }
            }
        }

        public void loadGrid()
        {
            var n = 0;
            var s = 0;
            bool readColumns = false;

            OpenFileDialog dig = new OpenFileDialog();
            dig.Multiselect = false;
            dig.Filter = "(*.txt*)|*.*";
            dig.DefaultExt = ".txt";
            if (dig.ShowDialog() == true)
            {
                StreamReader countFile = new StreamReader(dig.FileName);
                string strReadline = countFile.ReadLine();
                while (strReadline != null)
                {
                    if (!readColumns)
                    {
                        
                        s = strReadline.Split('|').Length - 1;
                        readColumns = true;
                    }
                    n++;
                    strReadline = countFile.ReadLine();
                }
                countFile.Close();

                StreamReader readFile = new StreamReader(dig.FileName);
                int rows = n;
                int columns = s;
                Grid loadedGrid = new Grid(rows, columns);
                strReadline = readFile.ReadLine();
                s = 0;
                string[] temperaturePhase;
                while (strReadline != null)
                {
                    string[] subs = strReadline.Split(' ');
                    for (n = 0; n < subs.Length; n++)
                    {
                        temperaturePhase = subs[n].Split('|');
                        loadedGrid.array[s, n] = new Cell(Convert.ToDouble(temperaturePhase[0]), Convert.ToDouble(temperaturePhase[1]));
                    }
                    strReadline = readFile.ReadLine();
                    s++;
                }
                readFile.Close();
                i = rows;
                j = columns;
                array = loadedGrid.array;
            }
        }
    }
}
