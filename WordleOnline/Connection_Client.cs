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
    public class Connection_Client
    {
        public bool end;
        TcpClient client;
        Thread thread;
        public BinaryReader br;
        public BinaryWriter bw;
        public int id;

        public bool IsConnected()
        {
            if (client == null) return false;

            return client.Connected;
        }

        public void ReceiveMessage()
        {
            while (!end)
            {
                try
                {
                    NetworkStream clientStream = client.GetStream();
                    br = new BinaryReader(clientStream);
                    //string receive = null;
                    //receive = br.ReadString();//Read
                    string receive = br.ReadString();//Read int
                    //MainWindow.LogWindow.log.AddLog("[Client]Receive:" + receive);
                    string cmd = receive.Substring(0, 3);

                    if (cmd == "DIS")
                    {
                        //MainWindow.LogWindow.log.AddLog("[Client]Server ended");
                        MainWindow.mainWindow.ConnectEnd(MainWindow.NetworkType.Client);
                        //Remove from clients
                        end = true;
                        client = null;
                        //connection.RemoveClient(client, this);
                        return;
                    }
                    else if(cmd == "UPD")
                    {
                        Thread thread = new Thread(() => Update(receive));
                        thread.Start();

                    }
                    else if(cmd == "IVD")
                    {
                        //MainWindow.LogWindow.log.AddLog("[Client]Invalid");
                        MainWindow.mainWindow.CheckReply(false, false, null);
                    }
                    else if(cmd == "CON")
                    {
                        //receive = receive.Substring(3, receive.Length - 3);

                        id = receive[3] - '0';

                        //TODO Connect
                        //MainWindow.LogWindow.log.AddLog("[Client]Server accepted");

                        MainWindow.mainWindow.Reset();
                        MainWindow.mainWindow.SetNetwork(MainWindow.NetworkType.Client);
                    }
                    else if(cmd == "APN")
                    {
                        MainWindow.mainWindow.Notification("Server is full", "#FF0000");
                        //MainWindow.LogWindow.log.AddLog("[Client]Server is full");
                    }
                    else if(cmd == "NEW")
                    {
                        MainWindow.mainWindow.NewPlayer(receive[3] - '0', receive.Substring(4));
                    }
                    else if(cmd == "EXT")//For new comer
                    {
                        Thread thread = new Thread(() => GetExist(receive.Substring(3)));
                        thread.Start();
                        //int id = receive[3] - '0';

                    }
                    else if(cmd == "GAM")//New game
                    {
                        Thread thread = new Thread(MainWindow.mainWindow.ServerNewGame);
                        thread.Start();
                        //MainWindow.mainWindow.ServerNewGame();
                    }
                    else if(cmd == "ANS")
                    {
                        Thread thread = new Thread(() => MainWindow.mainWindow.ShowAnswer(receive.Substring(3)));
                        thread.Start();
                        //MainWindow.mainWindow.ShowAnswer(receive.Substring(3));
                    }
                    else if(cmd == "CDC")
                    {
                        //MainWindow.LogWindow.log.AddLog("[Client]One client disconnected");

                        Thread thread = new Thread(() => MainWindow.mainWindow.ClientDisconnect(receive[3]-'0'));
                        thread.Start();
                    }
                    /*
                    else
                    {
                        MainWindow.LogWindow.log.AddLog("[Client]Receive: " + receive);
                    }
                    */
                }
                catch (System.Exception e)
                {
                    
                    MainWindow.LogWindow.log.AddLog("[Client]Receive Failed:"+ e.Message);
                }
            }
        }
        public void GetExist(string str)
        {
            MainWindow.LogWindow.log.AddLog("[Client]str:" + str);

            //return;
            string statusesstr = str.Substring(0,str.IndexOf(';'));
            string names = str.Substring(str.IndexOf(';')+1);

            for(int counter = 0;counter < 8;counter++)
            {
                string name = names.Substring(0, names.IndexOf(','));
                string statusstr = statusesstr.Substring(0, statusesstr.IndexOf(','));

                MainWindow.Slot.Status[][] statuses = new MainWindow.Slot.Status[5][];
                for(int i = 0; i < 5;i++)
                {
                    statuses[i] = new MainWindow.Slot.Status[6];
                }

                int x = 0, y = 0;
                for (int i = 0; i < statusstr.Length; i++)
                {
                    statuses[x][y] = (MainWindow.Slot.Status)statusstr[i] - '0';

                    y++;
                    if(y >= 6)
                    {
                        y = 0;
                        x++;
                        if(x >= 5)
                        {
                            x = 0;
                        }
                    }
                }

                MainWindow.mainWindow.SetNameAndStatuses(counter, name, statuses);

                if (names.IndexOf(',') != names.Length - 1)
                {
                    names = names.Substring(names.IndexOf(',') + 1);
                    statusesstr = statusesstr.Substring(statusesstr.IndexOf(',') + 1);
                }
                else
                {
                    break;
                }
            }
        }

        public void GetAnswer()
        {
            MainWindow.LogWindow.log.AddLog("[Client]Client Ans");

            NetworkStream clientStream = client.GetStream();
            bw = new BinaryWriter(clientStream);
            bw.Write("ANS");
        }

        public void Update(string receive)
        {
            MainWindow.LogWindow.log.AddLog("[Client]Updated");
            receive = receive.Substring(3, receive.Length - 3);
            MainWindow.LogWindow.log.AddLog("[Client]" + receive);

            int count = receive.Length / 31;//5*6+1

            MainWindow.Slot.Status[][][] statuses = new MainWindow.Slot.Status[count][][];

            for (int i = 0; i < count; i++)
            {
                statuses[i] = new MainWindow.Slot.Status[5][];
                for (int j = 0; j < 5; j++)
                {
                    statuses[i][j] = new MainWindow.Slot.Status[6];
                }
            }

            int x = 0;
            int y = 0;
            for (int i = 0; i < receive.Length; i++)
            {
                if (i % 31 != 30)
                {
                    statuses[i / 31][x][y] = (MainWindow.Slot.Status)(receive[i] - '0');


                    y++;
                    if (y >= 6)
                    {
                        y = 0;
                        x++;

                        if (x >= 5)
                        {
                            x = 0;
                        }
                    }
                }

            }

            string str = "";

            for (int i = 0; i < 5; i++)
            {
                str += statuses[1][i][0];
            }

            MainWindow.LogWindow.log.AddLog("[Client]" + str);

            MainWindow.mainWindow.Update(statuses);
        }

        public void Connect(string ip, int port, string name)
        {
            //client = new TcpClient("127.0.0.1", port);
            client = new TcpClient();
            //var result = client.BeginConnect("127.0.0.1", port, null, null);
            var result = client.BeginConnect(ip, port, null, null);
            var success = result.AsyncWaitHandle.WaitOne(System.TimeSpan.FromSeconds(5));

            MainWindow.mainWindow.isConnecting = false;
            if (!success || !client.Connected)
            {
                MainWindow.LogWindow.log.AddLog("[Client]Connect Failed");

                MainWindow.mainWindow.Notification("Connect Failed", "#FF0000");
                MainWindow.mainWindow.client = null;
            }
            else
            {
                //client = new TcpClient("127.0.0.1", port);
                MainWindow.LogWindow.log.AddLog("[Client]Connect Succesfully");
                end = false;

                thread = new Thread(ReceiveMessage);
                thread.Start();

                NetworkStream clientStream = client.GetStream();
                bw = new BinaryWriter(clientStream);
                bw.Write("NEW" + name);
            }
        }

        public void Send(string str)
        {
            NetworkStream clientStream = client.GetStream();
            bw = new BinaryWriter(clientStream);
            bw.Write(str);
        }

        //For checking word
        public void Check(string word)
        {
            NetworkStream clientStream = client.GetStream();
            bw = new BinaryWriter(clientStream);
            bw.Write("CHK"+word+id);
        }

        public void Disconnect()
        {
            //Send Somth to server to say you quit
            NetworkStream clientStream = client.GetStream();
            bw = new BinaryWriter(clientStream);
            bw.Write("DIS"+id);
            end = true;
            client.Close();
            //MainWindow.LogWindow.log.AddLog("[Client]Disconnect Succesfully");
            MainWindow.mainWindow.ConnectEnd(MainWindow.NetworkType.Client);
        }

        public int GetId()
        {
            return id;
        }
    }
}
