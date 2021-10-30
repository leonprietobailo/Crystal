using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Crystal
{
    class Rules
    {
        //Atributo de las reglas que contendrá los parámetros de simulación en un vector
        double[] rules = new double[7];

        //Constructor de las reglas
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

        //Constructor de las reglas
        public Rules()
        {
        }

        //Método que retorna en forma de vector las reglas
        public double[] getRules()
        {
            return rules;
        }
    }
}
