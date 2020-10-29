using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Threading;

namespace Automaton
{
    public partial class TableForm : Form
    {
        public form f;
        private string name;
        public bool table_check;
        private int state_count;
        private string[] states;
        private string init_state;
        public TableForm()
        {
            InitializeComponent();
        }

        public TableForm(form f)
        {
            InitializeComponent();
            this.f = f;
            table_check = true;
        }

        private void TableForm_Load(object sender, EventArgs e)
        {

        }


        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
        }

        private int Converter(string str, int mode)
        {
            int convert = 0;
            if (str == null) return 0;
            if(mode == 1)
            {
                for (int i = 0; i < str.Length; i++)
                {
                    if (str[i] == 'n')
                    {
                        convert += 1;
                    }
                    else if (str[i] == 'e')
                    {
                        convert += 2;
                    }
                    else if (str[i] == 's')
                    {
                        convert += 4;
                    }
                    else if (str[i] == 'w')
                    {
                        convert += 8;
                    }
                }
            }
            else if(mode == 2)
            {
                for (int i = 0; i < str.Length; i++)
                {
                    if (str[i] == 'n')
                    {
                        convert += 1;
                    }
                    else if (str[i] == 'e')
                    {
                        convert += 2;
                    }
                    else if (str[i] == 's')
                    {
                        convert += 3;
                    }
                    else if (str[i] == 'w')
                    {
                        convert += 4;
                    }
                }
            }
            return convert;
        }

        public string[] state1;
        public string[] state2;
        public int[] aio_input;
        public int[] aio_output;
        private void button1_Click_1(object sender, EventArgs e)
        {
            int count = dataGridView1.Rows.Count;
            state1 = new string[count];
            state2 = new string[count];
            aio_input = new int[count];
            aio_output = new int[count];

            for(int i = 0; i < count - 1; i ++)
            {
                if (Convert.ToString(dataGridView1.Rows[i].Cells[0].Value) == null || Convert.ToString(dataGridView1.Rows[i].Cells[1].Value) == null || Convert.ToString(dataGridView1.Rows[i].Cells[2].Value) == null || Convert.ToString(dataGridView1.Rows[i].Cells[3].Value) == null) continue;
                aio_input[i] = Converter(Convert.ToString(dataGridView1.Rows[i].Cells[1].Value), 1);
                aio_output[i] = Converter(Convert.ToString(dataGridView1.Rows[i].Cells[3].Value), 2);
                state1[i] = Convert.ToString(dataGridView1.Rows[i].Cells[0].Value);
                state2[i] = Convert.ToString(dataGridView1.Rows[i].Cells[2].Value);
            }
            table_check = true;
            Addall();
        }

        public void Addall()
        {
            f.savedAutomaton.SetInitState(init_state);
            foreach(string state in states)
            {
                if (state == init_state) continue;
                f.savedAutomaton.AddState(state);
            }
            for(int i = 0; i < state1.Length; i++)
            {
                if (f.savedAutomaton.Check_Transition(state1[i], new AIO(aio_input[i]))) continue;
                if (state1[i] == null) continue;
                f.savedAutomaton.AddNewTransition(state1[i], new AIO(aio_input[i]), state2[i], new AIO(aio_output[i]));
            }
        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }


        private void button2_Click_1(object sender, EventArgs e)
        {
            if(openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                name = openFileDialog1.FileName;
                File.OpenRead(name);
                using(StreamReader sr = new StreamReader(name))
                {
                    int count_row = dataGridView1.Rows.Count;
                    for(int i = 0; i < count_row - 1; i++)
                        dataGridView1.Rows.Clear();
                    string[] line_temp = new string[4];
                    string line;
                    while((line = sr.ReadLine()) != null)
                    {
                        line_temp = line.Split(' ');
                        dataGridView1.Rows.Add(line_temp);
                    }
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if(saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                name = saveFileDialog1.FileName;
                int count = dataGridView1.Rows.Count;
                string row;
                //File.OpenWrite(name);
                for (int i = 0; i < count - 1; i++)
                {
                    row = dataGridView1.Rows[i].Cells[0].Value + " " + dataGridView1.Rows[i].Cells[1].Value + " " + dataGridView1.Rows[i].Cells[2].Value + " " + dataGridView1.Rows[i].Cells[3].Value;
                    using (StreamWriter sw = new StreamWriter(name, true, System.Text.Encoding.Default))
                    {
                        sw.WriteLine(row);
                    }
                }
                
            }
        }

        private void button4_Click(object sender, EventArgs e) // count
        {
            if (textBox1.Text.Length < 1 || !int.TryParse(textBox1.Text, out state_count)) MessageBox.Show("Укажите количество состояний!", "Ошибка", MessageBoxButtons.OK);
            else
            {
                state_count = Convert.ToInt32(textBox1.Text);
                states = new string[state_count];
                for (int i = 0; i < state_count; i++)
                {
                    states[i] = "q" + Convert.ToString((i + 1));
                }
            }
        }
        private void button5_Click(object sender, EventArgs e)
        {
            if (textBox2.Text.Length < 1 || !int.TryParse(textBox2.Text, out state_count)) MessageBox.Show("Укажите стартовое  состояние!", "Ошибка", MessageBoxButtons.OK);
            else
            {
                if (Convert.ToInt32(textBox2.Text) > Convert.ToInt32(textBox1.Text)) MessageBox.Show("Стартовое состояние выбрано неверно!", "Ошибка", MessageBoxButtons.OK);
                else
                {
                    init_state = "q" + textBox2.Text;
                }
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            int count_row = dataGridView1.Rows.Count;
            for (int i = 0; i < count_row - 1; i++)
                dataGridView1.Rows.Clear();
        }
    }
}
