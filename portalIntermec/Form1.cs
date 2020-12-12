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
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace portalIntermec
{
    public partial class Form1 : Form
    {
        private Database db = null;
        private DatabaseAdonis dbAdonis = null;
        private BRIReader reader = null;
        private string address = default;
        private GPITrigger GPITrig = null;
        private Tag_EventHandlerAdv m_TagEventHandler = null;
        private int idCount = 0;
        private bool isReading = false;
        private bool isConnected = false;
        private bool m_IsContinuousReadStarted = false;
        private BindingList<Tag> lista = new BindingList<Tag>();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.dataGridView1.DataSource = lista;
            this.dataGridView1.Columns[0].HeaderText = "ID";
            this.dataGridView1.Columns[1].HeaderText = "Data/Hora";
            this.dataGridView1.Columns[2].HeaderText = "Tag";
            this.dataGridView1.Columns[0].Width = 40;
            this.dataGridView1.Columns[1].Width = 200;
            this.dataGridView1.Columns[2].Width = 325;
            this.db = new Database();
            this.dbAdonis = new DatabaseAdonis();
            this.address = readConfigFile();

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

        private void readTags()
        {
            this.EventRead();
        }

        void BRIReaderEventHandler_Tag(object sender, EVTADV_Tag_EventArgs EvtArgs)
        {
            this.onTagRead(EvtArgs.Tag);
        }

        public void EventRead()
        {
            //*****
            //* Sets up an event handler to handle the tag events. This event
            //* handler only needs to be registered once.
            //*****
            if (null == m_TagEventHandler)
            {
                m_TagEventHandler = new Tag_EventHandlerAdv(BRIReaderEventHandler_Tag);
                reader.EventHandlerTag += m_TagEventHandler;
            }

            //*****
            //* Sets up a timer to stop the continuous read.
            //*****

/*            if (null == m_StopReadTimer)
            {
                m_StopReadTimer = new Timer();
                m_StopReadTimer.Tick += new EventHandler(TimerTick_StopRead);
            }
            m_StopReadTimer.Enabled = false;
            m_StopReadTimer.Interval = aReadInterval;
*/
            try
            {
                if (reader.StartReadingTags(BRIReader.TagReportOptions.EVENT))
                {
 
                    m_IsContinuousReadStarted = true;
                    //m_StopReadTimer.Enabled = true; // Start the timer to stop continuous read.
                }
                else
                {
                    MessageBox.Show("Falha ao iniciar leitura contínua!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Falha ao iniciar leitura contínua, erro: " + ex.Message);
            }
        }

/*        void TimerTick_StopRead(object sender, EventArgs e)
        {
            if (m_PollTimer != null)
            {
                m_PollTimer.Enabled = false;
            }
            m_StopReadTimer.Enabled = false;

            try
            {
                reader.StopReadingTags();
                m_IsContinuousReadStarted = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Falha ao terminar leitura contínua, erro: " + ex.Message);
            }
        }*/

        public void onTagRead(Intermec.DataCollection.RFID.Tag tag)
        {
            if (Regex.IsMatch(tag.ToString(), @"^[0-9]*$"))
            {
                if (db.checkDupe(tag.ToString()) == 0)
                {
                    this.idCount++;
                    this.lista.Add(new Tag(this.idCount, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), tag.ToString()));
                    this.dataGridView1.Refresh();
                    db.insertDB(tag.ToString());
                    dbAdonis.update(tag.ToString());
                }
            }
        }
        public void insertTags(Intermec.DataCollection.RFID.Tag[] tags)
        {
            if(tags == null)
            {
                return;
            }
            for(int i = 0; i < tags.Length; i++)
            {
                onTagRead(tags[i]);
            }
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

        private void configToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void alterarEndereçoDoLeitorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form2 f2 = new Form2();
            f2.ShowDialog();
            this.address = readConfigFile();
        }

        private void sobreToolStripMenuItem_Click(object sender, EventArgs e)
        {
            String outputMessage = string.Format("Portal - Intermec{0}" +
                                                 "Developers: Willian Soares, Renato Denardin, Michel Rodrigues{0}" +
                                                 "Orientador: João Baptista Martins{0}" +
                                                 "gmicro - UFSM", Environment.NewLine);
            MessageBox.Show(outputMessage);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(!isConnected)
            {
                MessageBox.Show("Leitor desconectado. Estabeleça conexão antes de iniciar leitura!");
            }
            else
            {
                if (!isReading)
                {
                    this.isReading = true;
                    this.readTags();
                    button1.Enabled = false;
                }
                else
                {
                    MessageBox.Show("Uma sessão de leitura já está iniciada!");
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.isReading = false;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if(!isConnected)
            {
                try
                {
                    this.reader = new BRIReader(this, address);
                    this.isConnected = true;
                    MessageBox.Show("Leitor conectado com sucesso!");
                    button3.Enabled = false;
                    try
                    {
                        this.reader.Attributes.RFIDTagType = BRIReader.RFIDTagTypes.EPCC1G2;
                        this.reader.Attributes.ReadTries = 3;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Failed to set reader attribute, exception= " + ex.Message, "Error");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Impossível conectar ao host. Erro: " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Leitor já está conectado!");
            }
        }
    }
}