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
    public partial class GradeItemDialog : Form
    {
        public GradeItem Item { get; set; }

        public GradeItemDialog()
        {
            InitializeComponent();
            this.DialogResult = DialogResult.Cancel;
            numTextBox2.Text = "0";
        }

        private void InsertClick(object sender, EventArgs e)
        {
            // read in and validate values, then add new item to database
            if (textBox1.Text == "")
            {
                MessageBox.Show("Please enter Class ID");
                textBox1.Focus();
                return;
            }
            if (textBox2.Text == "")
            {
                MessageBox.Show("Please enter Assignment");
                textBox2.Focus();
                return;
            }

            int count;
            int finished;
            int length;
            int priority;

            if (!numTextBox1.GetValue(out count))
            {
                MessageBox.Show("Please enter number of items");
                numTextBox1.Focus();
                return;
            }
            if (!numTextBox2.GetValue(out finished))
            {
                MessageBox.Show("Please enter number finished");
                numTextBox2.Focus();
                return;
            }
            if (!numTextBox3.GetValue(out length))
            {
                MessageBox.Show("Please enter task length");
                numTextBox3.Focus();
                return;
            }
            if (!numTextBox4.GetValue(out priority))
            {
                MessageBox.Show("Please enter task priority");
                numTextBox4.Focus();
                return;
            }

            string classId = textBox1.Text;
            string assignment = textBox2.Text;

            bool done = checkBox1.Checked ? true : false;

            Item = new GradeItem(classId, assignment, count, finished, length, priority);
            Item.Done = done;

            this.DialogResult = DialogResult.OK;
        }

        private void CancelClick(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }
    }
}
