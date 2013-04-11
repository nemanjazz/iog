using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Execom.IOG.TypeVisual;

namespace Execom.IOG.TypesVisualisationApp
{
    public partial class SearchTypeDialog : Form
    {

        IDictionary<string, TypeVisualUnit> units;
        public TypeVisualUnit ResultType
        {
            get;
            private set;
        }

        public SearchTypeDialog(IDictionary<string, TypeVisualUnit> units)
        {
            InitializeComponent();
            lblError.Visible = false;
            this.units = units;
            var source = new AutoCompleteStringCollection();
            string[] stringArray = new string[units.Count];
            units.Keys.CopyTo(stringArray, 0);
            source.AddRange(stringArray);
            tbSearch.AutoCompleteCustomSource = source;
            tbSearch.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            tbSearch.AutoCompleteSource = AutoCompleteSource.CustomSource;
        }

        private void validateSearchAndReturn()
        {
            if (!units.Keys.Contains(tbSearch.Text))
                lblError.Visible = true;
            else
            {
                ResultType = units[tbSearch.Text];
                this.DialogResult = DialogResult.OK;
            }
        }

        private void tbSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                validateSearchAndReturn();
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            validateSearchAndReturn();
        }



        private void tbSearch_TextChanged(object sender, EventArgs e)
        {
            lblError.Visible = false;
        }



        


    }
}
