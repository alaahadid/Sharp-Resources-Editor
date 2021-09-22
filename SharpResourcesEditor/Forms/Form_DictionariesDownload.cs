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
using System.Net;
using System.IO;
using SevenZip;

namespace SharpResourcesEditor
{
    public partial class Form_DictionariesDownload : Form
    {
        public Form_DictionariesDownload()
        {
            InitializeComponent();
            //load list
            System.IO.Directory.CreateDirectory("Dictionaries");
            checker = new SpellChecker(".\\Dictionaries");

            foreach (DictionaryLink link in checker.AvailableToDownload)
            {
                ListViewItem item = new ListViewItem();
                item.Text = link.Name;
                item.SubItems.Add(link.NativeName);
                item.SubItems.Add(link.Description);

                listView1.Items.Add(item);
            }
        }
        private WebClient client = new WebClient();
        private SpellChecker checker;
        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.Items)
                item.Checked = true;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.Items)
                item.Checked = false;
        }
        //download
        private void button1_Click(object sender, EventArgs e)
        {
            List<int> indiacs = new List<int>();
            foreach (ListViewItem item in listView1.Items)
            {
                if (item.Checked)
                {
                    indiacs.Add(item.Index);
                }
            }
            if (indiacs.Count == 0)
            {
                MessageBox.Show("Please select one dictionary at least.", "Download Dictionaries");
                return;
            }
            label1_status.Visible = true;
            label1_status.Refresh();

            this.Cursor = Cursors.WaitCursor;
            foreach (int index in indiacs)
            {
                try
                {
                    //download
                    client.DownloadFile(checker.AvailableToDownload[index].Link,
                        Path.GetFullPath(".\\Dictionaries\\" + checker.AvailableToDownload[index].Name + ".oxt"));
                    //extract
                    SevenZipExtractor extractor = new SevenZipExtractor(Path.GetFullPath(".\\Dictionaries\\" + checker.AvailableToDownload[index].Name + ".oxt"));
                    Directory.CreateDirectory(Path.GetFullPath(".\\Dictionaries\\" + checker.AvailableToDownload[index].Name + "\\"));
                    extractor.ExtractArchive(Path.GetFullPath(".\\Dictionaries\\" + checker.AvailableToDownload[index].Name + "\\"));
                    //delete the file
                    File.Delete(Path.GetFullPath(".\\Dictionaries\\" + checker.AvailableToDownload[index].Name + ".oxt"));
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Unable to download dictionary (" +
                        checker.AvailableToDownload[index].Name + ")\n\n" + ex.Message + "\n\n" + ex.ToString(),
                        "Download Dictionaries");
                }
            }
            this.Cursor = Cursors.Default;
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }
    }
}
