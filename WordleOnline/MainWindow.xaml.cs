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
            public virtual void CheckWord(string input)
            {
            }

            public virtual void GetAnswer()
            {
            }
        }
        public class Network_LocalAndHost : Network
        {
            public char[] goalWord = new char[5];
            public int[] charCount = new int[26];

            public string GetGoalWord()
            {
                string outStr = "";

                for (int i = 0; i < 5; i++)
                {
                    outStr += goalWord[i];
                }

                return outStr;
            }

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

            public override void GetAnswer()
            {
                //Show on notif


                //mainWindow.Notification("The answer is " + outStr.ToUpper(), "#FF0000");
                mainWindow.Notification("The answer is " + GetGoalWord().ToUpper(), "#FF0000");
            }
            public override void CheckWord(string input)
            {
                Slot.Status[] _outStatuses;

                bool isValid;
                bool isFinished = mainWindow.CheckWord(input, out isValid, out _outStatuses);

                mainWindow.CheckReply(isFinished, isValid, _outStatuses);


                //statuses = _statuses;

                //return match;
            }
        }

        public void ShowAnswer(string str)
        {
            mainWindow.Notification("The answer is " + str.ToUpper(), "#FF0000");
        }

        //Deep calculation (Host or Local exclusive)
        public bool CheckWord(string input, out bool isValid, out Slot.Status[] statuses)
        {
            //statuses = new Slot.Status[5];
            isValid = mainWindow.dictionary.Contains(input.ToLower());
            if (isValid)
            {
                //Check
                //Slot.Status[] outStatuses = new Slot.Status[5];

                //isFinished = network.CheckWord(currentWord, out outStatuses);

                statuses = new Slot.Status[5];

                bool match = true;
                for (int i = 0; i < 5; i++)
                {
                    match = match && input[i] == (network as Network_LocalAndHost).goalWord[i];
                    if (input[i] == (network as Network_LocalAndHost).goalWord[i])
                    {
                        statuses[i] = Slot.Status.Exact;
                    }
                    else
                    {
                        if ((network as Network_LocalAndHost).charCount[input[i] - 'a'] == 0)
                        {
                            statuses[i] = Slot.Status.Wrong;
                        }
                        //!= 0 -> >0
                        else
                        {

                            int leftCount = 0;
                            int leftCorrectCount = 0;
                            int rightCount = 0;
                            //int rightCount = 0;

                            for (int j = i - 1; j >= 0; j--)
                            {
                                if (input[j] == input[i])
                                {
                                    leftCount++;
                                    if ((network as Network_LocalAndHost).goalWord[j] == input[j])
                                        leftCorrectCount++;
                                }
                            }
                            for (int j = i + i; j < 5; j++)
                            {
                                if (input[j] == input[i] && (network as Network_LocalAndHost).goalWord[j] == input[j])
                                    rightCount++;
                            }

                            //
                            if (leftCorrectCount + rightCount == (network as Network_LocalAndHost).charCount[input[i] - 'a'])
                            {
                                statuses[i] = Slot.Status.Wrong;
                            }
                            else
                            {
                                //
                                if (leftCount + rightCount == (network as Network_LocalAndHost).charCount[input[i] - 'a'])
                                {
                                    statuses[i] = Slot.Status.Wrong;
                                }
                                //
                                else
                                {
                                    statuses[i] = Slot.Status.Position;
                                }
                            }
                        }

                    }
                }
                return match;
            }
            else
            {
                //isValid = false;
                statuses = null;
                return false;
            }
        }

        public class Network_Client : Network
        {
            public Network_Client()
            {

            }

            public override void GetAnswer()
            {
                //return "TODO";
                //TODO Ask Server
                mainWindow.client.GetAnswer();
            }

            public override void CheckWord(string input)
            {
                mainWindow.client.Check(input);
            }
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
            public Status status;

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

                status = Status.Pending;
            }

            public void Reset()
            {
                background.Dispatcher.Invoke(()=> background.BorderThickness = new Thickness(2));
                background.Dispatcher.Invoke(() => background.BorderBrush = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#d3d6da")));
                text.Dispatcher.Invoke(() => text.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#000000")));
                background.Dispatcher.Invoke(() => background.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFF")));
                text.Dispatcher.Invoke(() => text.Text = "");
                status = Status.Pending;
            }

            public void SetBackground(int thick, string bgColor, string textColor)
            {
                background.Dispatcher.Invoke(() => background.BorderThickness = new Thickness(thick));
                background.Dispatcher.Invoke(() => background.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString(bgColor)));
                text.Dispatcher.Invoke(() => text.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString(textColor)));
            }

            public void SetStatus(Status status)
            {
                switch(status)
                {
                    case Status.Exact:
                        SetBackground(0, "#6aaa64", "#FFFFFF");
                        //background.BorderThickness = new Thickness(0);
                        //background.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#6aaa64"));
                        //text.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFF"));
                        break;
                    case Status.Pending:
                        SetBackground(2, "#d3d6da", "#000000");
                        //background.BorderThickness = new Thickness(2);
                        //background.BorderBrush = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#d3d6da"));
                        //text.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#000000"));
                        break;
                    case Status.Position:
                        SetBackground(0, "#c9b458", "#FFFFFF");
                        //background.BorderThickness = new Thickness(0);
                        //background.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#c9b458"));
                        //text.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFF"));
                        break;
                    case Status.Wrong:
                        SetBackground(0, "#787c7e", "#FFFFFF");
                        //background.BorderThickness = new Thickness(0);
                        //background.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#787c7e"));
                        //text.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFF"));
                        break;
                }
                this.status = status;
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
                    background.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#d3d6da"));
                    //background.BorderThickness = new Thickness(1);
                    //background.BorderBrush = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#d3d6da"));
                }

                public void SetBackground(string colorKey)
                {
                    //background.Dispatcher.Invoke(() => background.BorderThickness = new Thickness(thick));
                    background.Dispatcher.Invoke(() => background.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorKey)));
                }

                public void SetStatus(MainWindow.Slot.Status status)
                {
                    switch(status)
                    {
                        case MainWindow.Slot.Status.Exact:
                            //background.BorderThickness = new Thickness(0);
                            //background.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6aaa64"));
                            SetBackground("#6aaa64");
                            break;
                        case MainWindow.Slot.Status.Pending:
                            //background.BorderThickness = new Thickness(1);
                            //background.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#d3d6da"));
                            SetBackground("#d3d6da");
                            break;
                        case MainWindow.Slot.Status.Position:
                            //background.BorderThickness = new Thickness(0);
                            //background.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#c9b458"));
                            SetBackground("#c9b458");
                            break;
                        case MainWindow.Slot.Status.Wrong:
                            //background.BorderThickness = new Thickness(0);
                            //background.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#787c7e"));
                            SetBackground("#787c7e");
                            break;
                    }
                }
            }

            public Slot[][] slots;
            public Grid grid;
            public TextBlock name;
            public Canvas canvas;
            public MainWindow.Slot.Status[][] statuses;

            public PreviewSlot()
            {
                canvas = new Canvas();
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
                statuses = new MainWindow.Slot.Status[5][];
                slots = new Slot[5][];
                for (int i = 0; i < 5; i++)
                {
                    slots[i] = new Slot[6];
                    statuses[i] = new MainWindow.Slot.Status[6];
                    for (int j = 0; j < 6; j++)
                    {
                        slots[i][j] = new Slot();
                        statuses[i][j] = MainWindow.Slot.Status.Pending;

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

            public void Reset()
            {
                for (int i = 0; i < 5; i++)
                {
                    //for (int j = 0; j < 6; j++)
                    for (int j = 0; j < 6; j++)
                    {
                        slots[i][j].SetStatus(MainWindow.Slot.Status.Pending);
                    }
                }
            }

            public void Clear()
            {
                canvas.Dispatcher.Invoke(() => canvas.Visibility = Visibility.Hidden);
            }
            //
            public void SetSlots(MainWindow.Slot.Status[][] statuses)
            {
                this.statuses = statuses;
                Console.WriteLine("statuses " + statuses.Length);
                //for (int i = 0; i < 5; i++)
                for (int i = 0; i < statuses.Length; i++)
                {
                    Console.WriteLine("statuses[" + i + "] " + statuses[i].Length);
                    //for (int j = 0; j < 6; j++)
                    for (int j = 0; j < statuses[i].Length; j++)
                    {
                        slots[i][j].SetStatus(statuses[i][j]);
                    }
                }
            }

            public void SetName(string str)
            {
                name.Dispatcher.Invoke(() => name.Text = str);
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

                void SetKey(string bgColor, string textColor)
                {
                    background.Dispatcher.Invoke(() => background.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString(bgColor)));
                    //text.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString(textColor));
                    text.Dispatcher.Invoke(() => text.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString(textColor)));
                }

                public void Set(Slot.Status _status)
                {
                    switch(_status)
                    {
                        case Slot.Status.Exact:
                            SetKey("#6aaa64", "#FFFFFF");
                            //background.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#6aaa64"));
                            //text.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFF"));
                            break;
                        case Slot.Status.Pending:
                            SetKey("#d3d6da", "#000000");
                            //background.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#d3d6da"));
                            //text.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#000000"));
                            break;
                        case Slot.Status.Position:
                            SetKey("#c9b458", "#FFFFFF");
                            //background.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#c9b458"));
                            //text.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFF"));
                            break;
                        case Slot.Status.Wrong:
                            SetKey("#787c7e", "#FFFFFF");
                            //background.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#787c7e"));
                            //text.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFF"));
                            break;
                    }
                    status = _status;
                }

                public void Reset()
                {
                    background.Dispatcher.Invoke(() => background.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#d3d6da")));
                    text.Dispatcher.Invoke(() => text.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#000000")));
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
        public Network network;
        Keyboard keyboard;
        List<string> dictionary;
        Random rand;
        bool isFinished;
        bool isFocusingText;
        LogWindow logWindow;
        public Connection_Server server;
        public Connection_Client client;
        public static MainWindow mainWindow;
        PreviewSlot[] previewSlots;
        public bool isConnecting = false;

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

            for(int i =0;i< previewSlots.Length;i++)
            {
                previewSlots[i].Reset();
            }

            string randWord = dictionary[rand.Next(0, dictionary.Count)];
            (network as Network_LocalAndHost).NewGoalWord(randWord);

            Notification("Status: " + networkType.ToString(), "#000000");
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            mainWindow = this;

            networkType = NetworkType.Local;
            network = new Network_LocalAndHost();
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
                previewSlots[i] = new PreviewSlot();
                Grid.SetColumn(previewSlots[i].canvas, i%4);
                Grid.SetRow(previewSlots[i].canvas, i/4);
                //canvas.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#333333"));
                previewSlots[i].grid.Width = 75;
                previewSlots[i].grid.Height = 90;
                previewSlots[i].grid.HorizontalAlignment = HorizontalAlignment.Center;
                previewSlots[i].grid.VerticalAlignment = VerticalAlignment.Bottom;
                previewSlots[i].grid.Margin = new Thickness(0, 30, 0, 0);
                //OnlineGrid.Children.Add(previewSlots[i].grid);

                previewSlots[i].canvas.Children.Add(previewSlots[i].grid);
                previewSlots[i].canvas.Children.Add(previewSlots[i].name);
                previewSlots[i].canvas.Visibility = Visibility.Hidden;

                OnlineGrid.Children.Add(previewSlots[i].canvas);
                //canvas.Visibility = (Visibility)rand.Next(0, 2);

                //Test
                /*
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
                */
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

            Notification("Status: " + networkType.ToString(), "#000000");

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

        public void CheckReply(bool _isFinished, bool isValid, Slot.Status[] statuses)
        {
            if (isValid)
            {
                for (int i = 0; i < 5; i++)
                {
                    //if(statuses[i] == Slot.Status.Exact);
                    //DEBUG Statuses is null
                    slots[i][mainWindow.currentTry].SetStatus(statuses[i]);

                    switch (keyboard.keys[currentWord[i] - 'a'].status)
                    {
                        case Slot.Status.Exact:
                            break;
                        case Slot.Status.Pending:
                            keyboard.keys[currentWord[i] - 'a'].Set(statuses[i]);
                            break;
                        case Slot.Status.Position:
                            if (statuses[i] == Slot.Status.Exact)
                                mainWindow.keyboard.keys[mainWindow.currentWord[i] - 'a'].Set(statuses[i]);
                            break;
                        case Slot.Status.Wrong:
                            break;
                    }

                }
                currentTry++;
                currentIndex = 0;

                if(networkType == NetworkType.Host)
                {
                    //Send
                    server.Update();
                }

                //statusText.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#000000"));
                if (currentTry > 5 && !_isFinished)
                {
                    LogWindow.log.AddLog("[Client] GetAnswer Called ");
                    network.GetAnswer();
                    //statusText.Text = "The anwser is " + network.GetAnswer().ToUpper();
                    //Notification("The anwser is " + network.GetAnswer().ToUpper(), "#000000");
                }
                else
                {
                    Notification("Status: " + networkType.ToString(), "#000000");
                    //statusText.Text = "Status: Local";
                }
            }
            else
            {
                Notification("Invalid word", "#FF0000");
            }

            isFinished = _isFinished;
            //TODO Split the Anwser asking
        }

        public void ClientDisconnect(int id)
        {
            int own = 0;

            if(networkType == NetworkType.Client)
            {
                own = client.GetId();
            }
            //Server
            else
            {
                server.ClientDisconnect(id);
            }

            if (id > own)
            {
                id--;
            }
            else if(id < own)
            {
                if (networkType == NetworkType.Client)
                {
                    client.id--;
                }
            }

            MainWindow.LogWindow.log.AddLog("[Client]ClientDisconnect " + id + " : " + own);
            //Start from where to move
            for (int i = id;i < 7;i++)
            {
                previewSlots[i].SetName(previewSlots[i + 1].name.Dispatcher.Invoke(() => previewSlots[i + 1].name.Text));
                previewSlots[i].SetSlots(previewSlots[i + 1].statuses);
                previewSlots[i].canvas.Dispatcher.Invoke(() => previewSlots[i].canvas.Visibility = previewSlots[i + 1].canvas.Dispatcher.Invoke(() => previewSlots[i + 1].canvas.Visibility));
            }
            previewSlots[7].canvas.Dispatcher.Invoke(() => previewSlots[7].canvas.Visibility = Visibility.Hidden);
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

                        Notification("Status: " + networkType.ToString(), "#000000");
                        //statusText.Text = "Status: Local";
                        //statusText.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#000000"));
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

                        //isFinished = network.CheckWord(input.ToLower());
                        network.CheckWord(input.ToLower());
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
            if (isConnecting) return;
            switch (networkType)
            {
                case NetworkType.Client:
                    Notification("Only server can start a new game", "#FF0000");
                    break;
                case NetworkType.Host:
                    New();
                    server.ServerNewGame();
                    break;
                case NetworkType.Local:
                    New();
                    break;
            }
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            Notification("Press Esc to switch focus", "#FF0000");
            isFocusingText = true;
        }

        public void Notification(string str, string colorCode)
        {
            //statusText.Text = str;
            //statusText.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString(colorCode));

            statusText.Dispatcher.Invoke(() => statusText.Text = str);
            statusText.Dispatcher.Invoke(() => statusText.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString(colorCode)));
        }

        private void Host_OnClick(object sender, RoutedEventArgs e)
        {
            if (isConnecting) return;
            if (client == null)
            {
                if (server == null)
                {

                    int port;
                    if (int.TryParse(Port.Text, out port))
                    {

                        server = new Connection_Server();
                        networkType = NetworkType.Host;


                        server.Host(Name.Text, port);
                        Notification("Status: " + networkType.ToString(), "#000000");
                    }
                    else
                    {
                        Notification("Port is not a number", "#FF0000");
                    }
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

        public void NewPlayer(int id, string name)
        {
            int tar = 0;

            if (client != null)//Client; null = Server
            {
                if (id > client.GetId())
                {
                    tar = -1;
                }
            }
            else
            {
                tar = -1;
            }
            previewSlots[id + tar].canvas.Dispatcher.Invoke(()=> previewSlots[id + tar].canvas.Visibility = Visibility.Visible);
            previewSlots[id + tar].SetName(name);
            previewSlots[id + tar].Reset();
        }

        private void Connect_OnClick(object sender, RoutedEventArgs e)
        {
            if (isConnecting) return;

            if (server == null)
            {
                if (client == null)
                {
                    int port;

                    if (int.TryParse(Port.Text, out port))
                    {
                        client = new Connection_Client();

                        //TODO port only allow num
                        isConnecting = true;
                        Thread thread = new Thread(() => client.Connect(IPAddress.Dispatcher.Invoke(() => IPAddress.Text), port, Name.Dispatcher.Invoke(() => Name.Text)));
                        thread.Start();
                        Notification("Connecting...", "#FF0000");
                    }
                    else
                    {
                        Notification("Port is not a number", "#FF0000");
                    }
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

        public void ConnectEnd(NetworkType type)
        {
            client = null;
            server = null;
            networkType = NetworkType.Local;
            if (type == NetworkType.Client)
            {
                network = new Network_LocalAndHost();
                currentTry = 6;
                currentIndex = 0;
                Notification("Disconnected from server, press New to start a new game", "#FF0000");
            }

            //Clear
            for (int i = 0;i < previewSlots.Length;i++)
            {
                previewSlots[i].Clear();
            }

        }

        public Slot.Status[][] GetStatuses()
        {
            Slot.Status[][] statuses = new Slot.Status[5][];

            for (int i = 0; i < 5; i++)
            {
                statuses[i] = new Slot.Status[6];

                for(int j = 0;j < 6;j++)
                {
                    statuses[i][j] = slots[i][j].status;
                }
            }

            return statuses;
        }

        public void SetNetwork(NetworkType _network)
        {
            networkType = _network;

            if (_network == NetworkType.Client)
            {
                network = new Network_Client();
            }

            Notification("Status: " + networkType.ToString(), "#000000");

        }

        public void SetNameAndStatuses(int index, string name, Slot.Status[][] statuses)
        {
            previewSlots[index].canvas.Dispatcher.Invoke(() => previewSlots[index].canvas.Visibility = Visibility.Visible);
            previewSlots[index].SetName(name);
            previewSlots[index].SetSlots(statuses);
        }

        public void ServerNewGame()
        {
            Reset();

            for (int i = 0; i < previewSlots.Length; i++)
            {
                previewSlots[i].Reset();
            }

            Notification("Server has started a new game","#FF0000");
        }

        public void Update(Slot.Status[][][] statuses)
        {
            for(int i = 7; i >= 0;i--)
            {
                if (i >= statuses.Length - 1)
                {
                    mainWindow.previewSlots[i].canvas.Dispatcher.Invoke(()=> mainWindow.previewSlots[i].canvas.Visibility = Visibility.Hidden);
                }
                else
                {
                    mainWindow.previewSlots[i].canvas.Dispatcher.Invoke(() => mainWindow.previewSlots[i].canvas.Visibility = Visibility.Visible);
                }
                //Set 
            }

            int id = 0;
            if (networkType == NetworkType.Client)
            {
                //Get id
                id = client.GetId();
            }
            //You are server, means your id = 0
            for (int i = 0; i < statuses.Length; i++)
            {
                if (i < id)
                {
                    previewSlots[i].SetSlots(statuses[i]);
                }
                else if(i == id)
                {
                    //Get the last from bottom
                    //And update
                    for(int j = 5;j >= 0;j--)
                    {
                        if(statuses[i][0][j] != Slot.Status.Pending)
                        {
                            //Check if this line is equal to currentTry
                            if (currentTry == j)
                            {
                                bool isFinished = false;
                                if (j == 5)
                                {
                                    isFinished = true;
                                    for (int k = 0;k < 5;k++)
                                    {
                                        isFinished = isFinished && statuses[i][k][j] == Slot.Status.Exact;
                                    }
                                }

                                LogWindow.log.AddLog("[Client] isFinished " + isFinished);

                                Slot.Status[] outStatuses = new Slot.Status[5];
                                for(int k =0;k < 5;k++)
                                {
                                    outStatuses[k] = statuses[i][k][j];
                                }

                                //CheckReply(isFinished, true, statuses[i][j]);
                                CheckReply(isFinished, true, outStatuses);

                                //currentTry++;
                                //currentIndex = 0;
                                //Change
                            }
                            /*
                            //Have to pending
                            else if(slots[0][j].status == Slot.Status.Pending)
                            {
                                //Wrong
                                Notification("Invalid word A", "#FF0000");
                            }
                            */
                            break;
                        }
                    }
                }
                //i > id
                else
                {
                    previewSlots[i - 1].SetSlots(statuses[i]);
                }
            }
        }

        //Own slot
        public void Reset()
        {
            currentTry = 0;
            currentIndex = 0;
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    slots[i][j].Reset();
                }
            }

            keyboard.Reset();
        }

        private void Disconnect_OnClick(object sender, RoutedEventArgs e)
        {
            if (isConnecting) return;
            //Not connected
            switch (networkType)
            {
                case NetworkType.Local:
                    logWindow.AddLog("[Local]You are not connected");
                    break;
                case NetworkType.Client:
                    client.Disconnect();
                    network = new Network_LocalAndHost();
                    //client.Send(IPAddress.Text);
                    break;
                case NetworkType.Host:
                    server.Disconnect();
                    network = new Network_LocalAndHost();
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
