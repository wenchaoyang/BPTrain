using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

//using AForge;
//using AForge.Neuro;
//using AForge.Neuro.Learning;
//using AForge.Controls;

namespace bpannIris
{
    class Program
    {
        private static double premnmx( double num , double min , double max  )//归一化到-1—1区间
        {
            if (num > max)
                num = max;
            if (num < min)
                num = min;
            if (max - min == 0.0) return -1.0;
            return 2*(num - min) / (max - min) - 1;
        }

        private static double premnmx1(double num, double min, double max)//归一化到0—1区间
        {
            if (num > max)
                num = max;
            if (num < min)
                num = min;

            return (num - min) / (max - min);
        }
        public static double average = 0.0;
        public static int trainNum = 45;
        public static int testNum = 5;
        public static int totalNum = 50;
        static int Shuxing = 31;
        static int type = 5;
        static double [][][] tenTimeIn = new double[10][][];
        static double [][][] tenTimeOut = new double[10][][];
        static int[] timeRecord = new int[10];
        static bool tenOK = false;
        static int whatTimes = 0;
       
        public static void TenTimesChange(double[][] total_I, double[][] total_O, double[][] train_I, double[][] train_O, double[][] test_I, double[][] test_O)
        {
            int i, j, k, t;
            
            if(tenOK == false)
            {
                for (k = 0; k < 10; k++) timeRecord[k] = 0;
                Random radom = new Random();
                tenOK = true;
                whatTimes = 0;
                for (i = 0; i < totalNum; i++)//随机产生是个分组
                {                    
                    t = radom.Next(0, 10);
                    //Console.Write(t+ " ");
                    if (timeRecord[t] < testNum)
                    {
                        tenTimeIn[t][timeRecord[t]] = total_I[i];
                        tenTimeOut[t][timeRecord[t]] = total_O[i];
                        timeRecord[t]++;
                    }
                    else
                    {
                        i--;
                    }
                }
                tenTimeIn[whatTimes].CopyTo(test_I,0);
                tenTimeOut[whatTimes].CopyTo(test_O,0);
                for(i = 0;i<10 && i < whatTimes;i++)///////
                {
                    tenTimeIn[i].CopyTo(train_I, testNum * i);
                }
                for(i=whatTimes + 1 ; i<10; i++ )
                {
                    tenTimeIn[i].CopyTo(train_I, testNum * (i - 1));
                }
                for (i = 0; i < 10 && i < whatTimes; i++)//////
                {
                    tenTimeOut[i].CopyTo(train_O, testNum * i);
                }
                for (i = whatTimes + 1; i < 10; i++)
                {
                    tenTimeOut[i].CopyTo(train_O, testNum * (i - 1));
                }
            }
            else
            {
                whatTimes++;
                tenTimeIn[whatTimes].CopyTo(test_I, 0);
                tenTimeOut[whatTimes].CopyTo(test_O, 0);
                for (i = 0; i < 10 && i < whatTimes; i++)///////
                {
                    tenTimeIn[i].CopyTo(train_I, testNum * i);
                }
                for (i = whatTimes + 1; i < 10; i++)
                {
                    tenTimeIn[i].CopyTo(train_I, testNum * (i - 1));
                }
                for (i = 0; i < 10 && i < whatTimes; i++)//////
                {
                    tenTimeOut[i].CopyTo(train_O, testNum * i);
                }
                for (i = whatTimes + 1; i < 10; i++)
                {
                    tenTimeOut[i].CopyTo(train_O, testNum * (i - 1));
                }
                if (whatTimes > 8) tenOK = false;
            }
            
        }
        /// <summary>
        /// 主函数
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            
            BPwork bp = new BPwork();
            double[][] trainInput = new double[trainNum][] ;
            double[][] trainOutput = new double[trainNum][];

            double[][] totalIn = new double[totalNum][];
            double[][] totalOut = new double[totalNum][];

            double[][] testInput = new double[testNum][];
            double[][] testOutput = new double[testNum][];

            double [] max = new double [Shuxing] ; 
            double [] min = new double [Shuxing] ;
            for (int i = 0; i < Shuxing; ++i)
            {
                max[i] = double.MinValue;
                min[i] = double.MaxValue; 
            }
            for (int i = 0; i < 10; i++)
            {
                tenTimeIn[i] = new double[testNum][];
                tenTimeOut[i] = new double[testNum][];
                timeRecord[i] = 0;
            }
            // 读取训练数据
            StreamReader reader = new StreamReader("Guiyi4TrainData.txt");

            for (int i = 0; i < totalNum; ++i)
            {
                string value = reader.ReadLine();

                string[] temp = value.Split('\t');

                totalIn[i] = new double[Shuxing];
                totalOut[i] = new double[type];

                for (int j = 0; j < Shuxing; j++)
                {
                    totalIn[i][j] = double.Parse(temp[j]);
                    //if (j == Shuxing - 1) totalIn[i][j] = double.Parse(temp[j * 5]);
                    //else
                    //    totalIn[i][j] = double.Parse(temp[j * 5]) + double.Parse(temp[j * 5 + 1]) + double.Parse(temp[j * 5 + 3]) + double.Parse(temp[j * 5 + 2]) + double.Parse(temp[j * 5 + 4]);
                    if (totalIn[i][j] > max[j])
                        max[j] = totalIn[i][j];

                    if (totalIn[i][j] < min[j])
                        min[j] = totalIn[i][j];
                }
                for (int j = 0; j < type; ++j)
                    totalOut[i][j] = 0;
                totalOut[i][int.Parse(temp[Shuxing]) - 1] = 1;              
            }

            // 归一化
            for (int i = 0; i < totalNum; ++i)
            {
                for (int j = 0; j < Shuxing; ++j)
                {
                    totalIn[i][j] = premnmx(totalIn[i][j], min[j], max[j]);
                }
            }
            for (int tim = 0; tim < 100; tim++ )
            {
                //System.Console.Write(" 读入完毕，开始随机分组");
                TenTimesChange(totalIn, totalOut, trainInput, trainOutput, testInput, testOutput);
                //System.Console.Write(" 随机分组完毕，开始训练");

                int num_hiddn = 7;
                bp.BpTrain(trainInput, trainOutput, Shuxing, num_hiddn, type, 0.01, 0.0, 0.8, trainNum, 6000);
                //System.Console.Write(" 训练完毕，开始识别");

                // 对测试数据进行分类， 并统计正确率
                int hitNum = 0;
                int trueNum = 1;
                bp.CodeRecognize(testInput, testNum, Shuxing, num_hiddn, type);
                for (int i = 0; i < testNum; ++i)
                {
                    for (int j = 0; j < type; j++)
                    {
                        if (testOutput[i][j] == 1.0) trueNum = j + 1;
                    }
                    if (bp.recognize[i] == trueNum)
                        ++hitNum;
                }
                double truePer = Math.Round(100.0 * hitNum / testNum,2);
                System.Console.Write(" {0} ", truePer);
                average += truePer;
                if ((tim + 1) % 10 == 0) 
                {
                    Console.Write("        {0}", average / 10);
                    Console.WriteLine(" ");
                    average = 0;
                    ////保存最值表
                    //FileStream fs = new FileStream("MaxMin.txt", FileMode.OpenOrCreate, FileAccess.Write);
                    //fs.Close();
                    //StreamWriter sw = File.AppendText("MaxMin.txt");
                    //sw.Write("double[] max = {");
                    //for (int i = 0; i < Shuxing; i++)
                    //{
                    //    sw.Write(max[i].ToString() + ", ");                       
                    //}
                    //sw.Write("}\n");
                    //sw.WriteLine("");
                    //sw.Write("double[] min = {");
                    //for (int i = 0; i < Shuxing; i++)
                    //{
                    //    sw.Write(min[i].ToString() + ", ");
                    //}
                    //sw.Write("}\n");
                    //sw.WriteLine("");
                    //sw.Close();
                }
            }
            System.Console.Read();
         
        }
    }
}
