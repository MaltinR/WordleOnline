using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Threading;

namespace WordleOnline
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //For debug
        public class LogWindow
        {
            public static LogWindow log;
            public Window window;
            TextBlock text;
            public LogWindow()
            {
                window = new Window();
                window.Width = 300;
                window.Height = 600;

                Canvas canvas = new Canvas();
                canvas.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#d3d6da"));
                //canvas.Margin = new Thickness(100, 600, 100, 600);
                text = new TextBlock();
                //text.Width = 100;
                //text.Height = 600;
                text.Text = "";
                Canvas.SetLeft(text, 0);
                Canvas.SetTop(text, 0);

                canvas.Children.Add(text);
                window.Content = canvas;
                //window.Show();

                log = this;
            }

            public void AddLog(string str)
            {
                text.Dispatcher.Invoke(() => text.Text += str + "\n");

            }

            public void ClearLog()
            {
                text.Text = "";
            }
        }

        public enum NetworkType
        {
            Local,
            Host,
            Client
        }
        public NetworkType networkType;

        public class Network
        {
            public virtual bool CheckWord(char[] input, out Slot.Status[] statuses)
            {
                statuses = new Slot.Status[5];
                return false;
            }

            public virtual string GetAnswer()
            {
                return "";
            }
        }
        public class Network_LocalAndHost : Network
        {
            public char[] goalWord = new char[5];
            public int[] charCount = new int[26];

            public void NewGoalWord(string word)
            {
                for (int i = 0; i < 5; i++)
                {
                    goalWord[i] = word[i];
                }

                for (int i = 0; i < 26; i++)
                {
                    charCount[i] = 0;
                    for (int j = 0; j < 5; j++)
                    {
                        if (goalWord[j] == 'a' + i)
                        {
                            charCount[i]++;
                        }
                    }
                }
            }

            public Network_LocalAndHost()
            {
            }

            public override string GetAnswer()
            {
                string outStr = "";

                for(int i = 0;i < 5;i++)
                {
                    outStr += goalWord[i];
                }
                return outStr;
            }
            public override bool CheckWord(char[] input, out Slot.Status[] statuses)
            {
                Slot.Status[] _statuses = new Slot.Status[5];

                bool match = true;
                for(int i = 0;i < 5;i++)
                {
                    match = match && input[i] == goalWord[i];
                    if(input[i] == goalWord[i])
                    {
                        _statuses[i] = Slot.Status.Exact;
                    }
                    else
                    {
                        if(charCount[input[i] - 'a'] == 0)
                        {
                            _statuses[i] = Slot.Status.Wrong;
                        }
                        //!= 0 -> >0
                        else
                        {

                            int leftCount = 0;
                            int leftCorrectCount = 0;
                            int rightCount = 0;
                            //int rightCount = 0;

                            for(int j = i -1;j >= 0;j--)
                            {
                                if (input[j] == input[i])
                                {
                                    leftCount++;
                                    if (goalWord[j] == input[j])
                                        leftCorrectCount++;
                                }
                            }
                            for(int j = i + i;j < 5;j++)
                            {
                                if (input[j] == input[i] && goalWord[j] == input[j])
                                    rightCount++;
                            }

                            //
                            if (leftCorrectCount + rightCount == charCount[input[i] - 'a'])
                            {
                                _statuses[i] = Slot.Status.Wrong;
                            }
                            else
                            {
                                //
                                if (leftCount + rightCount == charCount[input[i] - 'a'])
                                {
                                    _statuses[i] = Slot.Status.Wrong;
                                }
                                //
                                else
                                {
                                    _statuses[i] = Slot.Status.Position;
                                }
                            }
                        }

                    }
                }

                statuses = _statuses;

                return match;
            }
        }
        public class Network_Local : Network_LocalAndHost
        {
            public Network_Local() : base()
            {
            }
        }
        public class Network_Host : Network_LocalAndHost
        {

        }
        public class Network_Client : Network
        {

        }
        //Each slot
        public class Slot
        {
            public enum Status
            {
                Exact,
                Position,
                Wrong,
                Pending
            }

            public Char character;
            public TextBlock text;
            //Background
            public Border background;

            public Slot()
            {
                background = new Border();
                background.Margin = new Thickness(2);
                background.BorderThickness = new Thickness(2);
                background.BorderBrush = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#d3d6da"));
                text = new TextBlock();
                text.Text = "";
                text.FontSize = 30;
                text.FontWeight = FontWeights.Bold;
                text.HorizontalAlignment = HorizontalAlignment.Center;
                text.VerticalAlignment = VerticalAlignment.Center;
            }

            public void Reset()
            {
                background.BorderThickness = new Thickness(2);
                background.BorderBrush = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#d3d6da"));
                text.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#000000"));
                background.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFF"));
                text.Text = "";
            }

            public void SetStatus(Status status)
            {
                switch(status)
                {
                    case Status.Exact:
                        background.BorderThickness = new Thickness(0);
                        background.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#6aaa64"));
                        text.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFF"));
                        break;
                    case Status.Pending:
                        background.BorderThickness = new Thickness(2);
                        background.BorderBrush = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#d3d6da"));
                        text.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#000000"));
                        break;
                    case Status.Position:
                        background.BorderThickness = new Thickness(0);
                        background.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#c9b458"));
                        text.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFF"));
                        break;
                    case Status.Wrong:
                        background.BorderThickness = new Thickness(0);
                        background.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#787c7e"));
                        text.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFF"));
                        break;
                }
            }
        }

        public class PreviewSlot
        {
            public class Slot
            {
                public Border background;

                public Slot()
                {
                    background = new Border();
                    background.Margin = new Thickness(1);
                    background.BorderThickness = new Thickness(1);
                    background.BorderBrush = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#d3d6da"));
                }

                public void SetStatus(MainWindow.Slot.Status status)
                {
                    switch(status)
                    {
                        case MainWindow.Slot.Status.Exact:
                            background.BorderThickness = new Thickness(0);
                            background.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6aaa64"));
                            break;
                        case MainWindow.Slot.Status.Pending:
                            background.BorderThickness = new Thickness(1);
                            background.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#d3d6da"));
                            break;
                        case MainWindow.Slot.Status.Position:
                            background.BorderThickness = new Thickness(0);
                            background.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#c9b458"));
                            break;
                        case MainWindow.Slot.Status.Wrong:
                            background.BorderThickness = new Thickness(0);
                            background.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#787c7e"));
                            break;
                    }
                }
            }

            public Slot[][] slots;
            public Grid grid;
            public TextBlock name;

            public PreviewSlot()
            {
                //15
                grid = new Grid();

                for (int i = 0; i < 5; i++)
                {
                    ColumnDefinition tempColumn = new ColumnDefinition();
                    grid.ColumnDefinitions.Add(tempColumn);
                }
                for (int i = 0; i < 6; i++)
                {
                    RowDefinition tempRow = new RowDefinition();
                    grid.RowDefinitions.Add(tempRow);
                }
                //Init each slot
                slots = new Slot[5][];
                for (int i = 0; i < 5; i++)
                {
                    slots[i] = new Slot[6];
                    for (int j = 0; j < 6; j++)
                    {
                        slots[i][j] = new Slot();

                        Grid.SetRow(slots[i][j].background, j);
                        Grid.SetColumn(slots[i][j].background, i);

                        grid.Children.Add(slots[i][j].background);
                    }
                }

                name = new TextBlock();
                name.FontSize = 12;
                name.Text = "Martin";
                name.HorizontalAlignment = HorizontalAlignment.Center;
                name.Width = 75;
                name.Margin = new Thickness(0, 10, 0, 0);
            }

            //
            public void SetSlots(MainWindow.Slot.Status[][] statuses)
            {
                for (int i = 0; i < 5; i++)
                {
                    for (int j = 0; j < 6; j++)
                    {
                        slots[i][j].SetStatus(statuses[i][j]);
                    }
                }
            }
        }

        public class Keyboard
        {
            public Canvas canvas;
            public Keyboard_Key[] keys;
            public Grid[] grids;

            public class Keyboard_Key
            {
                public Border background;
                public TextBlock text;
                public Slot.Status status;

                public Keyboard_Key(char _char)
                {
                    background = new Border();
                    text = new TextBlock();

                    background.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#d3d6da"));
                    background.Margin = new Thickness(1);
                    text.Text = _char.ToString();
                    text.HorizontalAlignment = HorizontalAlignment.Center;
                    text.VerticalAlignment = VerticalAlignment.Center;
                    text.FontSize = 18;
                    status = Slot.Status.Pending;
                }

                public void Set(Slot.Status _status)
                {
                    switch(_status)
                    {
                        case Slot.Status.Exact:
                            background.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#6aaa64"));
                            text.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFF"));
                            break;
                        case Slot.Status.Pending:
                            background.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#d3d6da"));
                            text.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#000000"));
                            break;
                        case Slot.Status.Position:
                            background.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#c9b458"));
                            text.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFF"));
                            break;
                        case Slot.Status.Wrong:
                            background.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#787c7e"));
                            text.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFF"));
                            break;
                    }
                    status = _status;
                }

                public void Reset()
                {
                    background.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#d3d6da"));
                    text.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#000000"));
                    status = Slot.Status.Pending;
                }
            }
            public Keyboard()
            {
                keys = new Keyboard_Key[26];
                grids = new Grid[3];

                for(int i = 0; i < 3;i++)
                {
                    grids[i] = new Grid();
                    //grids[i].Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#d3d6da"));
                    grids[i].Height = 35;
                }
                grids[0].Width = 300;
                grids[1].Width = 270;
                grids[2].Width = 210;
                grids[0].Margin = new Thickness(0, 0, 0, 0);
                grids[1].Margin = new Thickness(15, 35, 0, 0);
                grids[2].Margin = new Thickness(45, 70, 0, 0);

                for (int i = 0; i < 10; i++)
                {
                    ColumnDefinition tempColumn = new ColumnDefinition();
                    grids[0].ColumnDefinitions.Add(tempColumn);
                }
                for (int i = 0; i < 9; i++)
                {
                    ColumnDefinition tempColumn = new ColumnDefinition();
                    grids[1].ColumnDefinitions.Add(tempColumn);
                }
                for (int i = 0; i < 7; i++)
                {
                    ColumnDefinition tempColumn = new ColumnDefinition();
                    grids[2].ColumnDefinitions.Add(tempColumn);
                }

                for(int i = 0; i < 26;i++)
                {
                    keys[i] = new Keyboard_Key((char)('A' + i));
                }

                #region KeyboardAssignment
                Assign(keys['a' - 'a'],1,0);
                Assign(keys['b' - 'a'],2,4);
                Assign(keys['c' - 'a'],2,2);
                Assign(keys['d' - 'a'],1,2);
                Assign(keys['e' - 'a'],0,2);
                Assign(keys['f' - 'a'],1,3);
                Assign(keys['g' - 'a'],1,4);
                Assign(keys['h' - 'a'],1,5);
                Assign(keys['i' - 'a'],0,7);
                Assign(keys['j' - 'a'],1,6);
                Assign(keys['k' - 'a'],1,7);
                Assign(keys['l' - 'a'],1,8);
                Assign(keys['m' - 'a'],2,6);
                Assign(keys['n' - 'a'],2,5);
                Assign(keys['o' - 'a'],0,8);
                Assign(keys['p' - 'a'],0,9);
                Assign(keys['q' - 'a'],0,0);
                Assign(keys['r' - 'a'],0,3);
                Assign(keys['s' - 'a'],1,1);
                Assign(keys['t' - 'a'],0,4);
                Assign(keys['u' - 'a'],0,6);
                Assign(keys['v' - 'a'],2,3);
                Assign(keys['w' - 'a'],0,1);
                Assign(keys['x' - 'a'],2,1);
                Assign(keys['y' - 'a'],0,5);
                Assign(keys['z' - 'a'],2,0);
                #endregion

                canvas = new Canvas();

                Canvas.SetLeft(canvas, 335 + 70);
                Canvas.SetTop(canvas, 250);

                for (int i = 0; i < 3; i++)
                {
                    grids[i].HorizontalAlignment = HorizontalAlignment.Right;
                    grids[i].VerticalAlignment = VerticalAlignment.Bottom;
                    //grids[i].Margin = new Thickness(25, 25, 0, 0);
                    canvas.Children.Add(grids[i]);
                }

            }

            public void Reset()
            {
                for (int i = 0; i < 26; i++)
                {
                    keys[i].Reset();
                }
            }

            public void Assign(Keyboard_Key key, int layer, int index)
            {
                Grid.SetColumn(key.background, index);
                Grid.SetColumn(key.text, index);
                grids[layer].Children.Add(key.background);
                grids[layer].Children.Add(key.text);
            }
        }

        int currentTry;//Y
        int currentIndex;//X
        char[] currentWord;
        Slot[][] slots;
        Network network;
        Keyboard keyboard;
        List<string> dictionary;
        Random rand;
        bool isFinished;
        bool isFocusingText;
        LogWindow logWindow;
        Connection_Server server;
        Connection_Client client;
        public static MainWindow mainWindow;
        PreviewSlot[] previewSlots;

        public MainWindow()
        {
            InitializeComponent();
        }
        
        //New guess (Local/ Host only)
        //DEBUG: STAYS FiRST Line
        public void New()
        {
            currentTry = 0;
            currentIndex = 0;
            isFinished = false;
            isFocusingText = false;

            Window.Activate();
            //System.Windows.Input.Keyboard.ClearFocus();

            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    slots[i][j].Reset();
                }
            }

            keyboard.Reset();

            string randWord = dictionary[rand.Next(0, dictionary.Count)];
            (network as Network_LocalAndHost).NewGoalWord(randWord);

            Notification("Status: Local", "#000000");
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            mainWindow = this;

            networkType = NetworkType.Local;
            network = new Network_Local();
            Grid guessGrid = new Grid();
            rand = new Random();

            keyboard = new Keyboard();
            mainCanvas.Children.Add(keyboard.canvas);

            currentWord = new char[5];
            guessGrid.Width = 300;
            guessGrid.Height = 360;

            #region OnlineUI

            for (int i = 0; i < 4; i++)
            {
                ColumnDefinition tempColumn = new ColumnDefinition();
                OnlineGrid.ColumnDefinitions.Add(tempColumn);
            }
            for (int i = 0; i < 2; i++)
            {
                RowDefinition tempRow = new RowDefinition();
                OnlineGrid.RowDefinitions.Add(tempRow);
            }

            previewSlots = new PreviewSlot[8];
            for(int i =0;i<8;i++)
            {
                Canvas canvas = new Canvas();

                //canvas.Width = 75;
                //canvas.Height = 90;

                previewSlots[i] = new PreviewSlot();
                Grid.SetColumn(canvas, i%4);
                Grid.SetRow(canvas, i/4);
                //canvas.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#333333"));
                previewSlots[i].grid.Width = 75;
                previewSlots[i].grid.Height = 90;
                previewSlots[i].grid.HorizontalAlignment = HorizontalAlignment.Center;
                previewSlots[i].grid.VerticalAlignment = VerticalAlignment.Bottom;
                previewSlots[i].grid.Margin = new Thickness(0, 30, 0, 0);
                //OnlineGrid.Children.Add(previewSlots[i].grid);

                canvas.Children.Add(previewSlots[i].grid);
                canvas.Children.Add(previewSlots[i].name);

                OnlineGrid.Children.Add(canvas);
                canvas.Visibility = (Visibility)rand.Next(0, 2);

                //Test
                Slot.Status[][] _statuses = new Slot.Status[5][];
                for(int j = 0;j < 5;j++)
                {
                    _statuses[j] = new Slot.Status[6];
                    for(int k = 0; k < 6;k++)
                    {
                        _statuses[j][k] = (Slot.Status)rand.Next(0, 4);
                    }
                }

                previewSlots[i].SetSlots(_statuses);
            }

            #endregion

            dictionary = new List<string>();
            //Read from dictionary.txt
            string dictPath = System.Reflection.Assembly.GetEntryAssembly().Location;
            dictPath = dictPath.Substring(0, dictPath.Length - (1 + System.IO.Path.GetFileName(dictPath).Length));
            dictPath += @"\Dictionary\dictionary.txt";

            Notification("Cannot read dictionary file, please restart", "#FF0000");

            string[] lines = System.IO.File.ReadAllLines(dictPath);
            foreach (string line in lines)
            {
                // Use a tab to indent each line of the file.
                //Console.WriteLine("\t" + line);
                //statusText.Text = line;
                dictionary.Add(line);
            }

            Notification("Status: Local", "#000000");

            //Debug
            logWindow = new LogWindow();

            for (int i = 0;i < 5;i++)
            {
                ColumnDefinition tempColumn = new ColumnDefinition();
                guessGrid.ColumnDefinitions.Add(tempColumn);
            }
            for (int i = 0; i < 6; i++)
            {
                RowDefinition tempRow = new RowDefinition();
                guessGrid.RowDefinitions.Add(tempRow);
            }

            //Init each slot
            slots = new Slot[5][];
            for (int i = 0;i < 5;i++)
            {
                slots[i] = new Slot[6];
                for (int j =0;j < 6;j++)
                {
                    slots[i][j] = new Slot();

                    Grid.SetRow(slots[i][j].background, j);
                    Grid.SetColumn(slots[i][j].background, i);
                    Grid.SetRow(slots[i][j].text, j);
                    Grid.SetColumn(slots[i][j].text, i);

                    guessGrid.Children.Add(slots[i][j].background);
                    guessGrid.Children.Add(slots[i][j].text);
                }
            }

            //guessGrid.Margin = 
            guessGrid.HorizontalAlignment = HorizontalAlignment.Left;
            guessGrid.VerticalAlignment = VerticalAlignment.Top;
            guessGrid.Margin = new Thickness(25, 25, 0, 0);

            mainCanvas.Children.Add(guessGrid);

            Window.Content = mainCanvas;

            New();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void KeyboardDown(object sender, KeyEventArgs e)
        {
            if (!isFinished)
            {
                //enter
                //backspace
                //a-z
                if (e.Key == Key.Escape)
                {
                    if (isFocusingText)
                    {
                        System.Windows.Input.Keyboard.Focus(mainCanvas);

                        statusText.Text = "Status: Local";
                        statusText.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#000000"));
                        isFocusingText = false;
                    }
                }
                else if (e.Key == Key.Return || e.Key == Key.Enter)
                {
                    if (currentIndex == 5 && currentTry <= 5)
                    {
                        string input = "";
                        for (int i = 0; i < 5; i++)
                        {
                            input += currentWord[i];
                        }

                        //if (dictionary.Contains(currentWord.ToString()))
                        //if (dictionary.Contains(currentWord.ToString().ToLower()))
                        if (dictionary.Contains(input.ToLower()))
                        {
                            //Check
                            Slot.Status[] outStatuses;

                            isFinished = network.CheckWord(currentWord, out outStatuses);

                            for (int i = 0; i < 5; i++)
                            {
                                slots[i][currentTry].SetStatus(outStatuses[i]);

                                switch (keyboard.keys[currentWord[i] - 'a'].status)
                                {
                                    case Slot.Status.Exact:
                                        break;
                                    case Slot.Status.Pending:
                                        keyboard.keys[currentWord[i] - 'a'].Set(outStatuses[i]);
                                        break;
                                    case Slot.Status.Position:
                                        if (outStatuses[i] == Slot.Status.Exact)
                                            keyboard.keys[currentWord[i] - 'a'].Set(outStatuses[i]);
                                        break;
                                    case Slot.Status.Wrong:
                                        break;
                                }

                            }
                            currentTry++;
                            currentIndex = 0;

                            statusText.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#000000"));
                            if (currentTry > 5 && !isFinished)
                            {
                                statusText.Text = "The anwser is " + network.GetAnswer().ToUpper();
                            }
                            else
                            {
                                statusText.Text = "Status: Local";
                            }
                        }
                        else
                        {
                            statusText.Text = "Invalid word";
                            statusText.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF0000"));
                        }
                    }
                }
                else if (e.Key == Key.Back)
                {
                    if (!isFocusingText)
                    {
                        if (currentIndex > 0)
                        {
                            currentIndex--;

                            slots[currentIndex][currentTry].text.Text = "";
                        }
                    }
                }
                else
                {
                    if (!isFocusingText)
                    {
                        for (int i = 0; i < 26; i++)
                        {
                            if (e.Key == (Key)((int)Key.A + i))
                            {
                                if (currentTry <= 5 && currentIndex <= 4)
                                {
                                    currentWord[currentIndex] = (char)((int)'a' + i);
                                    slots[currentIndex][currentTry].text.Text = ((char)(currentWord[currentIndex] + ('A' - 'a'))).ToString();
                                    currentIndex++;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void New_OnClick(object sender, RoutedEventArgs e)
        {
            New();
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            Notification("Press Esc to switch focus", "#FF0000");
            isFocusingText = true;
        }

        public void Notification(string str, string colorCode)
        {
            statusText.Text = str;
            statusText.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString(colorCode));
        }

        private void Host_OnClick(object sender, RoutedEventArgs e)
        {
            if (client == null)
            {
                if (server == null)
                {
                    server = new Connection_Server();
                    networkType = NetworkType.Host;

                    server.Host();
                }
                else
                {
                    Notification("You are already hosting a server", "#FF0000");
                }
            }
            else
            {
                Notification("You are already on another server", "#FF0000");
            }
        }

        private void Connect_OnClick(object sender, RoutedEventArgs e)
        {
            if (server == null)
            {
                if (client == null)
                {
                    client = new Connection_Client();
                    networkType = NetworkType.Client;

                    client.Connect();
                }
                else
                {
                    Notification("You are already on another server", "#FF0000");
                }
            }
            else
            {
                Notification("You are already hosting a server", "#FF0000");
            }
        }

        public void ConnectEnd()
        {
            client = null;
            server = null;
            networkType = NetworkType.Local;
        }

        private void Disconnect_OnClick(object sender, RoutedEventArgs e)
        {
            //Not connected
            switch (networkType)
            {
                case NetworkType.Local:
                    logWindow.AddLog("[Local]You are not connected");
                    break;
                case NetworkType.Client:
                    client.Disconnect();
                    //client.Send(IPAddress.Text);
                    break;
                case NetworkType.Host:
                    server.Disconnect();
                    //server.Send(IPAddress.Text);
                    break;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            switch (networkType)
            {
                case NetworkType.Local:
                    break;
                case NetworkType.Client:
                    client.Disconnect();
                    break;
                case NetworkType.Host:
                    server.Disconnect();
                    break;
            }
        }
    }
}
