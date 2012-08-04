using System;
using System.Data;
using System.Data.Odbc;
using System.Collections;
using System.IO;
using System.Threading;

using MySql.Data.MySqlClient;

namespace Holo.OldDatabase
{
    public static class dataHandling
    {
        /// <summary>
        /// Converts a DataColumn to an array .
        /// </summary>
        /// <param name="dCol">The DataColumn input.</param>
        public static string[] dColToArray(DataColumn dCol)
        {
            string[] dString = new string[dCol.Table.Rows.Count];
            for (int l = 0; l < dString.Length; l++)
                dString[l] = Convert.ToString(dCol.Table.Rows[l][0]);
            return dString;            
        }

        /// <summary>
        /// Converts a DataColumn to an array .
        /// </summary>
        /// <param name="dCol">The DataColumn input.</param>
        /// <param name="Tick">The output type of the array will become int.</param>
        public static int[] dColToArray(DataColumn dCol, object Tick)
        {
            int[] dInt = new int[dCol.Table.Rows.Count];
            for (int l = 0; l < dInt.Length; l++)
                dInt[l] = Convert.ToInt32(dCol.Table.Rows[l][0]);
            return dInt;
        }

    }
    /// <summary>
    /// A reuseable instance of a database connection, for accessing/writing data into the database.
    /// </summary>
    public class Database
    {
        #region Declares
        /// <summary>
        /// The MySqlConnection object of this connection. This object is private.
        /// </summary>
        private MySqlConnection Connection;
        /// <summary>
        /// The MySqlDataAdapter object of this connection, required for inserting data etc. This object is private.
        /// </summary>
        private MySqlDataAdapter dataAdapter = new MySqlDataAdapter();
        /// <summary>
        /// The MySqlCommand object of this connection, used for executing commands at the database. This object is private.
        /// </summary>
        private MySqlCommand Command = new MySqlCommand();
        /// <summary>
        /// A boolean indicating if the Database object should be closed after the next query.
        /// </summary>
        public bool closeAfterNextQuery;
        /// <summary>
        /// The connection string for connections. This string is static.
        /// </summary>
        public static string connectionString = "Server=" + Eucalypt.dbHost + ";Uid=" + Eucalypt.dbUsername +";Pwd=" + Eucalypt.dbPassword + ";Database=" + Eucalypt.dbName + ";Port=" + Eucalypt.dbPort + ";Pooling=false";//Max Pool Size=" + Eucalypt.dbPool;

        private bool _Ready = false;
        /// <summary>
        /// Gets the current readystate. (connected yes/no)
        /// </summary>
        public bool Ready
        {
            get { return this._Ready; }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes the Database object, with the options to open the database upon constructing, and/or to make the Database object tidy up (close connection and dispose resources) after the first query.
        /// </summary>
        /// <param name="openNow">Indicates if the database connection should be opened already.</param>
        /// <param name="closeAfterNextQuery">Indicates if the Database object should close the connection and dispose resources after the first query.</param>
        public Database(bool openNow, bool closeAfterFirstQuery, int ID)
        {
            if (openNow)
                this.Open();
            this.closeAfterNextQuery = closeAfterFirstQuery;
        }
        #endregion

        #region Methods
        #region Opening and closing database
        /// <summary>
        /// Attempts to open a connection to the database and prepares for use.
        /// </summary>
        public void Open()
        {
            // Attempt to connect to the database and handle exceptions
            try
            {
                this.Connection = new MySqlConnection(connectionString);
                this.Connection.Open();
                this.Command.Connection = this.Connection;
                this.dataAdapter.SelectCommand = this.Command;
                this._Ready = true;
            }
            catch (MySqlException ex) // Error while connecting
            {
                WriteError(ex.Message);
            }
        }
        /// <summary>
        /// Closes the connection to the database, if connected. All resources are disposed.
        /// </summary>
        public void Close()
        {
            if (this._Ready)
            {
                this.Connection.Close();
                this.Connection = null;
                this.dataAdapter = null;
                this.Command = null;
                this.closeAfterNextQuery = false;
                this._Ready = false;
            }
        }
        #endregion

        #region Data access
        /// <summary>
        /// Returns a DataSet object containing requested data of various tables.
        /// </summary>
        /// <param name="Query">The query to run at the database.</param>
        public DataSet getDataSet(string Query)
        {
            //Out.WritePlain(Query);
            DataSet dReturn = new DataSet();
            if (_Ready)
            {                
                try
                {
                    this.Command.CommandText = Query;
                    this.dataAdapter.Fill(dReturn);
                }
                catch (Exception ex) { WriteError(ex.Message + "\n(^^" + Query + "^^)"); }
                if (this.closeAfterNextQuery)
                    this.Close();

                return dReturn;
            }
            Out.WriteError("Database connection not active");
            return dReturn;
        }
        /// <summary>
        /// Returns a DataTable object containing requested data of a single table.
        /// </summary>
        /// <param name="Query">The query to run at the database.</param>
        public DataTable getTable(string Query)
        {
            //Out.WritePlain(Query);
            DataTable dReturn = new DataTable();
            
            if (_Ready)
            {
                try
                {
                    this.Command.CommandText = Query;
                    this.dataAdapter.Fill(dReturn);
                }
                catch (Exception ex) { WriteError(ex.Message + "\n(^^" + Query + "^^)"); }
                if (this.closeAfterNextQuery)
                    this.Close();

                return dReturn;
            }
            Out.WriteError("Database connection not active");
            return dReturn;
        }
        /// <summary>
        /// Returns a DataTable object containing requested data of a single table.
        /// </summary>
        /// <param name="Query">The query to run at the database.</param>
        /// <param name="Quiet">Prevents an error from being printed to the server.</param>
        public DataTable getTable(string Query, object Quiet)
        {
            
            DataTable dReturn = new DataTable();
            //Out.WritePlain(Query);
            if (_Ready)
            {
                try
                {
                    this.Command.CommandText = Query;
                    this.dataAdapter.Fill(dReturn); 
                }
                catch { }
                if (this.closeAfterNextQuery)
                    this.Close();

                return dReturn;
            }
            Out.WriteError("Database connection not active");
            return dReturn;
        }
        /// <summary>
        /// Returns a DataRow object containing requested data of a single row of a single table.
        /// </summary>
        /// <param name="Query">The query to run at the database.</param>
        public DataRow getRow(string Query)
        {           
            DataRow dReturn = new DataTable().NewRow();
            ////Out.WritePlain(Query);
            //Out.WritePlain("Retrieving datarow; " + Query);
            if (_Ready)
            {
                try
                {
                    DataSet tmpSet = new DataSet();
                    this.Command.CommandText = Query;
                    
                    
                    this.dataAdapter.Fill(tmpSet);
                    
                    
                    
                    dReturn = tmpSet.Tables[0].Rows[0];
                }
                catch { }

                if (this.closeAfterNextQuery)
                    this.Close();

                return dReturn;
            }
            Out.WriteError("Database connection not active");
            return dReturn;
        }
        /// <summary>
        /// Returns a DataColumn object containing requested data of a single column of a single table.
        /// </summary>
        /// <param name="Query">The query to run at the database.</param>
        public DataColumn getColumn(string Query)
        {
            //Out.WritePlain(Query);
            DataColumn dReturn = new DataTable().Columns.Add();
            
            if (_Ready)
            {
            try
            {
                DataSet tmpSet = new DataSet();
                this.Command.CommandText = Query;

                
                this.dataAdapter.Fill(tmpSet);
                
                
                
                dReturn = tmpSet.Tables[0].Columns[0];
            }
            catch { }

            if (this.closeAfterNextQuery)
                this.Close();

            return dReturn;
            }
            Out.WriteError("Database connection not active");
            return dReturn;
            
        }
        /// <summary>
        /// Retrieves a single field value from the database and returns it as a string.
        /// </summary>
        /// <param name="Query">The query to run at the database.</param>
        public string getString(string Query)
        {
            //Out.WritePlain("Retrieving string; " + Query);
            //Out.WritePlain(Query);
            string s = "";
            
            if (_Ready)
            {
            try
            {
                this.Command.CommandText = Query;
                s = this.Command.ExecuteScalar().ToString();
            }
            catch { }
            if (this.closeAfterNextQuery)
                this.Close();

            return s;
            }
            Out.WriteError("Database connection not active");
            return "";
        }
        /// <summary>
        /// Retrieves a single field value from the database and returns it as an integer.
        /// </summary>
        /// <param name="Query">The query to run at the database.</param>
        public int getInteger(string Query)
        {
            //Out.WritePlain(Query);
            //Out.WritePlain("Retrieving int; " + Query);
            int i = 0;
            
            if (_Ready)
            {
            try
            {
                this.Command.CommandText = Query;
                i = int.Parse(this.Command.ExecuteScalar().ToString());
            }
            catch { }
            if (this.closeAfterNextQuery)
                this.Close();

            return i;
            }
            return i;
        }
        /// <summary>
        /// Returns a boolean indicating if there were results for a certain query.
        /// </summary>
        /// <param name="Query">The query to run at the database.</param>
        public bool findsResult(string Query)
        {
            //Out.WritePlain(Query);
            //Out.WritePlain("Retrieving findresult; " + Query);
            bool Found = false;
            
            if (_Ready)
            {
            try
            {
                
                this.Command.CommandText = Query;
                
                MySqlDataReader dReader = this.Command.ExecuteReader();
                
                
                Found = dReader.HasRows;
                 
                dReader.Close();
            }
            catch (Exception ex) { WriteError(ex.Message + "\n(^^" + Query + "^^)"); }
            if (this.closeAfterNextQuery)
                this.Close();

            return Found;
            }
            Out.WriteError("Database connection not active");
            return Found;
        }
        #endregion

        #region Other
        /// <summary>
        /// Adds a parameter with a value to the current parameter collection, for use in queries. A '@' symbol is placed infront of the parameter key automatically.
        /// </summary>
        /// <param name="Parameter">The parameter key to add. '@' symbol is added infront.</param>
        /// <param name="Value">The value of the parameter, can be any type.</param>
        public void addParameterWithValue(string Parameter, object Value)
        {
            
            this.Command.Parameters.AddWithValue("@" + Parameter, Value);
        }

        public void addRawParameter(MySqlParameter Parameter)
        {
            
            this.Command.Parameters.Add(Parameter);
        }
        /// <summary>
        /// Clears all parameters from the parameter collection.
        /// </summary>
        public void clearParameters()
        {
            
            this.Command.Parameters.Clear();
        }
        /// <summary>
        /// Attempts to open a connection to the database to execute a query.
        /// </summary>
        /// <param name="Query">The query string to execute.</param>
        public void runQuery(string Query)
        {
            //Out.WritePlain(Query);
            if (_Ready)
            {
                try
                {
                    this.Command.CommandText = Query;
                    
                    
                    this.Command.ExecuteNonQuery();
                    
                    
                    
                }
                catch (Exception ex) { WriteError(ex.Message + "\n(^^" + Query + "^^)"); }
                if (this.closeAfterNextQuery)
                    this.Close();
            }
            else
                Out.WriteError("Database connection not active");
        }
        /// <summary>
        /// Writes Message to the server and DB.err.
        /// </summary>
        /// <param name="Message">The message to write.</param>
        public static void WriteError(string Message)
        {
            try
            {
                FileStream Writer = new FileStream("DB.err", FileMode.Append, FileAccess.Write);
                byte[] Msg = System.Text.ASCIIEncoding.ASCII.GetBytes(Message + "\r\n");
                Writer.Write(Msg, 0, Msg.Length);
            }
            catch
            {
                Message = "FAILED TO SAVE ERROR TO FILE: " + Message;
            }
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.Write("[DATABASE ERROR] => ");
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine(Message);
            Console.ForegroundColor = ConsoleColor.Gray;
        }
        #endregion
        #endregion
        public string Stripslash(string Query)
        {
            try { return Query.Replace(@"\", "\\").Replace("'", @"\'"); }
            catch { return ""; }
        }
    }
}