using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
   

namespace CringeServer
{




    class ServerApp
    {
        /*static int port = 6666;
        static string inputIP = "192.168.0.1";
        static int maxListen = 50;

        static string serverName = @"MyPC\SQLEXPRESS";
        static string dbName = "CringeDataBase";
        static string username = @"BigBoss";
        static string password = "11112222";*/

        static void Main(string[] args)
        {
            try
            {
                ConnectionHandler handler = new ConnectionHandler();
                //handler.inputConnectionHandler();
                //handler.startWork();
                handler.workLoop();
                //Thread.Sleep(1000);
                //handler.stopWork(3);
                //handler.continueWork();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
            }

            /*DBhandler dbH = new DBhandler(serverName, dbName, username, password);
            dbH.addQueryInPool(1, "SELECT* FROM CringeDataBase.dbo.CringeCollection", "get_cringe_collection");
            dbH.startHandler();

            Thread.Sleep(5000);
            string s = dbH.getFromRequestPool(1, "get_cringe_collection");
            Console.WriteLine(s);*/
      
            Thread.CurrentThread.Join();
        }
    }
}
