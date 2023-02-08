using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GNSS数据质量分析自编程序
{
    class Class1
    {
        public static string filepath;
        public static int end_of_header;
        public static double interval;
        public static double endline;
        public static string[] PRNbox=new string[60];
        public static string[] GPSbox = new string[35];
        public static string[] BDSbox = new string[35];
        public static int satellite_number;
        public static int  [,] Error_GPSbox_info = new int[35,50];
        public static int [,] Error_BDSbox_info = new int [35,50];



    }
}
