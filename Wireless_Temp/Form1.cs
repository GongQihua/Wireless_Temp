using EasyModbus;
using ScottPlot;
using ScottPlot.Drawing.Colormaps;
using ScottPlot.Renderable;
using System;
using System.Collections;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics.Eventing.Reader;
using System.Drawing.Drawing2D;
using System.IO.Ports;
using System.Threading;
using static ScottPlot.Plottable.PopulationPlot;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Wireless_Temp
{
    public partial class Form1 : Form
    {
        public System.DateTime time;
        public Form1()
        {
            InitializeComponent();
            SetWindowRegion();
            Control.CheckForIllegalCrossThreadCalls = false;
            time = System.DateTime.Now;
            toolStripStatusLabel2.Text = "Starting Time: " + time.ToString();
            //Thread thread3 = new Thread(status_check);
            //thread3.IsBackground = true;
            //thread3.Start();
        }
        public ModbusClient modbusClient = new ModbusClient("192.168.0.0", 502);
        public int sensor_num = 0;
        public ArrayList cur = new ArrayList();
        public int end_signal = 0;
        public int Read_model = 0;
        public void SetWindowRegion()//»­Ô²½Ç
        {
            GraphicsPath FormPath;
            Rectangle rect = new Rectangle(0, 0, this.Width, this.Height);
            FormPath = GetRoundedRectPath(rect, 50);
            this.Region = new Region(FormPath);

        }
        private GraphicsPath GetRoundedRectPath(Rectangle rect, int radius)//»­Ô²½Ç
        {
            int diameter = radius;
            Rectangle arcRect = new Rectangle(rect.Location, new Size(diameter, diameter));
            GraphicsPath path = new GraphicsPath();

            // ×óÉÏ½Ç
            path.AddArc(arcRect, 180, 90);

            // ÓÒÉÏ½Ç
            arcRect.X = rect.Right - diameter;
            path.AddArc(arcRect, 270, 90);

            // ÓÒÏÂ½Ç
            arcRect.Y = rect.Bottom - diameter;
            path.AddArc(arcRect, 0, 90);

            // ×óÏÂ½Ç
            arcRect.X = rect.Left;
            path.AddArc(arcRect, 90, 90);
            path.CloseFigure();//±ÕºÏÇúÏß
            return path;
        }

        //public void status_check()
        //{
        //    while (true)
        //    {
        //        if(modbusClient.Connected == true)
        //        {
        //            pictureBox4.Visible = true;
        //            pictureBox5.Visible = false;
        //        }
        //        if(modbusClient.Connected == false)
        //        {
        //            pictureBox4.Visible = false;
        //            pictureBox5.Visible = true;
        //        }
        //        time = System.DateTime.Now;
        //        toolStripStatusLabel2.Text = "System Time: " + time.ToString();
        //        //Thread.Sleep(5000);
        //    }
        //}
        private void metroBadge1_Click(object sender, EventArgs e)
        {
            string address = textBox1.Text;
            Int32 port = Convert.ToInt32(textBox2.Text);
            modbusClient = new ModbusClient($"{address}", port);
            try
            {
                modbusClient.UnitIdentifier = 1;
                modbusClient.Baudrate = 9600;
                modbusClient.Parity = Parity.None;
                modbusClient.StopBits = StopBits.One;
                modbusClient.ConnectionTimeout = 10000;
                modbusClient.Connect();

                if (modbusClient.Connected)
                {
                    MessageBox.Show("Connection Success\n");

                    checkBox1.Checked = true;
                }

            }
            catch (Exception)
            {
                modbusClient.Disconnect();
                MessageBox.Show("Connection Failed, Try Again\n");
                return;
            }
        }
        private void metroBadge2_Click(object sender, EventArgs e)
        {
            if (!modbusClient.Connected)
            {
                MessageBox.Show("Please build the connection first!");
                return;
            }
            int[] check = modbusClient.ReadHoldingRegisters(01005, 12);
            sensor_num = 0;
            for (int i = 0; i < 12; i++)
            {
                if (check[i] != 0)
                {
                    sensor_num += 1;
                }
            }
            MessageBox.Show($"{sensor_num.ToString()} Sensor(s) Detected");

            for (int i = 0; i < sensor_num; i++)
            {
                int[] array = modbusClient.ReadHoldingRegisters(01105, sensor_num);
                while (array[i] == -26215)
                {
                    array = modbusClient.ReadHoldingRegisters(01105, sensor_num);
                }
                cur.Add(array[i]);
            }
            MessageBox.Show("Sensor initialized");

            checkBox2.Checked = true;
            pictureBox4.Visible = true;
            pictureBox5.Visible = false;
        }

        private void metroEllipse1_Click(object sender, EventArgs e)
        {
            if (!modbusClient.Connected)
            {
                MessageBox.Show("Please build the connection first!");
                return;
            }

            if (cur.Count == 0)
            {
                MessageBox.Show("Please initialized the sensor!");
                return;
            }
            if (textBox1.Text == String.Empty || textBox2.Text == String.Empty)
            {
                MessageBox.Show("Please input delay and batch number!");
                return;
            }
            int text1 = Convert.ToInt32(textBox5.Text) * 1000;
            if (text1 < 1000 || text1 > 600000)
            {
                MessageBox.Show("Unable Delay Time");
                return;
            }
            SQLiteConnection conn = new SQLiteConnection("Data Source = Wireless_Temp.db;Version=3;");
            conn.Open();
            string sql = "select * from sqlite_master WHERE type = 'table' order by name";
            SQLiteCommand command = new SQLiteCommand(sql, conn);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                string file_string = "table" + metroTextBox1.Text;
                if (file_string == reader["name"].ToString())
                {
                    MessageBox.Show("Repeat batch number");
                    return;
                }
            }
            conn.Close();

            Thread thread = new Thread(Record_Modbus);
            thread.IsBackground = true;
            thread.Start();
            MessageBox.Show("Recording Start\n");
            return;
        }
        private void Record_Modbus()
        {
            //int epoch = Convert.ToInt32(textBox2.Text);
            int delay = Convert.ToInt32(textBox5.Text) * 1000;
            string batchNo = metroTextBox1.Text;
            int[] result;
            int n = 1;
            end_signal = 0;
            while (end_signal != 1 && modbusClient.Connected)
            {
                double[] res = new double[12];
                //string res_string = "";
                result = modbusClient.ReadHoldingRegisters(01105, sensor_num);
                for (int i = 0; i < sensor_num; i++)
                {
                    if (result[i] == -26215)
                    {
                        result[i] = Convert.ToInt32(cur[i]);
                    }
                    else
                    {
                        cur[i] = result[i];
                    }
                    //double res = result[i] / 100.00;
                    res[i] = result[i] / 100.00;
                    //res_string += "," + res.ToString();
                }
                //WriteCSV(n.ToString(), res_string, batchNo);
                WriteDB(n, res, batchNo);
                n++;
                Thread.Sleep(delay);
            }

            MessageBox.Show("Finish Recording\n");
            //modbusClient.Disconnect();
            return;
        }
        private void WriteDB(int id, double[] res, string batchNo)
        {
            String Time = Convert.ToString(System.DateTime.Now);
            //SQLiteConnection.CreateFile("Wireless_Temp.db");
            SQLiteConnection conn = new SQLiteConnection("Data Source = Wireless_Temp.db;Version=3;");
            conn.Open();

            string line_name = "id int, Time text";
            for (int i = 1; i <= sensor_num; i++)
            {
                string sensor_name = ", Sensor" + i.ToString() + " double";
                line_name += sensor_name;
            }
            string query = $"create table if not exists table{batchNo}({line_name})";
            //MessageBox.Show(query);
            SQLiteCommand command = new SQLiteCommand(query, conn);
            command.ExecuteNonQuery();

            string line_set = "id, Time";
            for (int i = 1; i <= sensor_num; i++)
            {
                string sensor_set = ", Sensor" + i.ToString();
                line_set += sensor_set;
            }
            string line_value = $"{id.ToString()}, '{Time}'";
            for (int i = 0; i < sensor_num; i++)
            {
                string sensor_value = ", " + res[i].ToString();
                line_value += sensor_value;
            }
            string sql = $"insert into table{batchNo}({line_set}) values({line_value})";
            //MessageBox.Show(sql);
            command = new SQLiteCommand(sql, conn);
            command.ExecuteNonQuery();

            conn.Close();
            return;
        }

        private void metroEllipse2_Click(object sender, EventArgs e)
        {
            end_signal = 1;
        }

        private void metroButton1_Click(object sender, EventArgs e)
        {
            if (!modbusClient.Connected)
            {
                MessageBox.Show("Please build the connection first!");
                return;
            }
            Thread thread2 = new Thread(Read_sensor);
            if (Read_model == 0)
            {
                Read_model = 1;
                formsPlot1.Plot.Clear();
                dataGridView2.Rows.Clear();
                //Thread thread2 = new Thread(Read_sensor);
                thread2.IsBackground = true;
                thread2.Start();
                MessageBox.Show("Sensor Reading Start\n");
            }
            else
            {
                Read_model = 0;
                //thread2.Abort();
                thread2.Interrupt();
                formsPlot1.Plot.Clear();
                formsPlot1.Refresh();
                dataGridView2.Rows.Clear();
                MessageBox.Show("Sensor Reading Stoped\n");
            }
            return;
        }

        private void Read_sensor()
        {
            int delay = Convert.ToInt32(textBox5.Text) * 1000;
            dataGridView2.Rows.Add();
            formsPlot1.Plot.XLabel("Sensor Name");
            formsPlot1.Plot.YLabel("Temperature");
            formsPlot1.Plot.Title("Sensor Status");

            while (Read_model == 1)
            {
                int[] sensor_val = modbusClient.ReadHoldingRegisters(01105, 12);
                for (int i = 0; i < sensor_num; i++)
                {
                    dataGridView2[i, 0].Value = Convert.ToDouble(sensor_val[i] / 100.00);
                    //formsPlot1.Plot.AddDataLogger().Add(Convert.ToDouble(i), Convert.ToDouble(sensor_val[i] / 100.00));
                    //formsPlot1.Plot.Legend();
                }
                formsPlot1.Plot.Clear();
                double[] Chart_value = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                for (int i = 0; i < dataGridView2.ColumnCount; i++)
                {
                    Chart_value[i] = Convert.ToDouble(dataGridView2[i, 0].Value);
                }
                double[] positions = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
                string[] labels = { "Sensor1", "Sensor2", "Sensor3", "Sensor4", "Sensor5", "Sensor6", "Sensor7", "Sensor8", "Sensor9", "Sensor10", "Sensor11", "Sensor12" };
                //formsPlot1.Plot.AddBar(Chart_value, positions);
                formsPlot1.Plot.AddBar(Chart_value, positions).ShowValuesAboveBars = true;
                formsPlot1.Plot.XTicks(positions, labels);
                //formsPlot1.Plot.SetAxisLimits(yMin: 0);
                formsPlot1.Refresh();
                Thread.Sleep(delay);
            }
            return;
        }

        //private void metroButton2_Click(object sender, EventArgs e)
        //{
        //    if(Read_model == 0)
        //    {
        //        MessageBox.Show("Recording not Started yet");
        //    }
        //    formsPlot1.Plot.Clear();
        //    formsPlot1.Refresh();
        //    dataGridView2.Rows.Clear();
        //    Read_model = 0;
        //}
        private void metroTile2_Click(object sender, EventArgs e)
        {
            SQLiteConnection conn = new SQLiteConnection("Data Source = Wireless_Temp.db;Version=3;");
            conn.Open();
            string sql = "select * from sqlite_master WHERE type = 'table' order by name";
            SQLiteCommand command = new SQLiteCommand(sql, conn);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                comboBox1.Items.Add(reader["name"].ToString());
            }
            conn.Close();
        }

        private void metroButton3_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem == null)
            {
                MessageBox.Show("Loading Error, please check!");
                return;
            }
            dataGridView1.DataSource = null;
            //chart1.Series.Clear();
            formsPlot2.Plot.Clear();

            SQLiteConnection conn = new SQLiteConnection("Data Source = Wireless_Temp.db;Version=3;");
            conn.Open();
            string ob = comboBox1.SelectedItem.ToString();
            SQLiteDataAdapter mAdapter = new SQLiteDataAdapter($"select * from {ob}", conn);
            DataTable dt = new DataTable();
            mAdapter.Fill(dt);
            //dt.DefaultView.RowFilter = "id >= 100 and id <= 300";
            dataGridView1.DataSource = dt;
            conn.Close();

            int len = dataGridView1.ColumnCount - 2;
            formsPlot2.Plot.XLabel("Time");
            formsPlot2.Plot.YLabel("Temperature");
            double[] x_value = new double[dataGridView1.RowCount];
            for (int i = 0; i < dataGridView1.RowCount; i++)
            {
                x_value[i] = Convert.ToDouble(dataGridView1[0, i].Value);
            }
            string[] x_name = new string[dataGridView1.RowCount];
            for (int i = 0; i < dataGridView1.RowCount; i++)
            {
                x_name[i] = Convert.ToString(dataGridView1[1, i].Value);
            }
            formsPlot2.Plot.XAxis.ManualTickPositions(x_value, x_name);
            for (int n = 0; n < len; n++)
            {
                string label_name = $"Sensor{n + 1}";
                double[] Y_value = new double[dataGridView1.RowCount];
                for (int i = 0; i < dataGridView1.RowCount; i++)
                {
                    Y_value[i] = Convert.ToDouble(dataGridView1[n + 2, i].Value);
                }
                //formsPlot2.Plot.YAxis.(Y_value);
                formsPlot2.Plot.AddScatter(x_value, Y_value, label: label_name);
                formsPlot2.Plot.Legend();
            }
            formsPlot2.Refresh();

        }

        private void metroButton4_Click(object sender, EventArgs e)
        {
            if (!modbusClient.Connected)
            {
                MessageBox.Show("Please build the connection first!");
                return;
            }
            int address = Convert.ToInt32(textBox3.Text);
            int value = Convert.ToInt32(textBox4.Text);
            modbusClient.WriteMultipleRegisters(address, new int[] { value });
        }
    }
}
