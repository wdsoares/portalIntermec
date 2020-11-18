using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace portalIntermec
{
    public partial class Form2 : Form
    {
        private string arq = "";
        public Form2()
        {
            InitializeComponent();
            this.readConfigFile();
        }
        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.textBox1.Clear();
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(textBox1.TextLength > 0)
            {
                try
                {
                    this.changeAddress(textBox1.Text);
                }
                catch(Exception ex)
                {
                    MessageBox.Show("Erro na escrita do arquivo. Cód. de erro: " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Você precisa informar um novo endereço!");
            }
        }

        public void readConfigFile()
        {
            string address = default;

            try
            {
                this.arq = File.ReadAllText("./dbSettings.json");
            }
            catch (IOException e)
            {
                MessageBox.Show("Não foi possível ler o arquivo de configs!");
                MessageBox.Show(e.Message);
            }

            if (arq.Length > 1)
            {
                JObject obj = JObject.Parse(arq);
                address = (string)obj["intermec"]["address"];
                textBox1.Text = address;
            }
        }

        private void changeAddress(string addr)
        {
            if (arq.Length > 1)
            {
                JObject obj = JObject.Parse(arq);
                JObject intermec = (JObject)obj["intermec"];
                intermec["address"] = addr;
                try
                {
                    File.WriteAllText(@"./dbSettings.json", JsonConvert.SerializeObject(obj, Formatting.Indented));
                    MessageBox.Show("Configurações salvas!");
                    this.Close();
                }
                catch
                {
                    MessageBox.Show("Não foi possível escrever no arquivo");
                }
            }
            else
            {
                MessageBox.Show("Erro na leitura do arquivo de configurações");
            }
        }
    }
}
