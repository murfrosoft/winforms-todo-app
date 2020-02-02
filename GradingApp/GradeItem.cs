using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GradingApp
{
    public class GradeItem
    {
        // Properties
        public int ID { get; set; }  // Primary Key

        public string ClassID { get; set; }     // 3401
        public string Assignment { get; set; }  // Proj1
        public int ItemsCount { get; set; }     // how many items total
        public int ItemsFinished { get; set; }  // how many items finished
        public int ItemLength { get; set; }     // length (minutes) to finish 1 item
        public int ItemsPriority { get; set; }  // holds a priority value
        public bool Done { get; set; }          // whether task is finished or not

        public GradeItem(string cid, string assign, int count, int finished, int len, int priority)
        {
            ClassID = cid;
            Assignment = assign;
            ItemsCount = count;
            ItemsFinished = finished;
            ItemLength = len;
            ItemsPriority = priority;
            Done = false;
        }

        public string SQLiteUpdate(int id)
        {
            // generate SQLite Insert command for this item
            string sql = "UPDATE grades " +
                         $"SET class_id = '{ClassID}', " +
                         $"assignment = '{Assignment}', " +
                         $"count = '{ItemsCount}', " +
                         $"finished = '{ItemsFinished}', " +
                         $"length = '{ItemLength}', " +
                         $"priority = '{ItemsPriority}', ";
            sql += Done ? $"done = 1 " : $"done = 0 ";
            sql += $"WHERE id = {id};";

            return sql;
        }

        public string SQLiteInsert()
        {
            // generate SQLite Insert command for this item
            string sql = "insert into grades (class_id, assignment, count, finished, length, priority, done) " +
                         $"values ( '{ClassID}', '{Assignment}', {ItemsCount}, {ItemsFinished}, {ItemLength}, {ItemsPriority}, ";
            sql += Done ? "1)" : "0)";

            return sql;
        }

        static public string MakeTableText()
        {
            string sql = "CREATE TABLE IF NOT EXISTS grades (" +
                         "id INTEGER PRIMARY KEY, " +
                         "class_id TEXT NOT NULL, " +
                         "assignment TEXT NOT NULL, " +
                         "count INTEGER NOT NULL, " +
                         "finished INTEGER NOT NULL, " +
                         "length INTEGER NOT NULL, " +
                         "priority INTEGER NOT NULL," +
                         "done INTEGER NOT NULL);";
            return sql;
        }        
    }
}
