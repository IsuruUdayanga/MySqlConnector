using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MySqlConnector.Connection;
using MySqlConnector.Security;

namespace MySqlConnector
{
    internal class TestMySqlConnetor
    {
        public static void CreateConnection()
        {
            MySqlConnect.MYSQL_ConfigureAndInitialize("localhost", "3304", "stduent", "root", "123");
            MySqlConnect.MYSQL_Open();
            MySqlConnect.MYSQL_ConnectionIsOpen();

        }
    }
}
