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
                arq = File.ReadAllText("./dbSettings.json");
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

                this._connectionString = "server=" + host + ";user id=" + user + ";password=" + password + ";port=" + port + ";database=adonis";
                this.SetConnection(_connectionString);
            }
        }
        public void Update(string epc)
        {
            string sql = "UPDATE products p JOIN barcodes b " +
                         "ON p.id = b.product_id JOIN tags t " +
                         "ON b.id = t.barcode_id SET p.purchases = p.purchases + 1 " +
                         "WHERE t.epc =  \"" + epc + "\"";

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
    }
}
