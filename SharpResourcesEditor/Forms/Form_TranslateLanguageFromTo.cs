/* This file is part of Sharp Resources Editor
   A program that edit resources of .net project

   Copyright © Ala Ibrahim Hadid 2013 - 2015

   This program is free software: you can redistribute it and/or modify
   it under the terms of the GNU General Public License as published by
   the Free Software Foundation, either version 3 of the License, or
   (at your option) any later version.

   This program is distributed in the hope that it will be useful,
   but WITHOUT ANY WARRANTY; without even the implied warranty of
   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
   GNU General Public License for more details.

   You should have received a copy of the GNU General Public License
   along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SharpResourcesEditor
{
    public partial class Form_TranslateLanguageFromTo : Form
    {
        public Form_TranslateLanguageFromTo()
        {
            InitializeComponent();
            comboBox_from.SelectedItem = "English";
            comboBox_to.SelectedIndex = 0;
        }
        public string LanguageFrom
        { get { return comboBox_from.SelectedItem.ToString(); } }
        public string LanguageTo
        { get { return comboBox_to.SelectedItem.ToString(); } }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            Close();
        }
        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            Close();
        }

        private void comboBox_from_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox_from.SelectedIndex == comboBox_to.SelectedIndex)
                comboBox_to.SelectedIndex = 0;
        }

        private void comboBox_to_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox_from.SelectedIndex == comboBox_to.SelectedIndex)
                comboBox_from.SelectedIndex = 0;
        }
    }
}
