using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace portalIntermec
{
    public class Database
    {
        public Database()
        {
            this.ReadDBSettings();
        }
        private string _connectionString { get; set; }
        private MySqlConnection _connection { get; set; }
        private string readerAlias { get; set; }

        public void SetConnection(string conn)
        {
            _connection = new MySqlConnection(conn);
        }
        public void OpenConnection()
        {
            _connection.Open();
        }
        public void CloseConnection()
        {
            _connection.Close();
        }
        public int CheckDupe(string rdrTag)
        {
            int result = -1;
            string sql = "SELECT COUNT(id) FROM saida WHERE tag = '" + rdrTag + "'";
            MySqlCommand cmd = new MySqlCommand(sql, _connection);
            try
            {
                OpenConnection();
                MySqlDataReader dtr = cmd.ExecuteReader();
                while (dtr.Read())
                {
                    result = dtr.GetInt32(0);
                }
                dtr.Close();
            }
            catch
            {
                MessageBox.Show("Não foi possível checar duplicatas!");
            }
            finally
            {
                CloseConnection();
            }
            return result;
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
                string user = (string)obj["portal"]["user"];
                string password = (string)obj["portal"]["password"];
                string host = (string)obj["portal"]["host"];
                string port = (string)obj["portal"]["port"];
                string dbName = (string)obj["portal"]["dbName"];
                readerAlias = (string)obj["leitorConfigs"]["portalName"];

                this._connectionString = "server=" + host + ";user id=" + user + ";password=" + password + ";port=" + port + ";database=" + dbName;
                this.SetConnection(_connectionString);
            }
        }

        public void CreateDB()
        {
            string createSchema = "CREATE TABLE IF NOT EXISTS `portal`.`saida` " +
            "(`id` int NOT NULL AUTO_INCREMENT, `dataHora` datetime NOT NULL," +
            "`tag` varchar(100) DEFAULT NULL, `portalName` varchar(255) DEFAULT NULL ,PRIMARY KEY (`id`)) " +
            "ENGINE=InnoDB AUTO_INCREMENT=0 DEFAULT CHARSET=utf8mb4;";

            MySqlCommand cmd = new MySqlCommand(createSchema, _connection);

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

        public void InsertDB(string epc)
        {
            string sql = "INSERT INTO saida(dataHora, tag, portalName) VALUES (now(), \"" + epc + "\", \"" + readerAlias + "\")";
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

        public string Get()
        {
            List<Tag> lista = new List<Tag>();
            string result = "";

            try
            {
                string sql = "SELECT * FROM saida";
                MySqlCommand cmd = new MySqlCommand(sql, _connection);
                this.OpenConnection();
                MySqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    lista.Add(new Tag(rdr.GetInt32(0), Convert.ToString(rdr.GetDateTime(1)), rdr.GetString(2)));
                }
                this.CloseConnection();
                rdr.Close();
                result = String.Concat(result, JsonConvert.SerializeObject(lista));
            }
            catch (MySqlException e)
            {
                MessageBox.Show(e.Message);
            }

            return result;
        }
        public string Get(string tag)
        {
            List<Tag> lista = new List<Tag>();
            string result = "";

            string sql = "SELECT * FROM saida WHERE tag = \"" + tag + "\"";
            MySqlCommand cmd = new MySqlCommand(sql, _connection);

            try
            {
                this.OpenConnection();
                MySqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    lista.Add(new Tag(rdr.GetInt32(0), Convert.ToString(rdr.GetDateTime(1)), rdr.GetString(2)));
                }
                this.CloseConnection();
                rdr.Close();
                result = String.Concat(result, JsonConvert.SerializeObject(lista));
            }
            catch (MySqlException e)
            {
                MessageBox.Show(e.Message);
            }

            return result;
        }
    }
}
