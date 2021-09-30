using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameOfLife
{
    class Rules
    {
        // Hay que ver.
        double m, n, dt, d, e, b, dx, dy;
        
        public Rules(double mIn, double nIn, double dtIn, double dIn, double eIn, double bIn, double dxIn, double dyIn)
        {
            m = mIn;
            n = nIn;
            dt = dtIn;
            d = dIn;
            e = eIn;
            b = bIn;
            dx = dxIn;
            dy = dyIn;
        }

        public Rules()
        {
        }

        public bool getNextStatus(int neighbors, bool isInfected)
        {
            if (isInfected)
            {
                return infected[neighbors];
            }
            else
            {
                return healed[neighbors];
            }
        }

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
