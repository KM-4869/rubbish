using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Windows.Forms.DataVisualization.Charting;

namespace GNSS数据质量分析自编程序
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void 打开ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.ShowDialog();
            ofd.InitialDirectory = Application.StartupPath;
            ofd.Title = "选择打开的o文件";
            ofd.Multiselect = false;
            ofd.RestoreDirectory = true;
            //打开选择文件对话框
           
           Class1.filepath = ofd.FileName;
            textBox1.Text = Class1.filepath;//获取并输出文件路径
            double line = 0;
            StreamReader reader = new StreamReader(Class1.filepath);//读取当前文件
            
            for(int i=1;;i++)
            {
                string oneline_header = reader.ReadLine();
                
               
                richTextBox1.Text = richTextBox1.Text + oneline_header+"\n";
                if(oneline_header.Contains("INTERVAL"))
                {
                    
                    label5.Text = oneline_header.Substring(4,6) + "s";
                    Class1.interval = Convert.ToDouble(oneline_header.Substring(5,5));
                }
                if(oneline_header.Contains("END OF HEADER"))
                {
                    Class1.end_of_header = i;
                    break;
                }

                //将文件头，采样率输出，获取文件头行数
                
            }
            reader.ReadLine();
            for(int i=Class1.end_of_header+1; ;i++)
            {
                string oneline_header = reader.ReadLine();
                line = i;
                if (oneline_header == null)
                {
                    break;
                }
            }
            //获取文件总行数
            Class1.endline = line;
            label7.Text = Convert.ToString(Class1.endline);
            
        }

        private void label1_Click(object sender, EventArgs e)
        {
            
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {

        }

        private void 清理ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBox1.Text = null;
            textBox1.Text = null;
        }

        private void 卫星观测数据完整度分析ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            panel1.Visible = true;
        }

        private void 文件ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            panel1.Visible = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            dataGridView1.ColumnCount = 7;
            dataGridView1.TopLeftHeaderCell.Value = "PRN序号";
            dataGridView1.Columns[0].Name = "最早出现历元";
            dataGridView1.Columns[1].Name = "最晚出现历元";
            dataGridView1.Columns[2].Name = "理论历元数";
            dataGridView1.Columns[3].Name = "f1完整率";
            dataGridView1.Columns[4].Name = "f2完整率";
            dataGridView1.Columns[5].Name = "f3完整率";
            dataGridView1.Columns[6].Name = "ALL完整率";

            
            Class1.PRNbox[0] = " ";


            /*
             * 此循环为总循环，
             * 过程为查找新卫星-查找该卫星出现历元数据是否完整-读完文件查找最后出现历元，
             * 循环次数应为该文件出现的所有不同的卫星个数
             */
            for (int k = 0; ; k++)
            {
                int epoch = 0;
                string PRN = " ";
                double PRN_firstshow_epoch = 0;
                double PRN_lastshow_epoch = 0;
                double f1_existnumber = 0;
                double f2_existnumber = 0;
                double f3_existnumber = 0;
                double All_existnumber = 0;
                
                bool end = false;
                double line = 0;


                bool b1 = false; bool b2 = true; bool b3 = true;
                string oneline_body = " ";
                StreamReader reader = new StreamReader(Class1.filepath);//读取打开的文件
             

                //读完文件头
                for (int i = 1; i <= Class1.end_of_header; i++)
                {
                    reader.ReadLine();
                    line = line + 1;
                   
                }


                //查找新卫星，获取起始历元，判断起始历元的数据完整情况
                for (int i = 0; ; i++)
                {
                    oneline_body = reader.ReadLine();
                    line = line + 1;


                    /*
                     * 当查找语句已经读到文件尾行，说明已经不可能再有新卫星出现，结束循环
                     * 令bool变量end等于true以结束总循环
                     */
                    if(line==Class1.endline)
                    {
                        
                        end = true;
                        break;
                    }


                    bool repetition=false;

                    //读到“>”，使历元数+1
                    if (oneline_body.Substring(0, 1) == ">")
                    {
                        epoch = epoch + 1;
                        
                    }
                    else
                    {
                        for(int j=0;j<=k-1;j++)
                        {
                            //判断此行读取的卫星是否是新卫星
                            if (oneline_body.Substring(0,3)==Class1.PRNbox[j])
                            {
                                repetition =true;
                            }
                        }
                        if (repetition == false)
                        {
                            PRN = oneline_body.Substring(0, 3);
                            Class1.PRNbox[k] = PRN;    //如果不是重复卫星，则计入数组中
                            PRN_firstshow_epoch = epoch;     //得到卫星起始历元

                            /*
                             判断卫星数据是否缺失
                            如果卫星的伪距，相位，多普勒观测值等中相应位置出现0值或者空缺则判断缺失
                            布尔变量b1,b2,b3判断是否同时缺失，或者卫星是否有该频段
                             */
                            if (oneline_body.Substring(3, 14) != "         0.000" && oneline_body.Substring(3, 14) != "              " && oneline_body.Substring(19, 14) != "         0.000" && oneline_body.Substring(19, 14) != "              " && oneline_body.Substring(35, 14) != "         0.000" && oneline_body.Substring(35, 14) != "              " && oneline_body.Substring(51, 14) != "         0.000" && oneline_body.Substring(51, 14) != "              ")
                            {
                                f1_existnumber = f1_existnumber + 1;
                                b1 = true;
                            }
                            if (oneline_body.Length > 130)
                            {
                                b2 = false;
                                if (oneline_body.Substring(67, 14) != "         0.000" && oneline_body.Substring(67, 14) != "              " && oneline_body.Substring(83, 14) != "         0.000" && oneline_body.Substring(83, 14) != "              " && oneline_body.Substring(99, 14) != "         0.000" && oneline_body.Substring(99, 14) != "              " && oneline_body.Substring(115, 14) != "         0.000" && oneline_body.Substring(115, 14) != "              ")
                                {
                                    f2_existnumber = f2_existnumber + 1;
                                    b2 = true;
                                }
                            }
                            if (oneline_body.Length > 190)
                            {
                                b3 = false;
                                if (oneline_body.Substring(131, 14) != "         0.000" && oneline_body.Substring(131, 14) != "              " && oneline_body.Substring(147, 14) != "         0.000" && oneline_body.Substring(147, 14) != "              " && oneline_body.Substring(163, 14) != "         0.000" && oneline_body.Substring(163, 14) != "              " && oneline_body.Substring(179, 14) != "         0.000" && oneline_body.Substring(179, 14) != "              ")
                                {
                                    b3 = true;
                                    f3_existnumber = f3_existnumber + 1;
                                }
                            }
                            if (b1 == true && b2 == true && b3 == true)
                            {
                                All_existnumber = All_existnumber + 1;
                            }

                            break;//找到新卫星后打破循环
                        }
                    }
                }

                //查找新卫星语句读到文件尾行，则结束整个大循环，程序运行结束，所有卫星均被找到
                if (end == true)
                {
                    break;
                }


                //对上一循环找到的新卫星进行单一查找
                for (int i = 1; ; i++)
                {
                    

                    oneline_body = reader.ReadLine();


                    //读到文件尾行，所有该卫星都被找到，结束循环
                    if (oneline_body == null)
                    {
                        break;
                    }
                    if (oneline_body.Substring(0, 1) == ">")
                    {
                        epoch = epoch + 1;

                    }



                    //找到该卫星，进行数据完整判断
                    else if (oneline_body.Substring(0, 3) == PRN)
                    {
                        PRN_lastshow_epoch = epoch;//每找到该卫星，都将最后历元赋为当前历元

                        /*
                          判断卫星数据是否缺失
                          如果卫星的伪距，相位，多普勒观测值等中相应位置出现0值或者空缺则判断缺失
                          布尔变量b1,b2,b3判断是否同时缺失，或者卫星是否有该频段
                         */
                        if (oneline_body.Substring(3, 14) != "         0.000" && oneline_body.Substring(3, 14) != "              " && oneline_body.Substring(19, 14) != "         0.000" && oneline_body.Substring(19, 14) != "              " && oneline_body.Substring(35, 14) != "         0.000" && oneline_body.Substring(35, 14) != "              " && oneline_body.Substring(51, 14) != "         0.000" && oneline_body.Substring(51, 14) != "              ")
                        {
                            f1_existnumber = f1_existnumber + 1;
                            b1 = true;
                        }
                        if (oneline_body.Length > 130)
                        {
                            b2 = false;
                            if (oneline_body.Substring(67, 14) != "         0.000" && oneline_body.Substring(67, 14) != "              " && oneline_body.Substring(83, 14) != "         0.000" && oneline_body.Substring(83, 14) != "              " && oneline_body.Substring(99, 14) != "         0.000" && oneline_body.Substring(99, 14) != "              " && oneline_body.Substring(115, 14) != "         0.000" && oneline_body.Substring(115, 14) != "              ")
                            {
                                f2_existnumber = f2_existnumber + 1;
                                b2 = true;
                            }
                        }
                        if (oneline_body.Length > 190)
                        {
                            b3 = false;
                            if (oneline_body.Substring(131, 14) != "         0.000" && oneline_body.Substring(131, 14) != "              " && oneline_body.Substring(147, 14) != "         0.000" && oneline_body.Substring(147, 14) != "              " && oneline_body.Substring(163, 14) != "         0.000" && oneline_body.Substring(163, 14) != "              " && oneline_body.Substring(179, 14) != "         0.000" && oneline_body.Substring(179, 14) != "              ")
                            {
                                b3 = true;
                                f3_existnumber = f3_existnumber + 1;
                            }
                        }
                        if (b1 == true && b2 == true && b3 == true)
                        {
                            All_existnumber = All_existnumber + 1;
                        }
                    }
                }

                //将查找完的这一颗卫星的数据完整率计算并填入表格
                int index = dataGridView1.Rows.Add();
                dataGridView1.Rows[index].HeaderCell.Value = PRN;
                dataGridView1.Rows[index].Cells[0].Value = PRN_firstshow_epoch;
                dataGridView1.Rows[index].Cells[1].Value = PRN_lastshow_epoch;
                dataGridView1.Rows[index].Cells[2].Value = PRN_lastshow_epoch - PRN_firstshow_epoch + 1;
                dataGridView1.Rows[index].Cells[3].Value = f1_existnumber / (PRN_lastshow_epoch - PRN_firstshow_epoch + 1) * 100 + "%";
                dataGridView1.Rows[index].Cells[4].Value = f2_existnumber / (PRN_lastshow_epoch - PRN_firstshow_epoch + 1) * 100 + "%";
                dataGridView1.Rows[index].Cells[5].Value = f3_existnumber / (PRN_lastshow_epoch - PRN_firstshow_epoch + 1) * 100 + "%";
                dataGridView1.Rows[index].Cells[6].Value = All_existnumber / (PRN_lastshow_epoch - PRN_firstshow_epoch + 1) * 100 + "%";

                Class1.satellite_number = k;
          
            }

            

        }

        private void button2_Click(object sender, EventArgs e)
        {
           int  row = dataGridView1.Rows.Count;
            for(int i=0;i<row-1;i++)
            {
                dataGridView1.Rows.RemoveAt(0);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == -1 || comboBox2.SelectedIndex == -1)
            {
                comboBox1.Text = "???";
                comboBox2.Text = "???";
            }
            else
            {

                //将完整度分析时检测到的所有卫星分开GPS和BDS存入数组
                int j = 0;
                int k = 0;
                for (int i = 0; i <= Class1.satellite_number; i++)
                {
                    if (Class1.PRNbox[i].Substring(0, 1) == "G")
                    {

                        Class1.GPSbox[j] = Class1.PRNbox[i];
                        j = j + 1;
                    }
                    if (Class1.PRNbox[i].Substring(0, 1) == "C")
                    {

                        Class1.BDSbox[k] = Class1.PRNbox[i];
                        k = k + 1;
                    }
                }
                richTextBox2.Text = null;

                //输出卫星列表
                for (int i = 0; i < 60; i++)
                {
                    if (Class1.PRNbox[i] != null)
                    {
                        richTextBox2.Text = richTextBox2.Text + Class1.PRNbox[i] + "\r\n";
                    }
                }



                dataGridView2.ColumnCount = 1;
                dataGridView3.ColumnCount = 1;
                dataGridView2.TopLeftHeaderCell.Value = "PRN序号";
                dataGridView2.Columns[0].Name = "发生周跳历元";
                dataGridView3.TopLeftHeaderCell.Value = "PRN序号";
                dataGridView3.Columns[0].Name = "发生周跳历元";

                double L1 = 1575.42e6;
                double L2 = 1227.60e6;
                double B1 = 1561.098e6;
                double B2 = 1207.14e6;
                double c = 299792458;
                double lambda_L1 = c / L1;
                double lambda_L2 = c / L2;
                double lambda_B1 = c / B1;
                double lambda_B2 = c / B2;
                for(int a=0;a<Class1.Error_GPSbox_info.GetLength(0);a++)
                {
                    for(int b=0;b<Class1.Error_GPSbox_info.GetLength(1);b++)
                    {
                        Class1.Error_GPSbox_info[a, b] = 0;
                    }
                }


                //对每一个出现的GPS卫星按顺序进行周跳探测
                for (int i = 0; ; i++)
                {

                    if (Class1.GPSbox[i] == null)
                    {
                        break;
                    }




                    double present_phi1 = 0;
                    double present_phi2 = 0;
                    double present_p1 = 0;
                    double present_p2 = 0;
                    double next_phi1 = 0;
                    double next_phi2 = 0;
                    double next_p1 = 0;
                    double next_p2 = 0;
                    string time = " ";
                    int epoch_number = 0;
                    int present_epoch_number = 0;
                    int next_epoch_number = 0;
                    string oneline_body = " ";
                    bool first_find = true;
                    int i2 = 0;

                    StreamReader reader = new StreamReader(Class1.filepath);

                    //读完文件头
                    for (int a = 1; a <= Class1.end_of_header; a++)
                    {
                        reader.ReadLine();
                    }

                    for (int b = 1; ; b++)
                    {

                        oneline_body = reader.ReadLine();

                        //读到文件尾结束循环
                        if (oneline_body == null)
                        {
                            break;
                        }

                        //读取到“>”使历元数加一，获取此历元信息
                        if (oneline_body.Substring(0, 1) == ">")
                        {
                            time = oneline_body.Substring(2, 22);
                            epoch_number = epoch_number + 1;
                        }

                        //读到所查找的卫星，进行周跳判断
                        else if (oneline_body.Substring(0, 3) == Class1.GPSbox[i])
                        {

                            //如果第一次找到此卫星并且是有完整观测值，则记录两个频率的伪距与相位观测值
                            if (first_find == true && oneline_body.Substring(3, 14) != "         0.000" && oneline_body.Substring(3, 14) != "              " && oneline_body.Substring(19, 14) != "         0.000" && oneline_body.Substring(19, 14) != "              " && oneline_body.Substring(67, 14) != "         0.000" && oneline_body.Substring(67, 14) != "              " && oneline_body.Substring(83, 14) != "         0.000" && oneline_body.Substring(83, 14) != "              ")
                            {
                                present_p1 = Convert.ToDouble(oneline_body.Substring(3, 14));
                                present_p2 = Convert.ToDouble(oneline_body.Substring(67, 14));
                                present_phi1 = Convert.ToDouble(oneline_body.Substring(19, 14));
                                present_phi2 = Convert.ToDouble(oneline_body.Substring(83, 14));
                                present_epoch_number = epoch_number;
                                first_find = false;
                            }//如果为非第一次并有完整观测值，则记录两个频率的伪距与相位观测值
                            else if (first_find == false && oneline_body.Substring(3, 14) != "         0.000" && oneline_body.Substring(3, 14) != "              " && oneline_body.Substring(19, 14) != "         0.000" && oneline_body.Substring(19, 14) != "              " && oneline_body.Substring(67, 14) != "         0.000" && oneline_body.Substring(67, 14) != "              " && oneline_body.Substring(83, 14) != "         0.000" && oneline_body.Substring(83, 14) != "              ")
                            {
                                next_p1 = Convert.ToDouble(oneline_body.Substring(3, 14));
                                next_p2 = Convert.ToDouble(oneline_body.Substring(67, 14));
                                next_phi1 = Convert.ToDouble(oneline_body.Substring(19, 14));
                                next_phi2 = Convert.ToDouble(oneline_body.Substring(83, 14));
                                next_epoch_number = epoch_number;
                                //如果本历元与上历元数是相差为一，则它们是相邻的两历元，开始构造组合观测值并历元间求差判断周跳
                                if (next_epoch_number - present_epoch_number == 1)
                                {
                                    double GF_present = present_phi1 * lambda_L1 - present_phi2 * lambda_L2;
                                    double GF_next = next_phi1 * lambda_L1 - next_phi2 * lambda_L2;
                                    double MW_present = (present_phi1 - present_phi2) - ((L1 - L2) / (L1 + L2)) * (present_p1 / lambda_L1 + present_p2 / lambda_L2);
                                    double MW_next = (next_phi1 - next_phi2) - ((L1 - L2) / (L1 + L2)) * (next_p1 / lambda_L1 + next_p2 / lambda_L2);

                                    double dGF = GF_next - GF_present;
                                    double dMW = MW_next - MW_present;

                                    if (Math.Abs(dGF)> Convert.ToDouble(comboBox2.Text) || Math.Abs(dMW) > Convert.ToDouble(comboBox1.Text))
                                    {
                                        int index = dataGridView2.Rows.Add();
                                        dataGridView2.Rows[index].HeaderCell.Value = Class1.GPSbox[i];
                                        dataGridView2.Rows[index].Cells[0].Value = time;

                                        Class1.Error_GPSbox_info[i, i2] =epoch_number;//获取发生周跳的历元，并将其存起以便第三步进行按周跳分段
                                        i2 = i2 + 1;

                                    }
                                }
                                //不论是否是相邻历元，都将next历元数据赋为present，作为下一历元应减去的前项
                                present_p1 = next_p1;
                                present_p2 = next_p2;
                                present_phi1 = next_phi1;
                                present_phi2 = next_phi2;
                                present_epoch_number = next_epoch_number;
                            }
                        }
                    }


                     

                }
                
                /*
                 * 北斗的周跳探测原理与GPS相同
                 * 故不再详述
                 * 以下是北斗的周跳探测
                 */


                for (int i = 0; ; i++)
                {

                    if (Class1.BDSbox[i] == null)
                    {
                        break;
                    }




                    double present_phi1 = 0;
                    double present_phi2 = 0;
                    double present_p1 = 0;
                    double present_p2 = 0;
                    double next_phi1 = 0;
                    double next_phi2 = 0;
                    double next_p1 = 0;
                    double next_p2 = 0;
                    string time = " ";
                    int epoch_number = 0;
                    int present_epoch_number = 0;
                    int next_epoch_number = 0;
                    string oneline_body = " ";
                    bool first_find = true;
                    int i2 = 0;
                    StreamReader reader = new StreamReader(Class1.filepath);

                    //读完文件头
                    for (int a = 1; a <= Class1.end_of_header; a++)
                    {
                        reader.ReadLine();
                    }

                    for (int b = 1; ; b++)
                    {

                        oneline_body = reader.ReadLine();

                        //读到文件尾结束循环
                        if (oneline_body == null)
                        {
                            break;
                        }

                        if (oneline_body.Substring(0, 1) == ">")
                        {
                            time = oneline_body.Substring(2, 22);
                            epoch_number = epoch_number + 1;
                        }
                        else if (oneline_body.Substring(0, 3) == Class1.BDSbox[i])
                        {
                            if (first_find == true && oneline_body.Substring(3, 14) != "         0.000" && oneline_body.Substring(3, 14) != "              " && oneline_body.Substring(19, 14) != "         0.000" && oneline_body.Substring(19, 14) != "              " && oneline_body.Substring(67, 14) != "         0.000" && oneline_body.Substring(67, 14) != "              " && oneline_body.Substring(83, 14) != "         0.000" && oneline_body.Substring(83, 14) != "              ")
                            {
                                present_p1 = Convert.ToDouble(oneline_body.Substring(3, 14));
                                present_p2 = Convert.ToDouble(oneline_body.Substring(67, 14));
                                present_phi1 = Convert.ToDouble(oneline_body.Substring(19, 14));
                                present_phi2 = Convert.ToDouble(oneline_body.Substring(83, 14));
                                present_epoch_number = epoch_number;
                                first_find = false;
                            }
                            else if (first_find == false && oneline_body.Substring(3, 14) != "         0.000" && oneline_body.Substring(3, 14) != "              " && oneline_body.Substring(19, 14) != "         0.000" && oneline_body.Substring(19, 14) != "              " && oneline_body.Substring(67, 14) != "         0.000" && oneline_body.Substring(67, 14) != "              " && oneline_body.Substring(83, 14) != "         0.000" && oneline_body.Substring(83, 14) != "              ")
                            {
                                next_p1 = Convert.ToDouble(oneline_body.Substring(3, 14));
                                next_p2 = Convert.ToDouble(oneline_body.Substring(67, 14));
                                next_phi1 = Convert.ToDouble(oneline_body.Substring(19, 14));
                                next_phi2 = Convert.ToDouble(oneline_body.Substring(83, 14));
                                next_epoch_number = epoch_number;

                                if (next_epoch_number - present_epoch_number == 1)
                                {
                                    double GF_present = present_phi1 * lambda_B1 - present_phi2 * lambda_B2;
                                    double GF_next = next_phi1 * lambda_B1 - next_phi2 * lambda_B2;
                                    double MW_present = (present_phi1 - present_phi2) - ((B1 - B2) / (B1 + B2)) * (present_p1 / lambda_B1 + present_p2 / lambda_B2);
                                    double MW_next = (next_phi1 - next_phi2) - ((B1 - B2) / (B1 + B2)) * (next_p1 / lambda_B1 + next_p2 / lambda_B2);

                                    double dGF = GF_next - GF_present;
                                    double dMW = MW_next - MW_present;

                                    if (Math.Abs(dGF) > Convert.ToDouble(comboBox2.Text) || Math.Abs(dMW) > Convert.ToDouble(comboBox1.Text))
                                    {
                                        int index = dataGridView3.Rows.Add();
                                        dataGridView3.Rows[index].HeaderCell.Value = Class1.BDSbox[i];
                                        dataGridView3.Rows[index].Cells[0].Value = time;

                                        Class1.Error_BDSbox_info[i, i2] = epoch_number;
                                        i2 = i2 + 1;
                                    }
                                }

                                present_p1 = next_p1;
                                present_p2 = next_p2;
                                present_phi1 = next_phi1;
                                present_phi2 = next_phi2;
                                present_epoch_number = next_epoch_number;
                            }
                        }
                    }




                }
            }

        }

        private void 清理_Click(object sender, EventArgs e)
        {
            int row2 = dataGridView2.Rows.Count;
            int row3 = dataGridView3.Rows.Count;
            for (int i = 0; i < row2 - 1; i++)
            {
                dataGridView2.Rows.RemoveAt(0);
            }
            for (int i = 0; i < row3 - 1; i++)
            {
                dataGridView3.Rows.RemoveAt(0);
            }
        }

        private void 开始_Click(object sender, EventArgs e)
        {

            //进行一些图表的初始设定
            dataGridView4.ColumnCount = 2;
            dataGridView4.Columns[0].Name = "f1所有历元MP值标准差";
            dataGridView4.Columns[1].Name = "f2所有历元MP值标准差";
            dataGridView4.TopLeftHeaderCell.Value = "PRN号";
            chart1.Series.Clear();
            chart2.Series.Clear();
            chart1.ChartAreas.Clear();
            chart2.ChartAreas.Clear();
            chart1.Titles.Clear();
            chart2.Titles.Clear();
            chart1.ChartAreas.Add("0");
            chart2.ChartAreas.Add("0");
            chart1.Titles.Add("0");
            chart2.Titles.Add("0");
            
            chart1.ChartAreas[0].AxisX.Minimum = 0;
            chart1.ChartAreas[0].AxisX.Maximum = 1200;
            chart1.ChartAreas[0].AxisY.Minimum = -5;
            chart1.ChartAreas[0].AxisY.Maximum = 5;
            chart1.ChartAreas[0].AxisY.Title = "多路径误差（m）";
            chart1.ChartAreas[0].AxisX.Title = "历元数";
            chart2.ChartAreas[0].AxisX.Minimum = 0;
            chart2.ChartAreas[0].AxisX.Maximum = 1200;
            chart2.ChartAreas[0].AxisY.Minimum = -5;
            chart2.ChartAreas[0].AxisY.Maximum = 5;
            chart2.ChartAreas[0].AxisY.Title = "多路径误差（m）";
            chart2.ChartAreas[0].AxisX.Title = "历元数";
            chart1.Titles[0].Text = "卫星在L1频率上多路径效应";
            chart2.Titles[0].Text = "卫星在L2频率上的多路径效应";
            double L1 = 1575.42e6;
            double L2 = 1227.60e6;
            double B1 = 1561.098e6;
            double B2 = 1207.14e6;
            double c = 299792458;
            double lambda_L1 = c / L1;
            double lambda_L2 = c / L2;
            double lambda_B1 = c / B1;
            double lambda_B2 = c / B2;

            double f_L1 = 1575.42;
            double f_L2 = 1227.60;
            double f_B1 = 1561.098;
            double f_B2 = 1207.14;

            
            
            string oneline_body;

            double[] MP_f1 = new double[100000];
            double[] MP_f2 = new double[100000];
            int beginline = Class1.end_of_header;
            int i2 = 0;
            int i3 = 0;
            int epoch_number = 0;


            //此循环对每个GPS卫星进行搜索
            for (int i=0; ;i++)
            {

               //当GPS卫星搜索完成结束循环
                if (Class1.GPSbox[i] == null)
                {
                    break;
                }
                string GPSname = Class1.GPSbox[i];
                
                
                int N = 0;
                int line = 0;
                int[] epoch_number_box = new int[1200];
                StreamReader reader = new StreamReader(Class1.filepath);


                /*
                 * 每次循环先读取a行
                 * 如果此卫星没有周跳或者发生周跳的卫星的首次读取，则a等于文件头行数
                 * 如果是周跳卫星的非首次读取，则接着从上次读到的地方继续往下读取，以此根据周跳分段
                 */
                for (int a = 1; a <= beginline; a++)
                {
                    reader.ReadLine();
                    line = line + 1;
                }



                //每次读一行，进行判断与计算
                for (int b = 1; ; b++)
                {



                    oneline_body = reader.ReadLine();
                    line = line + 1;

                    //当一颗卫星被读完，进行一些初始化并结束按行读取的循环
                    if(oneline_body==null)
                    {
                        beginline = Class1.end_of_header;
                        i2 = 0;
                        epoch_number = 0;
                        break;
                    }

                    //读到">"使历元数加一
                    if(oneline_body.Substring(0,1)==">")
                    {
                        epoch_number = epoch_number + 1;
                    }



                    //当读取到所寻找的GPS卫星并且观测值完整时
                    if (oneline_body.Substring(0, 3) == Class1.GPSbox[i] && oneline_body.Substring(3, 14) != "         0.000" && oneline_body.Substring(3, 14) != "              " && oneline_body.Substring(19, 14) != "         0.000" && oneline_body.Substring(19, 14) != "              " && oneline_body.Substring(67, 14) != "         0.000" && oneline_body.Substring(67, 14) != "              " && oneline_body.Substring(83, 14) != "         0.000" && oneline_body.Substring(83, 14) != "              ")
                    {


                        /*
                         * 当读取到周跳历元时
                         * 使beginline等于当前行数减一，下一次读取从当前行开始
                         * i-1使下一次循环查找的卫星依然是此卫星
                         * i2+1使进入此卫星下一周跳的搜索
                         * 打破循环，后面输出周跳前的MP均值
                         */
                        if (epoch_number == Class1.Error_GPSbox_info[i, i2])
                        {
                            i2 = i2 + 1;
                            i = i - 1;
                            beginline = line-1;
                            break;
                        }


                        //获取观测值，按公式进行计算，存入数组
                        double p1 = Convert.ToDouble(oneline_body.Substring(3, 14));
                        double p2 = Convert.ToDouble(oneline_body.Substring(67, 14));
                        double phi1 = Convert.ToDouble(oneline_body.Substring(19, 14));
                        double phi2 = Convert.ToDouble(oneline_body.Substring(83, 14));

                        double MPL1 = p1 - (f_L1 * f_L1 + f_L2 * f_L2) / (f_L1 * f_L1 - f_L2 * f_L2) * phi1 * lambda_L1 + 2 * f_L2 * f_L2 / (f_L1 * f_L1 - f_L2 * f_L2) * phi2 * lambda_L2;
                        double MPL2 = p2 - 2 * f_L1 * f_L1 / (f_L1 * f_L1 - f_L2 * f_L2) * phi1 * lambda_L1 + (f_L1 * f_L1 + f_L2 * f_L2) / (f_L1 * f_L1 - f_L2 * f_L2) * phi2 * lambda_L2;

                        MP_f1[N] = MPL1;
                        MP_f2[N] = MPL2;
                        epoch_number_box[N] = epoch_number;
                        N = N + 1;

                        

                    }



                }

                double sum_MP_f1=0;
                double sum_MP_f2=0;
                double average_MP_f1;
                double average_MP_f2;
                //计算MP总和
                for(int k=0;k<N;k++)
                {
                    sum_MP_f1 = sum_MP_f1 + MP_f1[k];
                    sum_MP_f2 = sum_MP_f2 + MP_f2[k];
                }

                //计算平均值
                average_MP_f1 = sum_MP_f1 / N;
                average_MP_f2 = sum_MP_f2 / N;

                double sum_MP_f1_2=0;
                double sum_MP_f2_2 = 0;
                double error_MP_f1 = 0;
                double error_MP_f2 = 0;
                

                //计算与平均的差值并作图
                chart1.Series.Add(i3.ToString());
                chart2.Series.Add(i3.ToString());
                for (int k=0;k<N;k++)
                {
                    sum_MP_f1_2 = sum_MP_f1_2 + (MP_f1[k] - average_MP_f1) * (MP_f1[k] - average_MP_f1);
                    sum_MP_f2_2 = sum_MP_f2_2 + (MP_f2[k] - average_MP_f2) * (MP_f2[k] - average_MP_f2);

                    error_MP_f1 = MP_f1[k] - average_MP_f1;
                    error_MP_f2 = MP_f2[k] - average_MP_f2;
                    chart1.Series[i3].ChartType = SeriesChartType.Line;
                    chart2.Series[i3].ChartType = SeriesChartType.Line;
                    chart1.Series[i3].Points.Add(new DataPoint(epoch_number_box[k], error_MP_f1));
                    chart2.Series[i3].Points.Add(new DataPoint(epoch_number_box[k], error_MP_f2));
                }
                i3 = i3 + 1;


                //计算均方值并填入表格
                double sigma_MP_f1 = Math.Sqrt(sum_MP_f1_2 / (N - 1));
                double sigma_MP_f2 = Math.Sqrt(sum_MP_f2_2 / (N - 1));


                
                int index = dataGridView4.Rows.Add();
                dataGridView4.Rows[index].HeaderCell.Value = GPSname;
                dataGridView4.Rows[index].Cells[0].Value = sigma_MP_f1;
                dataGridView4.Rows[index].Cells[1].Value = sigma_MP_f2;
               
                

            }




            /*
             * 以下为北斗卫星的多路径分析结果
             * 过程与GPS类似
             * 故不作注释
             */


            i2 = 0;
            epoch_number = 0;
            beginline = Class1.end_of_header;



            for (int i = 0; ; i++)
            {
                if (Class1.BDSbox[i] == null)
                {
                    break;
                }
                string BDSname = Class1.BDSbox[i];


                int N = 0;
                int line = 0;
                StreamReader reader = new StreamReader(Class1.filepath);

                for (int a = 1; a <= beginline; a++)
                {
                    reader.ReadLine();
                    line = line + 1;
                }
                for (int b = 1; ; b++)
                {



                    oneline_body = reader.ReadLine();
                    line = line + 1;


                    if (oneline_body == null)
                    {
                        beginline = Class1.end_of_header;
                        i2 = 0;
                        epoch_number = 0;
                        break;
                    }

                    if (oneline_body.Substring(0, 1) == ">")
                    {
                        epoch_number = epoch_number + 1;
                    }




                    if (oneline_body.Substring(0, 3) == Class1.BDSbox[i] && oneline_body.Substring(3, 14) != "         0.000" && oneline_body.Substring(3, 14) != "              " && oneline_body.Substring(19, 14) != "         0.000" && oneline_body.Substring(19, 14) != "              " && oneline_body.Substring(67, 14) != "         0.000" && oneline_body.Substring(67, 14) != "              " && oneline_body.Substring(83, 14) != "         0.000" && oneline_body.Substring(83, 14) != "              ")
                    {


                        if (epoch_number == Class1.Error_BDSbox_info[i, i2])
                        {
                            i2 = i2 + 1;
                            i = i - 1;
                            beginline = line - 1;
                            break;
                        }


                        double p1 = Convert.ToDouble(oneline_body.Substring(3, 14));
                        double p2 = Convert.ToDouble(oneline_body.Substring(67, 14));
                        double phi1 = Convert.ToDouble(oneline_body.Substring(19, 14));
                        double phi2 = Convert.ToDouble(oneline_body.Substring(83, 14));

                        double MPB1 = p1 - (f_B1 * f_B1 + f_B2 * f_B2) / (f_B1 * f_B1 - f_B2 * f_B2) * phi1 * lambda_B1 + 2 * f_B2 * f_B2 / (f_B1 * f_B1 - f_B2 * f_B2) * phi2 * lambda_B2;
                        double MPB2 = p2 - 2 * f_B1 * f_B1 / (f_B1 * f_B1 - f_B2 * f_B2) * phi1 * lambda_B1 + (f_B1 * f_B1 + f_B2 * f_B2) / (f_B1 * f_B1 - f_B2 * f_B2) * phi2 * lambda_B2;

                        MP_f1[N] = MPB1;
                        MP_f2[N] = MPB2;

                        N = N + 1;



                    }



                }

                double sum_MP_f1 = 0;
                double sum_MP_f2 = 0;
                double average_MP_f1;
                double average_MP_f2;

                for (int k = 0; k < N; k++)
                {
                    sum_MP_f1 = sum_MP_f1 + MP_f1[k];
                    sum_MP_f2 = sum_MP_f2 + MP_f2[k];
                }

                average_MP_f1 = sum_MP_f1 / N;
                average_MP_f2 = sum_MP_f2 / N;

                double sum_MP_f1_2 = 0;
                double sum_MP_f2_2 = 0;

                for (int k = 0; k < N; k++)
                {
                    sum_MP_f1_2 = sum_MP_f1_2 + (MP_f1[k] - average_MP_f1) * (MP_f1[k] - average_MP_f1);
                    sum_MP_f2_2 = sum_MP_f2_2 + (MP_f2[k] - average_MP_f2) * (MP_f2[k] - average_MP_f2);
                }

                double sigma_MP_f1 = Math.Sqrt(sum_MP_f1_2 / (N - 1));
                double sigma_MP_f2 = Math.Sqrt(sum_MP_f2_2 / (N - 1));

                int index = dataGridView4.Rows.Add();
                dataGridView4.Rows[index].HeaderCell.Value = BDSname;
                dataGridView4.Rows[index].Cells[0].Value = sigma_MP_f1;
                dataGridView4.Rows[index].Cells[1].Value = sigma_MP_f2;
                

            }
        }

        private void 使用须知ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form2 form2 = new Form2();
            form2.Show();
        }

        private void tabPage3_Click(object sender, EventArgs e)
        {

        }
        private void button4_Click(object sender, EventArgs e)
        {
            int row = dataGridView4.Rows.Count;
            for(int i=0;i<row-1;i++)
            {
                dataGridView4.Rows.RemoveAt(0);
            }
        }
    }
}
