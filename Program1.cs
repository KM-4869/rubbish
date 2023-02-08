using System;
using System.IO;
using MathWorks.MATLAB.NET.Arrays;
using Rotation_matrixNative;

namespace GPS和北斗卫星位置计算
{
    class Program
    {
        static void Main(string[] args)
        {

            string filename2 = @"D:\卫星导航原理\GPS与北斗卫星坐标.txt";
            StreamWriter writer = new StreamWriter(filename2);

            for (double t = 0; t <= 86400; t = t + 60)
            {
                int lines = 11;
                Console.WriteLine("    " +Math.Floor( t/3600)+"  h   "+(t%3600)/60+"  min");
                writer.WriteLine("    " + Math.Floor(t / 3600) + "  h   " + (t % 3600) / 60 + "  min");
                for (int GPS_number = 1; GPS_number <= 32; GPS_number++)
                {

                    string filename = @"D:\卫星导航原理\brdm3350.19p";
                    StreamReader reader = new StreamReader(filename);

                    double[,,] T_GPS_Info = new double[15, 6, 4];//创建三维数组用来储存一颗卫星的所有时刻接受的导航电文
                    string first_PRN = "0";

                    int i_length;
                    double min_t;
                    double t1;
                    double GM = 3.986005 * 100000000000000;
                    double we = 7.292115 * 0.00001;
                    string PRN;

                    for (int i = 1; i <= lines; i++)
                    {
                        reader.ReadLine();
                    }
                    //读完一颗卫星数据后从头开始，读掉lines行，读到下一颗卫星
                    for (int i = 0; ; i++)
                    {
                        string firstline = reader.ReadLine(); lines = lines + 1;
                        string[] part = firstline.Split(" ");
                        PRN = part[0];

                        if (i == 0)
                        {
                            first_PRN = part[0];
                        }
                        if (PRN != first_PRN)
                        {
                            i_length = i;
                            lines = lines - 1;
                            break;
                        }//判断是否是同一颗卫星

                        for (int k = 0; k < 6; k++)
                        {
                            string oneline = reader.ReadLine(); lines = lines + 1;

                            T_GPS_Info[i, k, 0] = Convert.ToDouble(oneline.Substring(4, 19));
                            T_GPS_Info[i, k, 1] = Convert.ToDouble(oneline.Substring(23, 19));
                            T_GPS_Info[i, k, 2] = Convert.ToDouble(oneline.Substring(42, 19));
                            T_GPS_Info[i, k, 3] = Convert.ToDouble(oneline.Substring(61, 19));
                        }//将导航电文存进数组中
                        reader.ReadLine(); lines = lines + 1;

                    }

                    min_t = Math.Abs(t - T_GPS_Info[0, 2, 0]);
                    for (int i = 1; i < i_length; i++)
                    {

                        t1 = Math.Abs(t - T_GPS_Info[i, 2, 0]);
                        if (min_t > t1)
                        {
                            min_t = t1;
                        }
                    }//匹配与t时刻最接近的数据信息

                    for (int i = 0; i < i_length; i++)
                    {
                        if (Math.Abs(t - T_GPS_Info[i, 2, 0]) == min_t)
                        {
                            double toe = T_GPS_Info[i, 2, 0];
                            double e = T_GPS_Info[i, 1, 1];
                            double n0 = Math.Sqrt(GM) / (T_GPS_Info[i, 1, 3] * T_GPS_Info[i, 1, 3] * T_GPS_Info[i, 1, 3]);
                            double n = n0 + T_GPS_Info[i, 0, 2];
                            double M = T_GPS_Info[i, 0, 3] + n * (t - toe);
                            double E = M;
                            for (int interation = 1; interation < 10; interation++)
                            {
                                E = M + e * Math.Sin(E);
                            }

                            double f = Math.Atan2(Math.Sqrt(1 - e * e) * Math.Sin(E), (Math.Cos(E) - e));
                            double u_ = T_GPS_Info[i, 3, 2] + f;

                            double cos2u_ = Math.Cos(2 * u_);
                            double sin2u_ = Math.Sin(2 * u_);

                            double delta_u = T_GPS_Info[i, 1, 0] * cos2u_ + T_GPS_Info[i, 1, 2] * sin2u_;
                            double delta_r = T_GPS_Info[i, 3, 1] * cos2u_ + T_GPS_Info[i, 0, 1] * sin2u_;
                            double delta_i = T_GPS_Info[i, 2, 1] * cos2u_ + T_GPS_Info[i, 2, 3] * sin2u_;

                            double u = u_ + delta_u;
                            double r = T_GPS_Info[i, 1, 3] * T_GPS_Info[i, 1, 3] * (1 - e * Math.Cos(E)) + delta_r;
                            double angle = T_GPS_Info[i, 3, 0] + delta_i + T_GPS_Info[i, 4, 0] * (t - toe);


                            double x = r * Math.Cos(u);
                            double y = r * Math.Sin(u);

                            double L = T_GPS_Info[i, 2, 2] + (T_GPS_Info[i, 3, 3] - we) * t - T_GPS_Info[i, 3, 3] * toe;

                            double cosi = Math.Cos(angle);
                            double sini = Math.Sin(angle);
                            double cosL = Math.Cos(L);
                            double sinL = Math.Sin(L);

                            double X = x * cosL - y * cosi * sinL;
                            double Y = x * sinL + y * cosi * cosL;
                            double Z = y * sini;

                            Console.WriteLine("{0,-5}{1,-30}{2,-30}{3,-30}", first_PRN, X, Y, Z);
                            writer.WriteLine("{0,-5}{1,-30}{2,-30}{3,-30}", first_PRN, X, Y, Z);
                            break;
                        }
                    }
                }
             }

            

            for (double t = 0; t <= 86400; t = t + 60)
            {
                int lines = 70463;
                Console.WriteLine("    " + Math.Floor(t / 3600) + "  h   " + (t % 3600) / 60 + "  min");
                writer.WriteLine("    " + Math.Floor(t / 3600) + "  h   " + (t % 3600) / 60 + "  min");
                for (int BDS_number = 1; BDS_number <= 42; BDS_number++)
                {
                    string filename = @"D:\卫星导航原理\brdm3350.19p";
                    StreamReader reader = new StreamReader(filename);

                    double[,,] T_BDS_Info = new double[35, 6, 4];
                    string first_PRN = "0";

                    int i_length;
                    double min_t;
                    double t1;
                    double GM = 3.986004418 * 100000000000000;
                    double we = 7.292115 * 0.00001;
                    string PRN;

                    for (int i = 1; i <= lines; i++)
                    {
                        reader.ReadLine();
                    }

                    for (int i = 0; ; i++)
                    {
                        string firstline = reader.ReadLine(); lines = lines + 1;
                        string[] part = firstline.Split(" ");
                        PRN = part[0];

                        if (i == 0)
                        {
                            first_PRN = part[0];
                        }
                        if (PRN != first_PRN)
                        {
                            i_length = i;
                            lines = lines - 1;
                            break;
                        }

                        for (int k = 0; k < 6; k++)
                        {
                            string oneline = reader.ReadLine(); lines = lines + 1;

                            T_BDS_Info[i, k, 0] = Convert.ToDouble(oneline.Substring(4, 19));
                            T_BDS_Info[i, k, 1] = Convert.ToDouble(oneline.Substring(23, 19));
                            T_BDS_Info[i, k, 2] = Convert.ToDouble(oneline.Substring(42, 19));
                            T_BDS_Info[i, k, 3] = Convert.ToDouble(oneline.Substring(61, 19));
                        }
                        reader.ReadLine(); lines = lines + 1;

                        
                  
                     }

                    min_t = Math.Abs(t - T_BDS_Info[0, 2, 0]);
                    for (int i = 1; i < i_length; i++)
                    {

                        t1 = Math.Abs(t - T_BDS_Info[i, 2, 0]);
                        if (min_t > t1)
                        {
                            min_t = t1;
                        }
                    }

                    for (int i = 0; i < i_length; i++)
                    {
                        if (Math.Abs(t - T_BDS_Info[i, 2, 0]) == min_t)
                        {
                            double toe = T_BDS_Info[i, 2, 0];
                            double e = T_BDS_Info[i, 1, 1];
                            double n0 = Math.Sqrt(GM) / (T_BDS_Info[i, 1, 3] * T_BDS_Info[i, 1, 3] * T_BDS_Info[i, 1, 3]);
                            double n = n0 + T_BDS_Info[i, 0, 2];
                            double M = T_BDS_Info[i, 0, 3] + n * (t - toe-14);
                            double E = M;
                            for (int interation = 1; interation < 10; interation++)
                            {
                                E = M + e * Math.Sin(E);
                            }

                            double f = Math.Atan2(Math.Sqrt(1 - e * e) * Math.Sin(E), (Math.Cos(E) - e));
                            double u_ = T_BDS_Info[i, 3, 2] + f;

                            double cos2u_ = Math.Cos(2 * u_);
                            double sin2u_ = Math.Sin(2 * u_);

                            double delta_u = T_BDS_Info[i, 1, 0] * cos2u_ + T_BDS_Info[i, 1, 2] * sin2u_;
                            double delta_r = T_BDS_Info[i, 3, 1] * cos2u_ + T_BDS_Info[i, 0, 1] * sin2u_;
                            double delta_i = T_BDS_Info[i, 2, 1] * cos2u_ + T_BDS_Info[i, 2, 3] * sin2u_;

                            double u = u_ + delta_u;
                            double r = T_BDS_Info[i, 1, 3] * T_BDS_Info[i, 1, 3] * (1 - e * Math.Cos(E)) + delta_r;
                            double angle = T_BDS_Info[i, 3, 0] + delta_i + T_BDS_Info[i, 4, 0] * (t - toe-14);


                            double x = r * Math.Cos(u);
                            double y = r * Math.Sin(u);
                            
                            double L ;
                            double cosi ;
                            double sini ;
                            double cosL ;
                            double sinL ;
                            double X;
                            double Y;
                            double Z;
                            if (BDS_number<=5)//前五个GEO卫星另外计算
                            {
                                L = T_BDS_Info[i, 2, 2] + T_BDS_Info[i, 3, 3]  * (t-toe-14) - we * toe;
                                 cosi = Math.Cos(angle);
                                 sini = Math.Sin(angle);
                                 cosL = Math.Cos(L);
                                 sinL = Math.Sin(L);
                                 X = x * cosL - y * cosi * sinL;
                                 Y = x * sinL + y * cosi * cosL;
                                 Z = y * sini;

                                double[,] XGK = new double[3, 1];
                                
                                XGK[0, 0] = X;
                                XGK[1, 0] = Y;
                                XGK[2, 0] = Z;
                                
                                MWArray matrix_XGK = new MWNumericArray(XGK);
                                MWArray phi_x = new MWNumericArray(-5 * Math.PI / 180);
                                MWArray phi_y = new MWNumericArray(0);
                                MWArray phi_z = new MWNumericArray(we * (t - toe-14));
                                Class1 class1 = new Class1();
                                object obj = class1.Rotation_matrix(6, phi_x, phi_y, phi_z, matrix_XGK);//调用自定义的MATLAB函数进行旋转矩阵，完成坐标转换
                                object[] objs = (object[])obj;
                                double[,] XK = (double[,])objs[4];
                                Console.WriteLine("{0,-5}{1,-30}{2,-30}{3,-30}", first_PRN, XK[0, 0], XK[1, 0], XK[2, 0]);

                               
                                writer.WriteLine("{0,-5}{1,-30}{2,-30}{3,-30}", first_PRN, XK[0, 0], XK[1, 0], XK[2, 0]);
                            }
                            else
                            {
                                L = T_BDS_Info[i, 2, 2] + (T_BDS_Info[i, 3, 3]-we) * (t - toe - 14) - we * toe;
                                cosi = Math.Cos(angle);
                                 sini = Math.Sin(angle);
                                 cosL = Math.Cos(L);
                                 sinL = Math.Sin(L);
                                 X = x * cosL - y * cosi * sinL;
                                 Y = x * sinL + y * cosi * cosL;
                                 Z = y * sini;
                                Console.WriteLine("{0,-5}{1,-30}{2,-30}{3,-30}", first_PRN, X, Y, Z);
                                writer.WriteLine("{0,-5}{1,-30}{2,-30}{3,-30}", first_PRN, X, Y, Z);
                            }
                           

                            
                            break;
                        }
                    }

                }


            }
            writer.Close();
        }


    }
}
