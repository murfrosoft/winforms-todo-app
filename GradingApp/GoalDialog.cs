using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GradingApp
{
    public partial class GoalDialog : Form
    {
        public int Goal { get; set; }

        public GoalDialog()
        {
            InitializeComponent();
            this.DialogResult = DialogResult.Cancel;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // Cancel option
            this.DialogResult = DialogResult.Cancel;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // submit
            int goal;
            if( numTextBox1.GetValue(out goal) )
            {
                this.Goal = goal;
                this.DialogResult = DialogResult.OK;
            }
            else
            {
                MessageBox.Show("Please enter an integer value.");
                numTextBox1.Focus();
            }
        }
    }
}
