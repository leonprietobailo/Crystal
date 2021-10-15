using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameOfLife
{
    class Rules
    {
        // Hay que ver.
        double[] rules = new double[7];

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
    }
}
