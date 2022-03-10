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
        string name;
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
            public string name;
            TcpClient client;
            public MainWindow.Slot.Status[][] statuses;

            public void ResetStatuses()
            {
                for(int i = 0; i < statuses.Length;i++)
                {
                    for(int j = 0;j < statuses[i].Length;j++)
                    {
                        statuses[i][j] = MainWindow.Slot.Status.Pending;
                    }
                }
            }

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

            public void Update()
            {
                MainWindow.Slot.Status[][][] statuses = new MainWindow.Slot.Status[connection.clientThreads.Count() + 1][][];

                //MainWindow.Slot.Status[][] _statuse

                statuses[0] = MainWindow.mainWindow.GetStatuses();

                for (int i = 0; i < connection.clientThreads.Count(); i++)
                {
                    statuses[i + 1] = connection.clientThreads[i].statuses;
                }

                //MainWindow.mainWindow.Update(statuses);

                //MainWindow.LogWindow.log.AddLog("[Server] Debug 003");
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
                            Thread thread = new Thread(() => MainWindow.mainWindow.ClientDisconnect(receive[3] - '0'));
                            thread.Start();

                            MainWindow.LogWindow.log.AddLog("[Server]One Client Quit Flag01");
                            //Clients
                            foreach (TcpClient client in connection.tcpClients)
                            {
                                if (client != this.client)
                                {
                                    clientStream = client.GetStream();
                                    bw = new BinaryWriter(clientStream);
                                    bw.Write("CDC" + receive[3]);
                                }
                            }

                            return;
                        }
                        else if (cmd == "CHK")
                        {
                            Thread checkThread = new Thread(()=>Check(receive));
                            checkThread.Start();

                        }
                        else if(cmd == "NEW")
                        {
                            name = receive.Substring(3);

                            //Local
                            MainWindow.mainWindow.NewPlayer(id, name);

                            //Clients
                            foreach (TcpClient client in connection.tcpClients)
                            {
                                if (client == this.client)
                                {
                                    Thread thread = new Thread(NewComer);
                                    thread.Start();
                                }
                                else
                                {
                                    clientStream = client.GetStream();
                                    bw = new BinaryWriter(clientStream);
                                    bw.Write("NEW" + id + name);
                                }
                            }
                        }
                        else if(cmd == "ANS")
                        {
                            clientStream = client.GetStream();
                            bw = new BinaryWriter(clientStream);
                            bw.Write("ANS" + (MainWindow.mainWindow.network as MainWindow.Network_LocalAndHost).GetGoalWord());
                        }
                    }
                    catch
                    {
                        //MainWindow.LogWindow.log.AddLog("[Server]Receive Failed");
                        //AddLog("Connect Failed");
                    }
                }
            }
            public void NewComer()
            {
                MainWindow.Slot.Status[][][] statuses = new MainWindow.Slot.Status[connection.clientThreads.Count() + 1][][];

                //MainWindow.Slot.Status[][] _statuse

                statuses[0] = MainWindow.mainWindow.GetStatuses();

                for (int i = 0; i < connection.clientThreads.Count(); i++)
                {
                    statuses[i + 1] = connection.clientThreads[i].statuses;
                }

                //MainWindow.mainWindow.Update(statuses);

                //MainWindow.LogWindow.log.AddLog("[Server] Debug 003");
                //TODO Send to data back (To all client, bc all players need news)
                //3D Status Array [#Player][x][y]
                //Pick all the data and send
                string outStr = "";

                for (int i = 0; i < statuses.Length; i++)
                {
                    if (i == id) continue;
                    for (int j = 0; j < statuses[i].Length; j++)
                    {
                        for (int k = 0; k < statuses[i][j].Length; k++)
                        {
                            outStr += ((int)statuses[i][j][k]).ToString();
                        }
                    }
                    outStr += ",";
                }
                outStr += ";";

                outStr += connection.name +",";
                for(int i = 0;i < connection.clientThreads.Count;i++)
                {
                    if (i+1 == id) continue;
                    outStr += connection.clientThreads[i].name+",";
                }

                NetworkStream clientStream = client.GetStream();
                bw = new BinaryWriter(clientStream);
                bw.Write("EXT" + outStr);
            }
        }

        public void ClientDisconnect(int id)
        {
            for(int i = 0;i < clientThreads.Count;i++)
            {
                if(clientThreads[i].id > id)
                {
                    //Move
                    clientThreads[i].id--;
                }
            }
        }

        public void ServerNewGame()
        {
            //Clear clients record
            for(int i = 0;i < clientThreads.Count;i++)
            {
                clientThreads[i].ResetStatuses();
            }

            foreach (TcpClient client in tcpClients)
            {
                //NetworkStream clientStream = tcpClient.GetStream();
                NetworkStream clientStream = client.GetStream();
                bw = new BinaryWriter(clientStream);
                bw.Write("GAM");
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

                    if (tcpClients.Count < 8)
                    {

                        int id = clientThreads.Count + 1;//0 = server

                        clientThreads.Add(new ClientThread(tcpClients[tcpClients.Count - 1], id));


                        NetworkStream _clientStream = tcpClients[tcpClients.Count - 1].GetStream();
                        bw = new BinaryWriter(_clientStream);
                        bw.Write("CON" + id);

                        //TODO Send Old Data
                    }
                    else
                    {
                        NetworkStream _clientStream = tcpClients[tcpClients.Count - 1].GetStream();
                        bw = new BinaryWriter(_clientStream);
                        bw.Write("APN");
                        tcpClients[tcpClients.Count - 1].Close();
                        tcpClients.RemoveAt(tcpClients.Count - 1);
                    }
                }
                catch
                {

                }
            }
        }

        public void Host(string _name, int port)
        {
            //IPAddress ip = IPAddress.Parse("127.0.0.1");//Server ip
            //IPAddress ip = IPAddress.Parse("127.0.0.1");//Server ip

            tcpListener = new TcpListener(IPAddress.Any, port);
            tcpListener.Start();//Start

            name = _name;

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
            foreach (ClientThread client in clientThreads)
            {
                //NetworkStream clientStream = tcpClient.GetStream();
                client.Update();
            }
        }

        public void Disconnect()
        {
            MainWindow.LogWindow.log.AddLog("[Server]Server Ended");
            MainWindow.mainWindow.ConnectEnd(MainWindow.NetworkType.Host);
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
