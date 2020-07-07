using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Data.SqlClient;

namespace SnuggleBot
{
    class SQLQueryIssuer
    {
        SqlConnectionStringBuilder MasterConnection = new SqlConnectionStringBuilder();
        public SQLQueryIssuer()
        {
            SetupConnection();
        }
       
        private void SetupConnection()
        {

            // Setup connection string info
            MasterConnection.DataSource = "Removed for security";
            MasterConnection.UserID = "Removed for security";
            MasterConnection.Password = "Removed for security";
            MasterConnection.InitialCatalog = "Removed for security";
            MasterConnection.MultipleActiveResultSets = true;
            MasterConnection.PersistSecurityInfo = false;
            MasterConnection.ConnectTimeout = 30;
        }

        public SqlDataReader SendQuery(string Entry)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(MasterConnection.ConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(Entry, connection))
                    {
                        connection.Open();
                        using (SqlDataReader Reader = cmd.ExecuteReader())
                        {
                            return Reader;
                        }
                    }
                }
            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
                return null;
            }catch(Exception e)
            {
                Console.WriteLine(e.ToString());
                return null;
            }
        }

        public string GetConnectionString()
        {
            return MasterConnection.ConnectionString;
        }

        public int SendNonQuery(string entry)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(MasterConnection.ConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(entry, connection))
                    {
                        connection.Open();
                        int affectedRows = cmd.ExecuteNonQuery();
                        return affectedRows;
                    }
                }
            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
                return -1;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return -1;
            }
        }

        public void LogCommand(string GuildID, string UserID, string Command)
        {
            Console.WriteLine("SQl logging: " + Command);
        }
    }

}
