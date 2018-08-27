using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace VigenereDecrypter
{
    public partial class Form1 : Form
    {
        
        public static string alpha  = "abcdefghijklmnopqrstuvwxyz";
        

        public Form1()
        {
            InitializeComponent();
            encrypt_button.Enabled = false;
            decrypt_button.Enabled = false;
        }
       
        private void keylengthBtn_Click(object sender, EventArgs e)
        {
            string input = inputTextBox.Text.ToLower().Trim();
            bool check = input.Length == 0 ? false : true ;
            if(check == false)
            {
                MessageBox.Show("Invalid Entry!");
            }
            else
            {
                keyLength(input);

            }

        }

        private void displayData(Dictionary<string, int> alphaFrequency)
        {

            chart1.ChartAreas[0].AxisX.LabelStyle.Interval = 1;

            Series S1 = new Series();
          
            var ordered = alphaFrequency.OrderBy(x => x.Key);

            foreach (var key in ordered)
            {
                S1.Points.AddXY(key.Key.ToString(), key.Value.ToString());
               
            }
            chart1.Series.Add(S1);
            chart1.Series[0].Color = Color.Gray;

           
    
        }

        public void keyLength(string input)
        {
            Dictionary<string, int> alphaFrequency = new Dictionary<string, int>()
            {
                {"a", 0},  {"d", 0}, {"g", 0}, {"j", 0}, {"l", 0}, {"o", 0}, {"r", 0}, {"u", 0}, {"x", 0},
                {"b", 0 }, {"e",0}, {"h",0}, {"k", 0}, {"m", 0}, {"p", 0}, {"s", 0}, {"v", 0 }, {"y", 0},
                {"c", 0 },  {"f", 0}, {"i", 0}, {"n", 0}, {"q", 0}, {"t", 0}, {"w",0}, {"z", 0}
            };

            List<int[]> list = new List<int[]>();

            string copy = input;
            int tmp = 0; // temporary count of coincidences
            int CI = 0; // largest count of coincidences
            int shift = 0; // shift with the largest CI
            int iter = 1; // temporal displacement

                while (iter < 20)
                {
                    copy = input.Substring(0, (input.Length - iter));

                    for (int j = 0; j < input.Length; j++)
                    {
                        
                        if (input[j] == copy[(j + iter) % copy.Length])
                        {
                            tmp += 1;
                        }
                        if (tmp >= CI && tmp != 25)
                        {
                            CI = tmp;
                            shift = iter;
                        }
                    }
                    list.Add(new int[]{iter,tmp});
                    
                    tmp = 0;
                    iter++;

                    foreach (char c in input)
                    {
                        if (alphaFrequency.ContainsKey(c.ToString()))
                        {
                            alphaFrequency[c.ToString()] += 1;

                        }
                    }
                }
              
                for(int i = 0; i< list.Count();i++)
                {
                    var x = list[i];
                    this.dataGridView1.Rows.Add(x[0],x[1]);
                }
                largestCIBox.Text = CI.ToString();
                largestshiftBox.Text = shift.ToString();

                displayData(alphaFrequency); // goto function to show char frequency
                findkeyLetters(shift, input); // goto function to find whole key
        }


        public void findkeyLetters(int shift, string text)
        {
            List<double> big_dotproduct_list = new List<double>();
           
            Dictionary<string, int> cipherFrequency = new Dictionary<string, int>()

            {
                {"a", 0},  {"d", 0}, {"g", 0}, {"j", 0}, {"l", 0}, {"o", 0}, {"r", 0}, {"u", 0}, {"x", 0},
                {"b", 0 }, {"e",0}, {"h",0}, {"k", 0}, {"m", 0}, {"p", 0}, {"s", 0}, {"v", 0 }, {"y", 0},
                {"c", 0 },  {"f", 0}, {"i", 0}, {"n", 0}, {"q", 0}, {"t", 0}, {"w",0}, {"z", 0}
            };
            for (int off = 0; off <= shift-1; off++)
            {

                for (int i = off; i < text.Length; i += shift)
                {
                    if (cipherFrequency.ContainsKey(text[i].ToString()))
                    {
                        cipherFrequency[text[i].ToString()]++;
                    }

                }

                int sum = 0;
                List<double> dot_product_list = new List<double>();

                var vector = cipherFrequency.OrderBy(key => key.Key);
                List<double> w = new List<double>();

                foreach (var x in vector)
                {
                    sum += cipherFrequency[x.Key]; // total number of letters counted
                }
                // Console.WriteLine(sum);

                foreach (var x in vector)
                {
                    if (cipherFrequency[x.Key] == 0)
                    {
                        w.Add(0);
                    }
                    else
                    {
                        w.Add((Convert.ToDouble(cipherFrequency[x.Key]) / sum));
                    }
                }

                sum = 0;
                double dot_product = 0;
                cipherFrequency = cipherFrequency.ToDictionary(p => p.Key, p => 0);

                for (int j = 0; j <= 25; j++)
                {
                    double[] x = new double[] { 0.082, 0.015, 0.028, 0.043, 0.127, 0.022, 0.020, 0.061, 0.070, 0.002, 0.008, 0.040, 0.024, 0.067, 0.075, 0.019, 0.001, 0.060, 0.063, 0.091, 0.028, 0.010, 0.023, 0.001, 0.020, 0.001 };// char freq vals   

                    var a_j = ShiftRight(x, j);

                    for (int k = 0; k <= 25; k++)
                    {
                        dot_product += (w[k] * a_j.ElementAt(k));
                    }

                    dot_product_list.Add(Math.Round(dot_product, 4));
                    dot_product = 0;

                }
                big_dotproduct_list.Add(dot_product_list.IndexOf(dot_product_list.Max()));
            }
            
            string out_str = "";
     
            big_dotproduct_list.ForEach(x => out_str += alpha[(int)x]);
            keyTextBox.Text += out_str;
            decrypt(text, out_str);

        }

        public static int MathMod(int a, int b)
        {
            int c = ((a % b) + b) % b;
            return c;
        }
        public static IEnumerable<T> ShiftRight<T>(IList<T> values, int shift)
        {
            for (int index = 0; index < values.Count; index++)
            {
                yield return values[MathMod(index - shift, values.Count)];
            }
        }


        private void decrypt(string text, string key)
        {
            
            Dictionary<string, int> alpha = new Dictionary<string, int>()
            {
                {"a", 0},  {"d", 3}, {"g", 6}, {"j", 9},  {"m", 12}, {"p", 15}, {"s", 18}, {"v", 21}, {"y", 24},
                {"b", 1 }, {"e",4},  {"h",7},  {"k", 10}, {"n", 13}, {"q", 16}, {"t", 19}, {"w", 22 }, {"z", 25},
                {"c", 2 }, {"f", 5}, {"i", 8}, {"l",11 }, {"o", 14}, {"r",17 }, {"u",20},  {"x", 23}
            };
            text = text.ToLower();
            key = key.ToLower();
            string out_string = "";
            int start_key = 0;
            int key_val;
            int char_index;
            int e_val;
            int out_val;
            for (int i = 0; i < text.Length; i++)
            {
                if (start_key == key.Length)
                {
                    start_key = 0;
                }

                key_val = alpha[key[start_key].ToString()];

                char_index = alpha[text[i].ToString()];

                e_val = ((char_index - key_val));
                out_val = (e_val >= 0 ? e_val : (26 - (e_val * (-1))));
                
                out_string += alpha.First(x => x.Value == out_val).Key;

                start_key += 1;

            }
           
            inout_textBox.Clear();
            inout_textBox.Text = out_string;
            inout_textBox.Text += '\n' + key;
        }
        private void encrypt(string text, string key)
        {

            Dictionary<string, int> alpha = new Dictionary<string, int>()
            {
                {"a", 0},  {"d", 3}, {"g", 6}, {"j", 9},  {"m", 12}, {"p", 15}, {"s", 18}, {"v", 21}, {"y", 24},
                {"b", 1 }, {"e",4},  {"h",7},  {"k", 10}, {"n", 13}, {"q", 16}, {"t", 19}, {"w", 22 }, {"z", 25},
                {"c", 2 }, {"f", 5}, {"i", 8}, {"l",11 }, {"o", 14}, {"r",17 }, {"u",20},  {"x", 23}
            };
            text = text.ToLower();
            key = key.ToLower();
            string out_string = "";
            int start_key = 0;
            int key_val;
            int char_index;
            int e_val;
            for (int i = 0; i < text.Length; i++)
            {
                if (start_key == key.Length)
                {
                    start_key = 0;
                }

                key_val = alpha[key[start_key].ToString()];

                char_index = alpha[text[i].ToString()];

                e_val = ((char_index + key_val)%26);
                
                out_string += alpha.First(x => x.Value == e_val).Key;

                start_key += 1;

            }

            inout_textBox.Clear();
            inout_textBox.Text = out_string;
            inout_textBox.Text += '\n' + key;
        }


        private void encrypt_button_Click(object sender, EventArgs e)
        {
            string key = keyTextBox.Text.ToLower().Trim(); 
            string text = inout_textBox.Text.ToLower().Trim(); 
            bool check = text.Length == 0 || key.Length == 0 ? false : true;
            if (check == false)
            {
                MessageBox.Show("Invalid Entry!");
            }
            else
            {
                
                encrypt(text, key);
            }

            
        }

        private void decrypt_button_Click(object sender, EventArgs e)
        {
            string key = keyTextBox.Text.ToLower().Trim();
            string text = inout_textBox.Text.ToLower().Trim(); ;
            bool check = text.Length == 0 || key.Length == 0 ? false : true;
            if (check == false)
            {
                MessageBox.Show("Invalid Entry!");
            }
            else
            {
                decrypt(text, key);
            }
           
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            encrypt_button.Enabled = true;
            decrypt_button.Enabled = true;
        }

        private void disable_radiobutton_CheckedChanged(object sender, EventArgs e)
        {
            encrypt_button.Enabled = false;
            decrypt_button.Enabled = false;
        }

        private void resetButton_Click(object sender, EventArgs e)
        {
            keyTextBox.Clear();
            inout_textBox.Clear();

        }
    }
}
