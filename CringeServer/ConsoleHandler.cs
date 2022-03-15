using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace CringeServer
{
        public static class appInforamation
        {
            public static class serverInfo
            {
                public static int port = 8005;
                public static string inputIP = "192.168.0.1";

                public static int currentConnectionCount = 0;
                public static int currentResponesIsWating = 0;

                public static bool serverIsWork = false;

                public static List<int> availablePorts = new List<int>();
                public static List<string> availableIP = new List<string>();
                public static int maxListeners = 50;

                public static void changeIP(ref string _ip)
                {
                    inputIP = _ip;
                    redrawInfo("ipv4");
                }
                public static void changePort(ref int _port)
                {
                    port = _port;
                    redrawInfo("port");
                }
                public static void changeServerWorkStatus(ref bool _state)
                {
                    serverIsWork = _state;
                    redrawInfo("work_state");
                }

                public static string getCurrentMachineIP(string _protocol = "IPv4")
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

                //
                public static void dropCurrentConnectionCount()
                {
                    currentConnectionCount = 0;
                    redrawInfo("connection_count");
                }
                public static void increseCurrentConnectionCount()
                {
                    ++currentConnectionCount;
                    redrawInfo("connection_count");
                }
                public static void decreaseCurrentConnectionCount()
                {
                    --currentConnectionCount;
                    redrawInfo("connection_count");
                }
                //
                //
                public static void dropCurrentResponesIsWating()
                {
                    currentResponesIsWating = 0;
                    redrawInfo("respones");
                }
                public static void increaseCurrentResponesIsWating()
                {
                    ++currentResponesIsWating;
                    redrawInfo("respones");
                }
                public static void decreaseCurrentResponesIsWating()
                {
                    --currentResponesIsWating;
                    redrawInfo("respones");
                }
            }

            public static class dbInfo
            {
                public static string serverName = @"MyPC\SQLEXPRESS";
                public static string dataBaseName = "CringeDataBase";
                public static string userName = @"BigBoss";
                public static string password = "11112222";

                public static int requestInStorage = 0;
                public static int responesInStorage = 0;
                public static bool dbAPIworkState = false;

                public static void increaseDBRespones()
                {
                    ++responesInStorage;
                    redrawInfo("resp", "db");
                }
                public static void decreaseDBRespones()
                {
                    --responesInStorage;
                    redrawInfo("resp", "db");
                }
                //
                public static void increaseBDRequest()
                {
                    ++requestInStorage;
                    redrawInfo("req", "db");
                }
                public static void decreaseBDRequest()
                {
                    ++requestInStorage;
                    redrawInfo("req", "db");
                }
                //
                public static void changeBDWorkState(ref bool _workState)
                {
                    dbAPIworkState = _workState;
                    redrawInfo("work_state", "db");
                }
            }

            public static class appInfo
            {
                public static int crashCount = 0;
                public static int logCount = 0;
                public static int errorCount = 0;

                public static void increaseLogCount()
                {
                    ++logCount;
                    redrawInfo("log", "app");
                }
                public static void dropLogCount()
                {
                    logCount = 0;
                    redrawInfo("log", "app");
                }
                //
                public static void increaseCrashCount()
                {
                    ++crashCount;
                    redrawInfo("crash", "app");
                }
                public static void dropCrashCount()
                {
                    crashCount = 0;
                    redrawInfo("crash", "app");
                }
                //
                public static void increaseErrorCount()
                {
                    ++errorCount;
                    redrawInfo("err", "app");
                }
                public static void dropErrorCount()
                {
                    errorCount = 0;
                    redrawInfo("err", "app");
                }
                //
                public static string getCurrentThreadCount(string _param = "max")
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
            }


            public static bool writeLogs { get; set; }

            private static Mutex consoleMutex = new Mutex();

            private static Dictionary<string, string> serverInfoStrings = new Dictionary<string, string>();
            private static Dictionary<string, string> dbInfoStrings = new Dictionary<string, string>();
            private static Dictionary<string, string> appInfoStrings = new Dictionary<string, string>();

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
                    appInfo.increaseLogCount();
                    redrawInfo("log", "app");
                }
                else if (_param == "err")
                {
                    endDirectory = @"\WorkLogs";
                    logWord = "::ERROR::";
                    appInfo.increaseErrorCount();
                    redrawInfo("err", "app");
                }
                else if (_param == "crash")
                {
                    endDirectory = @"\CrushLogs";
                    logWord = "::CRUSH::";
                    appInfo.increaseCrashCount();
                    redrawInfo("crash", "app");
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
                    File.AppendAllText(pathToFile, "************************ERROR************************\n");

                File.AppendAllText(pathToFile, (DateTime.Now.ToLongTimeString() + logWord + _logMessage + "\n"));

                if (_param == "err")
                    File.AppendAllText(pathToFile, "*****************************************************\n");
                //
            }
            public static void printAllInformation()
            {
                Console.Clear();

                foreach (var el in serverInfoStrings)
                {
                    Console.WriteLine(el.Value);
                }

                foreach (var el in dbInfoStrings)
                {
                    Console.WriteLine(el.Value);
                }

                foreach (var el in appInfoStrings)
                {
                    Console.WriteLine(el.Value);
                }
            }

            // Prepare string fo output in console
            public static void prepareConsoleOutput()
            {
                // Server info preparations
                serverInfoStrings.Clear();
                serverInfoStrings.Add("first_sep", "///////////////////////////////////////////////////////////");             //0
                serverInfoStrings.Add("title", "_______________________SERVER INFO_________________________");             //1
                serverInfoStrings.Add("ipv4", $"Current server IPv4:\t\t{serverInfo.getCurrentMachineIP()}");                        //2
                serverInfoStrings.Add("ipv6", $"Current server IPv6:\t\t{serverInfo.getCurrentMachineIP("IPv6")}");                  //3
                serverInfoStrings.Add("port", $"Current server port:\t\t{serverInfo.port}");                              //4
                serverInfoStrings.Add("work_state", $"Connection handler is work:\t" + serverInfo.serverIsWork.ToString());     //5
                serverInfoStrings.Add("connection_count", $"Current connection count:\t{serverInfo.currentConnectionCount}");         //6
                serverInfoStrings.Add("respones", $"Current respones is waiting:\t{serverInfo.currentResponesIsWating}");     //7

                serverInfoStrings.Add("end_sep", "///////////////////////////////////////////////////////////");             //8


                // Data base preparations
                dbInfoStrings.Clear();
                dbInfoStrings.Add("first_sep", "///////////////////////////////////////////////////////////");     //0
                dbInfoStrings.Add("title", "_______________________DATABASE INFO_______________________");     //1
                dbInfoStrings.Add("req", $"Current query from users:\t{dbInfo.requestInStorage}");           //2
                dbInfoStrings.Add("resp", $"Current respones to users:\t{dbInfo.responesInStorage}");         //3
                dbInfoStrings.Add("work_state", $"Current work status:\t{dbInfo.dbAPIworkState}");                  //4

                dbInfoStrings.Add("end_sep", "///////////////////////////////////////////////////////////");     //5

                // App info preparations
                appInfoStrings.Clear();
                appInfoStrings.Add("first_sep", "///////////////////////////////////////////////////////////");     //0
                appInfoStrings.Add("title", "________________________APP INFO___________________________");     //1
                appInfoStrings.Add("log", $"Current log count:\t\t{appInfo.logCount}");                          //2
                appInfoStrings.Add("err", $"Current error count:\t\t{appInfo.errorCount}");                      //3
                appInfoStrings.Add("crash", $"Current crash count:\t\t{appInfo.crashCount}");                      //4

                appInfoStrings.Add("end_sep", "///////////////////////////////////////////////////////////");     //5
            }


            // Console out funcs {
            private static (int, int) getXYOfNumberInString(string _key, ref Dictionary<string, string> _dict)
            {
                int x = 0, y = 0;

                for (int i = 0; i < _dict.Count; ++i)
                {
                    if (_dict.ToArray()[i].Key == _key)
                    {
                        x = i;
                        foreach (var ch in _dict.ToArray()[i].Value)
                        {
                            if (ch == '\t')
                            {
                                for (int j = 8; j > 0; --j)
                                {
                                    if (((y + j) % 8) == 0)
                                    {
                                        y += j;
                                        break;
                                    }
                                }
                                continue;
                            }
                            ++y;
                        }

                    }
                }

                //forget last symbol
                --y;
                return (x, y);
            }


            /// <summary>
            /// If source == "server" -> 
            /// _keyValue must be:
            /// "ipv4", "ipv6", "threads", "errors", "crashs", "logs", "work_state", 
            /// "connection_count", "respones"
            /// If source == "db" ->
            /// _keyValue must be:
            /// "req", "resp", "work_state
            /// If source == "app"
            /// _keyValue must be:
            /// "log", "err", "crash"
            /// </summary>
            /// <param name="_keyValue"> </param>
            /// <param name="_source"></param>
            private static void redrawInfo(string _keyValue, string _source = "server")
            {
                // User cursor position
                int xu, yu;

                consoleMutex.WaitOne();
                (xu, yu) = Console.GetCursorPosition();

                int x, y;

                if (_source == "server")
                {
                    (x, y) = getXYOfNumberInString(_keyValue, ref serverInfoStrings);
                if (x < 0 || y < 0)
                    return;

                    Console.SetCursorPosition(y, x);
                }
                else if (_source == "db")
                {
                    (x, y) = getXYOfNumberInString(_keyValue, ref dbInfoStrings);
                if (x < 0 || y < 0)
                    return;

                Console.SetCursorPosition(y, x + serverInfoStrings.Count);
                }
                else if (_source == "app")
                {
                    (x, y) = getXYOfNumberInString(_keyValue, ref appInfoStrings);
                if (x < 0 || y < 0)
                    return;

                Console.SetCursorPosition(y, x + serverInfoStrings.Count + dbInfoStrings.Count);
                }
                else
                    return;


                if (_source == "server")
                {
                    switch (_keyValue)
                    {
                        case "ipv4":
                            Console.Write(serverInfo.getCurrentMachineIP());
                            break;

                        case "ipv6":
                            Console.Write(serverInfo.getCurrentMachineIP("IPv6"));
                            break;

                        case "work_state":
                            Console.Write(serverInfo.serverIsWork.ToString());
                            break;

                        case "connection_count":
                            Console.Write(serverInfo.currentConnectionCount);
                            break;

                        case "respones":
                            Console.Write(serverInfo.currentResponesIsWating);
                            break;

                        default:

                            break;
                    }
                }
                else if (_source == "db")
                {
                    switch (_keyValue)
                    {
                        case "req":
                            Console.Write(dbInfo.requestInStorage);
                            break;

                        case "resp":
                            Console.Write(dbInfo.responesInStorage);
                            break;

                        case "work_state":
                            Console.Write(dbInfo.dbAPIworkState);
                            break;

                        default:

                            break;
                    }
                }
                else if (_source == "app")
                {
                    switch (_keyValue)
                    {
                        case "log":
                            Console.Write(appInfo.logCount);
                            break;

                        case "err":
                            Console.Write(appInfo.errorCount);
                            break;

                        case "crash":
                            Console.Write(appInfo.crashCount);
                            break;

                        case "thread":
                            Console.Write(appInfo.getCurrentThreadCount());
                            break;

                        default:
                            break;
                    }
                }

                Console.SetCursorPosition(xu, yu);

                consoleMutex.ReleaseMutex();

            }

        }
    
}
