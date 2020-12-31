using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace portalIntermec
{
    public class DatabaseAdonis
    {
        private string _connectionString { get; set; }
        private MySqlConnection _connection { get; set; }
        private string readerAlias { get; set; }

        public DatabaseAdonis()
        {
            this.ReadDBSettings();
        }
        public void OpenConnection()
        {
            _connection.Open();
        }
        public void CloseConnection()
        {
            _connection.Close();
        }
        public void SetConnection(string conn)
        {
            _connection = new MySqlConnection(conn);
        }
        public void ReadDBSettings()
        {
            string arq = "";

            try
            {
                arq = File.ReadAllText("./dbSettings.json", Encoding.UTF8);
            }
            catch (IOException e)
            {
                MessageBox.Show("Não foi possível ler o arquivo de configs! Erro: " + e.Message);
            }

            if (arq.Length > 1)
            {
                JObject obj = JObject.Parse(arq);
                string user = (string)obj["adonis"]["user"];
                string password = (string)obj["adonis"]["password"];
                string host = (string)obj["adonis"]["host"];
                string port = (string)obj["adonis"]["port"];
                string dbName = (string)obj["adonis"]["dbName"];
                readerAlias = (string)obj["leitorConfigs"]["portalName"];

                _connectionString = "server=" + host + ";user id=" + user + ";password=" + password + ";port=" + port + ";database=" + dbName;
                SetConnection(_connectionString);
            }
        }
        public void Update(string epc)
        {
            int tag_id = GetTagID(epc);
            string sql = "INSERT INTO saidas(tag_id, created_at, updated_at, portalName) VALUES (" + tag_id + " , now(), now(), `" + readerAlias + "`)";

            MySqlCommand cmd = new MySqlCommand(sql, _connection);
            try
            {
                this.OpenConnection();
                cmd.ExecuteNonQuery();
                this.CloseConnection();
            }
            catch (MySqlException e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private int GetTagID(string epc)
        {
            string sql = "SELECT t.id FROM tags t WHERE t.epc like \"" + epc + "\"";
            int tag_id = -1;
            MySqlDataReader rdr = null;

            try
            {
                MySqlCommand cmd = new MySqlCommand(sql, this._connection);
                OpenConnection();
                rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    tag_id = rdr.GetInt32(0);
                }
            }
            catch (MySqlException e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                CloseConnection();
                rdr.Close();
            }

            return tag_id;
        }
    }
}
