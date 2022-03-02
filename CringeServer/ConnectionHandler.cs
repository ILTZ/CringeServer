using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;


using LOG = SomeUsefulStuff.LogginProcedures;
using DBinfo = SomeUsefulStuff.DataBaseInfo;

namespace SomeUsefulStuff
{
    public static class DataBaseInfo
    {
        public const string serverName = @"MyPC\SQLEXPRESS";
        public const string dataBaseName = "CringeDataBase";
        public const string userName = @"BigBoss";
        public const string password = "11112222";
    }

    public static class LogginProcedures
    {
        public static bool writeLogs { get; set; }

        private static int crashCount = 0;
        private static int logCount = 0;
        private static int errorCount = 0;


        // _param define path and description of log.
        // _param can be: "log"(default), "err" or "crash"
        public static void printLogInFile(string _logMessage, string _param = "log")
        {
            if (!writeLogs)
                return;

            string endDirectory = "";
            string logWord = "";

            if (_param == "log")
            {
                endDirectory = @"\WorkLogs";
                logWord = "::LOG::";
                ++logCount;
            }
            else if (_param == "err")
            {
                endDirectory = @"\WorkLogs";
                logWord = "::ERROR::";
                ++errorCount;
            }
            else if (_param == "crash")
            {
                endDirectory = @"\CrushLogs";
                logWord = "::CRUSH::";
                ++crashCount;
            }


            if (!(Directory.Exists(Directory.GetParent(Directory.GetCurrentDirectory()).ToString() + @"\Logs")))
            {
                Directory.CreateDirectory(Directory.GetParent(Directory.GetCurrentDirectory()).ToString()
                    + @"\Logs");
            }
            if (!(Directory.Exists(Directory.GetParent(Directory.GetCurrentDirectory()).ToString() + @"\Logs" + @"\WorkLogs")))
            {
                Directory.CreateDirectory(Directory.GetParent(Directory.GetCurrentDirectory()).ToString()
                    + @"\Logs" + endDirectory);
            }

            string pathToFile = Directory.GetParent(Directory.GetCurrentDirectory()).ToString()
                + @"\Logs" + (endDirectory + @"\") + (DateTime.Today.ToString("d") + "logs.txt");


            // 
            if (_param == "err")
                File.AppendAllText(pathToFile, "************************ERROR************************");

            File.AppendAllText(pathToFile, (DateTime.Now.ToLongTimeString() + logWord + _logMessage + "\n"));

            if (_param == "err")
                File.AppendAllText(pathToFile, "*****************************************************");
            //
        }


        public static int getLogCount()
        {
            return logCount;
        }
        public static void dropLogCount()
        {
            crashCount = 0;
        }
        //
        public static int getCrashCount()
        {
            return crashCount;
        }
        public static void dropCrashCount()
        {
            crashCount = 0;
        }
        //
        public static int getErrorCount()
        {
            return errorCount;
        }
        public static void dropErrorCount()
        {
            errorCount = 0;
        }
    }
    public class ServerConnectionInformation
    {
        // Required variables {
        public int port { get; set; }
        public string inputIP { get; set; }
        public int maxListen { get; set; }
        // Required variables }

        // Some statistic info {
        public int currentConnectionCount { get; set; }
        public int currentResponesIsWating { get; set; }
        // Some statistic info }
   
        // Some work process bool var {
        public bool serverIsWork { get; set; }
        // Some work process bool var }


        public ServerConnectionInformation(int _port = 8005, string _inputIP = "192.168.0.1", int _maxListen = 50)
        {
            port = _port;
            //inputIP = _inputIP;
            inputIP = getCurrentMachineIP();
            maxListen = _maxListen;


            currentConnectionCount = 0;
            currentResponesIsWating = 0;


            serverIsWork = false;
        }


        public string getCurrentMachineIP(string _protocol = "IPv4")
        {
            String host = System.Net.Dns.GetHostName();
            AddressFamily tempAF = AddressFamily.Unknown;

            switch (_protocol)
            {
                case "IPv4":
                    tempAF = AddressFamily.InterNetwork;
                    break;

                case "IPv6":
                    tempAF = AddressFamily.InterNetworkV6;
                    break;

                default:

                    break;
            }

            foreach (System.Net.IPAddress IPit in Dns.GetHostAddresses(host))
            {
                if (IPit.AddressFamily == tempAF)
                    return IPit.ToString();
            }

            return "Addres with this protocol not found.";
        }
        
        public string getCurrentThreadCount(string _param = "max")
        {
            int maxThread = 0;
            int maxRWThread = 0;

            System.Threading.ThreadPool.GetMaxThreads(out maxThread, out maxRWThread);

            if (_param == "max")
                return Convert.ToString(maxThread);
            else if (_param == "maxrw")
                return Convert.ToString(maxRWThread);


            return "uncorrect_param";
        }
       
        public void dropCurrentConnectionCount()
        {
            currentConnectionCount = 0;
        }
        public void increseCurrentConnectionCount()
        {
            currentConnectionCount++;
        }
        public void decreaseCurrentConnectionCount()
        {
            --currentConnectionCount;
        }
        //
        //
        public void dropCurrentResponesIsWating()
        {
            currentResponesIsWating = 0;
        }
        public void increaseCurrentResponesIsWating()
        {
            currentResponesIsWating++;
        }
        public void decreaseCurrentResponesIsWating()
        {
            currentResponesIsWating--;
        }
        //
        //
        public void printCurrentServerInformation()
        {
            Console.Clear();

            Console.WriteLine("/////////////////////////////////////////");
            Console.WriteLine("_______________SERVER INFO_______________");
            Console.WriteLine($"Current server IPv4:\t\t{getCurrentMachineIP()}");
            Console.WriteLine($"Current server IPv6:\t\t{getCurrentMachineIP("IPv6")}");
            Console.WriteLine($"Current thread/rwthread:\t{getCurrentThreadCount()}/{getCurrentThreadCount("maxrw")}");
            Console.WriteLine($"Current error count:\t\t{LOG.getErrorCount()}");
            Console.WriteLine($"Current crash count:\t\t{LOG.getCrashCount()}");
            Console.WriteLine($"Current log count:\t\t{LOG.getLogCount()}");
            Console.WriteLine($"Connection handler is work:\t" + serverIsWork.ToString());
            Console.WriteLine($"Current connection count:\t{currentConnectionCount}");
            Console.WriteLine($"Current respones is waiting:\t{currentResponesIsWating}");

            Console.WriteLine("/////////////////////////////////////////");
        }

    }
}


namespace CringeServer
{
    class responesToUser
    {
        public responesToUser(int _userID, string _comand)
        {
            userID = _userID;
            userComand = _comand;
            byteData = null;

            isHandle = false;
        }
        public responesToUser(int _userID, string _comand, ref byte[] _byteData)
        {
            userID = _userID;
            userComand = _comand;
            byteData = _byteData;

            isHandle = false;
        }

        public int userID { get; set; }        
        public string userComand { get; set; }
        public byte[] byteData { get; set; }
        public bool isHandle { get; set; }

    }
    class ConnectionHandler
    {
        private IPEndPoint ipPoint = null;
        private Socket listenSocket = null;
        private DBhandler handler = null;

        // Some lists {
        private List<ClientSocket> clientsSocket = null;
        private List<Thread> threadingStorage = null;
        private List<responesToUser> responesStorage = null;    
        // Some lists }

        // Some mutexes {
        private Mutex responesStorageMutex = null;
        private Mutex mut = null;
        private Mutex socketMutex = null;   
        // Some mutexes }        

        // Some info {
        private SomeUsefulStuff.ServerConnectionInformation serverInfo = null;
        // Some info }

        //private bool isWork = false;


        public ConnectionHandler(int iPort = 8005, string iInputIP = "192.168.1.119", int iMaxListen = 50)
        {
            LOG.writeLogs = true;

            // Init variables {
            serverInfo = new SomeUsefulStuff.ServerConnectionInformation(iPort, iInputIP, iMaxListen);
            // 
            try
            {
                ipPoint = new IPEndPoint(IPAddress.Parse(serverInfo.inputIP), serverInfo.port);
                listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                listenSocket.Bind(ipPoint);
                listenSocket.Listen(serverInfo.maxListen);
            }
            catch (Exception e)
            {
                LOG.printLogInFile(e.Message.ToString(), "crush");
                return;
            }
            //
            serverInfo.serverIsWork = true;
            
            LOG.printLogInFile("Server_is_start");

            clientsSocket = new List<ClientSocket>();
            threadingStorage = new List<Thread>();
            responesStorage = new List<responesToUser>();

            responesStorageMutex = new Mutex();
            mut = new Mutex();
            socketMutex = new Mutex();

            handler = new DBhandler(DBinfo.serverName, DBinfo.dataBaseName, DBinfo.userName, DBinfo.password);

            // Some threads {
            Thread vThread = new Thread(getServerInformation);
            Thread socketsThread = new Thread(listeningExistSockets);
            Thread outputThread = new Thread(outputConnectionHandler);
            // Some threads }

            //vThread.Start();

            threadingStorage.Add(vThread);
            threadingStorage.Add(socketsThread);
            threadingStorage.Add(outputThread);

            
            handler.startHandler();


            LOG.printLogInFile("DataBaseHandler_is_start");
        }

        public bool rebindIPPoint()
        {
            try
            {
                ipPoint.Address = IPAddress.Parse(serverInfo.inputIP);
                ipPoint.Port = serverInfo.port;

                listenSocket.Bind(ipPoint);
                listenSocket.Listen(serverInfo.maxListen);

                return true;
            }
            catch (Exception ex)
            {
                LOG.printLogInFile(ex.Message.ToString(), "err");
            }


            return false;
        }

        public void getServerInformation()
        {
            while (serverInfo.serverIsWork)
            {
                printServerInfo();
                Thread.Sleep(500);
            }
        }

        public void printServerInfo()
        {
            if (serverInfo != null)
            {
                serverInfo.printCurrentServerInformation();
            }
        }

        public void startWork()
        {
            foreach (Thread t in threadingStorage)
            {
                if (t.ThreadState == ThreadState.Unstarted)
                {
                    t.Start();
                }
                else if (t.ThreadState == ThreadState.Stopped)
                {
                    t.Start();
                }
            }

            inputConnectionHandler();

            
        }

        public void stopWork()
        {
            foreach (Thread t in threadingStorage)
            {
                if (t.ThreadState == ThreadState.Running)
                {
                    
                }
            }
        }

        public void workLoop()
        {
            Task mainTusk = Task.Run(() =>
            {
                while (serverInfo.serverIsWork)
                {
                    outputConnectionHandlerB();
                    listeningExistSocketsB();
                    Thread.Sleep(500);
                }
            });
            Task userTransactionsTask = Task.Run(() =>
            {
                while (serverInfo.serverIsWork)
                {
                    printServerInfo();
                    Thread.Sleep(500);
                }
            });

            inputConnectionHandler();
        }



// Service function {

        private void addInRequestStorage(int userID, string comand)
        {
            responesStorageMutex.WaitOne();

            responesStorage.Add(new responesToUser(userID, comand));

            serverInfo.increaseCurrentResponesIsWating();
            LOG.printLogInFile($"Request from user ID={userID} " +
                $"and command {comand} wass add in storage.");         

            responesStorageMutex.ReleaseMutex();
        }

        private void deleteFromRequestStorage(int userID, string comand)
        {
            responesStorageMutex.WaitOne();

            foreach (responesToUser it in responesStorage.ToList())
            {
                if ((it.userID == userID) && (it.userComand == comand))
                {
                    responesStorage.Remove(it);

                    serverInfo.decreaseCurrentResponesIsWating();
                    LOG.printLogInFile($"Request from user ID={userID} " +
                        $"and command {comand} wass remove from storage.");

                    break;
                }
            }

            responesStorageMutex.ReleaseMutex();
        }

        // Sometimes server is need to send some data to clients.
        private void addClientSocket(ref ClientSocket _socket)
        {
            if (clientsSocket.Contains(_socket))
                return;

            mut.WaitOne();

            clientsSocket.Add(_socket);

            serverInfo.increseCurrentConnectionCount();
            LOG.printLogInFile($"Client with ID={_socket.clientID} was connected.");            

            mut.ReleaseMutex();
        }

        private void deleteFromClientsSocket(int _userID)
        {
            mut.WaitOne();

            foreach (ClientSocket s in clientsSocket.ToList())
            {
                if (s.clientID == _userID)
                {
                    s.closeConnection();
                    clientsSocket.Remove(s);

                    serverInfo.decreaseCurrentConnectionCount();
                    LOG.printLogInFile($"Client with ID={_userID} was disconnected from server.");
                }
            }

            mut.ReleaseMutex();
        }

// Service function }

        public void outputConnectionHandler()
        {
            while (serverInfo.serverIsWork)
            {
                foreach (ClientSocket s in clientsSocket.ToList())
                {
                    foreach(responesToUser it in responesStorage.ToList())
                    {
                        if (s.clientID == it.userID)
                        {
                            CringeInfo temp = handler.getFromRequestStorage(it.userID, it.userComand);
                                
                            if (temp.filled)
                            {
                                if (sendMessageToClient(ref temp))
                                {
                                    LOG.printLogInFile($"Message for user ID={it.userID} " +
                                        $"with command={it.userComand} was succesful sended.");

                                    deleteFromRequestStorage(it.userID, it.userComand);                                  
                                }
                            }
                            
                        }
                    }
                }
                Thread.Sleep(500);
            }
        }

        public void outputConnectionHandlerB()
        {
            foreach (ClientSocket s in clientsSocket.ToList())
            {
                if (s.isBusy)
                    continue;
                s.isBusy = true;

                foreach (responesToUser it in responesStorage.ToList())
                {
                    if (it.isHandle)
                        continue;

                    if (s.clientID == it.userID)
                    {
                        it.isHandle = true;
                        // Task {
                        Task getInfoTask = Task.Run(() =>
                        {
                            CringeInfo temp = handler.getFromRequestStorage(it.userID, it.userComand);

                            if (temp.filled)
                            {
                                if (sendMessageToClient(ref temp))
                                {
                                    LOG.printLogInFile($"Message for user ID={it.userID} " +
                                        $"with command={it.userComand} was succesful sended.");

                                    deleteFromRequestStorage(it.userID, it.userComand);
                                }
                            }
                            s.isBusy = false;
                        });
                        // Task }
                    }
                }
                s.isBusy = false;
            }
        }

        public void listeningExistSockets()
        {
            while (serverInfo.serverIsWork)
            {
                try
                {
                    //
                    foreach (ClientSocket s in clientsSocket.ToList())
                    {
                        if (s.socketAvailable())
                        {                            
                            string ans = s.getStringFromClient();
                            commandHandler(ans);
                        }
                    }
                    //
                }
                catch (Exception e)
                {
                    LOG.printLogInFile(e.Message.ToString(), "err");
                }
                Thread.Sleep(500);
            }
        }

        public void listeningExistSocketsB()
        {
            try
            {
                //
                foreach (ClientSocket s in clientsSocket.ToList())
                {
                    if (s.isBusy)
                        continue;

                    if (s.socketAvailable())
                    {
                        s.isBusy = true;
                        Task tempTask = Task.Run(() =>
                        {
                            commandHandler(s.getStringFromClient());
                            s.isBusy = false;
                        });
                    }
                    s.isBusy = false;
                }
                //
            }
            catch (Exception e)
            {
                LOG.printLogInFile(e.Message.ToString(), "err");
            }

        }

        public void inputConnectionHandler()
        {
            //threadingStorage[1].Start();
            //threadingStorage[2].Start();
            
            while (serverInfo.serverIsWork)
            {
                try
                { 
                    //
                    Socket handler = listenSocket.Accept();
                    string comand = socketHandler(ref handler);
                    string[] reciveComandData = comand.Split(';');

                    ClientSocket s = new ClientSocket(Convert.ToInt32(reciveComandData[1]),ref handler);
                    addClientSocket(ref s);
                    
                    commandHandler(comand);

                    Thread.Sleep(500);
                    //
                 }
                 catch (Exception e)
                 {
                    LOG.printLogInFile(e.Message.ToString(), "err");
                 }
            }
                       
        }

        // Build and return string from current bytes in socket
        private string socketHandler(ref Socket fillSocket)
        {
            StringBuilder builder = new StringBuilder();
            int bytes = 0;
            byte[] data = new byte[256];

            do
            {
                bytes = fillSocket.Receive(data);
                builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
            } while (fillSocket.Available > 0);
            //Console.WriteLine(DateTime.Now.ToShortTimeString() + ": " + builder.ToString());

            return builder.ToString();
        }

        /// <summary>
        /// Command string pattern:     "comand_name;UserID;CringeID;somethingID;......":
        /// Need modify:                "action(respones/request);comand_name;UserID;CringeID;somethingID;......;@
        //      <command_name> == 'insert_cringe_in_collection'
        //          ;<User_ID>;<Cringe_ID>
        //      <comand_name> == 'get_cringe_collection'
        //          ;<User_ID>
        //      <command_name> == 'first_connection'
        //          ;<User_ID>
        //      <command_name> == 'close_connection'
        //          ;<User_ID>
        //      <command_name> == 'add_message_to_open_chat'
        //          ;<User_ID>;<Message>
        /// </summary>
        /// <param name="iCommand"></param>
        public void commandHandler(string iCommand)
        {
            string[] reciveComandData = iCommand.Split(';');
            
            switch(reciveComandData[0])
            {
                // When app first time (on it session) connect with server
                case ("first_connection"):
                    sendMessageToClient(Convert.ToInt32(reciveComandData[1]), "connected_succesful;0.");
                    break;

                // When app was close socket no need anymore
                case ("close_connection"):
                    deleteFromClientsSocket(Convert.ToInt32(reciveComandData[1]));
                    break;

                case ("insert_cringe_in_collection"):

                    break;

                case ("get_cringe_collection"):
                    handler.getCringeCollectionO(Convert.ToInt32(reciveComandData[1]), reciveComandData[0]);
                    addInRequestStorage(Convert.ToInt32(reciveComandData[1]), reciveComandData[0]);
                    break;

                case ("get_random_cringe_info"):
                    handler.getRandomCringeO(Convert.ToInt32(reciveComandData[1]), reciveComandData[0]);
                    addInRequestStorage(Convert.ToInt32(reciveComandData[1]), reciveComandData[0]);
                    break;

                case ("get_messages_in_room_on_id"):

                    break;

                

                default:
                    //sendMessageToClient(Convert.ToInt32(reciveComandData[1]), "This comand is not exist.");
                    break;
            }


        }

// Communication with client {

        public bool sendMessageToClient(int _userID, string _message)
        {
            try
            {
                //
                mut.WaitOne();

                foreach (ClientSocket s in clientsSocket)
                {
                    if (s.clientID == _userID)
                    {
                        if (!s.isBusy)
                        {
                            if (!(s.sendStringToClient(ref _message)))
                                return false;
                        }
                    }
                }

                mut.ReleaseMutex();
                //
            }
            catch (Exception e)
            {
                LOG.printLogInFile(e.Message.ToString(), "err");
                return false;
            }

            return true;
        }
              
        public bool sendMessageToClient(ref CringeInfo _respones)
        {
            try
            {
                //
                mut.WaitOne();

                foreach (ClientSocket s in clientsSocket.ToList())
                {
                    if (s.clientID == _respones.userID)
                    {
                        if (!s.isBusy)
                        {
                            if (!(s.sendInfoToClient(ref _respones)))
                                return false;
                        }
                    }
                }

                mut.ReleaseMutex();
                //
            }
            catch (Exception e)
            {
                LOG.printLogInFile(e.Message.ToString(), "err");
                return false;
            }

            return true;
        }

// Communication with client }






////////////////////////
/// that will be fun ///
////////////////////////
        private void cryptMessage(ref byte[] _outputMessage)
        {

        }
        private void deCryptMessage(ref byte[] _inputMessage)
        {

        }

    }
}
