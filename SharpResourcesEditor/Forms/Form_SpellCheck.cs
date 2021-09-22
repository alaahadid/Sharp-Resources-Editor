using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SharpResourcesEditor
{
    public partial class Form_SpellCheck : Form
    {
        public Form_SpellCheck(string textToCheck, SpellCheckerDictionary dic)
        {
            this.dic = dic;
            this.textToCheck = textToCheck;
            InitializeComponent();
            //view the text in the editor
            richTextBox_text.Text = textToCheck;
            // Update
            TheTextAfterCheck = richTextBox_text.Text;
            //get words as array to use in the spell checker
            words = textToCheck.Split(" .,-?!:;\"“”()[]{}|<>/+\r\n¿¡…—".ToCharArray());
            //spell the next word
            wordIndex = 0;
            CheckNextWord();
        }
        private SpellCheckerDictionary dic;
        private string textToCheck;
        private string[] words;
        private int wordIndex = 0;
        private CheckResult result = CheckResult.Abort;
        public string TheTextAfterCheck
        {
            get;
            private set;
        }

        private void CheckNextWord()
        {
            if (wordIndex < words.Length)
            {
                //is this a checkable word
                if (words[wordIndex].Length > 0 &&
                    !words[wordIndex].Contains("0") &&
                    !words[wordIndex].Contains("1") &&
                    !words[wordIndex].Contains("2") &&
                    !words[wordIndex].Contains("3") &&
                    !words[wordIndex].Contains("4") &&
                    !words[wordIndex].Contains("5") &&
                    !words[wordIndex].Contains("6") &&
                    !words[wordIndex].Contains("7") &&
                    !words[wordIndex].Contains("8") &&
                    !words[wordIndex].Contains("9") &&
                    !words[wordIndex].Contains("%") &&
                    !words[wordIndex].Contains("&") &&
                    !words[wordIndex].Contains("@") &&
                    !words[wordIndex].Contains("$") &&
                    !words[wordIndex].Contains("*") &&
                    !words[wordIndex].Contains("=") &&
                    !words[wordIndex].Contains("£") &&
                    !words[wordIndex].Contains("#") &&
                    !words[wordIndex].Contains("_") &&
                    !words[wordIndex].Contains("½") &&
                    !words[wordIndex].Contains("^") &&
                    !words[wordIndex].Contains("£"))
                {
                    //check the word
                    if (dic.Hunspell.Spell(words[wordIndex]))
                    {
                        wordIndex++;
                        CheckNextWord();
                    }
                    //let's see if this word exist in the replacements list to replace directly
                    else if (dic.ReplacementsList.Keys.Contains(words[wordIndex]))
                    {
                        ReplaceWord(words[wordIndex], dic.ReplacementsList[words[wordIndex]]);
                        wordIndex++;
                        CheckNextWord();
                    }
                    else
                    {
                        //show the word
                        textBox_word.Text = words[wordIndex];
                        SelectWord(words[wordIndex]);
                        //get suggestions
                        listBox1.Items.Clear();
                        string[] suggestions = dic.Hunspell.Suggest(words[wordIndex]).ToArray();
                        listBox1.Items.AddRange(suggestions);
                        if (listBox1.Items.Count > 0)
                            listBox1.SelectedIndex = 0;
                    }
                }
                else//ignore the word
                {
                    wordIndex++;
                    CheckNextWord();
                }
            }
            else//end of the text !! abort with ok.
            {
                result |= CheckResult.Ok;
                Close();
            }
        }
        public CheckResult ResultOfCheck
        { get { return result; } }
        public enum CheckResult : int
        {
            Ok = 0x1,
            IgnoredSomeWords = 0x2,
            Abort = 0x4,
            UsedSomeWords = 0x10,
        }
        private void ReplaceWord(string word, string replacement)
        {
            SelectWord(word);
            richTextBox_text.SelectedText = replacement;
            SelectWord(replacement);

            // Update
            TheTextAfterCheck = richTextBox_text.Text;
        }
        public void SelectWord(int startIndex, int length)
        {
            richTextBox_text.Select(startIndex, length);
        }
        public void SelectWord(string word)
        {
            string txt = textToCheck.ToString();

            for (int i = 0; i < txt.Length; i++)
            {
                if (txt.Length - i >= word.Length)
                {
                    if (txt.Substring(i, word.Length) == word)
                    {
                        richTextBox_text.Select(i, word.Length);
                    }
                }
            }
        }
        // Abort
        private void button1_Click(object sender, EventArgs e)
        {
            result = CheckResult.Abort;
            Close();
        }
        //ok, next word, assuming the user fixed the error automatically
        private void button8_Click(object sender, EventArgs e)
        {
            wordIndex++;
            CheckNextWord();
        }
        // Use
        private void button5_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex >= 0)
            {
                ReplaceWord(textBox_word.Text, listBox1.SelectedItem.ToString());
                wordIndex++;
                CheckNextWord();
            }
            else
            {
                MessageBox.Show("Please select a word from the suggestions list first.",
                     "Spell Check");
            }
        }
        // USe always
        private void button6_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex >= 0)
            {
                ReplaceWord(textBox_word.Text, listBox1.SelectedItem.ToString());
                // Update
                TheTextAfterCheck = richTextBox_text.Text;
                wordIndex++;
                CheckNextWord();
            }
            else
            {
                MessageBox.Show("Please select a word from the suggestions list first.",
                       "Spell Check");
            }
        }
        // Ignore
        private void button2_Click(object sender, EventArgs e)
        {
            wordIndex++;
            CheckNextWord();
            result |= CheckResult.IgnoredSomeWords;
        }
        // Ignore always
        private void button3_Click(object sender, EventArgs e)
        {
            //add the word to the ignore list
            dic.IgnoreList.Add(textBox_word.Text);
            //add to hunspell
            dic.Hunspell.Add(textBox_word.Text);
            wordIndex++;
            CheckNextWord();
            result |= CheckResult.IgnoredSomeWords;
        }
        // Double click on the list box to use 
        private void listBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            button5_Click(this, new EventArgs());   //Use
        }
        private void richTextBox_text_TextChanged(object sender, EventArgs e)
        {
            // Update
            TheTextAfterCheck = richTextBox_text.Text;
        }
    }
}
