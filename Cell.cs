using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameOfLife
{
    class Cell
    {
        // ATRIBUTOS
        double temperature, phase, temperatureNext, phaseNext;
        //CONSTRUCTORES
        public Cell(double temperatureIn, double phaseIn)
        {
            temperature = temperatureIn;
            phase = phaseIn;
        }

        public void getNextStatus(Rules r, double[] uN, double[] pN)
        {
            // ESTRUCTURA VECTOR 
            double[] rules = r.getRules();
            double m = rules[0];
            double dt = rules[1];
            double d = rules[2];
            double e = rules[3];
            double b = rules[4];
            double dx = rules[5];
            double dy = rules[6];
            
            // PHASE
            double dPHI2dxy = (pN[2] - 2.0 * phase + pN[1]) / dx / dx + (pN[0] - 2.0 * phase + pN[3]) / dy / dy;
            double dPHIdt = 1.0 / e / e / m * (phase * (1.0 - phase) * (phase - 1.0 / 2.0 + 30.0 * e * b * d * temperature * phase * (1.0 - phase)) + e * e * dPHI2dxy);

            // TEMPERATURE
            double du2dxy = (uN[2] - 2.0 * temperature + uN[1]) / dx / dx + (uN[0] - 2.0 * temperature + uN[3]) / dy / dy;
            double dudt = du2dxy - 1.0 / d * (30.0 * Math.Pow(phase, 2) - 60.0 * Math.Pow(phase, 3) + 30.0 * Math.Pow(phase, 4)) * dPHIdt;

            // NEXT STATUS
            phaseNext = phase + dPHIdt * dt;
            temperatureNext = temperature + dudt * dt;
        }

        public void setNextStatus()
        {
            phase = phaseNext;
            temperature = temperatureNext;

        }

        public double getTemperature()
        {
            return temperature;
        }

        public double getPhase()
        {
            return phase;
        }
    }
}