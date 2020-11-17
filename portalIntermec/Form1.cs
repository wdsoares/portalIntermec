using Intermec.DataCollection.RFID;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace portalIntermec
{
    public partial class Form1 : Form
    {
        private Database db = null;
        private DatabaseAdonis dbAdonis = null;
        private BRIReader reader = null;
        private string address = default;
        private GPITrigger GPITrig = null;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        { 
            this.db = new Database();
            this.dbAdonis = new DatabaseAdonis();
            this.address = readConfigFile();
            reader = new BRIReader(this, address);

            try
            {
                this.db.createDB();
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Exception: " + ex.Message);
            }
        }

        public string readConfigFile()
        {
            string arq = "";
            string address = default;

            try
            {
                arq = File.ReadAllText("./dbSettings.json");
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
            }
            return address;
        }

        public void closeConn()
        {
            try
            {
                this.reader.DeleteGPITrigger(GPITrig);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}