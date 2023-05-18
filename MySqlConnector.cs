using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace MySqlConnector
{
    enum MYSQLCON_STD_INFO : int
    { 
        SUCCESS     =  0,
        ERROR       = -1,
        INFO        = -2,
        WARNING     = -3
    };

    public enum MYSQLCON_STD : int
    {
        MYSQL_SUCCESS           =  MYSQLCON_STD_INFO.SUCCESS, 
        MYSQL_ERROR             =  MYSQLCON_STD_INFO.ERROR, 
        MYSQL_NOT_FOUND         =  MYSQLCON_STD_INFO.WARNING,
        MYSQL_ZERO_AFFECTED     =  MYSQLCON_STD_INFO.INFO
    };

    #region MySqlConnector Exception

    public sealed class MYSQL_InitException : Exception
    {
        public MYSQL_InitException(string message) : base(message) { }
    }

    public sealed class MYSQL_InternelException : Exception
    {
        public MYSQL_InternelException(string message) : base(message) { }
    }

    public sealed class MYSQL_ClosedConException : Exception
    {
        public MYSQL_ClosedConException(string message) : base(message) { }
    }

    #endregion

    namespace  Connection
    {
        public sealed class MySqlConnect
        {
            #region Properties

            /// <summary>
            /// Connection to the MYSQL Server database
            /// </summary>
            private static MySqlConnection MYSQL_Connection { get; set; }

            /// <summary>
            /// SQL Statments Executor
            /// </summary>
            private static MySqlCommand MYSQL_Command { get; set; }

            /// <summary>
            /// Read Data from MYSQL Server Database
            /// </summary>
            private static MySqlDataReader MYSQL_DataReader { get; set; }

            /// <summary>
            /// Get data as a Table
            /// </summary>
            private static MySqlDataAdapter MYSQL_DataAdapter { get; set; }

            /// <summary>
            /// Temp holding place for imported tables
            /// </summary>
            private static DataSet MYSQL_DataSet { get; set; }

            /// <summary>
            /// MYSQL Server Name
            /// </summary>
            private static string MYSQL_SERVER { get; set; }

            /// <summary>
            /// Database Name
            /// </summary>
            private static string MYSQL_DATABASE { get; set; }

            /// <summary>
            /// Username 
            /// </summary>
            private static string MYSQL_USER { get; set; }

            /// <summary>
            /// Password
            /// </summary>
            private static string MYSQL_SHADOW { get; set; }

            /// <summary>
            /// Accessing port
            /// </summary>
            private static string MYSQL_PORT { get; set; }

            /// <summary>
            /// Connnection string
            /// </summary>
            private static string MYSQL_CONNECTION_STRING { get; set; }

            /// <summary>
            /// Conection good
            /// </summary>
            private static bool MYSQL_CONNECTION_INITIALIZED { get; set; }

            /// <summary>
            /// Last Error Message
            /// </summary>
            private static string MYSQL_LAST_ERROR { get; set; }

            /// <summary>
            /// MYSQL Connector Standard Error
            /// </summary>
            public static int MYSQL_ERROR { get { return -1; } }


            /// <summary>
            /// MYSQL Connector success
            /// </summary>
            public static int MYSQL_SUCCESS { get { return 0; } }

            /// <summary>
            /// MYSQL_NOVALUE is a generic type value that defines <c>null</c> for any type.
            /// since this is a generic type you must cast it back to type that your actually using..
            /// <para>
            /// <b>Example:</b>
            /// <code>
            /// (string)MYSQL_NOVALUE
            /// </code>
            /// </para>
            /// </summary>
            public static object MYSQL_NOVALUE { get { return null; } }

            #endregion

            #region Methods

            /// <summary>
            /// Initalize MySqlConnection with the information you have given. Make sure your informations are correct.
            /// Connection is default closed. you can open it anytime
            /// <para>
            /// <b>Exceptions</b>
            /// <see cref="MYSQL_InternelException"/>
            /// </para>
            /// </summary>
            /// <param name="server">   Name of the Server that you are going to connect</param>
            /// <param name="database"> Name of the Database that you want to connect</param>
            /// <param name="username"> USERNAME is base on server side access</param>
            /// <param name="password"> PASSWORD is base on server side access</param>
            /// <returns>Returns <c>MYSQL_SUCCESS</c> on success or <c>MYSQL_ERROR</c> on failure. Use <c>MYSQL_LastError()</c> method for get more information about the failure.</returns>
            public static MYSQLCON_STD MYSQL_ConfigureAndInitialize(string server, string port, string database, string username, string password)
            {
                try
                {
                    MYSQL_SERVER            = server;
                    MYSQL_PORT              = port;
                    MYSQL_DATABASE          = database;
                    MYSQL_USER              = username;
                    MYSQL_SHADOW            = password;
                    MYSQL_CONNECTION_STRING = $"SERVER={MYSQL_SERVER};PORT={MYSQL_PORT};DATABASE={MYSQL_DATABASE};UID={MYSQL_USER};PASSWORD={MYSQL_SHADOW};";

                    MYSQL_Connection = new MySqlConnection(MYSQL_CONNECTION_STRING);
                    MYSQL_Connection.Open();
                    if (MYSQL_Connection.State == ConnectionState.Open)
                    {
                        ///Checking connection to the server
                        if (MYSQL_Connection.Ping() == false)
                        {
                            MYSQL_Connection.Close();
                            MYSQL_CONNECTION_INITIALIZED = false;
                            MYSQL_LAST_ERROR = "MYSQL Connector Ping test faild. Your server is not responding...";
                            return MYSQLCON_STD.MYSQL_ERROR;
                        }
                    }

                    MYSQL_CONNECTION_INITIALIZED = true;
                    MYSQL_Command = new MySqlCommand(string.Empty, MYSQL_Connection);
                    MYSQL_DataAdapter = new MySqlDataAdapter(string.Empty, MYSQL_Connection);
                    MYSQL_DataSet = new DataSet();
                    MYSQL_Connection.Close();
                    return MYSQLCON_STD.MYSQL_SUCCESS;
                }
                catch (Exception ex)
                {
                    throw new MYSQL_InternelException(ex.Message);
                }
            }


            /// <summary>
            /// Open Connection
            /// <para>
            /// <b>Exceptions:</b>
            /// <see cref="MYSQL_InitException"/>,
            /// <see cref="MYSQL_InternelException"/>
            /// </para>
            /// </summary>
            public static void MYSQL_Open()
            {
                try
                {
                    if (MYSQL_CONNECTION_INITIALIZED)
                    {
                        if (MYSQL_Connection.State != ConnectionState.Open)
                            MYSQL_Connection.Open();
                    }
                    else
                    {
                        MYSQL_LAST_ERROR = "MYSQL Connection is not correctly initialized. Try MYSQL_ConfigureAndInitialize(...) method.";
                        throw new MYSQL_InitException(MYSQL_LAST_ERROR);
                    }
                }
                catch(Exception ex)
                {
                    MYSQL_LAST_ERROR = ex.Message;
                    throw new MYSQL_InternelException(ex.Message);
                }     
            }


            /// <summary>
            /// Close Connection
            /// <para>
            /// <b>Exceptions:</b>
            /// <see cref="MYSQL_InitException"/>,
            /// <see cref="MYSQL_InternelException"/>
            /// </para>
            /// </summary>
            public static void MYSQL_Close()
            {
                try
                {
                    if (MYSQL_CONNECTION_INITIALIZED)
                    {
                        if (MYSQL_Connection.State == ConnectionState.Open)
                            MYSQL_Connection.Close();
                    }
                    else
                    {
                        MYSQL_LAST_ERROR = "MYSQL Connection is not correctly initialized. Try MYSQL_ConfigureAndInitialize(...) method.";
                        throw new MYSQL_InitException(MYSQL_LAST_ERROR);
                    }
                }
                catch(Exception ex)
                {
                    MYSQL_LAST_ERROR = ex.Message;
                    throw new MYSQL_InternelException(ex.Message);
                }       
            }


            /// <summary>
            /// Check for the State of Connection
            /// <para>
            /// <b>Exceptions:</b>
            /// <see cref="MYSQL_InitException"/>
            /// </para>
            /// </summary>
            /// <returns>Returns true (open) or false (closed)</returns>
            public static bool MYSQL_ConnectionIsOpen()
            {
                if(MYSQL_CONNECTION_INITIALIZED)
                {
                    if (MYSQL_Connection.State == ConnectionState.Open)
                        return true;
                    else
                        return false;
                }
                else
                {
                    MYSQL_LAST_ERROR = "MYSQL Connection is not correctly initialized. Try MYSQL_ConfigureAndInitialize(...) method.";
                    throw new MYSQL_InitException(MYSQL_LAST_ERROR);
                }
            }

            /// <summary>
            /// <para>
            /// This method use for, if the user forget to close the reader after using it.
            /// we are going check if mysql reader still running and close it.Also this method
            /// use only internally, it should not visible to the user.
            /// </para>
            /// </summary>
            private static void MYSQL_CloseReaderIF()
            {
                if (MYSQL_DataReader.IsClosed != true)
                {
                    MYSQL_DataReader.Close();
                }
            }


            /// <summary>
            /// Read data from your database using a SQL statement.
            /// 
            /// <para> 
            /// <b>NOTE:</b>
            /// While the MySql.Data.MySqlClient.MySqlDataReader is in use, the associated MySql.Data.MySqlClient.MySqlConnection
            /// is busy serving the MySql.Data.MySqlClient.MySqlDataReader, and no other operations can be performed on the MySqlConnection 
            /// other than closing it. This is the case until the MySql.Data.MySqlClient.MySqlDataReader.Close method of the MySql.Data.MySqlClient.MySqlDataReader
            /// </para>
            /// You can use <c>MYSQL_CloseReader()</c> method to close the MySqlDataReader.
            /// <para>
            /// <b>Exceptions:</b>
            /// <see cref="MYSQL_InitException"/>,
            /// <see cref="MYSQL_InternelException"/>
            /// </para>
            /// </summary>
            /// <param name="query">SQL Statement</param>
            /// <returns>Returns data on success or <c>null</c> on failure. Use <c>MYSQL_LastError()</c> method for get more information about the failure.</returns>
            public static MySqlDataReader MYSQL_Reader(string query)
            {
                try
                {
                    MYSQL_CloseReaderIF();

                    if(MYSQL_CONNECTION_INITIALIZED)
                    {
                        if (MYSQL_ConnectionIsOpen())
                        {
                            MYSQL_Command.CommandText = query;
                            MYSQL_DataReader = MYSQL_Command.ExecuteReader();
                            MYSQL_DataReader.Read();
                            if (MYSQL_DataReader.HasRows)
                            {
                                return MYSQL_DataReader;
                            }
                            else
                            {
                                MYSQL_LAST_ERROR = "Unable to locate any data using this SQL statement.";
                                return null;
                            }
                        }
                        else
                        {
                            MYSQL_LAST_ERROR = "MYSQL Connection offline.";
                            throw new MYSQL_ClosedConException(MYSQL_LAST_ERROR);
                        }
                    }
                    else
                    {
                        MYSQL_LAST_ERROR = "MYSQL Connection is not correctly initialized. Try MYSQL_ConfigureAndInitialize(...) method.";
                        throw new MYSQL_InitException(MYSQL_LAST_ERROR);
                    }
                }
                catch (Exception ex)
                {
                    MYSQL_LAST_ERROR = ex.Message;
                    throw new MYSQL_InternelException(ex.Message);
                }
            }


            /// <summary>
            /// Close the MySqlDataReader
            /// </summary>
            public static void MYSQL_CloseReader()
            {
                if(MYSQL_CONNECTION_INITIALIZED)
                {
                    if (MYSQL_DataReader.IsClosed != true && MYSQL_DataReader != null)
                    {
                        MYSQL_DataReader.Close();
                    }
                    else
                    {
                        MYSQL_LAST_ERROR = "MYSQL Reader is already closed or returned NULL";
                        throw new NullReferenceException(MYSQL_LAST_ERROR);
                    }
                }
                else
                {
                    MYSQL_LAST_ERROR = "MYSQL Connection is not correctly initialized. Try MYSQL_ConfigureAndInitialize(...) method.";
                    throw new MYSQL_InitException(MYSQL_LAST_ERROR);
                }
            }

            
            /// <summary>
            /// Add a new records to your table using a SQL statement.
            /// <para>
            /// <b>Exceptions:</b>
            /// <see cref="MYSQL_ClosedConException"/>,
            /// <see cref="MYSQL_InternelException"/>
            /// </para>
            /// </summary>
            /// <param name="query">SQL Statement</param>
            /// <returns>Number of affected rows. For other type of statements returns <c>MYSQL_ZERO_AFFECTED</c>.</returns>
            public static int MYSQL_Writer(string query)
            {
                try
                {
                    MYSQL_CloseReaderIF();

                    if (MYSQL_ConnectionIsOpen())
                    {
                        MYSQL_Command.CommandText = query;
                        int rows_affect = MYSQL_Command.ExecuteNonQuery();
                        if (rows_affect >= 0)
                            return rows_affect;
                        else
                            return (int)MYSQLCON_STD.MYSQL_ZERO_AFFECTED;
                    }
                    else
                    {

                        MYSQL_LAST_ERROR = "MYSQL Connection is offline.";
                        throw new MYSQL_ClosedConException(MYSQL_LAST_ERROR);
                    }
                }
                catch (Exception ex)
                {
                    MYSQL_LAST_ERROR = ex.Message;
                    throw new MYSQL_InternelException(ex.Message);
                }
            }


            /// <summary>
            /// Temp import table to Data Set. Importing table copy of the current table in your data source. 
            /// Any changes that happen in your data source table not going to happen in your imported table. 
            /// You can access your using <c>MYSQL_TableAccess()</c> method
            /// </summary>
            /// <param name="tableName">Name of table that your going import from your data source</param>
            /// <returns>Return <c>MYSQL_SUCCESS</c> on success or <c>MYSQL_ERROR</c> on failure. Use <c>MYSQL_LastError()</c> for get more information about the failure</returns>
            public static MYSQLCON_STD MYSQL_ImportTable(string tableName)
            {
                try
                {
                    if (MYSQL_DataSet.Tables.Contains(tableName))
                    {
                        MYSQL_LAST_ERROR = $"{tableName} is already in the data set.";
                        return MYSQLCON_STD.MYSQL_ERROR;
                    }
                    else
                    {
                        MYSQL_DataAdapter.SelectCommand.CommandText = $"SELECT * FROM {tableName};";
                        MYSQL_DataAdapter.SelectCommand.ExecuteNonQuery();
                        MYSQL_DataAdapter.Fill(MYSQL_DataSet.Tables[tableName]);
                        return MYSQLCON_STD.MYSQL_SUCCESS;
                    }
                }
                catch (Exception ex)
                {
                    MYSQL_LAST_ERROR = ex.Message;
                    throw new MYSQL_InternelException(ex.Message);
                }
            }


            /// <summary>
            /// Access your temp imported tables.You can use LINQ (Language Integrated Query) manipulate data in your table. Get more information about LINQ
            /// from <see href="https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/linq/">here</see>
            /// </summary>
            /// <param name="tableName">Name of table that your imported from your data source</param>
            /// <returns>Return Table on success or <c>MYSQL_NOVALUE</c> on failure. Use <c>MYSQL_LastError()</c> for get more information about the failure</returns>
            public static DataTable MYSQL_TableAccess(string tableName)
            {
                if (MYSQL_DataSet.Tables.Contains(tableName))
                {
                    return MYSQL_DataSet.Tables[tableName];
                }

                MYSQL_LAST_ERROR = $"Can not find any table named {tableName}.";
                return (DataTable)MYSQL_NOVALUE;
            }


            /// <summary>
            /// Return last error message or exeception message
            /// </summary>
            /// <returns>Error message</returns>
            public static string MYSQL_LastError()
            {
                return MYSQL_LAST_ERROR;
            }

            #endregion
        }
    }

    namespace Security
    {
        public enum EncryptionAlgorithm
        {
            AES,
            DES,
        }

        public enum HashAlgorithm
        {
            MD5,
            SHA256,
            SHA512
        }

        public class MySqlSecurity
        {

            public static string MYSQL_HashMD5(string text)
            {
                return Encoding.UTF8.GetString(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(text)));
            }

            public static string MYSQL_HashSHA256(string text)
            {
                return Encoding.UTF8.GetString(SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(text)));
            }

            public static string MYSQL_HashSHA512(string text)
            {
                return Encoding.UTF8.GetString(SHA512.Create().ComputeHash(Encoding.UTF8.GetBytes(text)));
            }

            //public static string MYSQL_Encrypt(string text, string master_key, EncryptionAlgorithm encryption_algorithm, HashAlgorithm hash_algorithm)
            //{
            //    if(encryption_algorithm == EncryptionAlgorithm.AES)
            //    {
            //        byte[] AES_KEY = null;
            //        byte[] AES_IV = null;

            //        if (hash_algorithm == HashAlgorithm.MD5)
            //        {
            //            AES_KEY = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(master_key));
            //            AES_IV = MD5.Create().ComputeHash(AES_KEY);
            //        }

            //        if (hash_algorithm == HashAlgorithm.SHA256)
            //        {
            //            AES_KEY = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(master_key));
            //            AES_IV = MD5.Create().ComputeHash(AES_KEY);
            //        }

            //        if (hash_algorithm == HashAlgorithm.SHA512)
            //        {
            //            AES_KEY = SHA512.Create().ComputeHash(Encoding.UTF8.GetBytes(master_key));
            //            AES_IV = MD5.Create().ComputeHash(AES_KEY);
            //        }
            //    }

                
            //}

            public static string MYSQL_DecryptAES(string encrypted_text, string master_key, HashAlgorithm hash_algorithm)
            {
                return null;
            }
        }
    }
}