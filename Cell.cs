using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameOfLife
{
    class Cell
    {
        // Atributos de la celda
        double temperature, phase, temperatureNext, phaseNext;

        //Constructor de la celda
        public Cell(double temperatureIn, double phaseIn)
        {
            temperature = temperatureIn;
            phase = phaseIn;
        }

        //Método que permite conocer la temperatura de una celda
        public double getTemperature()
        {
            return temperature;
        }

        //Método que permite conocer la fase de una celda
        public double getPhase()
        {
            return phase;
        }

        //Método que permite calcular el próximo estado de fase y temperatura de una celda mediante la ecuación del cristal
        public void getNextStatus(Rules r, double[] uN, double[] pN)
        {
            // Vector que contiene el conjunto de parámetros
            double[] rules = r.getRules();
            double m = rules[0];
            double dt = rules[1];
            double d = rules[2];
            double e = rules[3];
            double b = rules[4];
            double dx = rules[5];
            double dy = rules[6];
            
            // Cálculo de fase
            double dPHI2dxy = (pN[2] - 2.0 * phase + pN[1]) / dx / dx + (pN[0] - 2.0 * phase + pN[3]) / dy / dy;
            double dPHIdt = 1.0 / e / e / m * (phase * (1.0 - phase) * (phase - 1.0 / 2.0 + 30.0 * e * b * d * temperature * phase * (1.0 - phase)) + e * e * dPHI2dxy);

            // Cálculo de temperatura
            double du2dxy = (uN[2] - 2.0 * temperature + uN[1]) / dx / dx + (uN[0] - 2.0 * temperature + uN[3]) / dy / dy;
            double dudt = du2dxy - 1.0 / d * (30.0 * Math.Pow(phase, 2) - 60.0 * Math.Pow(phase, 3) + 30.0 * Math.Pow(phase, 4)) * dPHIdt;

            // Se establece el próximo estado de fase y temperatura de la celda
            phaseNext = phase + dPHIdt * dt;
            temperatureNext = temperature + dudt * dt;
        }

        //Método que permite establecer el próximo estado de la celda como el estado actual
        public void setNextStatus()
        {
            phase = phaseNext;
            temperature = temperatureNext;
        }
    }
}