using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Wireless_Temp
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }
        private void metroEllipse2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void metroEllipse1_Click(object sender, EventArgs e)
        {
            string pass = textBox2.Text;
            if (pass == "123456")
            {
                MessageBox.Show("Login Success!");
                Form1.checkBox4.Checked = true;
                this.Close();
            }
            else
            {
                MessageBox.Show("Wrong Password");
            }
        }
    }
}
