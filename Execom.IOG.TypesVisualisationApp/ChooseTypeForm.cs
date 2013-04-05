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
    public partial class ChooseTypeForm : Form
    {
        private IDictionary<TypeVisualUnit, ICollection<TypeVisualUnit>> childrenDictionary;
        private IDictionary<TypeVisualUnit, ICollection<TypeVisualUnit>> parentsDictionary;
        private IList<TypeVisualUnit> units;

        public TypeVisualUnit CurrentType
        {
            get;
            private set;
        }

        public ChooseTypeForm(ICollection<TypeVisualUnit> units)
        {
            InitializeComponent();
            this.units = new List<TypeVisualUnit>(units);
            TypeVisualUtilities.GetChildrenAndParentsDictonaryOfTypes(this.units, out childrenDictionary, out parentsDictionary);
            initialiseListBoxes(this.units);
        }

        private void initialiseListBoxes(IList<TypeVisualUnit> units)
        {
            listBoxParents.Items.Clear();
            listBoxChildren.Items.Clear();
            CurrentType = units[units.Count - 1];
            tbCurrentType.Text = CurrentType.ToString();
            fillListBoxes();
            listBoxChildren.Focus();
        }

        private void fillListBoxes()
        {
            listBoxChildren.DataSource = childrenDictionary[CurrentType];
            listBoxChildren.DisplayMember = "Name";
            listBoxParents.DataSource = parentsDictionary[CurrentType];
            listBoxParents.DisplayMember = "Name";
            listBoxChildren.SelectedIndex = -1;
            listBoxParents.SelectedIndex = -1;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (CurrentType != null)
                this.DialogResult = DialogResult.OK;
            else
                MessageBox.Show("Please select a type");
        }

        private void listboxTypes_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ListBox senderListBox = (ListBox)sender;
            int index = senderListBox.IndexFromPoint(e.Location);
            if (index != System.Windows.Forms.ListBox.NoMatches && index == senderListBox.SelectedIndex)
            {
                chooseType(index, senderListBox);
            }
        }

        private void listboxTypes_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ListBox senderListBox = (ListBox)sender;
                if (senderListBox.SelectedIndex != -1)
                    chooseType(senderListBox.SelectedIndex, senderListBox);
            }
        }

        private void chooseType(int selectedIndex, ListBox sender)
        {
            CurrentType = (TypeVisualUnit) sender.Items[selectedIndex];
            tbCurrentType.Text = CurrentType.ToString();
            fillListBoxes();
            sender.Focus();
        }



    }
}
