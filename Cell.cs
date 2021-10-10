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
            double n = rules[1];  // ATENTO
            double dt = rules[2];
            double d = rules[3];
            double e = rules[4];
            double b = rules[5];
            double dx = rules[6];
            double dy = rules[7];

            // PHASE
            double dPHI2dxy = (pN[2] - 2 * phase + pN[1]) / dx / dx + (pN[0] - 2 * phase + pN[3]) / dy / dy;
            double dPHIdt = 1 / e / e / m * (phase * (1 - phase) * (phase - 1 / 2 + 30 * e * b * d * temperature * phase * (1 - phase)) + e * e * dPHI2dxy);

            // TEMPERATURE
            double du2dxy = (uN[2] - 2 * temperature + uN[1]) / dx / dx + (uN[0] - 2 * temperature + uN[3]) / dy / dy;
            double dudt = du2dxy - 1 / d * (30 * Math.Pow(phase, 2) - 60 * Math.Pow(phase, 3) + 30 * Math.Pow(phase, 4)) * dPHIdt;

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




        //METODOS
        //public void healCell()
        //{
        //    this.infected = false;
        //}

        //public void sickCell()
        //{
        //    this.infected = true;
        //}

        //public void changeStatus()
        //{
        //    if (this.infected)
        //    {
        //        this.infected = false;
        //    }
        //    else
        //    {
        //        this.infected = true;
        //    }
        //}

        //public void setNextStatus(Rules r, int neighborsHealed)
        //{
        //    int neighborsIinfected = 8 - neighborsHealed;
        //    if (r.getRules() == 0) // CONWAY
        //    {

        //        if (infected) // C = 1
        //        {
        //            if (neighborsIinfected < 2 || neighborsIinfected > 3)
        //            {
        //                infected = false;
        //            }
        //        }
        //        else // C = 0
        //        {
        //            if (neighborsIinfected == 3)
        //            {
        //                infected = true;
        //            }
        //        }
        //    }

        //    else //COVID19
        //    {
        //        if (infected) // C = 1
        //        {
        //            if (neighborsIinfected < 2 || neighborsIinfected > 4)
        //            {
        //                infected = false;
        //            }
        //        }
        //        else // C = 0
        //        {
        //            if (neighborsIinfected == 3 || neighborsIinfected == 4)
        //            {
        //                infected = true;
        //            }
        //        }
        //    }

        //}

        //public void setNextStatus(Rules r, int neighborsHealed)
        //{
        //    int neighborsInfected = 8 - neighborsHealed;
        //    infected = r.getNextStatus(neighborsInfected, infected);
        //}
    }
}