using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuroNet
{
    class Neuron
    {
        public string name;
        public double[,] weight;
        public int countTraining;
		public double learning_factor = 0.5;

         public Neuron() {}
         public string GetName() { return name; }

         public void Clear(string name, int x, int y)
         {
             this.name = name;
             weight = new double[x,y];
             for (int n = 0; n < weight.GetLength(0); n++)
                 for (int m = 0; m < weight.GetLength(1); m++) weight[n, m] = 0;
             countTraining = 0;
         }

         public double GetRes(int[,] data){
             if (weight.GetLength(0) != data.GetLength(0) || weight.GetLength(1) != data.GetLength(1)) return -1;
             double res = 0;
             for (int n = 0; n < weight.GetLength(0); n++)
                 for (int m = 0; m < weight.GetLength(1); m++) 
                     res += 1 - Math.Abs(weight[n, m] - data[n, m]);
             return res / (weight.GetLength(0) * weight.GetLength(1));
         }

         public int Training(int[,] data)
         {
             if (data == null || weight.GetLength(0) != data.GetLength(0) || weight.GetLength(1) != data.GetLength(1)) return countTraining;
             countTraining++;
             for (int n = 0; n < weight.GetLength(0); n++)
                 for (int m = 0; m < weight.GetLength(1); m++)
                 {
                     double v = data[n, m] == 0 ? 0 : 1;
                     weight[n, m] += 2 * (v - learning_factor) / countTraining;
                     if (weight[n, m] > 1) weight[n, m] = 1;
                     if (weight[n, m] < 0) weight[n, m] = 0;
                 }
             return countTraining;
         }
    }
}
