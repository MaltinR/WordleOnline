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
        public List<TcpClient> tcpClients;
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
            public int id;
            bool end;
            Thread thread;
            public BinaryReader br;
            public BinaryWriter bw;
            TcpClient client;
            public MainWindow.Slot.Status[][] statuses;

            public ClientThread(TcpClient c, int _id)
            {
                end = false;
                client = c;
                id = _id;
                thread = new Thread(ReceiveMessage);
                statuses = new MainWindow.Slot.Status[5][];
                for(int i = 0;i < 5;i++)
                {
                    statuses[i] = new MainWindow.Slot.Status[6];

                    for(int j = 0;j < 6;j++)
                    {
                        statuses[i][j] = MainWindow.Slot.Status.Pending;
                    }
                }

                thread.Start();
            }

            public void Stop()
            {
                end = true;
                //client.Close();
                client = null;
            }

            public void Check(string receive)
            {
                int id = receive[receive.Length - 1] - '0';
                receive = receive.Substring(3, receive.Length - 4);

                MainWindow.Slot.Status[] _statuses;
                bool isValid;
                MainWindow.LogWindow.log.AddLog("[Server]Recieve word: " + receive);
                //Check
                MainWindow.mainWindow.CheckWord(receive, out isValid, out _statuses);
                //TODO Save to correspond client's data
                //Find 
                MainWindow.LogWindow.log.AddLog("[Server] Debug:" + id);

                string str = "";
                for(int i = 0;i<5;i++)
                {
                    str += _statuses[i];
                }
                MainWindow.LogWindow.log.AddLog("[Server] Debug Out:" + str);

                if (isValid)
                {
                    MainWindow.LogWindow.log.AddLog("[Server] Debug 001 " + (connection.clientThreads[id - 1].statuses.Length - 1));
                    for (int i = connection.clientThreads[id - 1].statuses[0].Length - 1; i >= -1; i--)
                    {
                        //MainWindow.LogWindow.log.AddLog("[Server] Debug 001A " + i + ":" + connection.clientThreads[id - 1].statuses[0][i]);
                        if (i == -1 || connection.clientThreads[id - 1].statuses[0][i] != MainWindow.Slot.Status.Pending)
                        {
                            //Previous
                            for (int j = 0;j < 5;j++)
                            {
                                connection.clientThreads[id - 1].statuses[j][i+1] = _statuses[j];
                            }
                            break;
                        }
                    }

                    MainWindow.LogWindow.log.AddLog("[Server] Debug 002");
                    //TODO Update UI
                    MainWindow.Slot.Status[][][] statuses = new MainWindow.Slot.Status[connection.clientThreads.Count() + 1][][];

                    //MainWindow.Slot.Status[][] _statuse

                    statuses[0] = MainWindow.mainWindow.GetStatuses();

                    for (int i = 0; i < connection.clientThreads.Count(); i++)
                    {
                        statuses[i + 1] = connection.clientThreads[i].statuses;
                    }

                    MainWindow.mainWindow.Update(statuses);

                    /*
                    MainWindow.LogWindow.log.AddLog("[Server] statuses " + statuses.Length);
                    Console.WriteLine("statuses " + statuses.Length);
                    for (int k = 0; k < statuses.Length; k++)
                    {
                        MainWindow.LogWindow.log.AddLog("[Server] statuses[" + k + "] " + statuses[k].Length);
                        Console.WriteLine("statuses[" + k + "] " + statuses[k].Length);
                        for (int i = 0; i < statuses[k].Length; i++)
                        {
                            MainWindow.LogWindow.log.AddLog("[Server] statuses[" + k + "][" + i + "] " + statuses[k][i].Length);
                            Console.WriteLine("statuses[" + k + "][" + i + "] " + statuses[k][i].Length);
                        }
                    }
                    */

                    MainWindow.LogWindow.log.AddLog("[Server] Debug 003");
                    //TODO Send to data back (To all client, bc all players need news)
                    //3D Status Array [#Player][x][y]
                    //Pick all the data and send
                    string outStr = "";

                    for (int i = 0; i < statuses.Length; i++)
                    {
                        for (int j = 0; j < statuses[i].Length; j++)
                        {
                            for (int k = 0; k < statuses[i][j].Length; k++)
                            {
                                outStr += ((int)statuses[i][j][k]).ToString();
                            }
                        }
                        outStr += ",";
                    }

                    MainWindow.LogWindow.log.AddLog("[Server]Updated");

                    foreach (TcpClient client in connection.tcpClients)
                    {
                        //NetworkStream clientStream = tcpClient.GetStream();
                        NetworkStream _clientStream = client.GetStream();
                        bw = new BinaryWriter(_clientStream);
                        bw.Write("UPD" + outStr);
                    }
                }
                else
                {
                    MainWindow.LogWindow.log.AddLog("[Server]Invalid");
                    //Invalid
                    NetworkStream _clientStream = client.GetStream();
                    bw = new BinaryWriter(_clientStream);
                    bw.Write("IVD");
                }
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

                        string receive = br.ReadString();
                        string cmd = receive.Substring(0, 3);

                        //MainWindow.LogWindow.log.AddLog("[Client]Receive: " + receive);
                        if (cmd == "DIS")
                        {
                            MainWindow.LogWindow.log.AddLog("[Server]One Client Quit");
                            //Remove from clients
                            connection.RemoveClient(client, this);
                            return;
                        }
                        else if (cmd == "CHK")
                        {
                            Thread checkThread = new Thread(()=>Check(receive));
                            checkThread.Start();

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

                    int id = clientThreads.Count + 1;//0 = server

                    clientThreads.Add(new ClientThread(tcpClients[tcpClients.Count - 1], id));


                    NetworkStream _clientStream = tcpClients[tcpClients.Count - 1].GetStream();
                    bw = new BinaryWriter(_clientStream);
                    bw.Write("CON" + id);
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
                bw.Write("DIS");
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
