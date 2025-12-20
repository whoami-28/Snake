using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using GDI = System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media;

namespace Snake
{
    public partial class Game : Form
    {
        private List<Point> snake = new List<Point>();
        private Point food;
        private Direction currentDirection = Direction.Right;
        private int score = 0;
        private int cellSize = 20;
        private int boardWidth = 36;
        private int boardHeight = 22;

        private Timer gameTimer = new Timer();
        private bool isDirectionChanged = false;
        private Random random = new Random();
        private bool isPaused = false;
        private bool isGameOver = false;

        private Dictionary<string, GDI.Image> fruitSprites = new Dictionary<string, GDI.Image>();
        private Dictionary<string, GDI.Image> snakeSprites = new Dictionary<string, GDI.Image>();
        private MediaPlayer mediaPlayer = new MediaPlayer();

        public Game()
        {
            InitializeComponent();
            pictureBox.Width = boardWidth * cellSize;
            pictureBox.Height = boardHeight * cellSize;
            this.ClientSize = new GDI.Size(pictureBox.Width + 24, pictureBox.Height + 24);
            this.MaximizeBox = false;
            this.MinimizeBox = true;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            gameTimer.Tick += GameTimer_Tick;
            this.KeyPreview = true;
            this.KeyDown += Form1_KeyDown;
            pictureBox.Paint += pictureBox_Paint;
            LoadSprites();
            InitializeMusicPlayer();
            StartGame();
        }
        private void InitializeMusicPlayer()
        {
            string musicFileName = "background_music.wav";
            string musicPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, musicFileName);
            mediaPlayer.Open(new Uri(musicPath));
            mediaPlayer.Volume = 1.0;
            mediaPlayer.MediaEnded += MediaPlayer_MediaEnded;
        }
        private void MediaPlayer_MediaEnded(object sender, EventArgs e)
        {
            mediaPlayer.Position = TimeSpan.Zero;
            mediaPlayer.Play();
        }
        private void StartGame()
        {
            isGameOver = false;
            score = 0;
            currentDirection = Direction.Right;
            snake.Clear();
            gameTimer.Interval = GlobalSettings.GameTimeInterval;
            snake.Add(new Point(boardWidth / 2, boardHeight / 2));
            snake.Add(new Point(boardWidth / 2 - 1, boardHeight / 2));
            snake.Add(new Point(boardWidth / 2 - 2, boardHeight / 2));

            if (GlobalSettings.IsMusicEnabled)
            {
                mediaPlayer.Play();
            }
            GenerateFood();
            gameTimer.Start();
            if (this.Controls.Find("lblScore", true).Length > 0)
            {
                this.Text = "Snake | Score: 0";
            }
        }
        private void GameTimer_Tick(object sender, EventArgs e)
        {
            if (isPaused)
            {
                return;
            }
            isDirectionChanged = false;
            MoveSnake();
            CheckCollisions();
            pictureBox.Invalidate();
        }
        private void MoveSnake()
        {
            Point currentHead = snake[0];
            Point newHead = new Point(currentHead.X, currentHead.Y);
            switch (currentDirection)
            {
                case Direction.Up: newHead.Y--; break;
                case Direction.Down: newHead.Y++; break;
                case Direction.Left: newHead.X--; break;
                case Direction.Right: newHead.X++; break;
            }
            snake.Insert(0, newHead);
            if (newHead.X == food.X && newHead.Y == food.Y)
            {
                score++;
                this.Text = "Snake | Score: " + score;
                if (GlobalSettings.SelectedMode != GameMode.NoAcceleration)
                {
                    if (score % 5 == 0)
                    {
                        if (gameTimer.Interval > 20)
                        {
                            gameTimer.Interval -= 10;
                        }
                    }
                }
                GenerateFood();
            }
            else
            {
                snake.RemoveAt(snake.Count - 1);
            }
        }
        private void GenerateFood()
        {
            int maxX = boardWidth - 1;
            int maxY = boardHeight - 1;
            do
            {
                food = new Point(random.Next(0, maxX + 1), random.Next(0, maxY + 1));
            }
            while (snake.Any(p => p.X == food.X && p.Y == food.Y));
        }
        private void CheckCollisions()
        {
            Point head = snake[0];
            if (head.X < 0 || head.Y < 0 || head.X >= boardWidth || head.Y >= boardHeight)
            {
                if (GlobalSettings.SelectedMode == GameMode.Teleport || GlobalSettings.SelectedMode == GameMode.Immortal)
                {
                    if (head.X < 0) head.X = boardWidth - 1;
                    else if (head.X >= boardWidth) head.X = 0;
                    else if (head.Y < 0) head.Y = boardHeight - 1;
                    else if (head.Y >= boardHeight) head.Y = 0;
                    snake[0] = head;
                }
                else
                {
                    GameOver();
                    return;
                }
            }
            if (GlobalSettings.SelectedMode != GameMode.Immortal)
            {
                for (int i = 1; i < snake.Count; i++)
                {
                    if (head.X == snake[i].X && head.Y == snake[i].Y)
                    {
                        GameOver();
                        return;
                    }
                }
            }
        }

        private void GameOver()
        {
            gameTimer.Stop();
            isGameOver = true;
            if (GlobalSettings.IsMusicEnabled)
            {
                mediaPlayer.Stop();
            }

            pictureBox.Invalidate();
        }
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (isGameOver)
            {
                if (e.KeyCode == Keys.Enter)
                {
                    StartGame();
                }
                else
                {
                    this.Close();
                }
                return;
            }
            if (e.KeyCode == Keys.Space)
            {
                TogglePause();
                if (isPaused) return;
            }

            if (isPaused)
            {
                return;
            }

            if (isDirectionChanged == false)
            {
                if (e.KeyCode == Keys.Right && currentDirection != Direction.Left)
                {
                    currentDirection = Direction.Right;
                    isDirectionChanged = true;
                }
                else if (e.KeyCode == Keys.Left && currentDirection != Direction.Right)
                {
                    currentDirection = Direction.Left;
                    isDirectionChanged = true;
                }
                else if (e.KeyCode == Keys.Up && currentDirection != Direction.Down)
                {
                    currentDirection = Direction.Up;
                    isDirectionChanged = true;
                }
                else if (e.KeyCode == Keys.Down && currentDirection != Direction.Up)
                {
                    currentDirection = Direction.Down;
                    isDirectionChanged = true;
                }
            }
        }
        private void TogglePause()
        {
            isPaused = !isPaused;

            if (isPaused)
            {
                timer.Stop();
                if (GlobalSettings.IsMusicEnabled)
                {
                    mediaPlayer.Pause();
                }
            }
            else
            {
                timer.Start();
                if (GlobalSettings.IsMusicEnabled)
                {
                    mediaPlayer.Play();
                }
            }
            pictureBox.Invalidate();
        }
        private void pictureBox_Paint(object sender, PaintEventArgs e)
        {
            GDI.Graphics canvas = e.Graphics;

            float scaleFactor = 1.5f;
            int enlargedSize = (int)(cellSize * scaleFactor);
            int headSize = (int)(cellSize * scaleFactor);
            int offset = (headSize - cellSize) / 2;
            bool useSprites = GlobalSettings.CurrentRenderStyle == RenderStyle.Normal;

            GDI.Pen gridPen = new GDI.Pen(GDI.Color.LimeGreen, 1);
            for (int i = 0; i <= boardWidth; i++)
            {
                canvas.DrawLine(gridPen, i * cellSize, 0, i * cellSize, pictureBox.Height);
            }
            for (int i = 0; i <= boardHeight; i++)
            {
                canvas.DrawLine(gridPen, 0, i * cellSize, pictureBox.Width, i * cellSize);
            }

            for (int i = 0; i < snake.Count; i++)
            {
                GDI.Rectangle rect = new GDI.Rectangle(
                    snake[i].X * cellSize,
                    snake[i].Y * cellSize,
                    cellSize,
                    cellSize
                );
                if (i == 0)
                {
                    rect = new GDI.Rectangle(
                        snake[i].X * cellSize - offset,
                        snake[i].Y * cellSize - offset,
                        headSize,
                        headSize
                    );
                }
                if (useSprites)
                {
                    GDI.Image segmentImage = GetCorrectSprite(i);
                    if (segmentImage != null)
                    {
                        canvas.DrawImage(segmentImage, rect);
                    }
                    else
                    {
                        canvas.FillRectangle(GDI.Brushes.Gray, rect);
                    }
                }
                else
                {
                    if (i == 0)
                    {
                        canvas.FillRectangle(GDI.Brushes.DarkGreen, rect);
                    }
                    else
                    {
                        canvas.FillRectangle(GDI.Brushes.Green, rect);
                    }
                }
            }

            GDI.Rectangle foodRect = new GDI.Rectangle(
                food.X * cellSize - offset,
                food.Y * cellSize - offset,
                enlargedSize,
                enlargedSize
            );

            if (useSprites)
            {
                string fruitName = GlobalSettings.CurrentFruitStyle.ToString();
                GDI.Image selectedFruitImage = fruitSprites.ContainsKey(fruitName) ? fruitSprites[fruitName] : Properties.Resources.apple;

                if (selectedFruitImage != null)
                {
                    canvas.DrawImage(selectedFruitImage, foodRect);
                }
                else
                {
                    canvas.FillRectangle(GDI.Brushes.Red, foodRect);
                }
            }
            else
            {
                canvas.FillRectangle(GDI.Brushes.Red, foodRect);
            }
            if (isPaused)
            {
                GDI.Brush semiTransparentBrush = new GDI.SolidBrush(GDI.Color.FromArgb(150, 0, 0, 0));
                canvas.FillRectangle(semiTransparentBrush, 0, 0, pictureBox.Width, pictureBox.Height);

                string pauseText = "PAUSE";
                GDI.Font font = new GDI.Font("Monocraft", 48, GDI.FontStyle.Bold);
                GDI.Brush brush = GDI.Brushes.White;

                GDI.SizeF textSize = canvas.MeasureString(pauseText, font);
                float x = (pictureBox.Width - textSize.Width) / 2;
                float y = (pictureBox.Height - textSize.Height) / 2;

                canvas.DrawString(pauseText, font, brush, x, y);
            }
            if (isGameOver)
            {
                GDI.Brush semiTransparentBrush = new GDI.SolidBrush(GDI.Color.FromArgb(180, 0, 0, 0));
                canvas.FillRectangle(semiTransparentBrush, 0, 0, pictureBox.Width, pictureBox.Height);

                string gameOverText = "GAME OVER";
                GDI.Font fontHeader = new GDI.Font("Monocraft", 50, GDI.FontStyle.Bold);
                GDI.Brush headerBrush = GDI.Brushes.Red;
                string instructionText = $"SCORE: {score}\n\n[ENTER] to Restart\n[ANY KEY] to Menu";
                GDI.Font fontInstruction = new GDI.Font("Monocraft", 20, GDI.FontStyle.Regular);

                GDI.Brush instructionBrush = GDI.Brushes.White;
                GDI.SizeF headerSize = canvas.MeasureString(gameOverText, fontHeader);
                float headerX = (pictureBox.Width - headerSize.Width) / 2;
                float headerY = (pictureBox.Height / 2) - headerSize.Height - 10;
                GDI.SizeF instructionSize = canvas.MeasureString(instructionText, fontInstruction);
                float instructionX = (pictureBox.Width - instructionSize.Width) / 2;
                float instructionY = (pictureBox.Height / 2) + 20;
                canvas.DrawString(gameOverText, fontHeader, headerBrush, headerX, headerY);
                canvas.DrawString(instructionText, fontInstruction, instructionBrush, instructionX, instructionY);
            }
        }

        private GDI.Image GetCorrectSprite(int index)
        {
            if (snakeSprites == null || snakeSprites.Count == 0) return null;

            Point current = snake[index];
    
            if (index == 0)
            {
                return snakeSprites[$"Head{currentDirection.ToString()}"];
            }

            if (index == snake.Count - 1)
            {
                Point prev1 = snake[index - 1];

                if (prev1.Y < current.Y) return snakeSprites["TailDown"];
                if (prev1.Y > current.Y) return snakeSprites["TailUp"];
                if (prev1.X < current.X) return snakeSprites["TailRight"];
                if (prev1.X > current.X) return snakeSprites["TailLeft"];
            }

            Point prev = snake[index - 1];
            Point next = snake[index + 1];

            if (prev.X == next.X)
            {
                return snakeSprites["BodyVertical"];
            }
            if (prev.Y == next.Y)
            {
                return snakeSprites["BodyHorizontal"];
            }
            Point A = prev;
            Point B = next;
            if ((A.X < current.X && B.Y < current.Y) || (B.X < current.X && A.Y < current.Y))
            {
                return snakeSprites["Corner_TopLeft"];
            }
            if ((A.X > current.X && B.Y < current.Y) || (B.X > current.X && A.Y < current.Y))
            {
                return snakeSprites["Corner_TopRight"];
            }
            if ((A.X < current.X && B.Y > current.Y) || (B.X < current.X && A.Y > current.Y))
            {
                return snakeSprites["Corner_BottomRight"];
            }
            if ((A.X > current.X && B.Y > current.Y) || (B.X > current.X && A.Y > current.Y))
            {
                return snakeSprites["Corner_BottomLeft"];
            }

            return null;
        }
        private void LoadSprites()
        {
            fruitSprites.Add(FruitView.Apple.ToString(), Properties.Resources.apple);
            fruitSprites.Add(FruitView.Banana.ToString(), Properties.Resources.banana);
            fruitSprites.Add(FruitView.Grape.ToString(), Properties.Resources.grape);
            fruitSprites.Add(FruitView.Melon.ToString(), Properties.Resources.melon);
            fruitSprites.Add(FruitView.Peach.ToString(), Properties.Resources.peach);
            fruitSprites.Add(FruitView.Pear.ToString(), Properties.Resources.pear);

            snakeSprites.Add("HeadRight", Properties.Resources.snakeheadright);
            snakeSprites.Add("HeadUp", Properties.Resources.snakeheadup);
            snakeSprites.Add("HeadDown", Properties.Resources.snakeheaddown);
            snakeSprites.Add("HeadLeft", Properties.Resources.snakeheadleft);

            snakeSprites.Add("BodyHorizontal", Properties.Resources.snakebodyhoriz);
            snakeSprites.Add("BodyVertical", Properties.Resources.snakebodyvert);

            snakeSprites.Add("Corner_TopLeft", Properties.Resources.snakeupleft);
            snakeSprites.Add("Corner_TopRight", Properties.Resources.snakeupright);
            snakeSprites.Add("Corner_BottomLeft", Properties.Resources.snakedownleft);
            snakeSprites.Add("Corner_BottomRight", Properties.Resources.snakedownright);

            snakeSprites.Add("TailRight", Properties.Resources.snakeright);
            snakeSprites.Add("TailLeft", Properties.Resources.snakeleft);
            snakeSprites.Add("TailUp", Properties.Resources.snakeup);
            snakeSprites.Add("TailDown", Properties.Resources.snakedown);
        }
        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}