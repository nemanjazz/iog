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
        private IDictionary<String, TypeVisualUnit> units;
        private string rootTypeName;

        public TypeVisualUnit CurrentType
        {
            get;
            private set;
        }

        public ChooseTypeForm(IDictionary<String, TypeVisualUnit> units, string rootTypeName)
        {
            InitializeComponent();
            this.rootTypeName = rootTypeName;
            this.units = new Dictionary<String, TypeVisualUnit>(units);
            TypeVisualUtilities.GetChildrenAndParentsDictonaryOfTypes(this.units, out childrenDictionary, out parentsDictionary);
            showRootType();
        }

        private void showRootType()
        {
            
            changeCurrentTypeAndRefreshLists(units[rootTypeName]);
            listBoxChildren.Focus();
        }

        private void changeCurrentTypeAndRefreshLists(TypeVisualUnit currentType)
        {
            CurrentType = currentType;
            tbCurrentType.Text = CurrentType.ToString();
            fillListBoxes();
        }

        private void fillListBoxes()
        {
            var listChildren = new List<TypeVisualUnit>(childrenDictionary[CurrentType]);
            var listParents = new List<TypeVisualUnit>(parentsDictionary[CurrentType]);
            listChildren.Sort();
            listParents.Sort();
            listBoxChildren.DataSource = listChildren;
            listBoxChildren.DisplayMember = "Name";
            listBoxParents.DataSource = listParents;
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
            changeCurrentTypeAndRefreshLists((TypeVisualUnit)sender.Items[selectedIndex]);
            sender.Focus();
        }

        private void btnBackToRoot_Click(object sender, EventArgs e)
        {
            showRootType();
        }

        private void btnSearchType_Click(object sender, EventArgs e)
        {
            var searchDialog = new SearchTypeDialog(units);
            if (searchDialog.ShowDialog() == DialogResult.OK)
            {
                changeCurrentTypeAndRefreshLists(searchDialog.ResultType);
            }
        }

    }
}
