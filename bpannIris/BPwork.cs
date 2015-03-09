﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace bpannIris
{
    class BPwork
    {
        /*** 返回0－1的双精度随机数 ***/
        double drnd()
        {
            Random radom = new Random();
            return (radom.Next(0,1));
        }
        /*** 返回-1.0到1.0之间的双精度随机数 ***/
        double dpn1()
        {
            return ((drnd() * 2.0) - 1.0);
        }
        double squash(double x)
        {
            //返回S激活函数
            return (1.0 / (1.0 + Math.Exp(-x)));
        }
        /*** 随机初始化权值 ***/
        void bpnn_randomize_weights(double[,] w, int m, int n)
        {
            int i, j;
            //调用dpn1随机初始化权值
            for (i = 0; i <= m; i++)
            {
                for (j = 0; j <= n; j++)
                {
                    w[i,j] = dpn1();
                }
            }
        }
        /******* 零初始化权值 *******/
        void bpnn_zero_weights(double[,] w, int m, int n)
        {
            int i, j;

            //将权值逐个赋0
            for (i = 0; i <= m; i++)
            {
                for (j = 0; j <= n; j++)
                {
                    w[i,j] = 0.0;
                }
            }
        }
        /*********前向传输*********/
        void bpnn_layerforward(double[] l1, double[] l2, double[,] conn, int n1, int n2)//(input_unites, hidden_unites,input_weights, n_in, n_hidden);
        {
            double sum;
            int j, k;

            /*** 设置阈值 ***/
            l1[0] = 1.0;

            /*** 对于第二层的每个神经元 ***/
            for (j = 1; j <= n2; j++)
            {
                /*** 计算输入的加权总和 ***/
                sum = 0.0;
                for (k = 0; k <= n1; k++)
                {
                    sum += conn[k,j] * l1[k];
                }

                l2[j] = squash(sum);
            }
        }
        /* 输出误差 */
        void bpnn_output_error(double[] delta, double[] target, double[] output, int nj)//(output_deltas, target, output_unites, n_out);
        {
            int j;
            double o, t, errsum;

            //先将误差归零
            errsum = 0.0;

            //循环计算delta
            for (j = 1; j <= nj; j++)
            {
                o = output[j];
                t = target[j];
                //计算delta值
                delta[j] = o * (1.0 - o) * (t - o);
            }
        }
        /* 隐含层误差 */
        void bpnn_hidden_error(double[] delta_h, int nh, double[] delta_o, int no, double[,] who, double[] hidden)
        {
            int j, k;
            double h, sum, errsum;

            //误差归零
            errsum = 0.0;

            //计算新delta
            for (j = 1; j <= nh; j++)
            {
                h = hidden[j];
                sum = 0.0;
                for (k = 1; k <= no; k++)
                {
                    sum += delta_o[k] * who[j,k];
                }
                delta_h[j] = h * (1.0 - h) * sum;
            }
        }
        /* 调整权值 */
        void bpnn_adjust_weights(double[] delta, int ndelta, double[] ly, int nly, double[,] w, double[,] oldw, double eta, double momentum)
        {
            double new_dw;
            int k, j;
            ly[0] = 1.0;
            //请参考文章中BP网络权值调整的计算公式
            for (j = 1; j <= ndelta; j++)
            {
                for (k = 0; k <= nly; k++)
                {
                    new_dw = ((eta * delta[j] * ly[k]) + (momentum * oldw[k,j]));
                    w[k,j] += new_dw;
                    oldw[k,j] = new_dw;
                }
            }
        }
        /*******保存权值**********/
        void w_weight(double[,] w, int n1, int n2, string name)
        {
            int i, j;
            double[] buffer = new double[(n1+1)*(n2+1)];

            //创建文件指针
            FileStream fs = new FileStream(name, FileMode.Create, FileAccess.Write);
            fs.Close();
            StreamWriter sw = File.AppendText(name);
            //填写缓冲区内容
            for (i = 0; i <= n1; i++)
            {
                for (j = 0; j <= n2; j++)
                {
                    // buffer[i * (n2 + 1) + j] = w[i,j];
                    sw.Write(w[i,j]);
                    sw.Write(' ');
                }
                sw.WriteLine(" ");  
            }
            sw.Close();
        }
        /************读取权值*************/
        bool r_weight(double[,] w, int n1, int n2, string name)
        {
	        int i, j;
            string[] tokens = new String[(n1 + 1) * (n2 + 1)];
	        //文件指针
	        StreamReader sr = new StreamReader(name);                   

	        //由缓冲区内容填写权值	        
            String Line = sr.ReadLine();
            i = 0;
            while (Line != null && !Line.Equals(" "))
            {
                tokens = Line.Split(' ');
                for (j = 0; j <= n2; ++j)
                {
                    w[i,j] = Convert.ToDouble(tokens[j]);
                }
                Line = sr.ReadLine();
                ++i;
            }
            sr.Close();
	        //返回true表示已经正确读取
	        return(true);
        }
        /****************************************************
        * 函数名称 BpTrain()
        *
        * 参数：
        *   double **data_in    -指向输入的特征向量数组的指针
        *	double **data_out   -指向理想输出数组的指针
        *   int n_in            -输入层结点的个数
        *   int n_hidden        -BP网络隐层结点的数目
        *   double min_ex       -训练时允许的最大均方误差
        *   double momentum     -BP网络的相关系数
        *   double eta          -BP网络的训练步长
        *   int num             -输入样本的个数
        *
        * 函数功能：
        *     根据输入的特征向量和期望的理想输出对BP网络尽行训练
        *     训练结束后将权值保存并将训练的结果显示出来
        ********************************************************/
        public int num_in;
        public int num_hidden;
        public int num_out;
        public void BpTrain(double[][] data_in, double[][] data_out, int n_in, int n_hidden,int outlayer, double min_ex, double momentum, double eta, int num, int max_train_time = 1500)
        {
	        //循环变量   
	        int i, k, l;
	        //输出层结点数目
            int n_out = outlayer;
	        //指向输入层数据的指针
	        double[] input_unites = new double[n_in + 1];
	        //指向隐层数据的指针
	        double[] hidden_unites = new double[n_hidden + 1];
	        //指向输出层数据的指针
	        double[] output_unites = new double[n_out + 1];

	        //指向隐层误差数据的指针
	        double[] hidden_deltas = new double[n_hidden + 1];
	        //指向输出层误差数剧的指针
	        double[] output_deltas = new double[n_out + 1];
	        //指向理想目标输出的指针
	        double[] target = new double[n_out + 1];
	        //指向输入层于隐层之间权值的指针
	        double[,] input_weights = new double[n_in + 1,n_hidden + 1];
	        //指向隐层与输出层之间的权值的指针
	        double[,] hidden_weights = new double[n_hidden + 1,n_out + 1];
	        //指向上一此输入层于隐层之间权值的指针
	        double[,] input_prev_weights = new double[n_in + 1,n_hidden + 1];
	        //指向上一此隐层与输出层之间的权值的指针
	        double[,] hidden_prev_weights = new double[n_hidden + 1,n_out + 1];

	        //每次循环后的均方误差误差值 
	        double ex = 0;
            	        
	        //对各种权值进行初始化初始化
	        bpnn_randomize_weights(input_weights, n_in, n_hidden);
	        bpnn_randomize_weights(hidden_weights, n_hidden, n_out);
	        bpnn_zero_weights(input_prev_weights, n_in, n_hidden);
	        bpnn_zero_weights(hidden_prev_weights, n_hidden, n_out);

	        //开始进行BP网络训练	       
	        for (l = 0; l<max_train_time ; l++)
	        {
		        //对均方误差置零
		        ex = 0;
		        //对样本进行逐个的扫描
		        for (k = 0; k<num; k++)
		        {
			        //将提取的样本的特征向量输送到输入层上
			        for (i = 1; i <= n_in; i++)
				        input_unites[i] = data_in[k][i - 1];

			        //将预定的理想输出输送到BP网络的理想输出单元
			        for (i = 1; i <= n_out; i++)
				        target[i] = data_out[k][i - 1];

			        //前向传输激活
			        //将数据由输入层传到隐层 
			        bpnn_layerforward(input_unites, hidden_unites,
				        input_weights, n_in, n_hidden);

			        //将隐层的输出传到输出层
			        bpnn_layerforward(hidden_unites, output_unites,
				        hidden_weights, n_hidden, n_out);

			        //误差计算
			        //将输出层的输出与理想输出比较计算输出层每个结点上的误差
			        bpnn_output_error(output_deltas, target, output_unites, n_out);

			        //根据输出层结点上的误差计算隐层每个节点上的误差
			        bpnn_hidden_error(hidden_deltas, n_hidden, output_deltas, n_out, hidden_weights, hidden_unites);

			        //权值调整
			        //根据输出层每个节点上的误差来调整隐层与输出层之间的权值    
			        bpnn_adjust_weights(output_deltas, n_out, hidden_unites, n_hidden,
				        hidden_weights, hidden_prev_weights, eta, momentum);

			        //根据隐层每个节点上的误差来调整隐层与输入层之间的权值    	
			        bpnn_adjust_weights(hidden_deltas, n_hidden, input_unites, n_in,
				        input_weights, input_prev_weights, eta, momentum);

			        //误差统计		
			        for (i = 1; i <= n_out; i++)
				        ex += (output_unites[i] - data_out[k][i - 1])*(output_unites[i] - data_out[k][i - 1]);
		        }

		        //计算均方误差
		        ex = ex / ((double)(num*n_out));

		        //如果均方误差已经足够的小，跳出循环，训练完毕  
		        if (ex<min_ex)break;
	        }

	        //相关保存
	        //保存输入层与隐层之间的权值
	        w_weight(input_weights, n_in, n_hidden, "win.dat");

	        //保存隐层与输出层之间的权值
	        w_weight(hidden_weights, n_hidden, n_out, "whi.dat");

	        //保存各层结点的个数
            num_hidden = n_hidden;
            num_in = n_in;
            num_out = n_out;
	        //w_num(n_in, n_hidden, n_out, "num");

	        //显示训练结果
            Console.WriteLine("迭代{0}次,平均误差{1}", l.ToString(), ex.ToString());
        }
        /*******************************************
        * 函数名称
        * CodeRecognize()
        * 参量
        *  double **data_in     -指向待识别样本特征向量的指针
        *  int num              -待识别的样本的个数
        *  int n_in             -Bp网络输入层结点的个数
        *  int n_hidden         -Bp网络隐层结点的个数
        *  int n_out            -Bp网络输出层结点的个数
        * 函数功能：
        *    读入输入样本的特征相量并根据训练所得的权值
        *    进行识别，将识别的结果写入result.txt
        ****************************************/
        public int[] recognize;
       public void CodeRecognize(double[][] data_in, int num, int n_in, int n_hidden, int n_out)
        {
	        //循环变量
	        int i, k;
	        // 指向识别结果的指针 
	        recognize = new int[num];	        
	        //指向输入层数据的指针
	        double[] input_unites = new double[n_in + 1];
	        //指向隐层数据的指针
	        double[] hidden_unites = new double[n_hidden + 1];
	        //指向输出层数据的指针
	        double[] output_unites = new double[n_out + 1];
	        //指向输入层于隐层之间权值的指针
	        double[,] input_weights = new double[n_in + 1,n_hidden + 1];
	        //指向隐层与输出层之间的权值的指针
	        double[,] hidden_weights = new double[n_hidden + 1,n_out + 1];

	        //读取权值
	        if (r_weight(input_weights, n_in, n_hidden, "win.dat") == false)
		        return;
	        if (r_weight(hidden_weights, n_hidden, n_out, "whi.dat") == false)
		        return;


	        //逐个样本扫描
	        for (k = 0; k<num; k++)
	        {
		        //将提取的样本的特征向量输送到输入层上
		        for (i = 1; i <= n_in; i++)
			        input_unites[i] = data_in[k][i - 1];

		        //前向输入激活
		        bpnn_layerforward(input_unites, hidden_unites,
			        input_weights, n_in, n_hidden);

		        bpnn_layerforward(hidden_unites, output_unites,
			        hidden_weights, n_hidden, n_out);

		        //根据输出结果进行识别
		        int result = 0;
                double value = 0.0;
		        //考察每一位的输出
		        for (i = 1; i <= n_out; i++)
		        {
			        //如果大于0.5判为1
			        if (output_unites[i] > value)
                    {
                        result = i;
                        value = output_unites[i];
                    }				        
		        }

		        //如果判定的结果小于等于9，认为合理
		        if (result <= 9)
			        recognize[k] = result;		        
	        }

	        //将识别结果写到文本中
            FileStream fs = new FileStream("result.txt", FileMode.OpenOrCreate, FileAccess.Write);
            fs.Close();
            StreamWriter sw = File.AppendText("result.txt");
	        for(i=0;i<num;i++)
            {
                sw.WriteLine(recognize[i].ToString());
               // Console.Write(recognize[i].ToString() + " ");
            }
            sw.Close();
        }

    }
}
