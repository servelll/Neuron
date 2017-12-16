using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace NeuroNet
{
    class NeuroWeb
    {
        private const int defaultNeironCount = 32;
        public const int neironInArrayWidth = 28;
        public const int neironInArrayHeight = 28;
        private const string memory = @"memory.txt";
        private List<Neuron> neironArray = null;

        public NeuroWeb()
        {
            neironArray = InitWeb();            
        }

        private static List<Neuron> InitWeb()
        {
            if (!File.Exists(memory)) return new List<Neuron>();
            string[] lines = File.ReadAllLines(memory);
            if (lines.Length == 0)
            {
                return new List<Neuron>();
            }
            string jStr = lines[0];
            JavaScriptSerializer json = new JavaScriptSerializer();
            List<Object> objects = json.Deserialize<List<Object>>(jStr);
            List<Neuron> res = new List<Neuron>();
            foreach (var o in objects)
            {
                res.Add(NeironCreate((Dictionary<string,Object>)o));
            }

            return res;
        }

        public void ClearWeb()
        {
            neironArray.Clear();
        }

        public string CheckLitera(int[,] arr)
        {
            string res = null;
            double max = 0;
            foreach (var n in neironArray)
            {
                double d = n.GetRes(arr);
                if (d > max)
                {
                    max = d;
                    res = n.GetName();
                }
            }
            return res;
        }

        public void SaveState()
        {
            JavaScriptSerializer json = new JavaScriptSerializer();
            string jStr = json.Serialize(neironArray);
            System.IO.StreamWriter file = new System.IO.StreamWriter(memory);
            file.WriteLine(jStr);
            file.Close();
        }      

        public string[] GetLiteras()
        {
            var res = new List<string>();
            for (int i = 0; i < neironArray.Count; i++)
            {
                res.Add(neironArray[i].GetName());
                // + ": " + neironArray[i].countTraining
            }

            res.Sort();
            return res.ToArray();
        }

        public void SetTraining(string trainingName, int[,] data, double new_learning_factor)
        {
            Neuron neiron = neironArray.Find(v => v.name.Equals(trainingName));
            if (neiron == null)
            {
                neiron = new Neuron();
                neiron.Clear(trainingName, neironInArrayWidth, neironInArrayHeight);
                neironArray.Add(neiron);
            }
            if (new_learning_factor != -1)
            {
                neiron.learning_factor = new_learning_factor;
            }

            int countTraining = neiron.Training(data);
            //MessageBox.Show("litera - " + neiron.GetName() + " count training = " + countTrainig.ToString());                
        }

        private static Neuron NeironCreate(Dictionary<string, object> o)
        {
            Neuron res = new Neuron()
            {
                name = (string)o["name"],
                countTraining = (int)o["countTraining"]
            };
            Object[] weightData = (Object[])o["weight"];
            int arrSize = (int)Math.Sqrt(weightData.Length);
            res.weight = new double[arrSize, arrSize];
            int index = 0;
            for (int n = 0; n < res.weight.GetLength(0); n++)
            {
                for (int m = 0; m < res.weight.GetLength(1); m++)
                {
                    res.weight[n, m] = Double.Parse(weightData[index].ToString());
                    index++;
                }
            }

            return res;
        }
    }
}
