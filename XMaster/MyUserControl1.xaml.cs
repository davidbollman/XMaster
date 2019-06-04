﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Gaming.Input;
using Windows.UI;
using Windows.UI.Core;
using Windows.System;


// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace MyUserControl
{
    public class DataChangedEventArgs : EventArgs
    {
        public DataChangedEventArgs()
        { }
    }

    public  partial class MyUserControl1 : UserControl
    {
        APlayer aPlayer;
        public int PlayerNum { get; set; }
        public int Timer { get; set }
        public DispatcherTimer gamepadTimer = new DispatcherTimer();

        public MyUserControl1()
        {
            aPlayer = new APlayer();
            this.InitializeComponent();
            aPlayer.RestartGame();
            aPlayer.DataChanged += eh_DataChanged;
            RegisterGamepads();
            GetGamepads();
            UpdateDisplay();
            Window.Current.CoreWindow.KeyDown += CoreWindow_KeyDown;
            gamepadTimer.Interval = new TimeSpan(0, 0, 0, 0, 10);
            gamepadTimer.Tick += GamepadTimerEvent;

        }

        public void eh_DataChanged(Object sender, DataChangedEventArgs e)
        {
            UpdateDisplay();
        }

        private void ButtonAnswerClick(object sender, RoutedEventArgs e)
        {
            int answer = Convert.ToInt32(((Button)sender).Content);
            aPlayer.AnswerClick(answer);
            UpdateDisplay();
        }

        private void ButtonRestart_Click(object sender, RoutedEventArgs e)
        {
            aPlayer.RestartGame();
            UpdateDisplay();
        }

        public void GamepadTimerEvent(object sender, object e)
        {
            if (myGamepads.Count() > PlayerNum - 1 && myGamepads.Count() > 0)
            {
                Gamepad gamepad = myGamepads[PlayerNum - 1];
                GamepadReading reading = gamepad.GetCurrentReading();
                GamepadReading lastreading = reading;
                textBlockButtons.Text = reading.Buttons.ToString();
                
                if (lastreading.Buttons != reading.Buttons)
                {
                    reading.A
                }
            }
        }

        public void UpdateDisplay()
        {
            this.PlayerGrid.Background = (this.textBlockMessage.Text.Contains("Winner") ) ? new SolidColorBrush(Colors.LightGreen) : new SolidColorBrush(Colors.White);
            this.textBlockScore.Text = "Score: " + aPlayer.p1.score.ToString();
            this.textBlockRight.Text = "Right: " + aPlayer.p1.correct.ToString();
            this.textBlockWrong.Text = "Wrong: " + aPlayer.p1.incorrect.ToString();
            this.textBlockTimer.Text = "This Time: " + aPlayer.p1.timer.ToString();
            this.textBlockBestTime.Text = "Best Time:" + ((aPlayer.p1.besttime == 99999) ? "" : aPlayer.p1.besttime.ToString());
            this.textBlockMessage.Text = aPlayer.p1.message;

            this.textBlockQuestion.Text = aPlayer.p1.x.ToString() + " x " + aPlayer.p1.y.ToString();

            this.buttonA.Content = aPlayer.p1.suggestion1.ToString();
            this.buttonB.Content = aPlayer.p1.suggestion2.ToString();
            this.buttonX.Content = aPlayer.p1.suggestion3.ToString();
            this.buttonY.Content = aPlayer.p1.suggestion4.ToString();

            this.buttonA.IsEnabled = aPlayer.dispatcherTimer.IsEnabled;
            this.buttonB.IsEnabled = aPlayer.dispatcherTimer.IsEnabled;
            this.buttonX.IsEnabled = aPlayer.dispatcherTimer.IsEnabled;
            this.buttonY.IsEnabled = aPlayer.dispatcherTimer.IsEnabled;
            this.textBlockPlayer.Text = PlayerNum.ToString();

            this.textBlockScore.Foreground = aPlayer.AnswerColor;

            if ((aPlayer.WrongAnswers.Count() > 0) && (this.listBox.Items.Count != aPlayer.WrongAnswers.Count))
            {
                this.listBox.Items.Add(aPlayer.WrongAnswers[aPlayer.WrongAnswers.Count - 1]);
            }

            if (aPlayer.WrongAnswers.Count() == 0)
            {
                this.listBox.Items.Clear();
            }

        }


        private readonly object myLock = new object();
        private List<Gamepad> myGamepads = new List<Gamepad>();
        private Gamepad mainGamepad;

        private void GetGamepads()
        {
            lock (myLock)
            {
                foreach (var gamepad in Gamepad.Gamepads)
                {
                    // Check if the gamepad is already in myGamepads; if it isn't, add it.
                    bool gamepadInList = myGamepads.Contains(gamepad);

                    if (!gamepadInList)
                    {
                        // This code assumes that you're interested in all gamepads.
                        myGamepads.Add(gamepad);
                        if ((myGamepads.Count() >= PlayerNum) && gamepad == myGamepads[PlayerNum - 1])
                        {
                            var ignored = Dispatcher.TryRunAsync(CoreDispatcherPriority.Normal, () => { IsEnabled = true; });
                        }
                    }
                }
            }
        }

        private void RegisterGamepads()
        {
            Gamepad.GamepadAdded += (object sender, Gamepad gamepad) =>
            {
                // Check if the just-added gamepad is already in myGamepads; if it isn't, add
                // it.
                lock (myLock)
                {
                    bool gamepadInList = myGamepads.Contains(gamepad);

                    if (!gamepadInList)
                    {
                        myGamepads.Add(gamepad);
                        if ((myGamepads.Count() >= PlayerNum) && gamepad == myGamepads[PlayerNum - 1])   
                        {
                            var ignored = Dispatcher.TryRunAsync(CoreDispatcherPriority.Normal, () => { IsEnabled = true; });
                        }
                    }
                }
            };


            Gamepad.GamepadRemoved += (object sender, Gamepad gamepad) =>
            {
                lock (myLock)
                {
                    int indexRemoved = myGamepads.IndexOf(gamepad);

                    if (indexRemoved > -1)
                    {
                        if (mainGamepad == myGamepads[indexRemoved])
                        {
                            mainGamepad = null;
                        }

                        myGamepads.RemoveAt(indexRemoved);
                    }
                }
            };


        }

        private void CoreWindow_KeyDown(CoreWindow sender, KeyEventArgs args)
        {
            if (args.Handled)
            {
                return;
            }

            if ((myGamepads.Count() >= PlayerNum) )
            {
                switch (args.VirtualKey)
                {
                    case VirtualKey.GamepadA:
                        ButtonAnswerClick(buttonA, null);
                        break;

                    case VirtualKey.GamepadB:
                        ButtonAnswerClick(buttonB, null);
                        break;

                    case VirtualKey.GamepadX:
                        ButtonAnswerClick(buttonX, null);
                        break;

                    case VirtualKey.GamepadY:
                        ButtonAnswerClick(buttonY, null);
                        break;
                }
            }
        }


    }

    public class APlayer
    {
        public struct Player
        {
            public int min;
            public int max;

            public int x;
            public int y;
            public int ans;

            public int suggestion1;
            public int suggestion2;
            public int suggestion3;
            public int suggestion4;

            public int correct;
            public int incorrect;
            public int score;
            public int timer;
            public int besttime;

            public string message;
        }

        public event EventHandler<DataChangedEventArgs> DataChanged;



        static int AnswersToWin = 7;
        public int TimeToWin = 60;

        public Player p1 = new Player();
        public DispatcherTimer dispatcherTimer = new DispatcherTimer();
        public SolidColorBrush AnswerColor = new SolidColorBrush(Colors.Black);
        public List<string> WrongAnswers = new List<string>();

        protected virtual void OnDataChanged(DataChangedEventArgs e)
        {
            DataChanged?.Invoke(this, e);
        }

        public APlayer()
        {
            p1.besttime = 99999;
            //dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 25);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Tick += dispatcherTimer_Tick;
            //            this.textBlockScore.Foreground = AnswerColor;

        }

        public void UpdateDisplay()
        {
            OnDataChanged(new DataChangedEventArgs()); ;
        }

        public void RestartGame()
        {
            p1.min = 1;
            p1.max = 10;
            p1.score = 0;
            p1.timer = 0;
            p1.ans = 0;
            p1.correct = 0;
            p1.incorrect = 0;
            p1.message = "";
            //this.listBox.Items.Clear();
            WrongAnswers.Clear();

            GetQuestion();
            UpdateDisplay();
            StartTimer();
        }

        public void GetQuestion()
        {
            Random rnd = new Random();

            p1.x = rnd.Next(p1.min, p1.max);
            p1.y = rnd.Next(p1.min, p1.max);
            p1.ans = p1.x * p1.y;

            int r = rnd.Next(1, 4);

            p1.suggestion1 = (r == 1) ? p1.ans : (rnd.Next(p1.min, p1.max) * rnd.Next(p1.min, p1.max));
            p1.suggestion2 = (r == 2) ? p1.ans : (rnd.Next(p1.min, p1.max) * rnd.Next(p1.min, p1.max));
            p1.suggestion3 = (r == 3) ? p1.ans : (rnd.Next(p1.min, p1.max) * rnd.Next(p1.min, p1.max));
            p1.suggestion4 = (r == 4) ? p1.ans : (rnd.Next(p1.min, p1.max) * rnd.Next(p1.min, p1.max));
        }

        public void StartTimer()
        {
            dispatcherTimer.Start();
        }

        public void dispatcherTimer_Tick(object sender, object e)
        {
            p1.timer++;
            UpdateDisplay();
        }

        public void AnswerClick(int answer)
        {
            if (answer == p1.ans)
            {

                p1.correct++;
                p1.score++;
                AnswerColor.Color = Colors.Green;
            }
            else
            {
                p1.incorrect++;
                if (p1.score > 0)
                    p1.score--;
                //  this.listBox.Items.Add(p1.x + " X " + p1.y + " = " + p1.ans);
                WrongAnswers.Add(p1.x + " X " + p1.y + " = " + p1.ans);

                AnswerColor.Color = Colors.Red;
            }

            if (p1.score >= AnswersToWin)
            {
                if (p1.timer < TimeToWin)
                {
                    p1.message = "Winner!!!  ";
                    if (p1.timer < p1.besttime)
                    {
                        p1.besttime = p1.timer;
                        p1.message += "Best Time Yet!!";
                    }
                }
                else
                {
                    p1.message = "Keep Trying, you can do it!";
                }
                dispatcherTimer.Stop();
            }
            else
            {
                this.GetQuestion();
            }

            UpdateDisplay();
        }



    }

}
