using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Windows.Forms;
using static Rollerball.ParticleState;
using System.Runtime.InteropServices;

namespace Rollerball
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game : Microsoft.Xna.Framework.Game
    {
        System.Windows.Forms.DataVisualization.Charting.Chart chart;
        System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
        System.Windows.Forms.DataVisualization.Charting.Legend legend = new System.Windows.Forms.DataVisualization.Charting.Legend();
        System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
        System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
        System.Windows.Forms.DataVisualization.Charting.Series series3 = new System.Windows.Forms.DataVisualization.Charting.Series();
        System.Windows.Forms.DataVisualization.Charting.Series series4 = new System.Windows.Forms.DataVisualization.Charting.Series();
        System.Windows.Forms.DataVisualization.Charting.Series series5 = new System.Windows.Forms.DataVisualization.Charting.Series();

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        private VideoCapture videoCapture;
        enum GameState { PLAYING, FINISHED, REQUESTRESTART, SHOWHIGHSCORES, COUNTDOWN };
        GameState gameState = GameState.SHOWHIGHSCORES;
        const int MAX_PLAYERS = 4;
        const int X_STARTVALUE = 4;
        int winningPlayerNumber;
        int numPlayersFinished;
        private List<Player> players = new List<Player>();
        Video video;
        Microsoft.Xna.Framework.Media.VideoPlayer videoPlayer;
        bool playingVideo = false;
        Texture2D textureBackground;
        Texture2D textureBackgroundHighscores;
        Texture2D textureFont;
        Texture2D textureFont2;
        Texture2D textureMarble;
        Texture2D textureMarble2;
        Texture2D textureTurtle;
        Texture2D textureDog;
        Texture2D textureElephant;
        Texture2D textureBird;
        Texture2D textureBallBird;
        Texture2D textureBallElephant;
        Texture2D textureBallDog;
        Texture2D textureBallTurtle;
        Texture2D textureAlpha;
        Texture2D textureCountDown;
        Texture2D textureLaser;
        Texture2D textureStartbutton;
        Texture2D textureArrow;
        Texture2D textureGeenArduino;
        Texture2D textureGewonnen;
        Texture2D[] textureMatrix;
        private SoundEffect soundEffectPoing;
        private SoundEffect soundEffectLaugh;
        private SoundEffect soundEffectPop;
        private SoundEffect soundEffectGlass;
        private SoundEffect soundEffectFinished;
        private SoundEffect soundEffectCountDown;
        float ballRotation;
        int GameTimeMilliSeconds;
        static SerialPort serialPort;
        List<Ball> balls = new List<Ball>();
        int secondsPlotted;
        List<Highscore> highscoresDay = new List<Highscore>();
        List<Highscore> highscoresMonth = new List<Highscore>();
        List<Highscore> highscoresAll = new List<Highscore>();
        double requestRestartTime;
        bool enterPressed;
        Font fontNormal;
        Font fontScore;
        int scoreYPosition;
        int scoreListType;
        int scoreListElapsed;
        int checkPortOpenElapsed;
        float countdownPhase = 1.0f;
        int countdownLetter = 5;
        int showWinningElapsed = 0;
        static Random rand = new Random();
        ParticleManager<ParticleState> particleManager;
        SmokePlumeParticleSystem smokePlume;

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndIntertAfter, int X, int Y, int cx, int cy, int uFlags);
        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int Which);

        public SpriteBatch SpriteBatch
        {
            get
            {
                return spriteBatch;
            }

            set
            {
                spriteBatch = value;
            }
        }

        public Game()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.PreferredBackBufferHeight = 1080;
            graphics.PreferredBackBufferWidth = 1920;
            graphics.IsFullScreen = false;      // note: when using windows controls we can't use this option to go fullscreen.
            graphics.SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;
            
            Form.FromHandle(Window.Handle).FindForm().WindowState = FormWindowState.Maximized;
            Form.FromHandle(Window.Handle).FindForm().FormBorderStyle = FormBorderStyle.None;
            Form.FromHandle(Window.Handle).FindForm().TopMost = true;
            SetWindowPos(Window.Handle, IntPtr.Zero, 0, 0, GetSystemMetrics(0), GetSystemMetrics(1), 64);
            
        }

        Highscore ReadHighscore(StreamReader srFile)
        {
            Highscore highscore = new Highscore();
            highscore.GameTimeMilliSeconds = Convert.ToInt32(srFile.ReadLine());
            highscore.Date = Convert.ToDateTime(srFile.ReadLine());
            highscore.Name = srFile.ReadLine();

            return highscore;
        }

        private void SaveTextureAsPng(Texture2D texture, string filename)
        {
            byte[] textureData = new byte[4 * texture.Width * texture.Height];
            texture.GetData<byte>(textureData);

            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(
                           texture.Width, texture.Height,
                           System.Drawing.Imaging.PixelFormat.Format32bppArgb
                         );

            System.Drawing.Imaging.BitmapData bmpData = bmp.LockBits(
                           new System.Drawing.Rectangle(0, 0, texture.Width, texture.Height),
                           System.Drawing.Imaging.ImageLockMode.WriteOnly,
                           System.Drawing.Imaging.PixelFormat.Format32bppArgb
                         );

            IntPtr safePtr = bmpData.Scan0;
            System.Runtime.InteropServices.Marshal.Copy(textureData, 0, safePtr, textureData.Length);
            bmp.UnlockBits(bmpData);

            bmp.Save(filename, System.Drawing.Imaging.ImageFormat.Png);
        }


        private void LoadHighscorePictures()
        {
            for (int k = 0; k < highscoresDay.Count; k++)
            {
                highscoresDay[k].Photo = Texture2D.FromStream(GraphicsDevice, new FileStream("D" + k + ".png", FileMode.Open));
            }
            for (int k = 0; k < highscoresMonth.Count; k++)
            {
                highscoresMonth[k].Photo = Texture2D.FromStream(GraphicsDevice, new FileStream("M" + k + ".png", FileMode.Open));
            }
            for (int k = 0; k < highscoresAll.Count; k++)
            {
                highscoresAll[k].Photo = Texture2D.FromStream(GraphicsDevice, new FileStream("A" + k + ".png", FileMode.Open));
            }
        }

        private void SaveHighscorePictures()
        {
            for(int k=0; k<highscoresDay.Count; k++)
            {
                SaveTextureAsPng(highscoresDay[k].Photo, "D" + k + ".png");
            }
            for (int k = 0; k < highscoresMonth.Count; k++)
            {
                SaveTextureAsPng(highscoresMonth[k].Photo, "M" + k + ".png");
            }
            for (int k = 0; k < highscoresAll.Count; k++)
            {
                SaveTextureAsPng(highscoresAll[k].Photo, "A" + k + ".png");
            }
        }

        private void LoadHighscores()
        {
            int numScores = 0;
            highscoresDay.Clear();
            highscoresMonth.Clear();
            highscoresAll.Clear();
            try
            {
                using (StreamReader srFile = new StreamReader("Highscores"))
                {
                    numScores = Convert.ToInt32(srFile.ReadLine());
                    for (int k=0; k<numScores; k++)
                    {
                        Highscore highscore = ReadHighscore(srFile);
                        highscoresDay.Add(highscore);
                    }
                    numScores = Convert.ToInt32(srFile.ReadLine());
                    for (int k = 0; k < numScores; k++)
                    {
                        Highscore highscore = ReadHighscore(srFile);
                        highscoresMonth.Add(highscore);
                    }
                    numScores = Convert.ToInt32(srFile.ReadLine());
                    for (int k = 0; k < numScores; k++)
                    {
                        Highscore highscore = ReadHighscore(srFile);
                        highscoresAll.Add(highscore);
                    }
                }
            } catch (FileNotFoundException)
            {

            }
            LoadHighscorePictures();
        }

        void WriteHighscore(StreamWriter srFile, Highscore highscore)
        {
            srFile.WriteLine(highscore.GameTimeMilliSeconds);
            srFile.WriteLine(highscore.Date);
            srFile.WriteLine(highscore.Name);
        }

        private void SaveHighscores()
        {
            using (StreamWriter srFile = new StreamWriter("Highscores"))
            {
                srFile.WriteLine(highscoresDay.Count);
                foreach (Highscore highscore in highscoresDay)
                {
                    WriteHighscore(srFile, highscore);
                }
                srFile.WriteLine(highscoresMonth.Count);
                foreach (Highscore highscore in highscoresMonth)
                {
                    WriteHighscore(srFile, highscore);
                }
                srFile.WriteLine(highscoresAll.Count);
                foreach (Highscore highscore in highscoresAll)
                {
                    WriteHighscore(srFile, highscore);
                }
            }
            SaveHighscorePictures();
        }

        void MakePhoto()
        {
            players[0].Photo = videoCapture.getFrameRectangle(new Microsoft.Xna.Framework.Rectangle(20, 200, 150, 150));
            players[1].Photo = videoCapture.getFrameRectangle(new Microsoft.Xna.Framework.Rectangle(170, 200, 150, 150));
            players[2].Photo = videoCapture.getFrameRectangle(new Microsoft.Xna.Framework.Rectangle(320, 200, 150, 150));
            players[3].Photo = videoCapture.getFrameRectangle(new Microsoft.Xna.Framework.Rectangle(470, 200, 150, 150));
        }

        private void UpdateHighscores()
        {
            // remove scores older than one month
            DateTime sampleDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            foreach (Highscore highscore in highscoresMonth.ToList())
            {
                if (highscore.Date < sampleDate)
                {
                    highscoresMonth.Remove(highscore);
                }
            }
            // remove scores older than one day
            sampleDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            foreach (Highscore highscore in highscoresDay.ToList())
            {
                if (highscore.Date < sampleDate)
                {
                    highscoresDay.Remove(highscore);
                }
            }
            // insert new scores in highscore lists
            foreach (Player player in players)
            {
                if (highscoresDay.Count<10 || player.GameTimeMilliSeconds < highscoresDay[highscoresDay.Count-1].GameTimeMilliSeconds)
                {
                    if(highscoresDay.Count==10)
                    {
                        highscoresDay.RemoveAt(highscoresDay.Count - 1);
                    }
                    highscoresDay.Add(new Highscore(DateTime.Now, player.Name, player.GameTimeMilliSeconds, player.Photo));
                    highscoresDay.Sort((x, y) => x.GameTimeMilliSeconds.CompareTo(y.GameTimeMilliSeconds));
                }

                if (highscoresMonth.Count < 10 || player.GameTimeMilliSeconds < highscoresMonth[highscoresMonth.Count - 1].GameTimeMilliSeconds)
                {
                    if (highscoresMonth.Count==10)
                    {
                        highscoresMonth.RemoveAt(highscoresMonth.Count - 1);
                    }
                    highscoresMonth.Add(new Highscore(DateTime.Now, player.Name, player.GameTimeMilliSeconds, player.Photo));
                    highscoresMonth.Sort((x, y) => x.GameTimeMilliSeconds.CompareTo(y.GameTimeMilliSeconds));
                }

                if (highscoresAll.Count < 10 || player.GameTimeMilliSeconds < highscoresAll[highscoresAll.Count - 1].GameTimeMilliSeconds)
                {
                    if (highscoresAll.Count==10)
                    {
                        highscoresAll.RemoveAt(highscoresAll.Count - 1);
                    }
                    highscoresAll.Add(new Highscore(DateTime.Now, player.Name, player.GameTimeMilliSeconds, player.Photo));
                    highscoresAll.Sort((x, y) => x.GameTimeMilliSeconds.CompareTo(y.GameTimeMilliSeconds));
                }
            }
        }

        private void InitChart()
        {
            secondsPlotted = 0;
            chart = new System.Windows.Forms.DataVisualization.Charting.Chart();
            ((System.ComponentModel.ISupportInitialize)(this.chart)).BeginInit();

            chartArea.Name = "ChartArea1";
            chartArea.AxisY.Maximum = 50;
            chartArea.AxisY.Minimum = 0;
            chartArea.AxisX.Minimum = 0;
            chart.ChartAreas.Add(chartArea);
            legend.Name = "Legend1";
            legend.Font = new System.Drawing.Font("Microsoft Sans Serif", 26F, System.Drawing.FontStyle.Bold);
            legend.BorderWidth = 7;
            chart.Legends.Add(legend);
            chart.Location = new System.Drawing.Point(17, 660);
            chart.Name = "chart";
            series1.ChartArea = "ChartArea1";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series1.Legend = "Legend1";
            series1.BorderWidth = 5;
            series1.Name = "Olifant";
            series1.Color = System.Drawing.Color.FromArgb(141, 119, 155);
            series2.ChartArea = "ChartArea1";
            series2.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series2.Legend = "Legend1";
            series2.Name = "Schildpad";
            series2.BorderWidth = 5;
            series2.Color = System.Drawing.Color.FromArgb(56, 67, 9);
            series3.ChartArea = "ChartArea1";
            series3.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series3.Legend = "Legend1";
            series3.Name = "Vogel";
            series3.BorderWidth = 5;
            series3.Color = System.Drawing.Color.FromArgb(21, 128, 180);
            series4.ChartArea = "ChartArea1";
            series4.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series4.Legend = "Legend1";
            series4.Name = "Hond";
            series4.BorderWidth = 5;
            series4.Color = System.Drawing.Color.FromArgb(123, 44, 14);
            series5.ChartArea = "ChartArea1";
            series5.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series5.Legend = "Legend1";
            series5.Name = "Finish";
            series5.BorderWidth = 5;
            series5.Color = System.Drawing.Color.Red;
            chart.Series.Add(series1);
            chart.Series.Add(series2);
            chart.Series.Add(series3);
            chart.Series.Add(series4);
            chart.Series.Add(series5);
            chart.Size = new System.Drawing.Size(1850, 380);
            chart.TabIndex = 9;
            chart.Text = "chart2";
            chart.Visible = false;
            chart.Series["Olifant"].Points.Clear();
            chart.Series["Schildpad"].Points.Clear();
            chart.Series["Vogel"].Points.Clear();
            chart.Series["Hond"].Points.Clear();
            chart.Series["Finish"].Points.Clear();
        }

        void UpdateChart()
        {
            if (GameTimeMilliSeconds % 30000 > 20000 && videoPlayer.State != MediaState.Playing)
            {
                chart.Visible = true;
                chart.Invalidate();
                chart.BringToFront();
            }
            else
                chart.Visible = false;

            while (secondsPlotted < GameTimeMilliSeconds / 1000)
            {
                chart.Series["Olifant"].Points.AddXY(secondsPlotted,players[0].Score);
                chart.Series["Schildpad"].Points.AddXY(secondsPlotted,players[1].Score);
                chart.Series["Vogel"].Points.AddXY(secondsPlotted,players[2].Score);
                chart.Series["Hond"].Points.AddXY(secondsPlotted,players[3].Score);
                chart.Series["Finish"].Points.AddXY(secondsPlotted, 44);
                secondsPlotted++;
            }
        }

        /*
        private void LoadSettings()
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Peter Popma\\RollerBall");
            if (key != null)
            {
                RecordScoreDay = Convert.ToInt32(key.GetValue("RecordScoreDay", 0));

            }
        }

        private void SaveSettings()
        {
            // Create or get existing subkey
            RegistryKey key = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Peter Popma\\RollerBall");

            key.SetValue("RecordScoreDay", RecordScoreDay);
        }
        */

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            InitChart();
            Control.FromHandle(Window.Handle).Controls.Add(chart);

            serialPort = new SerialPort();
            Console.WriteLine("Available Ports:");
            foreach (string s in SerialPort.GetPortNames())
            {
                Console.WriteLine("   {0}", s);
            }
            serialPort.PortName = "COM7";

            // Set the read/write timeouts
            serialPort.ReadTimeout = SerialPort.InfiniteTimeout;
            serialPort.WriteTimeout = SerialPort.InfiniteTimeout;
            serialPort.DataReceived += new SerialDataReceivedEventHandler(serialPort_Datareceived);
            try
            {
                serialPort.Open();
            } catch (IOException)
            {
                Console.WriteLine("USB port not available.");
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("USB port used (by arduino program?)");
            }
            // Initialize particle system for Fireworks
            // TODO: combine particle systems of Fireworks and Smoke into one.
            particleManager = new ParticleManager<ParticleState>(1024 * 20, ParticleState.UpdateParticle);

            // we'll see lots of these effects at once; this is ok
            // because they have a fairly small number of particles per effect.
            smokePlume = new SmokePlumeParticleSystem(this, 80);    // max 80 balls explode at once :)
            Components.Add(smokePlume);

            base.Initialize();
        }

        public void serialPort_Datareceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            int bytes = serialPort.BytesToRead;

            try
            {
                while (bytes > 0)     // if there is data in the buffer
                {
                    int data = serialPort.ReadByte();

                    switch (data)
                    {
                        case 49:        // 1
                            AddScore(0, 1);
                            break;
                        case 50:        // 2
                            AddScore(1, 1);
                            break;
                        case 51:        // 3
                            AddScore(2, 1);
                            break;
                        case 52:        // 4
                            AddScore(3, 1);
                            break;
                        case 53:        // 5
                            // comes always as first character (?)
                            break;
                        case 54:        // 6
                            AddScore(1, 2);
                            break;
                        case 55:        // 7
                            AddScore(2, 2);
                            break;
                        case 56:        // 8
                            AddScore(3, 2);
                            break;
                        case 57:        // 9
                            AddScore(0, 3);
                            break;
                        case 58:       
                            AddScore(1, 3);
                            break;
                        case 59:       
                            AddScore(2, 3);
                            break;
                        case 60:      
                            AddScore(3, 3);
                            break;
                        case 61:        // 5
                            AddScore(0, 2);
                            break;
                    }
                }
            } catch (IOException)
            {

            }
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            LoadHighscores();
            videoCapture = new VideoCapture(GraphicsDevice);

            // Create a new SpriteBatch, which can be used to draw textures.
            SpriteBatch = new SpriteBatch(GraphicsDevice);

            textureBackground = Content.Load<Texture2D>("background");
            textureBackgroundHighscores = Content.Load<Texture2D>("scorebackground");
            textureFont = Content.Load<Texture2D>("Font");
            textureFont2 = Content.Load<Texture2D>("Font2");
            textureMarble = Content.Load<Texture2D>("marble");
            textureMarble2 = Content.Load<Texture2D>("marble2");
            textureTurtle = Content.Load<Texture2D>("turtle");
            textureDog = Content.Load<Texture2D>("dog");
            textureElephant = Content.Load<Texture2D>("elephant");
            textureBird = Content.Load<Texture2D>("bird");
            textureBallBird = Content.Load<Texture2D>("ballbird");
            textureBallElephant = Content.Load<Texture2D>("ballelephant");
            textureBallDog = Content.Load<Texture2D>("balldog");
            textureBallTurtle = Content.Load<Texture2D>("ballturtle");
            textureAlpha = Content.Load<Texture2D>("alpha");
            textureCountDown = Content.Load<Texture2D>("countdown");
            textureLaser = Content.Load<Texture2D>("laser");
            textureStartbutton = Content.Load<Texture2D>("startbutton");
            textureArrow = Content.Load<Texture2D>("arrow");
            textureGeenArduino = Content.Load<Texture2D>("geenarduino");
            textureGewonnen = Content.Load<Texture2D>("gewonnen");

            textureMatrix = new Texture2D[10];
            for (int k = 0; k < 10; k++)
            {
                textureMatrix[k] = Content.Load<Texture2D>("matrix"+k);
            }

            fontScore = new Rollerball.Font();
            int[] fontExtraYOffset = { 4, 9, 7, 7, 7, 5, 5 };
            fontScore.Adjust(fontExtraYOffset, 0, 0);
            fontScore.Initialize(textureFont2);

            fontNormal = new Rollerball.Font();
            fontNormal.Initialize(textureFont);

            soundEffectPoing = Content.Load<SoundEffect>("poing");
            soundEffectLaugh = Content.Load<SoundEffect>("kidshoorah");
            soundEffectPop = Content.Load<SoundEffect>("idea");
            soundEffectGlass = Content.Load<SoundEffect>("glassbreak");
            soundEffectFinished = Content.Load<SoundEffect>("finished");
            soundEffectCountDown = Content.Load<SoundEffect>("counting");

            video = Content.Load<Video>("crowd");
            videoPlayer = new Microsoft.Xna.Framework.Media.VideoPlayer();
            gameState = GameState.SHOWHIGHSCORES;
        }

        void InitGame()
        {
            gameState = GameState.PLAYING;
            GameTimeMilliSeconds = 0;
            requestRestartTime = 0;
            numPlayersFinished = 0;
            MakePhoto();
            scoreYPosition = 0;
            scoreListType = 0;
            scoreListElapsed = 0;
            scoreYPosition = 0;
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            videoCapture.Dispose();
            SaveHighscores();
            // TODO: Unload any non ContentManager content here
        }

        private void AddScore(int playerNumber, int score)
        {
            if(!gameState.Equals(GameState.PLAYING))
            {
                return;
            }

            if (players[playerNumber].X > 1800)
            {
                return;
            }

            players[playerNumber].Score += score;
            if (players[playerNumber].Score > 44)
            {
                players[playerNumber].Score = 44;
            }

            players[playerNumber].Moving += score*40;

            for (int k=0; k<score; k++)
            {
                if(k==0)
                {
                    balls.Add(new Ball(-100 - (playerNumber*180), 950, 18, -20, players[playerNumber].TextureBall, 0, score, players[playerNumber].Color));
                }
                else
                {
                    balls.Add(new Ball(-100 - (playerNumber*180), 950, 18, -20, players[playerNumber].TextureBall, k*15, 0, players[playerNumber].Color));
                }
            }
        }

        private void CreateFirework(Vector2 Position)
        {
            Microsoft.Xna.Framework.Color color1 = new Microsoft.Xna.Framework.Color(rand.NextFloat(0, 1), rand.NextFloat(0, 1), rand.NextFloat(0, 1)); // yellow
            Microsoft.Xna.Framework.Color color2 = new Microsoft.Xna.Framework.Color(rand.NextFloat(0, 1), rand.NextFloat(0, 1), rand.NextFloat(0, 1)); // yellow

            for (int i = 0; i < 1200; i++)
            {
                float speed = 18f * (1f - 1 / rand.NextFloat(1f, 10f));
                Microsoft.Xna.Framework.Color color = Microsoft.Xna.Framework.Color.Lerp(color1, color2, rand.NextFloat(0, 1));
                var state = new ParticleState()
                {
                    Velocity = rand.NextVector2(speed, speed),
                    Type = ParticleType.None,
                    LengthMultiplier = 1
                };
                state.Velocity.Y -= 30;
                particleManager.CreateParticle(textureLaser, Position, color, 190, 1.5f, state);
            }
        }

        void AddRandomBalls()
        {
            Random rnd = new Random();
            int k = rnd.Next(1000);
            if (k < 4)
            {
                AddScore(k, (rnd.Next(3)+1));
            }
        }

        void AddFireworks()
        {
            int k = rand.Next(80);
            if (k < 4)
            {
                CreateFirework(new Vector2(rand.Next(1920), rand.Next(1080)));
            }
        }                   


        void UpdatePlayers()
        {
            foreach (Player player in players)
            {
                if (player.X <= 1800 && player.Moving > 0)
                {
                    player.Moving--;
                    player.X++;

                    if (player.X>1800)
                    {
                        // Player won
                        if (numPlayersFinished == 0)
                        {
                            chart.Visible = false;
                            winningPlayerNumber = players.IndexOf(player);
                            playingVideo = true;
                            videoPlayer.Play(video);
                        }
                        numPlayersFinished++;
                        player.GameTimeMilliSeconds = GameTimeMilliSeconds;
                        int numplayersStarted = 0;
                        foreach (Player player2 in players)
                        {
                            if(player2.X>X_STARTVALUE)
                            {
                                numplayersStarted++;
                            }
                        }
                        // Game finished
                        if (numPlayersFinished == numplayersStarted)
                        {
                            chart.Visible = true;
                            gameState = GameState.FINISHED;
                            UpdateHighscores();
                            if (videoPlayer.State == MediaState.Playing)
                            {
                                videoPlayer.Stop();
                                playingVideo = false;
                            }
                            soundEffectFinished.Play();
                        }
                    }
                }
            }
        }

        private void UpdateBalls()
        {
            for (int i = 0; i < balls.Count; i++)
            {
                Ball ball = balls[i];
                if (ball.isAlive())
                {
                    try
                    {
                        ball.UpdateFrame();
                        if (ball.Delay == 0)
                        {
                            ball.X += ball.XSpeed;
                            if (ball.X > 1700)
                            {
                                ball.Lifetime = 0;
                            }
                            ball.Y += ball.YSpeed;
                            ball.YSpeed += 0.8f;
                            if (ball.Y > ball.YInitial)
                            {
                                soundEffectPoing.Play();
                                ball.YSpeed = ball.YSpeedInitial;
                            }
                        }
                        else
                        {
                            ball.Delay--;
                        }
                    }
                    catch (OverflowException)
                    {
                        balls.RemoveAt(i);
                    }

                }
                else
                {
                    Vector2 position = new Vector2(ball.X, ball.Y);
                    smokePlume.AddParticles(position,ball.Color);

                    if (ball.Sound == 1)
                    {
                        soundEffectPop.Play();
                    }
                    if (ball.Sound == 2)
                    {
                        soundEffectGlass.Play();
                    }
                    if (ball.Sound == 3)
                    {
                        soundEffectLaugh.Play();
                    }
                    balls.RemoveAt(i);
                }
            }
        }

        private void ResetPlayers()
        {
            players.Clear();
            players.Add(new Player("Olifant", X_STARTVALUE, 170, textureElephant, textureBallElephant, new Color(211, 190, 229)));
            players.Add(new Player("Schildpad", X_STARTVALUE, 290, textureTurtle, textureBallTurtle, new Color(0, 255, 0)));
            players.Add(new Player("Vogel", X_STARTVALUE, 335, textureBird, textureBallBird, new Color(0, 200, 255)));
            players.Add(new Player("Hond", X_STARTVALUE, 430, textureDog, textureBallDog, new Color(255, 100, 0)));
        }

        private void UpdateGame(TimeSpan elapsedTime)
        {
            // The time since Update was called last.
            GameTimeMilliSeconds += Convert.ToInt32(elapsedTime.TotalMilliseconds);
            ballRotation += (float)elapsedTime.TotalSeconds * 1.5f;

            UpdateChart();
            AddRandomBalls();
            UpdatePlayers();
            UpdateBalls();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (!serialPort.IsOpen)     // keep trying to open the port
            {
                checkPortOpenElapsed += gameTime.ElapsedGameTime.Milliseconds;
                if (checkPortOpenElapsed > 5000)       // check every 5 seconds
                {
                    checkPortOpenElapsed = 0;
                    try
                    {
                        serialPort.Open();
                    }
                    catch (IOException)
                    {
                        Console.WriteLine("USB port not available.");
                    }
                    catch (UnauthorizedAccessException)
                    {
                        Console.WriteLine("USB port used (by arduino program?)");
                    }
                }
            }

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == Microsoft.Xna.Framework.Input.ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape))
                Exit();

            if (enterPressed && Keyboard.GetState().IsKeyUp(Microsoft.Xna.Framework.Input.Keys.Enter))
            {
                enterPressed = false;
            }

            if (!enterPressed && Keyboard.GetState().IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Enter))
            {
                enterPressed = true;
                if (gameState.Equals(GameState.REQUESTRESTART))
                {
                    ResetPlayers();
                    InitGame();
                }
                else if(gameState.Equals(GameState.SHOWHIGHSCORES))
                {
                    gameState = GameState.COUNTDOWN;
                    ResetPlayers();
                    //                    InitGame();
                    soundEffectCountDown.Play();
                }
                else if (gameState.Equals(GameState.PLAYING))
                {
                    gameState = GameState.REQUESTRESTART;
                    requestRestartTime = gameTime.TotalGameTime.TotalMilliseconds;
                }
                else if (gameState.Equals(GameState.FINISHED))
                {
                    gameState = GameState.SHOWHIGHSCORES;
                    chart.Visible = false;
                }
            }

            if (playingVideo)
            {
                if (videoPlayer.State == MediaState.Stopped)
                {
                    playingVideo = false;
                }
            }

            if (gameState.Equals(GameState.PLAYING))
            {
                UpdateGame(gameTime.ElapsedGameTime);
            }
            if (gameState.Equals(GameState.REQUESTRESTART))
            {
                if(gameTime.TotalGameTime.TotalMilliseconds-requestRestartTime>4000)
                {
                    // Cancel the request to restart
                    gameState = GameState.PLAYING;

                }
            }
            if (gameState.Equals(GameState.SHOWHIGHSCORES))
            {
                scoreYPosition += 2;
                scoreListElapsed += gameTime.ElapsedGameTime.Milliseconds;
                if (scoreListElapsed > 20000)
                {
                    scoreListElapsed = 0;
                    scoreYPosition = 0;
                    scoreListType++;
                    if (scoreListType > 2)
                    {
                        scoreListType = 0;
                    }
                }
            }
            if (gameState.Equals(GameState.COUNTDOWN))
            {
                countdownPhase *= 1.08f;
                if(countdownPhase>100)
                {
                    countdownPhase = 1;
                    countdownLetter--;
                    if(countdownLetter<0)
                    {
                        countdownLetter = 5;
                        InitGame();
                    }
                }
            }
            if (gameState.Equals(GameState.FINISHED))
            {
                showWinningElapsed += gameTime.ElapsedGameTime.Milliseconds;
                if(showWinningElapsed>20000)
                {
                    showWinningElapsed=0;
                    gameState = GameState.SHOWHIGHSCORES;
                    chart.Visible = false;
                }
                particleManager.Update();
                AddFireworks();
            }

            base.Update(gameTime);
        }

        string GameTimeToString(int GameTimeMilliSeconds)
        {
            string seconds = ((GameTimeMilliSeconds / 10000) % 6).ToString() + ((GameTimeMilliSeconds / 1000) % 10).ToString();
            string minutes = ((GameTimeMilliSeconds / 600000) % 10).ToString() + ((GameTimeMilliSeconds / 60000) % 10).ToString();
            string millisecs = (GameTimeMilliSeconds % 1000).ToString();
            if (millisecs.Length == 2)
                millisecs = "0" + millisecs;
            if (millisecs.Length == 1)
                millisecs = "00" + millisecs;

            return minutes + ":" + seconds + "." + millisecs;
        }

        void DrawGame()
        {
            SpriteBatch.Draw(textureBackground, new Vector2(0, 0), Microsoft.Xna.Framework.Color.White);

            int seconds = (GameTimeMilliSeconds / 1000) % 10;
            int seconds10 = (GameTimeMilliSeconds / 10000) % 6;
            int minutes = (GameTimeMilliSeconds / 60000) % 10;
            int minutes10 = (GameTimeMilliSeconds / 600000) % 10;
            SpriteBatch.Draw(textureMatrix[minutes10], new Vector2(1499, 26), Microsoft.Xna.Framework.Color.White);
            SpriteBatch.Draw(textureMatrix[minutes], new Vector2(1499 + 96, 26), Microsoft.Xna.Framework.Color.White);
            SpriteBatch.Draw(textureMatrix[seconds10], new Vector2(1499 + 225, 26), Microsoft.Xna.Framework.Color.White);
            SpriteBatch.Draw(textureMatrix[seconds], new Vector2(1499 + 321, 26), Microsoft.Xna.Framework.Color.White);

            foreach (Player player in players)
            {
                SpriteBatch.Draw(player.Texture, new Vector2(player.X, player.Y), new Microsoft.Xna.Framework.Rectangle(0, 0, player.Texture.Width, player.Texture.Height), Microsoft.Xna.Framework.Color.White, 0f, new Vector2(0, 0), 0.5f, SpriteEffects.None, 0f);
            }

            if (gameState.Equals(GameState.PLAYING))
            {
                foreach (Ball ball in balls.ToList())
                {
                    // ball yspeed: -20..22
                    SpriteBatch.Draw(ball.Texture, new Microsoft.Xna.Framework.Rectangle(Convert.ToInt32(ball.X), Convert.ToInt32(ball.Y), Convert.ToInt32(ball.Texture.Width * (1 + (Math.Abs(ball.YSpeed) / 180.0f))), Convert.ToInt32(ball.Texture.Height * (1 - (Math.Abs(ball.YSpeed) / 180.0f)))), new Microsoft.Xna.Framework.Rectangle(0, 0, ball.Texture.Width, ball.Texture.Height), Microsoft.Xna.Framework.Color.White, ballRotation, new Vector2(ball.Texture.Width / 2, ball.Texture.Height / 2), SpriteEffects.None, 0f);
                }
            }

            if (videoPlayer.State != MediaState.Stopped)
            {
                SpriteBatch.Draw(videoPlayer.GetTexture(), new Microsoft.Xna.Framework.Rectangle(0, 650, 960, 500), new Microsoft.Xna.Framework.Rectangle(0, 0, videoPlayer.GetTexture().Width, videoPlayer.GetTexture().Height), Microsoft.Xna.Framework.Color.White, 0.0f, new Vector2(0, 0), SpriteEffects.FlipHorizontally, 0.0f);
                SpriteBatch.Draw(videoPlayer.GetTexture(), new Microsoft.Xna.Framework.Rectangle(960, 650, 960, 500), Microsoft.Xna.Framework.Color.White);
            }

            if (gameState.Equals(GameState.FINISHED))
            {
                SpriteBatch.End();
                SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive);
                particleManager.Draw(SpriteBatch);
                SpriteBatch.End();
                SpriteBatch.Begin();
                fontNormal.Print(SpriteBatch, "Spel afgelopen", 420, 40, true);
                fontNormal.Print(SpriteBatch, players[winningPlayerNumber].Name + " heeft gewonnen!", 20, 240, true);
                fontNormal.Print(SpriteBatch, "winnende tijd; " + GameTimeToString(players[winningPlayerNumber].GameTimeMilliSeconds), 50, 440, true);
            }

            if (gameState.Equals(GameState.REQUESTRESTART))
            {
                fontNormal.Print(SpriteBatch, "Nieuw spel starten?", 200, 250, true);
                fontNormal.Print(SpriteBatch, "Druk nogmaals op start", 50, 460, true);
            }
        }

        private void DisplayAvatar(string name, int x, int y)
        {
            Texture2D texture = textureBird;
            switch(name)
            {
                case "Olifant":
                    texture = textureElephant;
                    break;
                case "Schildpad":
                    texture = textureTurtle;
                    break;
                case "Hond":
                    texture = textureDog;
                    break;
            }
            SpriteBatch.Draw(texture, new Microsoft.Xna.Framework.Rectangle(x, y, 100, 100), new Microsoft.Xna.Framework.Rectangle(0, 0, texture.Width, texture.Height), Microsoft.Xna.Framework.Color.White);
        }

        void DrawHighscores(GameTime gameTime)
        {
            SpriteBatch.Draw(textureBackgroundHighscores, new Vector2(0, 0), Microsoft.Xna.Framework.Color.White);

            if (scoreListType == 0)
            {
                for (int k=0; k<highscoresDay.Count; k++)
                {
                    fontScore.Print(SpriteBatch, (k+1).ToString(), 60, k * 180 + 1080 - scoreYPosition, false);
                    SpriteBatch.Draw(highscoresDay[k].Photo, new Vector2(160, k * 180 + 1040 - scoreYPosition), Microsoft.Xna.Framework.Color.White);
                    fontScore.Print(SpriteBatch, GameTimeToString(highscoresDay[k].GameTimeMilliSeconds), 380, k * 180 + 1080 - scoreYPosition, false);
                    fontScore.Print(SpriteBatch, highscoresDay[k].Date.ToString("HH:mm:ss"), 830, k * 180 + 1080 - scoreYPosition, false);
                    DisplayAvatar(highscoresDay[k].Name, 1230, k * 180 + 1080 - scoreYPosition);
                }
                SpriteBatch.Draw(textureBackgroundHighscores, new Vector2(0, 0), new Microsoft.Xna.Framework.Rectangle(0, 0, 1600, 280), Microsoft.Xna.Framework.Color.White);
                fontNormal.Print(SpriteBatch, "Topscores van vandaag", 70, 50, true);
            }
            else if (scoreListType == 1)
            {
                for (int k = 0; k < highscoresMonth.Count; k++)
                {
                    fontScore.Print(SpriteBatch, (k + 1).ToString(), 60, k * 180 + 1080 - scoreYPosition, false);
                    SpriteBatch.Draw(highscoresMonth[k].Photo, new Vector2(160, k * 180 + 1040 - scoreYPosition), Microsoft.Xna.Framework.Color.White);
                    fontScore.Print(SpriteBatch, GameTimeToString(highscoresMonth[k].GameTimeMilliSeconds), 380, k * 180 + 1080 - scoreYPosition, false);
                    fontScore.Print(SpriteBatch, highscoresMonth[k].Date.ToString("dd/MM/yy"), 830, k * 180 + 1080 - scoreYPosition, false);
                    DisplayAvatar(highscoresMonth[k].Name, 1230, k * 180 + 1080 - scoreYPosition);
                }
                SpriteBatch.Draw(textureBackgroundHighscores, new Vector2(0, 0), new Microsoft.Xna.Framework.Rectangle(0, 0, 1600, 280), Microsoft.Xna.Framework.Color.White);
                fontNormal.Print(SpriteBatch, "Topscores van de maand", 10, 50, true);
            }
            else
            {
                for (int k = 0; k < highscoresAll.Count; k++)
                {
                    fontScore.Print(SpriteBatch, (k + 1).ToString(), 60, k * 180 + 1080 - scoreYPosition, false);
                    SpriteBatch.Draw(highscoresAll[k].Photo, new Vector2(160, k * 180 + 1040 - scoreYPosition), Microsoft.Xna.Framework.Color.White);
                    fontScore.Print(SpriteBatch, GameTimeToString(highscoresAll[k].GameTimeMilliSeconds), 380, k * 180 + 1080 - scoreYPosition, false);
                    fontScore.Print(SpriteBatch, highscoresAll[k].Date.ToString("dd/MM/yy"), 830, k * 180 + 1080 - scoreYPosition, false);
                    DisplayAvatar(highscoresAll[k].Name, 1230, k * 180 + 1080 - scoreYPosition);
                }
                SpriteBatch.Draw(textureBackgroundHighscores, new Vector2(0, 0), new Microsoft.Xna.Framework.Rectangle(0, 0, 1600, 280), Microsoft.Xna.Framework.Color.White);
                fontNormal.Print(SpriteBatch, "Topscores aller tijden", 150, 50, true);
            }

            int alpha = (int)(gameTime.TotalGameTime.TotalMilliseconds / 2 % 610);
            if (alpha > 355)
            {
                alpha = 610 - alpha;
            }
            if (alpha > 255)
            {
                alpha = 255;
            }
            Microsoft.Xna.Framework.Color color = new Microsoft.Xna.Framework.Color(255, 255, 255, alpha);
            SpriteBatch.End();
            SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);
            SpriteBatch.Draw(textureStartbutton, new Vector2(1400, 650), new Microsoft.Xna.Framework.Rectangle(0, 0, textureStartbutton.Width, 328), color);
            SpriteBatch.Draw(textureStartbutton, new Vector2(1400, 970), new Microsoft.Xna.Framework.Rectangle(0, 328, textureStartbutton.Width, textureStartbutton.Height), color/*Microsoft.Xna.Framework.Color.White*/);
            SpriteBatch.End();
            SpriteBatch.Begin();
            SpriteBatch.Draw(textureArrow, new Vector2(1520, 520 - (int)(0.5*alpha)), new Microsoft.Xna.Framework.Rectangle(0, 0, textureArrow.Width, textureArrow.Height), Microsoft.Xna.Framework.Color.White);

        }

        void DrawCountDown()
        {
            int alpha = (int)(255 - 4*countdownPhase);
            if(alpha<0)
            {
                alpha = 0;
            }
            Microsoft.Xna.Framework.Color color = new Microsoft.Xna.Framework.Color(255, 255, 255, alpha);
            if (countdownLetter==5)
            {
                // ratio 2:3
                SpriteBatch.Draw(textureCountDown, new Microsoft.Xna.Framework.Rectangle((int)(860 - 100 * countdownPhase), (int)(390 - 150 * countdownPhase), (int)(200+200*countdownPhase), (int)(300+300*countdownPhase)), new Microsoft.Xna.Framework.Rectangle(0, 0, 214, 319), color);
            }
            if (countdownLetter == 4)
            {
                // ratio 2:3
                SpriteBatch.Draw(textureCountDown, new Microsoft.Xna.Framework.Rectangle((int)(860 - 100 * countdownPhase), (int)(390 - 150 * countdownPhase), (int)(200 + 200 * countdownPhase), (int)(300 + 300 * countdownPhase)), new Microsoft.Xna.Framework.Rectangle(214, 0, 266, 319), color);
            }
            if (countdownLetter == 3)
            {
                // ratio 2:3
                SpriteBatch.Draw(textureCountDown, new Microsoft.Xna.Framework.Rectangle((int)(860 - 100 * countdownPhase), (int)(390 - 150 * countdownPhase), (int)(200 + 200 * countdownPhase), (int)(300 + 300 * countdownPhase)), new Microsoft.Xna.Framework.Rectangle(480, 0, 222, 319), color);
            }
            if (countdownLetter == 2)
            {
                // ratio 2:3
                SpriteBatch.Draw(textureCountDown, new Microsoft.Xna.Framework.Rectangle((int)(860 - 100 * countdownPhase), (int)(390 - 150 * countdownPhase), (int)(200 + 200 * countdownPhase), (int)(300 + 300 * countdownPhase)), new Microsoft.Xna.Framework.Rectangle(702, 0, 232, 319), color);
            }
            if (countdownLetter == 1)
            {
                // ratio 2:3
                SpriteBatch.Draw(textureCountDown, new Microsoft.Xna.Framework.Rectangle((int)(800 - 100 * countdownPhase), (int)(390 - 150 * countdownPhase), (int)(200 + 200 * countdownPhase), (int)(300 + 300 * countdownPhase)), new Microsoft.Xna.Framework.Rectangle(936, 0, 178, 319), color);
            }
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.Black);

            BlendState blend = new BlendState();
            blend.AlphaSourceBlend = Blend.Zero;
            blend.AlphaDestinationBlend = Blend.SourceColor;
            blend.ColorSourceBlend = Blend.Zero;
            blend.ColorDestinationBlend = Blend.SourceColor;

            SpriteBatch.Begin();

            if(gameState.Equals(GameState.SHOWHIGHSCORES))
            {
                DrawHighscores(gameTime);
            }
            else
            {
                DrawGame();
                if (gameState.Equals(GameState.COUNTDOWN))
                {
                    DrawCountDown();
                }
                if (playingVideo)
                {
                    int pos = (int)(gameTime.TotalGameTime.TotalMilliseconds / 5 % 255);
                    if (pos > 128)
                    {
                        pos = 255 - pos;
                    }
                    Color color = new Color(255, 255, 255, 200);
                    SpriteBatch.Draw(players[winningPlayerNumber].Texture, new Vector2(740, 380-pos), color);
                    SpriteBatch.Draw(textureGewonnen, new Vector2(400, 600-pos), Color.White);
                }
            }

            if (!serialPort.IsOpen && gameTime.TotalGameTime.TotalSeconds%2>1)
            {
                SpriteBatch.Draw(textureGeenArduino, new Vector2(700, 50), new Microsoft.Xna.Framework.Rectangle(0, 0, textureGeenArduino.Width, textureGeenArduino.Height), Microsoft.Xna.Framework.Color.White);
            }

            SpriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
