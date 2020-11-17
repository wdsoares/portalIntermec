using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace portalIntermec
{
    public class DatabaseAdonis
    {
        private string _connectionString { get; set; }
        private MySqlConnection _connection { get; set; }


        public DatabaseAdonis()
        {
            this.readDBSettings();
        }
        public void openConnection()
        {
            _connection.Open();
        }
        public void closeConnection()
        {
            _connection.Close();
        }
        public void setConnection(string conn)
        {
            _connection = new MySqlConnection(conn);
        }
        public void readDBSettings()
        {
            string arq = "";

            try
            {
                arq = File.ReadAllText("./dbSettings.json");
            }
            catch (IOException e)
            {
                Console.WriteLine("Não foi possível ler o arquivo de configs!");
                Console.WriteLine(e.Message);
            }

            if (arq.Length > 1)
            {
                JObject obj = JObject.Parse(arq);
                string user = (string)obj["adonis"]["user"];
                string password = (string)obj["adonis"]["password"];
                string host = (string)obj["adonis"]["host"];
                string port = (string)obj["adonis"]["port"];

                this._connectionString = "server=" + host + ";user id=" + user + ";password=" + password + ";port=" + port + ";database=adonis";
                this.setConnection(_connectionString);
            }
        }
        public void update(string epc)
        {
            string sql = "UPDATE products p JOIN barcodes b " +
                         "ON p.id = b.product_id JOIN tags t " +
                         "ON b.id = t.barcode_id SET p.purchases = p.purchases + 1 " +
                         "WHERE t.epc =  \"" + epc + "\"";
            MySqlCommand cmd = new MySqlCommand(sql, _connection);
            try
            {
                this.openConnection();
                cmd.ExecuteNonQuery();
                this.closeConnection();
            }
            catch (MySqlException e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
