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

        public Rules(double mIn, double nIn, double dtIn, double dIn, double eIn, double bIn, double dxIn, double dyIn)
        {
            rules[0] = mIn;
            rules[1] = nIn;
            rules[2] = dtIn;
            rules[3] = dIn;
            rules[4] = eIn;
            rules[5] = bIn;
            rules[6] = dxIn;
            rules[7] = dyIn;
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

        public void setRules()
        {

        }

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
