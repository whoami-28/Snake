using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Snake
{
    public partial class Game : Form
    {
        private List<Point> snake = new List<Point>();
        private Point food;
        private Direction currentDirection = Direction.Right;
        private int score = 0;
        private int cellSize = 16;
        private int boardWidth = 36;
        private int boardHeight = 22;

        private Timer gameTimer = new Timer();
        private bool isDirectionChanged = false;
        private Random random = new Random();

        private Dictionary<string, Image> fruitSprites = new Dictionary<string, Image>();
        private Dictionary<string, Image> snakeSprites = new Dictionary<string, Image>();
        
        public Game()
        {
            InitializeComponent();
            pictureBox.Width = boardWidth * cellSize;
            pictureBox.Height = boardHeight * cellSize;
            gameTimer.Tick += GameTimer_Tick;
            this.KeyPreview = true;
            this.KeyDown += Form1_KeyDown;
            pictureBox.Paint += pictureBox_Paint;
            LoadSprites();
            StartGame();
        }
        private void StartGame()
        {
            score = 0;
            currentDirection = Direction.Right;
            snake.Clear();
            gameTimer.Interval = GlobalSettings.GameTimeInterval;
            snake.Add(new Point(boardWidth / 2, boardHeight / 2));
            snake.Add(new Point(boardWidth / 2 - 1, boardHeight / 2));
            snake.Add(new Point(boardWidth / 2 - 2, boardHeight / 2));

            GenerateFood();
            gameTimer.Start();
            if (this.Controls.Find("lblScore", true).Length > 0)
            {
                lblScore.Text = "Score: 0";
            }
        }
        private void GameTimer_Tick(object sender, EventArgs e)
        {
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
                lblScore.Text = "Score: " + score;
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
            string message = $"Игра окончена!\nВаш счёт: {score}\nНажмите ОК для начала новой игры.";

            if (MessageBox.Show(message, "Game Over", MessageBoxButtons.OK, MessageBoxIcon.Information) == DialogResult.OK)
            {
                this.Hide();
                StartMenu menu = new StartMenu();
                menu.Show();
            }
        }
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (isDirectionChanged)
            {
                return;
            }
            Direction newDirection = currentDirection;

            switch (e.KeyCode)
            {
                case Keys.Up:
                case Keys.W:
                    if (currentDirection != Direction.Down) newDirection = Direction.Up;
                    break;
                case Keys.Down:
                case Keys.S:
                    if (currentDirection != Direction.Up) newDirection = Direction.Down;
                    break;
                case Keys.Left:
                case Keys.A:
                    if (currentDirection != Direction.Right) newDirection = Direction.Left;
                    break;
                case Keys.Right:
                case Keys.D:
                    if (currentDirection != Direction.Left) newDirection = Direction.Right;
                    break;
            }
            if (newDirection != currentDirection)
            {
                currentDirection = newDirection;
                isDirectionChanged = true;
            }
        }
        private void pictureBox_Paint(object sender, PaintEventArgs e)
        {
            Graphics canvas = e.Graphics;

            float scaleFactor = 1.5f;
            int enlargedSize = (int)(cellSize * scaleFactor);
            int headSize = (int)(cellSize * scaleFactor);
            int offset = (headSize - cellSize) / 2;
            bool useSprites = GlobalSettings.CurrentRenderStyle == RenderStyle.Normal;

            Pen gridPen = new Pen(Color.LimeGreen, 1);
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
                Rectangle rect = new Rectangle(
                    snake[i].X * cellSize,
                    snake[i].Y * cellSize,
                    cellSize,
                    cellSize
                );
                if (i == 0)
                {
                    rect = new Rectangle(
                        snake[i].X * cellSize - offset,
                        snake[i].Y * cellSize - offset,
                        headSize,
                        headSize
                    );
                }
                if (useSprites)
                {
                    Image segmentImage = GetCorrectSprite(i);
                    if (segmentImage != null)
                    {
                        canvas.DrawImage(segmentImage, rect);
                    }
                    else
                    {
                        canvas.FillRectangle(Brushes.Gray, rect);
                    }
                }
                else
                {
                    if (i == 0)
                    {
                        canvas.FillRectangle(Brushes.DarkGreen, rect);
                    }
                    else
                    {
                        canvas.FillRectangle(Brushes.Green, rect);
                    }
                }
            }

            Rectangle foodRect = new Rectangle(
                food.X * cellSize - offset,
                food.Y * cellSize - offset,
                enlargedSize,
                enlargedSize
            );

            if (useSprites)
            {
                string fruitName = GlobalSettings.CurrentFruitStyle.ToString();
                Image selectedFruitImage = fruitSprites.ContainsKey(fruitName) ? fruitSprites[fruitName] : Properties.Resources.apple;

                if (selectedFruitImage != null)
                {
                    canvas.DrawImage(selectedFruitImage, foodRect);
                }
                else
                {
                    canvas.FillRectangle(Brushes.Red, foodRect);
                }
            }
            else
            {
                canvas.FillRectangle(Brushes.Red, foodRect);
            }
        }

        private Image GetCorrectSprite(int index)
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
