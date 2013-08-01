using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Execom.IOG.TypesVisualisationApp
{
    public partial class ExceptionDialog : Form
    {
        public ExceptionDialog(String message, Exception ex)
        {
            InitializeComponent();
            lblMessage.Text = message;
            tbException.Text = ex.GetType().ToString() + "\r\n" + ex.Message;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {

        }
    }
}
