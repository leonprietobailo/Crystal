using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameOfLife
{
    class Rules
    {
        // Hay que ver.
        double[] rules = new double[8];

        public Rules(double mIn, double dtIn, double dIn, double eIn, double bIn, double dxIn, double dyIn)
        {
            rules[0] = mIn;
            rules[1] = dtIn;
            rules[2] = dIn;
            rules[3] = eIn;
            rules[4] = bIn;
            rules[5] = dxIn;
            rules[6] = dyIn;
        }

        public Rules()
        {
        }

        public double[] getRules()
        {
            return rules;
        }



        //public bool getNextStatus(int neighbors, bool isInfected)
        //{
        //    if (isInfected)
        //    {
        //        return infected[neighbors];
        //    }
        //    else
        //    {
        //        return healed[neighbors];
        //    }
        //}


        //public void setCOVID19()
        //{
        //    infected[0] = false;
        //    infected[1] = false;
        //    infected[2] = true;
        //    infected[3] = true;
        //    infected[4] = true;
        //    infected[5] = false;
        //    infected[6] = false;
        //    infected[7] = false;
        //    infected[8] = false;

        //    healed[0] = false;
        //    healed[1] = false;
        //    healed[2] = false;
        //    healed[3] = true;
        //    healed[4] = true;
        //    healed[5] = false;
        //    healed[6] = false;
        //    healed[7] = false;
        //    healed[8] = false;
        //}
        //public void setNewVirus(bool[] i, bool[] h)
        //{
        //    infected = i;
        //    healed = h;
        //}


    }
}
