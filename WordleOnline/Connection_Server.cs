using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace WordleOnline
{
    public class Connection_Server
    {
        bool isConnecting;
        public static Connection_Server connection;
        public static List<TcpClient> tcpClients;
        public List<ClientThread> clientThreads;
        static TcpListener tcpListener;
        public BinaryReader br;
        public BinaryWriter bw;
        Thread acceptClientThread;

        public void RemoveClient(TcpClient client, ClientThread thread)
        {
            client.Close();
            tcpClients.Remove(client);
            clientThreads.Remove(thread);
        }

        public class ClientThread
        {
            bool end;
            Thread thread;
            public BinaryReader br;
            public BinaryWriter bw;
            TcpClient client;

            public ClientThread(TcpClient c)
            {
                end = false;
                client = c;
                thread = new Thread(ReceiveMessage);

                thread.Start();
            }

            public void Stop()
            {
                end = true;
                //client.Close();
                client = null;
            }

            public void ReceiveMessage()
            {
                while (!end)
                {
                    try
                    {
                        //NetworkStream clientStream = tcpClient.GetStream();
                        NetworkStream clientStream = client.GetStream();
                        br = new BinaryReader(clientStream);
                        //string receive = null;
                        //receive = br.ReadString();//Read

                        int receive = br.ReadInt32();//Read int
                        //MainWindow.LogWindow.log.AddLog("[Client]Receive: " + receive);
                        if (receive < 0)//Means is command
                        {
                            if (receive == -1)
                            {
                                MainWindow.LogWindow.log.AddLog("[Server]One Client Quit");
                                //Remove from clients
                                connection.RemoveClient(client, this);
                                return;
                            }
                        }
                        else
                        {
                            MainWindow.LogWindow.log.AddLog("[Server]Receive: " + receive);
                        }
                    }
                    catch
                    {
                        //MainWindow.LogWindow.log.AddLog("[Server]Receive Failed");
                        //AddLog("Connect Failed");
                    }
                }
            }
        }

        public Connection_Server()
        {
            isConnecting = true;
            tcpClients = new List<TcpClient>();
            clientThreads = new List<ClientThread>();
            connection = this;
        }


        public void AcceptNewClient()
        {
            while (isConnecting)
            {
                try
                {
                    tcpClients.Add(tcpListener.AcceptTcpClient());//Wait for client to connect
                                                                  //AddLog("Connect Succesfully");
                    MainWindow.LogWindow.log.AddLog("[Server]Connect Succesfully");

                    clientThreads.Add(new ClientThread(tcpClients[tcpClients.Count - 1]));
                }
                catch
                {

                }
            }
        }

        public void Host()
        {
            IPAddress ip = IPAddress.Parse("127.0.0.1");//Server ip

            tcpListener = new TcpListener(ip, 25565);
            tcpListener.Start();//Start

            MainWindow.LogWindow.log.AddLog("[Server]Connection Started");
            //AddLog("Connection Started");
            //tcpListener.BeginAcceptTcpClient(new AsyncCallback(DoAcceptTcpclient), tcpListener);
            //tcpClient = tcpListener.AcceptTcpClient();//Wait for client to connect

            acceptClientThread = new Thread(AcceptNewClient);
            acceptClientThread.Start();

            //Thread messageThread = new Thread(ReceiveMessage);
            //messageThread.Start();

        }
        public void Send(string str)
        {
            foreach (TcpClient client in tcpClients)
            {
                //NetworkStream clientStream = tcpClient.GetStream();
                NetworkStream clientStream = client.GetStream();
                bw = new BinaryWriter(clientStream);
                bw.Write(str);
            }
        }

        //The Wordle data
        public void Update()
        {

        }

        public void Disconnect()
        {
            MainWindow.LogWindow.log.AddLog("[Server]Server Ended");
            MainWindow.mainWindow.ConnectEnd();
            isConnecting = false;

            foreach (TcpClient client in tcpClients)
            {
                //NetworkStream clientStream = tcpClient.GetStream();
                NetworkStream clientStream = client.GetStream();
                bw = new BinaryWriter(clientStream);
                bw.Write(-1);
                client.Close();
            }
            tcpClients.Clear();
            foreach (ClientThread thread in clientThreads)
            {
                thread.Stop();
            }
            clientThreads.Clear();

            tcpListener.Stop();
            tcpListener = null;
        }
    }
}
