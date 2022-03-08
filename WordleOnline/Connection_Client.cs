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
                    int receive = br.ReadInt32();//Read int
                    if (receive < 0)//Means is command
                    {
                        if (receive == -1)
                        {
                            MainWindow.LogWindow.log.AddLog("[Client]Server ended");
                            MainWindow.mainWindow.ConnectEnd();
                            //Remove from clients
                            end = true;
                            client = null;
                            //connection.RemoveClient(client, this);
                            return;
                        }
                    }
                    else
                    {
                        MainWindow.LogWindow.log.AddLog("[Client]Receive: " + receive);
                    }
                }
                catch
                {
                    MainWindow.LogWindow.log.AddLog("[Client]Receive Failed");
                }
            }
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
            bw.Write(word);
        }

        public void Disconnect()
        {
            //Send Somth to server to say you quit
            NetworkStream clientStream = client.GetStream();
            bw = new BinaryWriter(clientStream);
            bw.Write(-1);
            end = true;
            client.Close();
            MainWindow.LogWindow.log.AddLog("[Client]Disconnect Succesfully");
            MainWindow.mainWindow.ConnectEnd();
        }
    }
}
