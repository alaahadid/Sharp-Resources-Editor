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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Globalization;
using System.ComponentModel.Design;
using System.Resources;
using System.Reflection;
using RavSoft;
namespace SharpResourcesEditor
{
    public partial class Form_Main : Form
    {
        public Form_Main()
        {
            InitializeComponent();
            this.Text = "Sharp Resources Editor v" + Assembly.GetExecutingAssembly().GetName().Version.ToString();
            settings.Reload();
            // Load files
            currentResourcesFolderPath = settings.LatestFolder;
            ReloadIDS(currentResourcesFolderPath);
            // Load settings
            this.Location = settings.Win_Location;
            this.Size = settings.Win_size;
            // Load spell checker
            checker = new SpellChecker("Dictionaries");
            RefreshDictionaries();
        }
        private string[] files;
        private ResXResourceReader currentResource;
        private string currentFilePath = "";
        private string currentResourcesFolderPath = "";
        private Properties.Settings settings = new Properties.Settings();
        private string targetLanguage = "English";
        private string sourceLanguage = "English";
        private bool isTranslating = false;
        private bool stopTranslating = false;
        private SpellChecker checker;

        private void ReloadIDS(string folder)
        {
            files = Directory.GetFiles(folder, "*.resx", SearchOption.AllDirectories);
            // Load filters
            toolStripComboBox1.Items.Clear();
            toolStripComboBox1.Items.Add("All");
            toolStripComboBox1.Items.Add("No ID");
            foreach (string file in files)
            {
                string[] ids = Path.GetFileNameWithoutExtension(file).Split(new char[] { '.' });
                if (ids.Length > 1)
                {
                    // the last element is the id
                    if (!toolStripComboBox1.Items.Contains(ids[ids.Length - 1]))
                        toolStripComboBox1.Items.Add(ids[ids.Length - 1]);
                }
            }
            toolStripComboBox1.SelectedIndex = 0;
        }
        private void RefreshFiles(string folder)
        {
            currentResourcesFolderPath = folder;
            folder_path.Text = folder;
            folder_path.ToolTipText = folder;

            currentResource = null;
            pathLabel.Text = "";
            dataGridView1.Rows.Clear();
            treeView1.Nodes.Clear();
            // Load files
            string[] files = Directory.GetFiles(currentResourcesFolderPath, "*.resx", SearchOption.AllDirectories);
            foreach (string file in files)
            {
                if (toolStripComboBox1.SelectedItem.ToString() == "All")
                {
                    TreeNode tr = new TreeNode();
                    tr.Text = Path.GetFileNameWithoutExtension(file);
                    tr.Tag = file;

                    treeView1.Nodes.Add(tr);
                }
                else if (toolStripComboBox1.SelectedItem.ToString() == "No ID")
                {
                    if (!Path.GetFileNameWithoutExtension(file).Contains('.'))
                    {
                        TreeNode tr = new TreeNode();
                        tr.Text = Path.GetFileNameWithoutExtension(file);
                        tr.Tag = file;

                        treeView1.Nodes.Add(tr);
                    }
                }
                else// Filtering ... 
                {
                    if (Path.GetFileNameWithoutExtension(file).Contains(toolStripComboBox1.SelectedItem.ToString()))
                    {
                        TreeNode tr = new TreeNode();
                        tr.Text = Path.GetFileNameWithoutExtension(file);
                        tr.Tag = file;

                        treeView1.Nodes.Add(tr);
                    }
                }
            }
        }
        private void OpenSelectedResource()
        {
            pathLabel.Text = "";
            dataGridView1.Rows.Clear();
            if (treeView1.SelectedNode == null)
            {
                currentResource = null;
                return;
            }
            currentFilePath = (string)treeView1.SelectedNode.Tag;
            pathLabel.Text = currentFilePath;
            try
            {
                // Read it
                currentResource = new ResXResourceReader(currentFilePath);
                IDictionaryEnumerator dict = currentResource.GetEnumerator();
                if (!Path.GetFileName(currentFilePath).Contains("Resource"))
                {
                    while (dict.MoveNext())
                    {
                        if (dict.Key.ToString().EndsWith(".Text") | dict.Key.ToString().EndsWith(".ToolTip") | dict.Key.ToString().EndsWith(".ToolTipText"))
                            dataGridView1.Rows.Add(dict.Key, dict.Value, dict.Value);
                    }
                }
                else
                {
                    while (dict.MoveNext())
                    {
                        dataGridView1.Rows.Add(dict.Key, dict.Value, dict.Value);
                    }
                }
                dict.Reset();// reset to write later
            }
            catch { }
        }
        private bool GetValue(string key, out string value)
        {
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                if (dataGridView1.Rows[i].Cells[0].Value.ToString() == key)
                {
                    string val = dataGridView1.Rows[i].Cells[2].Value.ToString();
                    if (val != null)
                    { value = val; }
                    else
                    { value = ""; }
                    return true;
                }
            }
            value = null;
            return false;
        }
        private void TranslateProgress()
        {
            treeView1.Enabled = dataGridView1.Enabled = toolStrip1.Enabled = toolStrip2.Enabled = toolStrip3.Enabled = false;

            toolStripProgressBar1.Visible = true;
            toolStripProgressBar1.Maximum = 100;
            Status_Label.Text = "Translating ....";
            Status_Label.IsLink = true;
            isTranslating = true; stopTranslating = false;
            Translator translator = new Translator();
            translator.TargetLanguage = targetLanguage;
            translator.SourceLanguage = sourceLanguage;

            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                if (stopTranslating)
                {
                    isTranslating = false;
                    toolStripProgressBar1.Visible = false;
                    treeView1.Enabled = dataGridView1.Enabled = toolStrip1.Enabled = toolStrip2.Enabled = toolStrip3.Enabled = true;
                    Status_Label.IsLink = false;
                    Status_Label.Text = "Translation canceled by user.";
                    return;
                }
                string translation = "";
                bool translated = true;
                if (dataGridView1.Rows[i].Cells[1].Value == null)
                    continue;
                string[] lines = dataGridView1.Rows[i].Cells[1].Value.ToString().Split('\n');
                foreach (string line in lines)
                {
                    translator.SourceText = line;
                    try
                    {
                        translator.Translate();
                    }
                    catch { translated = false; }
                    translation += translator.Translation + "\n";
                }
                if (translated && translation.Length > 0)
                {
                    translation = translation.Substring(0, translation.Length - 1);
                    //  TranslateSubItem(i, translation);
                    dataGridView1.Rows[i].Cells[2].Value = translation;
                }
                Application.DoEvents();
                int x = (i * 100) / dataGridView1.Rows.Count;
                toolStripProgressBar1.Value = x;
            }
            toolStripProgressBar1.Visible = false;
            isTranslating = false;
            Status_Label.IsLink = false;
            Status_Label.Text = "Done.";
            treeView1.Enabled = dataGridView1.Enabled = toolStrip1.Enabled = toolStrip2.Enabled = toolStrip3.Enabled = true;
        }
        private void RefreshDictionaries()
        {
            dictionaryToUseToolStripMenuItem.DropDownItems.Clear();
            foreach (SpellCheckerDictionary dic in checker.Dictionaries)
            {
                ToolStripMenuItem item = new ToolStripMenuItem(dic.Name + " (" + dic.NativeName + ")");
                item.Tag = dic;
                dictionaryToUseToolStripMenuItem.DropDownItems.Add(item);
            }
            if (dictionaryToUseToolStripMenuItem.DropDownItems.Count > 0)
                ((ToolStripMenuItem)dictionaryToUseToolStripMenuItem.DropDownItems[0]).Checked = true;
        }
        // Save
        private void button1_Click(object sender, EventArgs e)
        {
            if (currentResource == null)
                return;
            // Create writer
            ResXResourceWriter writer = new ResXResourceWriter(currentFilePath + "_temp");
            // Add resources
            IDictionaryEnumerator dict = currentResource.GetEnumerator();
            dict.Reset();
            while (dict.MoveNext())
            {
                string val = "";
                if (GetValue(dict.Key.ToString(), out val))
                {
                    writer.AddResource(dict.Key.ToString(), val);
                }
                else
                {
                    writer.AddResource(dict.Key.ToString(), dict.Value);
                }
            }
            currentResource.Close();
            writer.Close();
            // Delete original
            File.Delete(currentFilePath);
            // Copy temp
            File.Copy(currentFilePath + "_temp", currentFilePath);
            // Delete temp
            File.Delete(currentFilePath + "_temp");
            // Refresh to see results
            OpenSelectedResource();
        }
        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            OpenSelectedResource();
        }
        // Reload
        private void button2_Click(object sender, EventArgs e)
        {
            if (currentResource != null)
            {
                currentResource.Close();
            }
            OpenSelectedResource();
        }
        private void toolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshFiles(currentResourcesFolderPath);
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            settings.Win_Location = this.Location;
            settings.Win_size = this.Size;
            settings.LatestFolder = currentResourcesFolderPath;
            settings.Save();
        }
        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            Form_About frm = new Form_About();
            frm.ShowDialog(this);
        }
        private void dataGridView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == (Keys.Control | Keys.S))
            {
                //Save
                button1_Click(this, null);
            }
            if (e.KeyData == (Keys.Control | Keys.R))
            {
                //Reload
                button2_Click(this, null);
            }
        }
        // Translate All
        private void translateallToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form_TranslateLanguageFromTo frm = new Form_TranslateLanguageFromTo();
            frm.Text = "Google Translate All";
            if (frm.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
            {
                sourceLanguage = frm.LanguageFrom;
                targetLanguage = frm.LanguageTo;
                TranslateProgress();
            }
        }
        // Translate Selection
        private void translateselectionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedCells.Count != 1)
            {
                MessageBox.Show("Please select one row first.");
                return;
            }
            // Select the parent row
            dataGridView1.Rows[dataGridView1.SelectedCells[0].RowIndex].Selected = true;
            // Check the value
            if (dataGridView1.SelectedRows[0].Cells[1].Value == null)
            {
                MessageBox.Show("Can't be translated, empty field !");
                return;
            }
            Form_TranslateLanguageFromTo frm = new Form_TranslateLanguageFromTo();
            frm.Text = "Google Translate Selected Row";
            if (frm.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
            {
                sourceLanguage = frm.LanguageFrom;
                targetLanguage = frm.LanguageTo;

                Status_Label.Text = "Translating ...";
                statusStrip1.Refresh();

                Translator translator = new Translator();
                translator.TargetLanguage = frm.LanguageFrom;
                translator.SourceLanguage = frm.LanguageTo;
                string translation = "";
                bool translated = true;
                string[] lines = dataGridView1.SelectedRows[0].Cells[1].Value.ToString().Split('\n');
                foreach (string line in lines)
                {
                    translator.SourceText = line;
                    try
                    {
                        translator.Translate();
                    }
                    catch { translated = false; }
                    translation += translator.Translation + "\n";
                }
                if (translated && translation.Length > 0)
                {
                    translation = translation.Substring(0, translation.Length - 1);
                    //  TranslateSubItem(i, translation);
                    dataGridView1.SelectedRows[0].Cells[2].Value = translation;
                }
                Status_Label.Text = "Done.";
            }
        }
        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fol = new FolderBrowserDialog();
            fol.Description = "Open the folder that contain the resources files (.resx files)";
            fol.SelectedPath = currentResourcesFolderPath;
            if (fol.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
            {
                ReloadIDS(fol.SelectedPath);
                RefreshFiles(fol.SelectedPath);
            }
        }
        private void Status_Label_Click(object sender, EventArgs e)
        {
            if (isTranslating)
            {
                stopTranslating = true;
                Status_Label.Text = "Canceling ...";
            }
        }
        // download dictionaries
        private void downloadDictionariesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form_DictionariesDownload down = new Form_DictionariesDownload();
            down.ShowDialog(this);

            checker.RefreshDictionaries("Dictionaries");
            RefreshDictionaries();
        }
        // Spell check all
        private void spellCheckAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridView1.Rows.Count == 0)
            {
                MessageBox.Show("No row to check !?");
                return;
            }
            // Check out the dictionary
            SpellCheckerDictionary dic = null;
            foreach (ToolStripMenuItem item in dictionaryToUseToolStripMenuItem.DropDownItems)
            {
                if (item.Checked)
                {
                    dic = (SpellCheckerDictionary)item.Tag;
                    break;
                }
            }
            if (dic == null)
            {
                MessageBox.Show("There is no dictionary selected !\nPlease select a dictionary to use from the 'Spell Check>Dictionary to' use menu. You can download dictionaries as well via 'Spell Check>Download Dictionaries'");
                return;
            }

            Cursor = Cursors.WaitCursor;
            Status_Label.Text = "Spell Checking all, please wait ....";
            statusStrip1.Refresh();
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                // Check the value
                if (dataGridView1.Rows[i].Cells[2].Value == null)
                {
                    continue;
                }
                string text = dataGridView1.Rows[i].Cells[2].Value.ToString();
                // Do it
                Form_SpellCheck frm = new Form_SpellCheck(text, dic);
                try
                {
                    frm.ShowDialog(this);
                }
                catch
                {
                }
                Form_SpellCheck.CheckResult result = frm.ResultOfCheck;
                if ((result & Form_SpellCheck.CheckResult.Ok) == Form_SpellCheck.CheckResult.Ok)
                {
                    // It is ok ! update the value.
                    dataGridView1.Rows[i].Cells[2].Value = frm.TheTextAfterCheck;

                    dataGridView1.FirstDisplayedScrollingRowIndex = i;
                    dataGridView1.Refresh();
                }
                else if ((result & Form_SpellCheck.CheckResult.Abort) == Form_SpellCheck.CheckResult.Abort)
                {
                    break;
                }
            }
            Cursor = Cursors.Default;
            Status_Label.Text = "Spell Checking All done.";
            statusStrip1.Refresh();
            dic.Save();
            MessageBox.Show("Spell Check all finished.");
        }
        // Spell check selected row
        private void spellCheckSelectedRowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedCells.Count != 1)
            {
                MessageBox.Show("Please select one row first.");
                return;
            }
            // Check out the dictionary
            SpellCheckerDictionary dic = null;
            foreach (ToolStripMenuItem item in dictionaryToUseToolStripMenuItem.DropDownItems)
            {
                if (item.Checked)
                {
                    dic = (SpellCheckerDictionary)item.Tag;
                    break;
                }
            }
            if (dic == null)
            {
                MessageBox.Show("There is no dictionary selected !\nPlease select a dictionary to use from the 'Spell Check>Dictionary to' use menu. You can download dictionaries as well via 'Spell Check>Download Dictionaries'");
                return;
            }
            // Select the parent row
            dataGridView1.Rows[dataGridView1.SelectedCells[0].RowIndex].Selected = true;
            // Check the value
            if (dataGridView1.SelectedRows[0].Cells[2].Value == null)
            {
                MessageBox.Show("Can't be spell checked, empty field !");
                return;
            }
            string text = dataGridView1.SelectedRows[0].Cells[2].Value.ToString();
            // Do it
            Form_SpellCheck frm = new Form_SpellCheck(text, dic);
            try
            {
                frm.ShowDialog(this);
            }
            catch
            {
            }
            Form_SpellCheck.CheckResult result = frm.ResultOfCheck;
            if ((result & Form_SpellCheck.CheckResult.Ok) == Form_SpellCheck.CheckResult.Ok)
            {
                // It is ok ! update the value.
                dataGridView1.SelectedRows[0].Cells[2].Value = frm.TheTextAfterCheck;
            }
            dic.Save();
            MessageBox.Show("Spell Check finished.");
        }
        private void dictionaryToUseToolStripMenuItem_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            foreach (ToolStripMenuItem item in dictionaryToUseToolStripMenuItem.DropDownItems)
                item.Checked = false;

            ((ToolStripMenuItem)e.ClickedItem).Checked = true;
        }
    }
}
