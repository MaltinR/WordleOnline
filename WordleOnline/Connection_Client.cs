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
    class Connection_Client
    {
        public bool end;
        TcpClient client;
        Thread thread;
        public BinaryReader br;
        public BinaryWriter bw;
        int id;

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
                    string cmd = receive.Substring(0, 3);

                    if (cmd == "DIS")
                    {
                        MainWindow.LogWindow.log.AddLog("[Client]Server ended");
                        MainWindow.mainWindow.ConnectEnd();
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
                        MainWindow.LogWindow.log.AddLog("[Client]Invalid");
                        MainWindow.mainWindow.CheckReply(false, false, null);
                    }
                    else if(cmd == "CON")
                    {
                        //receive = receive.Substring(3, receive.Length - 3);

                        id = receive[3] - '0';


                        //TODO Connect
                        MainWindow.mainWindow.SetNetwork(MainWindow.NetworkType.Client);
                    }
                    /*
                    else
                    {
                        MainWindow.LogWindow.log.AddLog("[Client]Receive: " + receive);
                    }
                    */
                }
                catch
                {
                    MainWindow.LogWindow.log.AddLog("[Client]Receive Failed");
                }
            }
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

                    if(i/31 == 1)
                        MainWindow.LogWindow.log.AddLog("[Client] Debug ["+x+"]["+y+"] receive[" + i + "]");

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

        public void Connect()
        {
            client = new TcpClient("127.0.0.1", 25565);
            MainWindow.LogWindow.log.AddLog("[Client]Connect Succesfully");
            end = false;

            thread = new Thread(ReceiveMessage);
            thread.Start();
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
            bw.Write("DIS");
            end = true;
            client.Close();
            MainWindow.LogWindow.log.AddLog("[Client]Disconnect Succesfully");
            MainWindow.mainWindow.ConnectEnd();
        }

        public int GetId()
        {
            return id;
        }
    }
}
