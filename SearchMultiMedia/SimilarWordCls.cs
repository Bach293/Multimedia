using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchMultiMedia
{
    internal class SimilarWordCls
    {
        public static double[] getFeaVector2(string str, ref ArrayList fealst)
        {
            str = str.ToLower();
            string text = ConfigurationManager.AppSettings["VnTokenizerText"];
            saveToFileUTF8(text, str);
            string[] strlist = getTokTxt(text);

            getFeature(strlist, ref fealst);
            double[] vector = new double[fealst.Count];

            calVector(strlist, fealst, ref vector);

            return vector;
        }
        public static void saveToFileUTF8(string filename, string content)
        {
            StreamWriter streamWriter = new StreamWriter(filename, append: false, Encoding.UTF8);
            streamWriter.Write(content);
            streamWriter.Close();
        }

        public static string[] getTokTxt(string fileIn)
        {
            string text = ConfigurationManager.AppSettings["VnTokenizerTok"];
            string para = "-i " + fileIn + " -o " + text;
            RunExe(ConfigurationManager.AppSettings["VnTokenizerBat"], para);
            string str = readFileUTF8(text);
            return getFeaArr(str);
        }

        public static void RunExe(string exeFile, string para)
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo();
            processStartInfo.CreateNoWindow = false;
            processStartInfo.UseShellExecute = false;
            processStartInfo.FileName = exeFile;
            processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            processStartInfo.Arguments = para;
            Process process = Process.Start(processStartInfo);
            process.WaitForExit();
        }

        public static string readFileUTF8(string filename)
        {
            return File.ReadAllText(filename, Encoding.UTF8);
        }

        public static string[] getFeaArr(string str)
        {
            return str.Split(' ', '-', '"', '\'', ',', '.', '?', '!', ':', ';', '(', ')');
        }
        public static void getFeature(string[] strlist, ref ArrayList fealst)
        {
            foreach (string text in strlist)
            {
                string text2 = text.Trim();
                if (text2 != "" && checkFea(text2, fealst) < 0)
                {
                    fealst.Add(text2);
                }
            }
        }
        public static void calVector(string[] strlist, ArrayList fealst, ref double[] vector)
        {
            for (int i = 0; i < fealst.Count; i++)
            {
                vector[i] = 0.0;
                string text = (string)fealst[i];
                foreach (string text2 in strlist)
                {
                    if (text == text2.Trim())
                    {
                        vector[i] += 1.0;
                    }
                }
            }
        }

        public static int checkFea(string str, ArrayList fealst)
        {
            if (fealst.Count > 0)
            {
                for (int i = 0; i < fealst.Count; i++)
                {
                    string text = (string)fealst[i];
                    if (text == str)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }
        public static ArrayList unifyFealst(ArrayList fealst1, ArrayList fealst2)
        {
            ArrayList arrayList = new ArrayList();
            foreach (object item in fealst1)
            {
                arrayList.Add(item);
            }
            foreach (object item2 in fealst2)
            {
                string text = (string)item2;
                if (text != "" && checkFea(text, arrayList) < 0)
                {
                    arrayList.Add(text);
                }
            }
            return arrayList;
        }
        public static double calSimilarCosineAllFea(ArrayList allFealst, ArrayList fealst1, ArrayList fealst2, double[] vector1, double[] vector2)
        {
            double[] array = new double[allFealst.Count];
            double[] array2 = new double[allFealst.Count];
            for (int i = 0; i < allFealst.Count; i++)
            {
                array[i] = FindFeaVal(allFealst[i], fealst1, vector1);
                array2[i] = FindFeaVal(allFealst[i], fealst2, vector2);
            }

            return calCosine(array, array2, allFealst.Count);
        }

        private static double FindFeaVal(object obj, ArrayList fealst, double[] vector)
        {
            int num = checkFea((string)obj, fealst);
            if (num >= 0)
            {
                return vector[num];
            }
            return 0.0;
        }

        public static double calCosine(double[] vector1, double[] vector2, int count)
        {
            double num = 0.0;
            double num2 = 0.0;
            double num3 = 0.0;
            for (int i = 0; i < count; i++)
            {
                num += vector1[i] * vector2[i];
                num2 += Math.Pow(vector1[i], 2.0);
                num3 += Math.Pow(vector2[i], 2.0);
            }
            return num / Math.Sqrt(num2 * num3);
        }
    }
}
