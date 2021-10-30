using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Crystal
{
    class Cell
    {
        //Atributos de la celda: fase y temperatura actuales y en la próxima iteración
        double temperature, phase, temperatureNext, phaseNext;

        //Constructor de la celda
        public Cell(double temperatureIn, double phaseIn)
        {
            temperature = temperatureIn;
            phase = phaseIn;
        }

        //Método que retorna la temperatura de una celda
        public double getTemperature()
        {
            return temperature;
        }

        //Método que retorna la fase de una celda
        public double getPhase()
        {
            return phase;
        }

        //Método que permite calcular el próximo estado de fase y temperatura de una celda mediante la ecuación del cristal
        public void getNextStatus(Rules r, double[] uN, double[] pN)
        {
            //Vector que contiene el conjunto de parámetros
            double[] rules = r.getRules();

            //Cálculo del gradiente de fase
            double dPHI2dxy = (pN[2] - 2.0 * phase + pN[1]) / rules[5] / rules[5] + (pN[0] - 2.0 * phase + pN[3]) / rules[6] / rules[6];
            //Cálculo de la derivada parcial de la fase con respecto el tiempo
            double dPHIdt = 1.0 / rules[3] / rules[3] / rules[0] * (phase * (1.0 - phase) * (phase - 1.0 / 2.0 + 30.0 * rules[3] * rules[4] * rules[2] * temperature * phase * (1.0 - phase)) + rules[3] * rules[3] * dPHI2dxy);

            //Cálculo del gradiente de temperatura
            double du2dxy = (uN[2] - 2.0 * temperature + uN[1]) / rules[5] / rules[5] + (uN[0] - 2.0 * temperature + uN[3]) / rules[6] / rules[6];
            //Cálculo de la derivada parcial de la temperatura con respecto el tiempo
            double dudt = du2dxy - 1.0 / rules[2] * (30.0 * Math.Pow(phase, 2) - 60.0 * Math.Pow(phase, 3) + 30.0 * Math.Pow(phase, 4)) * dPHIdt;

            //Cálculo del próximo estado de fase y temperatura de la celda
            phaseNext = phase + dPHIdt * rules[1];
            temperatureNext = temperature + dudt * rules[1];
        }

        //Método que permite establecer el próximo estado de la celda como el estado actual
        public void setNextStatus()
        {
            phase = phaseNext;
            temperature = temperatureNext;
        }
    }
}