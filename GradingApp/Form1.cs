using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SQLite;

namespace GradingApp
{
    public partial class Form1 : Form
    {
        static SQLiteConnection dbConnection;
        private enum Sort { ID, Short, Long, CourseID, Done, Priority };
        private Sort sort;
        private int goal;

        private class Item
        {
            public string Name;
            public int Value;
            public int Finished;

            public Item(int value, string name, int finished)
            {
                Name = name;
                Value = value;
                Finished = finished;
            }
            public override string ToString()
            {
                // Generates the text shown in the combo box
                return Name;
            }
        }

        public Form1()
        {
            InitializeComponent();

            sort = Sort.ID;
            goal = 100;

            dbConnection = new SQLiteConnection("Data Source=GradeDatabase.db;Version=3;");
            dbConnection.Open();

            // Test for grades table
            try
            {
                string sql = "select * from grades";
                SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
                SQLiteDataReader reader = command.ExecuteReader();
            }
            catch
            {
                string sql = GradeItem.MakeTableText();
                SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
                command.ExecuteNonQuery();
            }            

            label7.Text = "";

            RefreshDisplay();
        }

        private void RefreshDisplay()
        {
            string sql;
            switch( sort )
            {
                case Sort.ID:
                    sql = "select * from grades order by id";
                    break;
                case Sort.Short:
                    sql = "select * from grades order by done, (count-finished)*length";
                    break;
                case Sort.Long:
                    sql = "select * from grades order by done, (count-finished)*length desc";
                    break;
                case Sort.CourseID:
                    sql = "select * from grades order by done, class_id, (count-finished)*length";
                    break;
                case Sort.Done:
                    sql = "select * from grades order by done";
                    break;
                case Sort.Priority:
                    sql = "select * from grades order by done, priority, (count-finished)*length";
                    break;
                default:
                    sql = "select * from grades order by id";
                    break;
            }
            DisplayQuery(sql);
        }

        private void DisplayQuery(string query)
        {
            string sql = query;
            SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();

            int totalLength = 0;
            int totalLengthLeft = 0;

            richTextBox1.Clear();
            comboBox1.Items.Clear();
            comboBox2.Items.Clear();
            switch(sort)
            {
                case Sort.Short:
                case Sort.Long:
                    richTextBox1.SelectionColor = Color.Gray;
                    richTextBox1.AppendText($"[{"ID",2}] {"PRI",-3} {"CLASS ID",-10} {"DESCRIPTION",-50} {"WORKLOAD",-8}  ");
                    richTextBox1.SelectionColor = Color.Black;
                    richTextBox1.AppendText($"{"TIME",-8}\n");
                    break;
                case Sort.CourseID:
                    richTextBox1.SelectionColor = Color.Gray;
                    richTextBox1.AppendText($"[{"ID",2}] {"PRI",-3} ");
                    richTextBox1.SelectionColor = Color.Black;
                    richTextBox1.AppendText($"{"CLASS ID",-10} ");
                    richTextBox1.SelectionColor = Color.Gray;
                    richTextBox1.AppendText($"{"DESCRIPTION",-50} {"WORKLOAD",-8}  {"TIME",-8}\n");
                    break;
                case Sort.Priority:
                    richTextBox1.SelectionColor = Color.Gray;
                    richTextBox1.AppendText($"[{"ID",2}] ");
                    richTextBox1.SelectionColor = Color.Black;
                    richTextBox1.AppendText($"{"PRI",-3} ");
                    richTextBox1.SelectionColor = Color.Gray;
                    richTextBox1.AppendText($"{"CLASS ID",-10} {"DESCRIPTION",-50} {"WORKLOAD",-8}  {"TIME",-8}\n");
                    break;
                default:
                    richTextBox1.SelectionColor = Color.Black;
                    richTextBox1.AppendText($"[{"ID",2}] ");
                    richTextBox1.SelectionColor = Color.Gray;
                    richTextBox1.AppendText($"{"PRI",-3} {"CLASS ID",-10} {"DESCRIPTION",-50} {"WORKLOAD",-8}  {"TIME",-8}\n");
                    break;
            }
            while (reader.Read())
            {
                int id, count, finished, length, priority, iDone;
                string classID, assignment;

                try
                {
                    id = (int)reader.GetInt64(0);
                    classID = reader.GetString(1);
                    assignment = reader.GetString(2);
                    count = (int)reader.GetInt64(3);
                    finished = (int)reader.GetInt64(4);
                    length = (int)reader.GetInt64(5);
                    priority = (int)reader.GetInt64(6);
                    iDone = (int)reader.GetInt64(7);
                }
                catch ( Exception ex )
                {
                    MessageBox.Show(ex.Message + "\nPerhaps it is time to refresh data base?", "Database access error");
                    return;
                }

                bool done = iDone == 1 ? true : false;
                int timeRemaining = (count - finished) * length;

                totalLength += count * length;
                if (!done)
                {
                    totalLengthLeft += timeRemaining;
                    Item i = new Item(id, $"{classID} - {assignment}", finished);
                    comboBox1.Items.Add(i);
                }

                Item j = new Item(id, $"{classID} - {assignment}", finished);
                comboBox2.Items.Add(j);

                if (done)
                {
                    richTextBox1.SelectionColor = Color.LightGray;
                    richTextBox1.AppendText($"[{id,2}] {priority,-3} {classID,-10} {assignment,-50} {finished,-2}/{count,2}   {timeRemaining,4} min\n");
                }
                else
                {
                    richTextBox1.SelectionColor = richTextBox1.ForeColor;
                    richTextBox1.AppendText($"[{id,2}] ");
                    richTextBox1.SelectionColor = GetColorByPriority(priority);
                    richTextBox1.AppendText($"{priority,-3} {classID,-10} {assignment,-50} ");
                    richTextBox1.SelectionColor = richTextBox1.ForeColor;
                    richTextBox1.AppendText($"{finished,-2}/{count,2}   {timeRemaining,4} min\n");
                }
                
            }

            // Total Progress Bar
            progressBar1.Maximum = totalLength;
            progressBar1.Value = totalLength - totalLengthLeft;
            label7.Text = $"{(totalLength - totalLengthLeft) / (double)totalLength:P1}";

            // Goal Progress Bar
            progressBar2.Maximum = goal * totalLength / 100;
            progressBar2.Value = (totalLength - totalLengthLeft) <= (goal * totalLength / 100) ? (totalLength - totalLengthLeft) : goal * totalLength / 100;
            label1.Text = $"Goal: {(totalLength - totalLengthLeft) / (double)(goal * totalLength / 100.0):P1}";

            richTextBox1.SelectionColor = Color.Gray;
            richTextBox1.AppendText($"\n{totalLengthLeft} minutes / {totalLengthLeft / 60.0:F} hours remain");
        }

        private Color GetColorByPriority( int priority )
        {
            switch(priority)
            {
                case 1:
                    return Color.Red;
                case 2:
                    return Color.DarkOrange;
                case 3:
                    return Color.LimeGreen;
                case 4:
                    return Color.DodgerBlue;
                case 5:
                    return Color.MediumBlue;
                default:
                    return Color.Black;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            dbConnection.Close();
        }        

        // UPDATE (COMPLETE)
        private void button3_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem == null)
            {
                MessageBox.Show("Please select item to complete");
                return;
            }

            Item itm = (Item)comboBox1.SelectedItem;
            int id = itm.Value;

            // delete syntax
            string sql = $"UPDATE grades SET done = 1 WHERE id = {id}";
            SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
            command.ExecuteNonQuery();

            RefreshDisplay();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (comboBox2.SelectedItem == null)
            {
                MessageBox.Show("Please select item to delete");
                return;
            }

            Item itm = (Item)comboBox2.SelectedItem;
            int id = itm.Value;

            // delete syntax
            string sql = $"DELETE FROM grades WHERE id = {id}";
            SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
            command.ExecuteNonQuery();

            RefreshDisplay();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Item itm = (Item)comboBox1.SelectedItem;
            //MessageBox.Show($"Selected grade with index {itm.Value}");
            numTextBox4.Text = itm.Finished.ToString();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem == null)
            {
                MessageBox.Show("Please select item to update");
                return;
            }

            int finished;
            if (!numTextBox4.GetValue(out finished))
            {
                MessageBox.Show("Please enter number of finished tasks");
                numTextBox4.Focus();
                return;
            }

            Item itm = (Item)comboBox1.SelectedItem;
            int id = itm.Value;

            string sql = $"UPDATE grades SET finished = {finished} WHERE id = {id}";
            SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
            command.ExecuteNonQuery();

            RefreshDisplay();
        }

        #region Sorting
        private void byShortestToolStripMenuItem_Click(object sender, EventArgs e)
        {
            sort = Sort.Short;
            RefreshDisplay();
        }

        private void byIDToolStripMenuItem_Click(object sender, EventArgs e)
        {
            sort = Sort.ID;
            RefreshDisplay();
        }

        private void byLongestToolStripMenuItem_Click(object sender, EventArgs e)
        {
            sort = Sort.Long;
            RefreshDisplay();
        }

        private void byCourseIDToolStripMenuItem_Click(object sender, EventArgs e)
        {
            sort = Sort.CourseID;
            RefreshDisplay();
        }

        private void byCompleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            sort = Sort.Done;
            RefreshDisplay();
        }

        private void byPriorityToolStripMenuItem_Click(object sender, EventArgs e)
        {
            sort = Sort.Priority;
            RefreshDisplay();
        }
        #endregion

        private void AddItemClick(object sender, EventArgs e)
        {
            GradeItemDialog gid = new GradeItemDialog();
            if (gid.ShowDialog() == DialogResult.OK)
            {
                GradeItem i = gid.Item;

                string sql = i.SQLiteInsert();
                SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
                command.ExecuteNonQuery();

                RefreshDisplay();
            }
            else
            {
                //MessageBox.Show("Task Add Canceled");
            }
        }


        private void UpdateGoalClick(object sender, EventArgs e)
        {
            GoalDialog gd = new GoalDialog();
            if( gd.ShowDialog() == DialogResult.OK )
            {
                goal = gd.Goal;
            }
            RefreshDisplay();
        }

        private void NewDatabaseClick(object sender, EventArgs e)
        {
            if( MessageBox.Show("Would you like to drop all tables and rebuild the database?", "Drop Tables", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                if(MessageBox.Show("Are you REALLY sure you want to drop all tables and rebuild the database?", "Drop Tables", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    string sql;
                    SQLiteCommand command;
                    sql = "DROP TABLE IF EXISTS grades";
                    command = new SQLiteCommand(sql, dbConnection);
                    command.ExecuteNonQuery();

                    sql = GradeItem.MakeTableText();
                    richTextBox1.Text = sql;
                    command = new SQLiteCommand(sql, dbConnection);
                    command.ExecuteNonQuery();

                    MessageBox.Show("done.");
                }
            }
        }

        private void AboutClick(object sender, EventArgs e)
        {
            MessageBox.Show("Created by Murphy as a procrastination tactic.", "About");
        }

        private void NotDoneClick(object sender, EventArgs e)
        {
            if (comboBox2.SelectedItem == null)
            {
                MessageBox.Show("Please select item to un-complete");
                return;
            }

            Item itm = (Item)comboBox2.SelectedItem;
            int id = itm.Value;

            // delete syntax
            string sql = $"UPDATE grades SET done = 0 WHERE id = {id}";
            SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
            command.ExecuteNonQuery();

            RefreshDisplay();
        }

    }
}
