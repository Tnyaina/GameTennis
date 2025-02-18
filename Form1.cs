using System;
using System.Drawing;
using System.Windows.Forms;
using GameTennis.Models;
using GameTennis.Forms;
using GameTennis.Data;

namespace GameTennis
{
    public partial class Form1 : Form
    {
        private Rectangle gameField;
        private Ball? ball;
        private Paddle? leftPaddleFront;
        private Paddle? leftPaddleBack;
        private Paddle? rightPaddleFront;
        private Paddle? rightPaddleBack;
        private System.Windows.Forms.Timer? gameTimer;
        private Score? gameScore;
        private TrajectoryForm? leftFrontTrajectoryForm;
        private TrajectoryForm? leftBackTrajectoryForm;
        private TrajectoryForm? rightFrontTrajectoryForm;
        private TrajectoryForm? rightBackTrajectoryForm;
        private MenuStrip? menuStrip;
        private Button? pauseButton;
        private Button? newGameButton;
        private bool isPaused = false;
        private const int WINDOW_WIDTH = 1400;
        private const int WINDOW_HEIGHT = 900;
        private const int FIELD_MARGIN = 150;
        private DatabaseConnection dbConnection;
        private int currentMatchId;
        private Button? targetLeftFrontButton;
        private Button? targetLeftBackButton;
        private Button? targetRightFrontButton;
        private Button? targetRightBackButton;

        public Form1()
        {
            InitializeComponent();
            InitializeGame();
        }

        private async void InitializeGame()
        {
            CleanupExistingGame();
            InitializeWindowSettings();
            
            var (playerNames, gameConfig) = await GetUserConfiguration();
            if (playerNames == null || gameConfig == null) return;

            InitializeUI();
            InitializeGameComponents(playerNames, gameConfig);
            await InitializeDatabase(playerNames);
            StartGameLoop();
        }

        private void CleanupExistingGame()
        {
            if (gameTimer != null)
            {
                gameTimer.Stop();
                gameTimer.Tick -= GameLoop;
                gameTimer.Dispose();
            }
        }

        private void InitializeWindowSettings()
        {
            this.DoubleBuffered = true;
            this.Size = new Size(WINDOW_WIDTH, WINDOW_HEIGHT);
            this.StartPosition = FormStartPosition.CenterScreen;
            dbConnection = new DatabaseConnection();
            currentMatchId = 0;
        }

        private async Task<(PlayerNamesForm? playerNames, GameConfigForm? gameConfig)> GetUserConfiguration()
        {
            using var playerNamesForm = new PlayerNamesForm();
            if (playerNamesForm.ShowDialog() != DialogResult.OK)
            {
                Application.Exit();
                return (null, null);
            }

            using var gameConfigForm = new GameConfigForm();
            if (gameConfigForm.ShowDialog() != DialogResult.OK)
            {
                Application.Exit();
                return (null, null);
            }

            return (playerNamesForm, gameConfigForm);
        }

        private void InitializeUI()
        {
            InitializeMenuStrip();
            InitializeButtons();
        }

        private Rectangle CreateGameField()
        {
            int fieldWidth = WINDOW_WIDTH - (FIELD_MARGIN * 2);
            int fieldHeight = WINDOW_HEIGHT - (FIELD_MARGIN * 2) - menuStrip!.Height;
            return new Rectangle(FIELD_MARGIN, FIELD_MARGIN, fieldWidth, fieldHeight);
        }

        private void InitializeGameComponents(PlayerNamesForm playerNames, GameConfigForm gameConfig)
        {
            gameField = CreateGameField();
            
            // Score et balle
            gameScore = new Score(playerNames.LeftPlayerName, playerNames.RightPlayerName);
            ball = new Ball(gameField.Width / 2 + gameField.Left, gameField.Height / 2 + gameField.Top);

            // Configuration des vitesses et points
            float onePointSpeed = gameConfig.OnePointZoneSpeed;
            float twoPointSpeed = onePointSpeed * gameConfig.TwoPointZonePoints;
            int twoPointZonePoints = gameConfig.TwoPointZonePoints;

            // Création des raquettes
            (leftPaddleFront, leftPaddleBack) = CreatePaddlePair(
                gameField.Left + 50,
                gameField.Left + 150,
                false,
                onePointSpeed,
                twoPointSpeed,
                twoPointZonePoints
            );

            (rightPaddleFront, rightPaddleBack) = CreatePaddlePair(
                gameField.Right - 50,
                gameField.Right - 150,
                true,
                onePointSpeed,
                twoPointSpeed,
                twoPointZonePoints
            );

            InitializeTrajectoryMenus(playerNames);
        }

        private (Paddle front, Paddle back) CreatePaddlePair(
            int frontX,
            int backX,
            bool isRight,
            float onePointSpeed,
            float twoPointSpeed,
            int twoPointZonePoints)
        {
            var front = new Paddle(frontX, gameField.Height / 2 + gameField.Top, gameField, isRight, true)
            {
                Speed = twoPointSpeed,
                Points = twoPointZonePoints
            };

            var back = new Paddle(backX, gameField.Height / 2 + gameField.Top, gameField, isRight, false)
            {
                Speed = onePointSpeed,
                Points = 1
            };

            return (front, back);
        }

        private async Task InitializeDatabase(PlayerNamesForm playerNames)
        {
            try
            {
                currentMatchId = await dbConnection!.SaveMatch(
                    playerNames.LeftPlayerName,
                    playerNames.RightPlayerName,
                    "En cours");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Erreur lors de l'initialisation du match : {ex.Message}",
                    "Erreur",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void StartGameLoop()
        {
            gameTimer = new System.Windows.Forms.Timer
            {
                Interval = 16 // ~60 FPS
            };
            gameTimer.Tick += GameLoop;
            gameTimer.Start();
        }

        private void InitializeButtons()
        {
            // Bouton Pause
            pauseButton = new Button
            {
                Text = "Pause",
                Size = new Size(100, 30),
                Location = new Point(WINDOW_WIDTH / 2 - 110, 20),
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            pauseButton.Click += PauseButton_Click;

            // Bouton Nouvelle Partie
            newGameButton = new Button
            {
                Text = "Nouvelle Partie",
                Size = new Size(100, 30),
                Location = new Point(WINDOW_WIDTH / 2 + 10, 20),
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Visible = false
            };

            var historyButton = new Button
            {
                Text = "Historique",
                Size = new Size(100, 30),
                Location = new Point(WINDOW_WIDTH / 2 - 230, 20),
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            historyButton.Click += HistoryButton_Click;

            // Nouveaux boutons de ciblage
            targetLeftFrontButton = new Button
            {
                Text = "Viser Avant Gauche",
                Size = new Size(120, 30),
                Location = new Point(WINDOW_WIDTH / 2 - 350, 20),
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            targetLeftFrontButton.Click += (s, e) =>
            {
                if (!isPaused && ball != null && leftPaddleFront != null)
                {
                    ball.TargetPaddle = leftPaddleFront;
                    ball.ResetBall();
                }
            };

            targetLeftBackButton = new Button
            {
                Text = "Viser Arrière Gauche",
                Size = new Size(120, 30),
                Location = new Point(WINDOW_WIDTH / 2 - 480, 20),
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            targetLeftBackButton.Click += (s, e) =>
            {
                if (!isPaused && ball != null && leftPaddleBack != null)
                {
                    ball.TargetPaddle = leftPaddleBack;
                    ball.ResetBall();
                }
            };

            targetRightFrontButton = new Button
            {
                Text = "Viser Avant Droite",
                Size = new Size(120, 30),
                Location = new Point(WINDOW_WIDTH / 2 + 230, 20),
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            targetRightFrontButton.Click += (s, e) =>
            {
                if (!isPaused && ball != null && rightPaddleFront != null)
                {
                    ball.TargetPaddle = rightPaddleFront;
                    ball.ResetBall();
                }
            };

            targetRightBackButton = new Button
            {
                Text = "Viser Arrière Droite",
                Size = new Size(120, 30),
                Location = new Point(WINDOW_WIDTH / 2 + 360, 20),
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            targetRightBackButton.Click += (s, e) =>
            {
                if (!isPaused && ball != null && rightPaddleBack != null)
                {
                    ball.TargetPaddle = rightPaddleBack;
                    ball.ResetBall();
                }
            };

            this.Controls.Add(historyButton);

            newGameButton.Click += NewGameButton_Click;

            this.Controls.Add(pauseButton);
            this.Controls.Add(newGameButton);
            this.Controls.Add(targetLeftFrontButton);
            this.Controls.Add(targetLeftBackButton);
            this.Controls.Add(targetRightFrontButton);
            this.Controls.Add(targetRightBackButton);
        }

        private void PauseButton_Click(object? sender, EventArgs e)
        {
            isPaused = !isPaused;
            if (isPaused)
            {
                gameTimer?.Stop();
                pauseButton!.Text = "Reprendre";
            }
            else
            {
                gameTimer?.Start();
                pauseButton!.Text = "Pause";
            }
        }

        private void HistoryButton_Click(object? sender, EventArgs e)
        {
            var historyForm = new HistoryForm(dbConnection!);
            historyForm.Show();
        }

        private void NewGameButton_Click(object? sender, EventArgs e)
    {
        if (gameTimer != null)
        {
            gameTimer.Stop();
            gameTimer.Tick -= GameLoop;
            gameTimer.Dispose();
            gameTimer = null;
        }

        // Fermer les fenêtres de trajectoire si elles sont ouvertes
        leftFrontTrajectoryForm?.Close();
        leftBackTrajectoryForm?.Close();
        rightFrontTrajectoryForm?.Close();
        rightBackTrajectoryForm?.Close();

        if (gameScore != null)
        {
            gameScore.ResetMatch(); // Réinitialiser complètement le match
        }

        this.Controls.Clear();
        InitializeGame();
    }

        private void InitializeMenuStrip()
        {
            menuStrip = new MenuStrip
            {
                BackColor = Color.DarkGray,
                ForeColor = Color.White,
                Font = new Font("Arial", 12)
            };
            this.Controls.Add(menuStrip);
        }

        

        private void InitializeTrajectoryMenus(PlayerNamesForm playerNamesForm)
        {
            ToolStripMenuItem optionsMenu = new ToolStripMenuItem("Options");

            // Raquettes gauches
            ToolStripMenuItem leftFrontTrajectoryMenuItem = new ToolStripMenuItem($"Trajectoire Avant {playerNamesForm.LeftPlayerName}", null, (s, e) =>
            {
                if (leftFrontTrajectoryForm == null || leftFrontTrajectoryForm.IsDisposed)
                {
                    leftFrontTrajectoryForm = new TrajectoryForm(leftPaddleFront!, playerNamesForm.LeftPlayerName, "Raquette Avant");
                    leftFrontTrajectoryForm.Location = new Point(0, menuStrip!.Height);
                }
                leftFrontTrajectoryForm.Show();
                leftFrontTrajectoryForm.Focus();
            });

            ToolStripMenuItem leftBackTrajectoryMenuItem = new ToolStripMenuItem($"Trajectoire Arrière {playerNamesForm.LeftPlayerName}", null, (s, e) =>
            {
                if (leftBackTrajectoryForm == null || leftBackTrajectoryForm.IsDisposed)
                {
                    leftBackTrajectoryForm = new TrajectoryForm(leftPaddleBack!, playerNamesForm.LeftPlayerName, "Raquette Arrière");
                    leftBackTrajectoryForm.Location = new Point(0, menuStrip!.Height + 320);
                }
                leftBackTrajectoryForm.Show();
                leftBackTrajectoryForm.Focus();
            });

            // Raquettes droites
            ToolStripMenuItem rightFrontTrajectoryMenuItem = new ToolStripMenuItem($"Trajectoire Avant {playerNamesForm.RightPlayerName}", null, (s, e) =>
            {
                if (rightFrontTrajectoryForm == null || rightFrontTrajectoryForm.IsDisposed)
                {
                    rightFrontTrajectoryForm = new TrajectoryForm(rightPaddleFront!, playerNamesForm.RightPlayerName, "Raquette Avant");
                    rightFrontTrajectoryForm.Location = new Point(this.Width - 400, menuStrip!.Height);
                }
                rightFrontTrajectoryForm.Show();
                rightFrontTrajectoryForm.Focus();
            });

            ToolStripMenuItem rightBackTrajectoryMenuItem = new ToolStripMenuItem($"Trajectoire Arrière {playerNamesForm.RightPlayerName}", null, (s, e) =>
            {
                if (rightBackTrajectoryForm == null || rightBackTrajectoryForm.IsDisposed)
                {
                    rightBackTrajectoryForm = new TrajectoryForm(rightPaddleBack!, playerNamesForm.RightPlayerName, "Raquette Arrière");
                    rightBackTrajectoryForm.Location = new Point(this.Width - 400, menuStrip!.Height + 320);
                }
                rightBackTrajectoryForm.Show();
                rightBackTrajectoryForm.Focus();
            });

            optionsMenu.DropDownItems.AddRange(new ToolStripItem[] {
                leftFrontTrajectoryMenuItem,
                leftBackTrajectoryMenuItem,
                rightFrontTrajectoryMenuItem,
                rightBackTrajectoryMenuItem
            });
            menuStrip!.Items.Add(optionsMenu);
        }

        private async void GameLoop(object? sender, EventArgs e)
    {
        if (!gameScore!.IsMatchOver && !isPaused)
        {
            // Mise à jour des positions
            ball!.Move(gameField);
            leftPaddleFront!.Move();
            leftPaddleBack!.Move();
            rightPaddleFront!.Move();
            rightPaddleBack!.Move();

            // Vérification des collisions avec les raquettes
            if (CheckPaddleCollision(leftPaddleFront))
            {
                gameScore.AddPointRight(leftPaddleFront.Points);
                if (currentMatchId > 0)
                {
                    await SaveCurrentScore();
                }
                ResetBall();
            }
            else if (CheckPaddleCollision(leftPaddleBack))
            {
                gameScore.AddPointRight(leftPaddleBack.Points);
                if (currentMatchId > 0)
                {
                    await SaveCurrentScore();
                }
                ResetBall();
            }
            else if (CheckPaddleCollision(rightPaddleFront))
            {
                gameScore.AddPointLeft(rightPaddleFront.Points);
                if (currentMatchId > 0)
                {
                    await SaveCurrentScore();
                }
                ResetBall();
            }
            else if (CheckPaddleCollision(rightPaddleBack))
            {
                gameScore.AddPointLeft(rightPaddleBack.Points);
                if (currentMatchId > 0)
                {
                    await SaveCurrentScore();
                }
                ResetBall();
            }

            // Gestion de la fin de match
            if (gameScore.IsMatchOver)
            {
                try
                {
                    // Mettre à jour le gagnant
                    await dbConnection!.UpdateMatchWinner(
                        currentMatchId,
                        gameScore.Winner);

                    // Sauvegarder le score final
                    await SaveCurrentScore();

                    pauseButton!.Visible = false;
                    newGameButton!.Visible = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Erreur lors de la sauvegarde du match : {ex.Message}",
                        "Erreur",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }
            }
        }

        this.Invalidate();
    }

        // Nouvelle méthode pour sauvegarder le score courant
        private async Task SaveCurrentScore()
    {
        try
        {
            await dbConnection!.SavePoint(
                currentMatchId,
                $"{gameScore!.GetLeftScore()} ({gameScore.GetSetsScore().Split('-')[0]})",
                $"{gameScore.GetRightScore()} ({gameScore.GetSetsScore().Split('-')[1]})");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors de la sauvegarde du point : {ex.Message}");
        }
    }

        private void ResetBall()
        {
            ball!.X = gameField.Left + (gameField.Width / 2);
            ball.Y = gameField.Top + (gameField.Height / 2);
            ball.ResetBall();
        }

        private bool CheckPaddleCollision(Paddle paddle)
        {
            float dx = ball!.X - paddle.X;
            float dy = ball.Y - paddle.Y;
            float distance = (float)Math.Sqrt(dx * dx + dy * dy);
            return distance < (ball.Radius + paddle.Radius);
        }

        protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        Graphics g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        // Fond noir pour toute la fenêtre
        g.Clear(Color.Black);

        // Zone de score
        using (Font scoreFont = new Font("Arial", 24, FontStyle.Bold))
        using (Font nameFont = new Font("Arial", 20))
        {
            // Noms des joueurs
            g.DrawString(gameScore!.LeftPlayerName, nameFont, Brushes.White,
                FIELD_MARGIN, menuStrip!.Height + 20);
            g.DrawString(gameScore.RightPlayerName, nameFont, Brushes.White,
                this.Width - FIELD_MARGIN - g.MeasureString(gameScore.RightPlayerName, nameFont).Width,
                menuStrip.Height + 20);

            // Scores du jeu en cours
            string leftScore = gameScore.GetLeftScore();
            string rightScore = gameScore.GetRightScore();

            g.DrawString(leftScore, scoreFont, Brushes.White,
                FIELD_MARGIN, menuStrip.Height + 50);
            g.DrawString(rightScore, scoreFont, Brushes.White,
                this.Width - FIELD_MARGIN - g.MeasureString(rightScore, scoreFont).Width,
                menuStrip.Height + 50);

            // Score des sets
            string setsScore = gameScore.GetSetsScore();
            g.DrawString($"Sets: {setsScore}", scoreFont, Brushes.White,
                this.Width / 2 - g.MeasureString($"Sets: {setsScore}", scoreFont).Width / 2,
                menuStrip.Height + 50);
        }

        // Dessiner le terrain de tennis
        using (SolidBrush fieldBrush = new SolidBrush(Color.ForestGreen))
        using (Pen fieldPen = new Pen(Color.White, 3))
        using (Pen linePen = new Pen(Color.White, 2))
        {
            // Terrain principal
            g.FillRectangle(fieldBrush, gameField);
            g.DrawRectangle(fieldPen, gameField);

            // Ligne centrale verticale
            g.DrawLine(fieldPen,
                gameField.Left + (gameField.Width / 2), gameField.Top,
                gameField.Left + (gameField.Width / 2), gameField.Bottom);

            // Lignes de service
            int serviceLineOffset = gameField.Width / 6;

            // Lignes horizontales
            g.DrawLine(linePen,
                gameField.Left, gameField.Top + (gameField.Height / 4),
                gameField.Right, gameField.Top + (gameField.Height / 4));
            g.DrawLine(linePen,
                gameField.Left, gameField.Bottom - (gameField.Height / 4),
                gameField.Right, gameField.Bottom - (gameField.Height / 4));

            // Lignes verticales des carrés de service
            g.DrawLine(linePen,
                gameField.Left + serviceLineOffset, gameField.Top + (gameField.Height / 4),
                gameField.Left + serviceLineOffset, gameField.Bottom - (gameField.Height / 4));
            g.DrawLine(linePen,
                gameField.Right - serviceLineOffset, gameField.Top + (gameField.Height / 4),
                gameField.Right - serviceLineOffset, gameField.Bottom - (gameField.Height / 4));
        }

        // Message de fin de match si nécessaire
        if (gameScore!.IsMatchOver)
        {
            using (Font winFont = new Font("Arial", 36, FontStyle.Bold))
            {
                string message = $"{gameScore.Winner} a gagné le match!";
                SizeF size = g.MeasureString(message, winFont);

                using (SolidBrush overlay = new SolidBrush(Color.FromArgb(180, 0, 0, 0)))
                {
                    g.FillRectangle(overlay,
                        gameField.Left + (gameField.Width - size.Width) / 2 - 20,
                        gameField.Top + (gameField.Height - size.Height) / 2 - 20,
                        size.Width + 40,
                        size.Height + 40);
                }

                g.DrawString(message, winFont, Brushes.Gold,
                    gameField.Left + (gameField.Width - size.Width) / 2,
                    gameField.Top + (gameField.Height - size.Height) / 2);
            }
        }

        // Dessiner les objets
        ball!.Draw(g);
        leftPaddleFront!.Draw(g);
        leftPaddleBack!.Draw(g);
        rightPaddleFront!.Draw(g);
        rightPaddleBack!.Draw(g);
    }
    }
}