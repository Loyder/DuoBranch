using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace WpfApp3
{
    public partial class DoomWindow : Window
    {
        public event EventHandler<int> IQRewarded;

        private const int PlayerSize = 48;
        private const int EnemySize = 48;
        private const int BulletSize = 16;
        private const int EnemyBulletSize = 14;
        private const double PlayerSpeed = 9.0;
        private const double BulletSpeed = 13.0;

        private DispatcherTimer gameTimer;
        private Image player;
        private Ellipse bullet;
        private List<Ellipse> enemyBullets = new List<Ellipse>();
        private bool isBulletActive = false;

        private double canvasWidth => GameCanvas.ActualWidth > 0 ? GameCanvas.ActualWidth : 800;
        private double canvasHeight => GameCanvas.ActualHeight > 0 ? GameCanvas.ActualHeight : 480;

        private int wave = 1;
        private int lives = 3;
        private Random rnd = new Random();
        private double enemyShootTimer = 0;

        // Мультивраги
        private List<Image> enemies = new List<Image>();
        private List<int> enemySpeeds = new List<int>();
        private List<bool> enemyKilled = new List<bool>();
        private List<double> enemyDeathAnim = new List<double>();

        // Для определения окончания игры
        private bool gameOver = false;

        public DoomWindow()
        {
            InitializeComponent();
            this.Closing += DoomWindow_Closing;
        }

        private void DoomWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!gameOver)
            {
                gameTimer?.Stop();
                MessageBox.Show("Вы проиграли! IQ не начислен.", "Поражение", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeGame();
        }

        private void InitializeGame()
        {
            GameCanvas.Children.Clear();
            GameCanvas.Children.Add(WaveText);
            GameCanvas.Children.Add(LivesText);

            player = CreateImage("Resources/ImagesDoom/brutal_doom_ico.png", PlayerSize, PlayerSize);
            player.Effect = new DropShadowEffect
            {
                Color = Colors.GreenYellow,
                BlurRadius = 20,
                ShadowDepth = 0,
                Opacity = 0.6
            };
            Canvas.SetLeft(player, canvasWidth / 2 - PlayerSize / 2);
            Canvas.SetTop(player, canvasHeight - PlayerSize * 1.8);
            GameCanvas.Children.Add(player);

            SpawnEnemies();

            isBulletActive = false;
            bullet = null;
            enemyBullets.Clear();

            UpdateUI();

            gameTimer = new DispatcherTimer();
            gameTimer.Interval = TimeSpan.FromMilliseconds(16);
            gameTimer.Tick += GameLoop;
            gameTimer.Start();

            this.KeyDown -= OnKeyDown;
            this.KeyDown += OnKeyDown;
            this.Focusable = true;
            this.Focus();
        }

        private void SpawnEnemies()
        {
            foreach (var enemy in enemies)
                GameCanvas.Children.Remove(enemy);

            enemies.Clear();
            enemySpeeds.Clear();
            enemyKilled.Clear();
            enemyDeathAnim.Clear();

            int enemyCount = wave < 5 ? 1 : wave - 3; // 5 волна — 2 врага, 6 — 3 и т.д.
            double spacing = canvasWidth / (enemyCount + 1);

            for (int i = 0; i < enemyCount; i++)
            {
                var enemy = CreateImage("Resources/ImagesDoom/icon_128x128.png", EnemySize, EnemySize);
                enemy.Effect = new DropShadowEffect
                {
                    Color = Colors.Red,
                    BlurRadius = 35,
                    ShadowDepth = 0,
                    Opacity = 0.7
                };

                Canvas.SetLeft(enemy, spacing * (i + 1) - EnemySize / 2);
                Canvas.SetTop(enemy, EnemySize * 0.8);
                GameCanvas.Children.Add(enemy);

                enemies.Add(enemy);
                enemySpeeds.Add((rnd.Next(0, 2) == 0 ? 1 : -1) * (6 + wave * 2));
                enemyKilled.Add(false);
                enemyDeathAnim.Add(1.0);
            }
        }

        private void GameLoop(object sender, EventArgs e)
        {
            bool allEnemiesKilled = true;
            for (int i = 0; i < enemies.Count; i++)
            {
                if (!enemyKilled[i])
                {
                    MoveEnemy(i);
                    allEnemiesKilled = false;
                }
                else
                {
                    AnimateEnemyDeath(i);
                }
            }

            if (allEnemiesKilled)
            {
                wave++;
                UpdateUI();
                if (wave > 10)
                {
                    gameOver = true;
                    IQRewarded?.Invoke(this, 10 * (wave - 1));
                    MessageBox.Show($"Победа! Все {wave - 1} волн пройдены. Вам начислено {(wave - 1) * 10} очков IQ.", "Победа", MessageBoxButton.OK, MessageBoxImage.Information);
                    gameTimer.Stop();
                    this.Close();
                    return;
                }
                SpawnEnemies();
                foreach (var eb in enemyBullets) GameCanvas.Children.Remove(eb);
                enemyBullets.Clear();
                return;
            }

            if (isBulletActive)
            {
                MoveBullet();
                CheckCollision();
            }

            MoveEnemyBullets();
            TryEnemyShoot();
            CheckPlayerHit();
        }

        private void MoveEnemy(int i)
        {
            var enemy = enemies[i];
            double currentLeft = Canvas.GetLeft(enemy);
            int speed = enemySpeeds[i];
            double proposedLeft = currentLeft + speed;
            if (proposedLeft < 0 || proposedLeft + EnemySize > canvasWidth)
            {
                speed *= -1;
                enemySpeeds[i] = speed;
            }
            Canvas.SetLeft(enemy, currentLeft + speed);

            Canvas.SetTop(enemy, EnemySize * 0.8 + Math.Sin((DateTime.Now.Ticks + i * 100000) / 2.1e7) * 15);
        }

        private void AnimateEnemyDeath(int i)
        {
            var enemy = enemies[i];
            enemyDeathAnim[i] -= 0.06;
            enemy.Opacity = Math.Max(0, enemyDeathAnim[i]);
            enemy.RenderTransform = new ScaleTransform(1 + (1 - enemyDeathAnim[i]), 1 + (1 - enemyDeathAnim[i]),
                EnemySize / 2, EnemySize / 2);
            if (enemyDeathAnim[i] <= 0)
            {
                GameCanvas.Children.Remove(enemy);
            }
        }

        private void MoveBullet()
        {
            if (bullet == null) return;
            double currentTop = Canvas.GetTop(bullet);
            if (currentTop <= 0)
            {
                DeactivateBullet();
                return;
            }
            Canvas.SetTop(bullet, currentTop - BulletSpeed);
        }

        private void CheckCollision()
        {
            if (bullet == null) return;

            for (int i = 0; i < enemies.Count; i++)
            {
                if (enemyKilled[i]) continue;
                var enemy = enemies[i];
                Rect bulletRect = new Rect(Canvas.GetLeft(bullet), Canvas.GetTop(bullet), BulletSize, BulletSize);
                Rect enemyRect = new Rect(Canvas.GetLeft(enemy), Canvas.GetTop(enemy), EnemySize, EnemySize);

                if (bulletRect.IntersectsWith(enemyRect))
                {
                    isBulletActive = false;
                    GameCanvas.Children.Remove(bullet);
                    bullet = null;
                    enemyKilled[i] = true;
                    enemyDeathAnim[i] = 1;
                    break;
                }
            }
        }

        private void TryEnemyShoot()
        {
            if (enemies.Count == 0) return;

            enemyShootTimer -= 0.016;
            if (enemyShootTimer <= 0)
            {
                // Выбираем случайного живого врага
                List<int> aliveIndices = new List<int>();
                for (int i = 0; i < enemies.Count; i++)
                    if (!enemyKilled[i]) aliveIndices.Add(i);

                if (aliveIndices.Count > 0)
                {
                    int shooter = aliveIndices[rnd.Next(aliveIndices.Count)];
                    EnemyShoot(shooter);
                }
                enemyShootTimer = 1.4 - wave * 0.09;
                if (enemyShootTimer < 0.5) enemyShootTimer = 0.5;
            }
        }

        private void EnemyShoot(int enemyIndex)
        {
            var enemy = enemies[enemyIndex];
            var eb = new Ellipse
            {
                Width = EnemyBulletSize,
                Height = EnemyBulletSize,
                Fill = new RadialGradientBrush(Colors.Red, Colors.DarkRed),
                Effect = new DropShadowEffect
                {
                    Color = Colors.Red,
                    BlurRadius = 10,
                    ShadowDepth = 0,
                    Opacity = 0.7
                }
            };
            double ebLeft = Canvas.GetLeft(enemy) + EnemySize / 2 - EnemyBulletSize / 2;
            double ebTop = Canvas.GetTop(enemy) + EnemySize;
            Canvas.SetLeft(eb, ebLeft);
            Canvas.SetTop(eb, ebTop);
            GameCanvas.Children.Add(eb);
            enemyBullets.Add(eb);
        }

        private void MoveEnemyBullets()
        {
            for (int i = enemyBullets.Count - 1; i >= 0; i--)
            {
                var eb = enemyBullets[i];
                double y = Canvas.GetTop(eb);
                Canvas.SetTop(eb, y + BulletSpeed * 0.84 + wave * 0.6);
                if (y > canvasHeight)
                {
                    GameCanvas.Children.Remove(eb);
                    enemyBullets.RemoveAt(i);
                }
            }
        }

        private void CheckPlayerHit()
        {
            if (player == null) return;

            Rect playerRect = new Rect(Canvas.GetLeft(player), Canvas.GetTop(player), PlayerSize, PlayerSize);

            for (int i = enemyBullets.Count - 1; i >= 0; i--)
            {
                var eb = enemyBullets[i];
                Rect ebRect = new Rect(Canvas.GetLeft(eb), Canvas.GetTop(eb), EnemyBulletSize, EnemyBulletSize);
                if (playerRect.IntersectsWith(ebRect))
                {
                    GameCanvas.Children.Remove(eb);
                    enemyBullets.RemoveAt(i);
                    OnPlayerHit();
                    break;
                }
            }
        }

        private void OnPlayerHit()
        {
            lives--;
            UpdateUI();
            player.Opacity = 0.4;
            var dt = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(120) };
            dt.Tick += (s, e) =>
            {
                player.Opacity = 1;
                ((DispatcherTimer)s).Stop();
            };
            dt.Start();

            if (lives <= 0)
            {
                gameOver = true;
                gameTimer.Stop();
                MessageBox.Show("Вы проиграли! IQ не начислен.", "Поражение", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (player == null) return;
            double currentLeft = Canvas.GetLeft(player);
            switch (e.Key)
            {
                case Key.Left:
                    if (currentLeft - PlayerSpeed >= 0)
                        Canvas.SetLeft(player, currentLeft - PlayerSpeed);
                    break;
                case Key.Right:
                    if (currentLeft + PlayerSize + PlayerSpeed <= canvasWidth)
                        Canvas.SetLeft(player, currentLeft + PlayerSpeed);
                    break;
                case Key.Space:
                    if (!isBulletActive)
                        FireBullet();
                    break;
            }
        }

        private void FireBullet()
        {
            bullet = new Ellipse
            {
                Width = BulletSize,
                Height = BulletSize,
                Fill = new RadialGradientBrush(Colors.Yellow, Colors.OrangeRed),
                Effect = new DropShadowEffect
                {
                    Color = Colors.Yellow,
                    BlurRadius = 15,
                    ShadowDepth = 0,
                    Opacity = 0.7
                }
            };
            double bulletLeft = Canvas.GetLeft(player) + PlayerSize / 2 - BulletSize / 2;
            double bulletTop = Canvas.GetTop(player) - BulletSize;
            Canvas.SetLeft(bullet, bulletLeft);
            Canvas.SetTop(bullet, bulletTop);
            GameCanvas.Children.Add(bullet);
            isBulletActive = true;
        }

        private void DeactivateBullet()
        {
            if (bullet != null)
            {
                GameCanvas.Children.Remove(bullet);
                bullet = null;
                isBulletActive = false;
            }
        }

        private Image CreateImage(string relativeUri, double width, double height)
        {
            var img = new Image
            {
                Width = width,
                Height = height,
                Source = new BitmapImage(new Uri($"pack://application:,,,/{relativeUri}", UriKind.Absolute)),
                RenderTransformOrigin = new Point(0.5, 0.5)
            };
            RenderOptions.SetBitmapScalingMode(img, BitmapScalingMode.HighQuality);
            return img;
        }

        private void UpdateUI()
        {
            WaveText.Text = $"Волна: {wave}";
            LivesText.Text = $"Жизни: {lives}";
        }
    }
}