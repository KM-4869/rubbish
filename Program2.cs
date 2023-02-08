using System;
using System.IO;
using System.Text.RegularExpressions;
using MathWorks.MATLAB.NET.Arrays;
using testNative;
using var_unit_weighNative;
using calculate_QXXNative;

namespace GNSS接收机位置解算程序
{
    class Program
    {
        static void Main(string[] args)
        {
            int lines = 0;
            int t = 0;
            double t1=0;
            double[,] T=new double[4,4];
            string row_st_t="?";
            double accumulate_X = 0;
            double accumulate_Y = 0;
            double accumulate_Z = 0;

            double accumulate_x_station = 0;
            double accumulate_y_station = 0;
            double accumulate_z_station = 0;

            double accumulate_x_station2 = 0;
            double accumulate_y_station2 = 0;
            double accumulate_z_station2 = 0;

            StreamWriter writer = new StreamWriter(@"D:\最优估寄\GNSS接收机位置解算程序\解算结果2.txt");
            StreamWriter writer2 = new StreamWriter(@"D:\最优估寄\GNSS接收机位置解算程序\解算结果2的站心坐标系坐标.txt");
            for (int j = 1; j <= 2880; j++)
            {
                
                

                double[,] B = new double[27, 4];
                double[,] P = new double[27, 27];
                double[,] l = new double[27, 1];

                double X0 = 0;
                double Y0 = 0;
                double Z0 = 0;
                double T0 = 0;
               
                for (int k = 1; k <= 10; k++)
                {//用于迭代的循环，最多迭代次数为十次
                    

                    string filename = @"D:\最优估寄\第一次编程练习_最小二乘\CUSV20210222.txt";
                    StreamReader reader = new StreamReader(filename);
                    for(int i=1;i<=lines;i++)
                    {
                        reader.ReadLine();
                    }//由于readline语句只能一行一行往下读，故创建循环，先读掉目标行之前的lines行

                    string row_st = reader.ReadLine();//读取某历元的首行
                    string[] info = Regex.Split(row_st, "\\s+", RegexOptions.None);//以空格为分割符，获取该历元的卫星数

                    t = Convert.ToInt32(info[4]);


                   

                    for (int i = 1; i <= Convert.ToInt32(info[4]); i++)
                    {//从有数据的首行开始读入坐标及伪距观测值，读到该历元拥有的卫星数为止

                        string row = reader.ReadLine();
                        string[] XYZS = Regex.Split(row, "\\s+", RegexOptions.None);
                        double X = Convert.ToDouble(XYZS[1]);
                        double Y = Convert.ToDouble(XYZS[2]);
                        double Z = Convert.ToDouble(XYZS[3]);
                        double S = Convert.ToDouble(XYZS[4]);


                        double S0 = Math.Sqrt((X - X0) * (X - X0) + (Y - Y0) * (Y - Y0) + (Z - Z0) * (Z - Z0));
                        double a = -(X - X0) / S0;
                        double b = -(Y - Y0) / S0;
                        double c = -(Z - Z0) / S0;//将获取的坐标值带入线性化公式计算
                        B[i - 1, 0] = a;
                        B[i - 1, 1] = b;
                        B[i - 1, 2] = c;
                        B[i - 1, 3] = 1;//获得系数矩阵B
                        P[i - 1, i - 1] = 1;
                        l[i - 1, 0] = S - S0 - T0;//获得矩阵P，l；



                    }
                    MWArray matrix_B = new MWNumericArray(B);
                    MWArray matrix_P = new MWNumericArray(P);
                    MWArray matrix_l = new MWNumericArray(l);//将数组传递为矩阵

                    Class1 classs = new Class1();
                    object resultObj = classs.test(1, matrix_B, matrix_P, matrix_l);//调用自己编写的test函数进行最小二乘求解
                    object[] resultObjs = (object[])resultObj;
                    double[,] x = (double[,])resultObjs[0];

                    X0 = X0 + x[0, 0];
                    Y0 = Y0 + x[1, 0];
                    Z0 = Z0 + x[2, 0];
                    T0 = T0 + x[3, 0];//求解结果加入估计值作为下次迭代值

                    MWArray matrix_x = new MWNumericArray(x);
                    if (x[0, 0] < 0.0001 && x[1, 0] < 0.0001 && x[2, 0] < 0.0001 && x[3, 0] < 0.0001)
                    {
                        break;
                    }

                    Class2 class2 = new Class2();
                    object resultObj2 = class2.var_unit_weigh(1, matrix_B, matrix_P, matrix_l, matrix_x);
                    object[] resultObj2s = (object[])resultObj2;
                    double[,] sigma2 = (double[,])resultObj2s[0];
                    t1 = sigma2[0, 0];//求解单位权中误差，t为一个全局变量用来传递值，不然求解结果好像不能在循环外输出


                    Class3 class3 = new Class3();
                    object resultObj3 = class3.calculate_QXX(1, matrix_B, matrix_P);
                    object[] resultObj3s = (object[])resultObj3;
                    double[,] QXX = (double[,])resultObj3s[0];//求解方差矩阵
                    
                    for(int n=0; n<=3;n++)
                    {
                        for(int m=0;m<=3;m++)
                        {
                            T[n, m] = QXX[n, m];
                        }
                    }//同理T【】二维数组为全局变量用来传递求解值并在循环外输出

                    row_st_t = row_st;

                }


                lines = lines + t+1;

                accumulate_X = accumulate_X + X0;
                accumulate_Y = accumulate_Y + Y0;
                accumulate_Z = accumulate_Z + Z0;
                
                
                writer.WriteLine(row_st_t);
                writer.WriteLine("{0,-45}{1,-45}{2,-45}{3,-45}", "X(m)", "Y(m)", "Z(m)", "T(m)");
                writer.WriteLine("{0,-30}{1,-30}{2,-30}{3,-30}", X0, Y0, Z0, T0);
                writer.WriteLine("验后单位权中误差:{0}", Math.Sqrt(t1));
                writer.WriteLine("验后估计方差（m2）");
                for (int n = 0; n <= 3; n++)
                {
                   
                       
                       writer.WriteLine("{0,-30}{1,-30}{2,-30}{3,-30}", T[n, 0], T[n, 1], T[n, 2], T[n, 3]);

                }
                writer.WriteLine("\r\n");



                double deg_B = 13.7359102399326;
                double deg_L = 100.533923836063;
                double H = 75.8291242532432;//所给参考坐标转化成的经纬度值，以备后续转站心坐标使用

                double cosB = Math.Cos(deg_B / 180 * Math.PI);
                double sinB = Math.Sin(deg_B / 180 * Math.PI);
                double cosL = Math.Cos(deg_L / 180 * Math.PI);
                double sinL = Math.Sin(deg_L / 180 * Math.PI);

                double XP= -1132915.01648681;
                double YP= 6092528.50388968;
                double ZP= 1504633.16777129;//参考站心坐标值

                double x_station = -sinB * cosL * (X0 - XP) - sinB * sinL * (Y0 - YP) + cosB * (Z0 - ZP);
                double y_station= -sinL * (X0 - XP) + cosL * (Y0 - YP);
                double z_station= cosB * cosL * (X0 - XP) + cosB * sinL * (Y0 - YP) + sinB * (Z0 - ZP);
                //根据课本上大地坐标转站心坐标公式编写，求解站心坐标系下的各解算结果坐标
                

                accumulate_x_station = accumulate_x_station + x_station;
                accumulate_y_station = accumulate_y_station + y_station;
                accumulate_z_station = accumulate_z_station + z_station;//用于计算均值

                accumulate_x_station2 = accumulate_x_station2 + x_station*x_station;
                accumulate_y_station2 = accumulate_y_station2 + y_station*y_station;
                accumulate_z_station2 = accumulate_z_station2 + z_station*z_station;//用于计算RMS

                writer2.WriteLine("{0,-35}{1,-35}{2,-35}", x_station, y_station, z_station);
                


            }
            writer.WriteLine("{0,-45}{1,-45}{2,-45}", "meanX=" + accumulate_X / 2880, "meanY=" + accumulate_Y / 2880, "meanZ=" + accumulate_Z / 2880);
            writer2.WriteLine("{0,-45}{1,-45}{2,-45}", "meanE(k)=" + accumulate_x_station / 2880, "meanN(k)=" + accumulate_y_station / 2880, "meanU(k)=" + accumulate_z_station / 2880);
            writer2.WriteLine("{0,-45}{1,-45}{2,-45}", "rmsE=" +Math.Sqrt( accumulate_x_station2 / 2880), "rmsN=" +Math.Sqrt( accumulate_y_station2 / 2880), "rmsU=" +Math.Sqrt(accumulate_z_station2 / 2880));
            writer.Close();
            writer2.Close();
        }
    }
}
