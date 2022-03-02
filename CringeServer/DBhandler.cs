using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Data.SqlClient;

using LOG = SomeUsefulStuff.LogginProcedures;
using DBinfo = SomeUsefulStuff.DataBaseInfo;

namespace CringeServer
{
    public struct CringeInfo
    {
        public CringeInfo(string rString, string comandString)
        {
            responesString = rString;
            command = comandString;
            userID = 0;
            byteDataStorage = new List<byte[]>();

            filled = true;
        }
        public CringeInfo(int _userID, string _responesString, string _firstCommand)
        {
            userID = _userID;
            responesString = _responesString;
            command = _firstCommand;
            byteDataStorage = new List<byte[]>();

            filled = true;
        }
        public CringeInfo(CringeInfo ci)
        {
            userID = ci.userID;
            responesString = ci.responesString;
            command = ci.command;
            byteDataStorage = ci.byteDataStorage;

            filled = true;
        }

        public int userID { get; set; }
        public string responesString { get; set; }
        public string command { get; set; }
        public List<byte[]> byteDataStorage { get; set; } // Default init == null!!
        public bool filled { get; set; }
    }

    public struct InputQuery
    {
        public InputQuery(int _userID, ref SqlCommand _comand, string _firstComand, string _method)
        {
            userID = _userID;
            sqlComand = _comand;
            firstComand = _firstComand;
            method = _method;
        }

        public int userID { get; set; }
        public SqlCommand sqlComand { get; set; }
        public string firstComand { get; set; }
        // This param define, what query will do:
        // <get> info from base or <set> something
        // in table
        public string method { get; set; }
    }
        


    class DBhandler
    {
        static string ConnectionString = @"Data Source=MYPC\SQLEXPRESS;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";

        private SqlConnection mainConnection = null;
        private SqlConnection subConnection = null;
        private List<SqlConnection> connectionStorage = null;


        private SqlConnectionStringBuilder sBuilder = null;

        

        private Mutex connectionMutex = new Mutex();
        private Mutex customCommandMutex = new Mutex();
        private Mutex responesLockMutex = new Mutex();

        // Storage for query that will be handle
        private Dictionary<int, CringeInfo> queryPool = null;
        // Storage for request 
        private Dictionary<int, CringeInfo> responesPool = null;
        //
        private List<CringeInfo> responesStorage = null;
        //
        private List<InputQuery> queryStorage = null;

        private List<Thread> threadPool = null;


        private bool serverWorkStatus = false;
        private bool appWorkStatus = false;

        Thread subT = null;



        int currentCringeCount = 0;


        public bool getServerStatus()
        {
            return serverWorkStatus;
        }

        public DBhandler(string serverName, string dbName, string username = "", string password = "")
        {
            try
            {
                /*SqlConnectionStringBuilder sBuilder = new SqlConnectionStringBuilder();
                sBuilder.DataSource = serverName;
                sBuilder.UserID = username;
                sBuilder.Password = password;
                sBuilder.InitialCatalog = dbName;
                sBuilder.TrustServerCertificate = true;*/

                try
                {
                    mainConnection = new SqlConnection(ConnectionString);
                    mainConnection.Open();
                    mainConnection.Close();

                    subConnection = new SqlConnection(ConnectionString);
                    subConnection.Open();
                    subConnection.Close();
                }
                catch (Exception ex)
                {
                    LOG.printLogInFile(ex.Message.ToString(), "crash");
                }

                connectionStorage = new List<SqlConnection>();
                connectionStorage.Add(mainConnection);
                connectionStorage.Add(subConnection);


                subT = new Thread(DBHandlerMainProcess);

                queryPool = new Dictionary<int, CringeInfo>();
                responesPool = new Dictionary<int, CringeInfo>();
                queryStorage = new List<InputQuery>();

                responesStorage = new List<CringeInfo>();
            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
                return;
            }

        }
        // Service function {
        private CringeInfo readerToCringeInfo(ref SqlDataReader reader)
        {
            CringeInfo temp = new CringeInfo();
            while (reader.Read())
            {
                for (int i = 0; i < reader.FieldCount; ++i)
                {
                    switch (reader.GetDataTypeName(i))
                    {
                        case "int":
                            temp.responesString += (Convert.ToString(reader.GetInt32(i)) + ';');
                            break;

                        case "string":
                            temp.responesString += (reader.GetString(i) + ';');
                            break;

                        case "varchar":
                            temp.responesString += (reader.GetString(i) + ';');
                            break;

                        case "nchar":
                            temp.responesString += (reader.GetString(i) + ';');
                            break;

                        case "image":
                            if (temp.byteDataStorage == null)
                            {
                                temp.byteDataStorage = new List<byte[]>();
                            }
                            temp.byteDataStorage.Add((byte[])reader[i]);
                            break;
                    }
                    
                }
                temp.responesString += "//;";
            }
            reader.DisposeAsync();
            return temp;
        }
        public void refreshConnection()
        {
            connectionMutex.WaitOne();

            mainConnection = new SqlConnection(ConnectionString);
            subConnection = new SqlConnection(ConnectionString);

            connectionMutex.ReleaseMutex();
        }
        private void printQueryResult(string queryResult)
        {
            string[] rowsString = queryResult.Split("//");
            Console.WriteLine("Executed query result:");
            foreach (var row in rowsString)
            {
                string[] colString = row.Split(";");
                foreach (var col in colString)
                {
                    Console.Write($"{col}\t");
                }
                Console.WriteLine();
            }
        }
        public string getDBHandlerStatus()
        {
            string serverStatus = "";

            serverStatus += "\n///////////////////////////////////////////";
            serverStatus += "\n_______________DBHandler_INFO______________";
            serverStatus += $"\nServer is work\t {Convert.ToString(serverWorkStatus)}";
            serverStatus += $"\nRequest in pool\t {queryPool.Count}";
            serverStatus += $"\nResponse in pool\t {responesPool.Count}";

            serverStatus += "\n///////////////////////////////////////////";

            return serverStatus;
        }

        // Service function }


        // Set status function {
        public void startHandler()
        {
            serverWorkStatus = true;
            appWorkStatus = true;

            subT.Start();
        }
        public void stopHandler()
        {
            if (serverWorkStatus)
            {
                serverWorkStatus = false;


            }
        }
        public void closeApp()
        {
            if (appWorkStatus)
            {
                appWorkStatus = false;
            }
        }

        // Set status function }

        private void addQueryInStorage(int _ID,ref SqlCommand _comand, string _firstComand, string _method)
        {
            customCommandMutex.WaitOne();

            queryStorage.Add(new InputQuery(_ID,ref _comand, _firstComand, _method));

            customCommandMutex.ReleaseMutex();
        }
        private void deleteQueryFromStorage(InputQuery _q)
        {
            customCommandMutex.WaitOne();

            queryStorage.Remove(_q);

            customCommandMutex.ReleaseMutex();
        }
        private void addResponesInStorage(ref CringeInfo info)
        {
            responesLockMutex.WaitOne();

            responesStorage.Add(info);

            responesLockMutex.ReleaseMutex();
        }
        private void deleteResponesFromStorage(ref CringeInfo info)
        {
            responesLockMutex.WaitOne();

            responesStorage.Remove(info);

            responesLockMutex.ReleaseMutex();
        }

        // Like <list_name>.RemoveAt(count)
        private void deleteResponesFromStorage(int count)
        {
            responesLockMutex.WaitOne();

            responesStorage.RemoveAt(count);

            responesLockMutex.ReleaseMutex();
        }

        // For connection handler (will take some respones)
        public CringeInfo getFromRequestStorage(int _userID, string _firstCommand)
        {
            if (appWorkStatus)
            {
                if (serverWorkStatus)
                {
                    for (int i = 0; i < responesStorage.Count; ++i)
                    {
                        if ((_userID == responesStorage[i].userID) && (_firstCommand == responesStorage[i].command))
                        {
                            CringeInfo temp = new CringeInfo(responesStorage[i]);
                            deleteResponesFromStorage(i);
                            return temp;
                        }
                    }
                }
            }

            return new CringeInfo();
        }

        public void workProcess()
        {

        }

        // Only check exist request from client and add new respones in storage that will be handl in other thread.
        public void DBHandlerMainProcess()
        {
            while (appWorkStatus)
            {
                while (serverWorkStatus)
                {
                    foreach (InputQuery q in queryStorage.ToList())
                    {
                        switch (q.method)
                        {
                            case "get":
                                CringeInfo temp = executeCustomQueryB(q.sqlComand);
                                temp.command = q.firstComand;
                                temp.userID = q.userID;
                                addResponesInStorage(ref temp);

                                deleteQueryFromStorage(q);
                                break;

                            case "set":

                                deleteQueryFromStorage(q);
                                break;
                        }
                    }
                }
            }
        }

        public CringeInfo executeCustomQueryB(SqlCommand comand)
        {
            bool comandExecuted = false;

            while (!comandExecuted)
            {
                foreach (SqlConnection con in connectionStorage)
                {
                    if (con.State == System.Data.ConnectionState.Open)
                    {
                        continue; // while we havent an free connection with database.
                    }

                    SqlDataReader reader = null;
                    CringeInfo temp = new CringeInfo();

                    try
                    {
                        comand.Connection = con;
                        con.Open();

                        using (reader = comand.ExecuteReader())
                        {
                            temp = readerToCringeInfo(ref reader);
                        }

                        con.Close();
                    }
                    catch (Exception e)
                    {
                        con.Close();
                        comand.Dispose();

                        LOG.printLogInFile(e.Message.ToString(), "err");

                        return new CringeInfo();
                    }

                    comandExecuted = true;

                    if (comandExecuted)
                    {
                        comand.Dispose();
                        return temp;
                    }
                }
            }

            LOG.printLogInFile("DBHandler::executeCuxtomQuery::main_loop_doesn't_work!", "err");

            return new CringeInfo();
        }

        private void executeNonQuery(SqlCommand comand)
        {
            bool comandExecuted = false;

            while (!comandExecuted)
            {
                foreach (SqlConnection con in connectionStorage)
                {
                    
                }
            }
        }
        
        // Build query to data base {
            // This functions only define queryes or stored procedures which will be add in query pool.
            // This queryes will be execute by other thread.
        private void getMessageData(int _userID)
        {
            
        }
        private void getMessageFromRoom(int _userID, int _roomID)
        {
             
        }
        private void addMessageInRoom(string _message, int _userID, int _messageRoomID)
        {

        }

        private void getCringeCollection(int _userID, string _firstComand)
        {
            string SqlExpression = "CringeDataBase.dbo.getCringeCollectionOnUserID";
            SqlCommand com = new SqlCommand(SqlExpression);
            SqlParameter param = new SqlParameter
            {
                ParameterName = "@uID",
                Value = _userID
            };
            com.Parameters.Add(param);
            com.CommandType = System.Data.CommandType.StoredProcedure;

            addQueryInStorage(_userID,ref com, _firstComand, "get");
        }

        private void getRandomCringe(int _userID, string _firstComand)
        {
            string SqlExpression = "CringeDataBase.dbo.getRandomCringeInformation";
            SqlCommand com = new SqlCommand(SqlExpression);
            com.CommandType = System.Data.CommandType.StoredProcedure;

            addQueryInStorage(_userID,ref com, _firstComand, "get");   
        }

        private void addCringeInCollection(int _userID, int _cringeID, string _firstCommand)
        {
            string sqlExpression = "CringeDataBase.dbo.addCringeInCringeCollection";
            SqlCommand com = new SqlCommand(sqlExpression);
            
            SqlParameter userIDP = new SqlParameter
            {
                ParameterName = "@uID",
                Value = _userID
            };
            SqlParameter cringeIDP = new SqlParameter
            {
                ParameterName = "@cID",
                Value = _cringeID
            };

            com.Parameters.Add(userIDP);
            com.Parameters.Add(cringeIDP);

            com.CommandType = System.Data.CommandType.StoredProcedure;

            addQueryInStorage(_userID, ref com, _firstCommand, "set");
        }

        private void updateOperationInfFromDB()
        {

        }

        // Build query to data base }


// Open func for ConnectionHandler {
        public void getCringeCollectionO(int _userID, string _firstComand)
        {
            getCringeCollection(_userID, _firstComand);
        }
        public void getRandomCringeO(int _userID, string _firstComand)
        {
            getRandomCringe(_userID, _firstComand);
        }
        public void addCringeInCollectionO(int _userID, int _cringeID, string _firstComand)
        {
            addCringeInCollection(_userID, _cringeID, _firstComand);
        }
//////////////////
// not done yet //
/////////////////
        public void getMessagesFromRoomO(int _userID, int _roomID, string _firstComand)
        {

        }
        public void getSingleCringeO(int _userID, int _cringeID, string _firstComand)
        {

        }
        public void addMessageInRoomO(int _userID, int _roomID, string _userMessage, string _firstComand)
        {

        }
        public void deleteMessageFromRoomO(int _userID, int _roomID, int _messageID, string _firstComand)
        {

        }
// Open func for ConnectionHandler }



    }
}
