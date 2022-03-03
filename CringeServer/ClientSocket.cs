using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;

using LOG = SomeUsefulStuff.LogginProcedures;

namespace CringeServer
{
    internal class ClientSocket
    {
        public int clientID { get; }

        private Socket clientSocket = null;

        public bool isBusy { get; set; }
        public bool isConnected { get; }
        public bool isDisconnected { get; }

        Mutex helpMut = null;


        public ClientSocket(int _userID, ref Socket _baseSocketRef)
        {
            clientID = _userID;
            clientSocket = _baseSocketRef;

            isConnected = true;
            isDisconnected = false;

            helpMut = new Mutex();
        }


       


        public string getStringFromClient()
        {
            StringBuilder builder = new StringBuilder();
            int bytes = 0;
            byte[] data = new byte[256];

            do
            {
                bytes = clientSocket.Receive(data);
                builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
            } while (clientSocket.Available > 0);


            return builder.ToString();
        }

        public byte[] getBytesFromClient()
        {
            byte[] buffer = new byte[1024];
            int bytes = 0;
            byte[] reciveBytesData = new byte[0];
            MemoryStream ms = new MemoryStream();

            do
            {
                bytes = clientSocket.Receive(buffer, buffer.Length, 0);
                ms.Write(buffer, 0, buffer.Length);

            } while (clientSocket.Available > 0);

            byte[] result = new byte[ms.Length];
            Array.Copy(ms.GetBuffer(), result, ms.Length);

            return result;
        }

        public bool sendStringToClient(ref String stringToClient)
        {
            helpMut.WaitOne();

            try
            {
                clientSocket.Send(Encoding.Unicode.GetBytes(stringToClient));
            }
            catch (Exception ex)
            {
                LOG.printLogInFile(ex.Message.ToString(), "err");
                return false;
            }
            
            helpMut.ReleaseMutex();

            return true;
        }

        public bool sendStringToClient(String stringToClient)
        {
            helpMut.WaitOne();

            try
            {
                clientSocket.Send(Encoding.Unicode.GetBytes(stringToClient));
            }
            catch (Exception ex)
            {
                LOG.printLogInFile(ex.Message.ToString(), "err");
                return false;
            }

            helpMut.ReleaseMutex();

            return true;
        }

        public bool sendBytesToClient(ref byte[] byteToClient)
        {
            try
            {
                clientSocket.Send(byteToClient);
            }
            catch (Exception ex)
            {
                LOG.printLogInFile(ex.Message.ToString(), "err");
                return false;
            }


            return true;
        }

        public bool sendBytesToClient(byte[] byteToClient)
        {
            try
            {
                clientSocket.Send(byteToClient);
            }
            catch (Exception ex)
            {
                LOG.printLogInFile(ex.Message.ToString(), "err");
                return false;
            }


            return true;
        }

        public bool sendInfoToClient(ref CringeInfo _respones)
        {
            helpMut.WaitOne();

            try
            {
                //
                sendStringToClient("start_respones");
                string finalMessage = "respones;" + _respones.command + ";" + _respones.responesString + ";";

                // if we have some bytes in message
                if ((_respones.byteDataStorage != null) && (_respones.byteDataStorage.Count > 0))
                {
                    if (clientReadyToRecive())
                        sendStringToClient(finalMessage);

                    if (clientReadyToRecive())
                        sendStringToClient("go_bytes");

                    foreach (byte[] bd in _respones.byteDataStorage)
                    {
                        sendBytesToClient(bd);
                    }

                    if (clientReadyToRecive())
                        sendStringToClient("end_bytes;");

                    if (clientReadyToRecive())
                        sendStringToClient("end_respones;");

                }
                else
                {
                    if (clientReadyToRecive())
                        sendStringToClient(finalMessage);

                    if (clientReadyToRecive())
                        sendStringToClient("end_respones");
                }

                helpMut.ReleaseMutex();
                
                return true;
                //
            }
            catch (Exception ex)
            {
                LOG.printLogInFile(ex.Message.ToString(), "err");
                helpMut.ReleaseMutex();
                return false;
            }



            return true;
        }


// Setvice func {

        private bool clientReadyToRecive()
        {
            while (true)
            {
                if (getStringFromClient() == "ready;" + Convert.ToString(clientID))
                    return true;

                if (getStringFromClient() == "close_connection;" + Convert.ToString(clientID))
                    return false;
            }

            return false;
        }

        private bool checkConnection()
        {
            if (clientSocket == null)   
                return false;


            clientSocket.Send(Encoding.Unicode.GetBytes("on_connection"));
            if (getStringFromClient() == "on_connection")
                return true;


            return false;
        }

        public bool socketAvailable()
        {
            if (clientSocket != null)
            {
                if (clientSocket.Available > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return false;
        }

        public void closeConnection()
        {
            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Close();
            clientSocket.Dispose();
        }

// Setvice func }
    }
}
