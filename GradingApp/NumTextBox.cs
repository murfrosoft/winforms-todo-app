using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GradingApp
{
    class NumTextBox : TextBox
    {
        // try to parse input text value as integer
        public bool GetValue( out int value )
        {
            if( int.TryParse( this.Text , out value ) )
            {
                return true;
            }
            else
            {
                this.Text = "";
                return false;
            }
        }
    }
}
