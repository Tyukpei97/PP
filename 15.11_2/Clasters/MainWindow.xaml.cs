using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Clasters
{
    public partial class MainWindow : Window
    {
        private readonly List<ClusterNode> nodes = new List<ClusterNode>();
        private readonly List<NetworkConnection> connections = new List<NetworkConnection>();
        private readonly Random random = new Random();
        private DispatcherTimer simulationTimer;
        private DispatcherTimer uptimeTimer;
        private TimeSpan uptime;
        private bool isSimulationRunning = false;
        private Point dragStartPoint;
        private ClusterNode draggedNode;
        private bool isDragging = false;

        public MainWindow()
        {
            InitializeComponent();
            InitializeTimers();
            Loaded += (s, e) => CreateBackgroundParticles();
        }

        private void InitializeTimers()
        {
            // Таймер симуляции
            simulationTimer = new DispatcherTimer();
            simulationTimer.Interval = TimeSpan.FromMilliseconds(100);
            simulationTimer.Tick += SimulationTimer_Tick;

            // Таймер времени работы
            uptimeTimer = new DispatcherTimer();
            uptimeTimer.Interval = TimeSpan.FromSeconds(1);
            uptimeTimer.Tick += (s, e) =>
            {
                uptime = uptime.Add(TimeSpan.FromSeconds(1));
                UptimeText.Text = uptime.ToString(@"mm\:ss");
            };
        }

        private void CreateBackgroundParticles()
        {
            // Создаем анимированные частицы для фона
            for (int i = 0; i < 15; i++)
            {
                var ellipse = new Ellipse
                {
                    Width = random.Next(5, 20),
                    Height = random.Next(5, 20),
                    Fill = new RadialGradientBrush
                    {
                        GradientStops =
                        {
                            new GradientStop(Color.FromArgb(255, 138, 43, 226), 0),
                            new GradientStop(Colors.Transparent, 1)
                        }
                    },
                    Opacity = random.NextDouble() * 0.3 + 0.1
                };

                Canvas.SetLeft(ellipse, random.Next(0, (int)MainCanvas.ActualWidth));
                Canvas.SetTop(ellipse, random.Next(0, (int)MainCanvas.ActualHeight));

                // Анимация движения
                var animX = new DoubleAnimation
                {
                    From = Canvas.GetLeft(ellipse),
                    To = random.Next(0, (int)MainCanvas.ActualWidth),
                    Duration = TimeSpan.FromSeconds(random.Next(10, 30)),
                    AutoReverse = true,
                    RepeatBehavior = RepeatBehavior.Forever
                };

                var animY = new DoubleAnimation
                {
                    From = Canvas.GetTop(ellipse),
                    To = random.Next(0, (int)MainCanvas.ActualHeight),
                    Duration = TimeSpan.FromSeconds(random.Next(15, 40)),
                    AutoReverse = true,
                    RepeatBehavior = RepeatBehavior.Forever
                };

                ellipse.BeginAnimation(Canvas.LeftProperty, animX);
                ellipse.BeginAnimation(Canvas.TopProperty, animY);

                BackgroundCanvas.Children.Add(ellipse);
            }
        }

        private void SimulationTimer_Tick(object sender, EventArgs e)
        {
            if (!isSimulationRunning) return;

            UpdateSimulation();
            UpdateStatistics();
            UpdateVisualization();
        }

        private void UpdateSimulation()
        {
            // Упрощенная логика для красивого визуала
            foreach (var node in nodes)
            {
                node.UpdateVisualState();
            }

            foreach (var connection in connections)
            {
                connection.UpdateVisualState();
            }

            // Случайные события для визуального интереса
            if (random.NextDouble() < 0.03 && nodes.Count > 0)
            {
                var node = nodes[random.Next(nodes.Count)];
            }
        }

        private void UpdateStatistics()
        {
            int activeNodes = 0;
            int failedNodes = 0;
            double totalPerformance = 0;
            int tasksCompleted = 0;

            foreach (var node in nodes)
            {
                if (node.IsActive)
                {
                    activeNodes++;
                    totalPerformance += node.Performance;
                    tasksCompleted += node.TasksCompleted;
                }
                else
                {
                    failedNodes++;
                }
            }

            ActiveNodesText.Text = activeNodes.ToString();
            FailedNodesText.Text = failedNodes.ToString();
            TotalPerformanceText.Text = $"{totalPerformance:F0}";
            TasksCompletedText.Text = tasksCompleted.ToString();

            // Сетевой трафик для визуализации
            double networkLoad = random.NextDouble() * 100;
            NetworkLoadText.Text = $"{networkLoad:F0}%";
        }

        private void UpdateVisualization()
        {
            // Визуальные обновления уже обрабатываются в отдельных классах
        }

        #region Обработчики UI событий

        private void AddNode_Click(object sender, RoutedEventArgs e)
        {
            var node = new ClusterNode(random)
            {
                Position = new Point(
                    random.Next(100, (int)MainCanvas.ActualWidth - 100),
                    random.Next(100, (int)MainCanvas.ActualHeight - 100)
                )
            };

            nodes.Add(node);
            MainCanvas.Children.Add(node.Visual);

            // Создаем красивые соединения
            if (nodes.Count > 1)
            {
                foreach (var existingNode in nodes)
                {
                    if (existingNode != node && random.NextDouble() < 0.4) // 40% шанс соединения
                    {
                        var connection = new NetworkConnection(node, existingNode, random);
                        connections.Add(connection);
                        MainCanvas.Children.Insert(0, connection.Visual);
                    }
                }
            }

            node.Pulse(); // Анимация при создании (пульсация с красным миганием)
            node.StartShiftTimer(); // Запускаем автоматический сдвиг через 0.2с
        }

        private void RemoveNode_Click(object sender, RoutedEventArgs e)
        {
            if (nodes.Count > 0)
            {
                var node = nodes[^1];
                node.FadeOut(); // Анимация исчезновения

                // Удаляем после анимации
                var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
                timer.Tick += (s, e) =>
                {
                    nodes.Remove(node);
                    MainCanvas.Children.Remove(node.Visual);

                    // Удаляем связанные соединения
                    connections.RemoveAll(c => c.SourceNode == node || c.TargetNode == node);
                    foreach (var connection in connections.ToArray())
                    {
                        if (connection.SourceNode == node || connection.TargetNode == node)
                        {
                            MainCanvas.Children.Remove(connection.Visual);
                            connections.Remove(connection);
                        }
                    }

                    timer.Stop();
                };
                timer.Start();
            }
        }

        private void StartSimulation_Click(object sender, RoutedEventArgs e)
        {
            if (!isSimulationRunning)
            {
                simulationTimer.Start();
                uptimeTimer.Start();
                isSimulationRunning = true;

                // Визуальная обратная связь
                foreach (var node in nodes)
                {
                    node.Activate();
                }
            }
        }

        private void StopSimulation_Click(object sender, RoutedEventArgs e)
        {
            if (isSimulationRunning)
            {
                simulationTimer.Stop();
                uptimeTimer.Stop();
                isSimulationRunning = false;

                foreach (var node in nodes)
                {
                    node.Deactivate();
                }
            }
        }

        private void ClearAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var node in nodes)
            {
                node.FadeOut();
            }

            var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(600) };
            timer.Tick += (s, e) =>
            {
                nodes.Clear();
                connections.Clear();
                MainCanvas.Children.Clear();
                UpdateStatistics();
                timer.Stop();
            };
            timer.Start();
        }

        private void SimulationSpeed_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (simulationTimer != null)
            {
                simulationTimer.Interval = TimeSpan.FromMilliseconds(110 - (e.NewValue * 10));
            }
        }

        #endregion

        #region Обработчики перетаскивания и управления окном

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
            }
            else
            {
                DragMove();
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var position = e.GetPosition(MainCanvas);
            var element = MainCanvas.InputHitTest(position) as FrameworkElement;

            if (element?.DataContext is ClusterNode node)
            {
                draggedNode = node;
                dragStartPoint = position;
                isDragging = true;
                draggedNode.Visual.CaptureMouse();
                e.Handled = true;
            }
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging && draggedNode != null)
            {
                var currentPosition = e.GetPosition(MainCanvas);
                draggedNode.Position = currentPosition;
                draggedNode.UpdateVisual();

                foreach (var connection in connections)
                {
                    if (connection.SourceNode == draggedNode || connection.TargetNode == draggedNode)
                    {
                        connection.UpdateVisual();
                    }
                }
            }
        }

        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isDragging)
            {
                draggedNode?.Visual.ReleaseMouseCapture();
                draggedNode = null;
                isDragging = false;
            }
        }

        #endregion
    }

    // Класс узла кластера с красивой визуализацией
    // Класс узла кластера с красивой визуализацией
    public class ClusterNode
    {
        public Ellipse Visual { get; private set; }
        public Point Position { get; set; }
        public bool IsActive { get; set; } = true;
        public double Performance { get; private set; }
        public int TasksCompleted { get; set; }
        private readonly Random random;
        private DispatcherTimer shiftTimer;

        public ClusterNode(Random random)
        {
            this.random = random;
            Performance = random.Next(500, 2000);
            CreateVisual();
            InitializeShiftTimer();
        }

        private void InitializeShiftTimer()
        {
            shiftTimer = new DispatcherTimer();
            shiftTimer.Interval = TimeSpan.FromMilliseconds(200); // 0.2 секунды
            shiftTimer.Tick += (s, e) =>
            {
                ShiftRight();
                shiftTimer.Stop();
            };
        }

        public void StartShiftTimer()
        {
            shiftTimer.Start();
        }

        private void ShiftRight()
        {
            // Сдвигаем на 2 единицы вправо
            Position = new Point(Position.X + 2, Position.Y);
            UpdateVisual();

            // Визуальная обратная связь - легкая пульсация при сдвиге
            var shiftAnim = new DoubleAnimation
            {
                To = 1.1,
                Duration = TimeSpan.FromMilliseconds(100),
                AutoReverse = true
            };
            Visual.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, shiftAnim);
            Visual.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, shiftAnim);
        }

        private void CreateVisual()
        {
            Visual = new Ellipse
            {
                Width = 60,
                Height = 60,
                Stroke = new LinearGradientBrush
                {
                    GradientStops =
                {
                    new GradientStop(Color.FromArgb(255, 138, 43, 226), 0),
                    new GradientStop(Color.FromArgb(255, 75, 0, 130), 1)
                }
                },
                StrokeThickness = 3,
                Fill = new RadialGradientBrush
                {
                    GradientStops =
                {
                    new GradientStop(Colors.White, 0),
                    new GradientStop(Color.FromArgb(255, 230, 230, 250), 0.7),
                    new GradientStop(Color.FromArgb(255, 138, 43, 226), 1)
                }
                },
                Effect = new DropShadowEffect
                {
                    Color = Color.FromArgb(255, 138, 43, 226),
                    BlurRadius = 15,
                    ShadowDepth = 5
                },
                RenderTransform = new ScaleTransform(1, 1),
                RenderTransformOrigin = new Point(0.5, 0.5),
                DataContext = this
            };

            // Анимированный ToolTip
            var toolTip = new Border
            {
                Background = new LinearGradientBrush
                {
                    GradientStops =
                {
                    new GradientStop(Color.FromArgb(204, 255, 255, 255), 0),
                    new GradientStop(Color.FromArgb(153, 230, 230, 250), 1)
                }
                },
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(10),
                Effect = new DropShadowEffect
                {
                    BlurRadius = 10,
                    Color = Color.FromArgb(255, 138, 43, 226)
                }
            };

            var toolTipContent = new StackPanel();
            toolTipContent.Children.Add(new TextBlock
            {
                Text = $"🚀 Node_{random.Next(1000, 9999)}",
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromArgb(255, 75, 0, 130))
            });
            toolTipContent.Children.Add(new TextBlock
            {
                Text = $"⚡ Perf: {Performance}",
                Foreground = new SolidColorBrush(Color.FromArgb(255, 138, 43, 226))
            });

            toolTip.Child = toolTipContent;
            Visual.ToolTip = toolTip;

            UpdateVisual();
        }

        public void UpdateVisual()
        {
            if (Visual == null) return;

            Canvas.SetLeft(Visual, Position.X - Visual.Width / 2);
            Canvas.SetTop(Visual, Position.Y - Visual.Height / 2);
        }

        public void UpdateVisualState()
        {
            // Случайные визуальные изменения для "живости"
            if (random.NextDouble() < 0.1)
            {
                var scale = random.NextDouble() * 0.2 + 0.9;
                var anim = new DoubleAnimation
                {
                    To = scale,
                    Duration = TimeSpan.FromMilliseconds(300)
                };
                Visual.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, anim);
                Visual.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, anim);
            }
        }

        public void Pulse()
        {
            // Пульсация с красным миганием
            var scaleAnim = new DoubleAnimation
            {
                To = 1.3,
                Duration = TimeSpan.FromMilliseconds(200),
                AutoReverse = true
            };

            // Анимация красного мигания
            var redFlashAnim = new ColorAnimation
            {
                To = Colors.Red,
                Duration = TimeSpan.FromMilliseconds(100),
                AutoReverse = true
            };

            // Применяем анимации
            Visual.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnim);
            Visual.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnim);

            // Анимируем обводку для красного мигания
            if (Visual.Stroke is LinearGradientBrush strokeBrush)
            {
                var originalColor1 = strokeBrush.GradientStops[0].Color;
                var originalColor2 = strokeBrush.GradientStops[1].Color;

                // Временно меняем цвет обводки на красный
                strokeBrush.GradientStops[0].Color = Colors.Red;
                strokeBrush.GradientStops[1].Color = Colors.DarkRed;

                // Возвращаем оригинальные цвета через 200ms
                var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(200) };
                timer.Tick += (s, e) =>
                {
                    strokeBrush.GradientStops[0].Color = originalColor1;
                    strokeBrush.GradientStops[1].Color = originalColor2;
                    timer.Stop();
                };
                timer.Start();
            }
        }

        public void Activate()
        {
            Visual.Fill = new RadialGradientBrush
            {
                GradientStops =
            {
                new GradientStop(Colors.White, 0),
                new GradientStop(Color.FromArgb(255, 224, 255, 255), 0.7),
                new GradientStop(Color.FromArgb(255, 138, 43, 226), 1)
            }
            };
        }

        public void Deactivate()
        {
            Visual.Fill = new RadialGradientBrush
            {
                GradientStops =
            {
                new GradientStop(Colors.White, 0),
                new GradientStop(Color.FromArgb(255, 230, 230, 250), 0.7),
                new GradientStop(Color.FromArgb(255, 138, 43, 226), 1)
            }
            };
        }

        public void FadeOut()
        {
            var anim = new DoubleAnimation
            {
                To = 0,
                Duration = TimeSpan.FromMilliseconds(500)
            };
            Visual.BeginAnimation(UIElement.OpacityProperty, anim);
        }
    }

    // Класс сетевого соединения
    public class NetworkConnection
    {
        public Line Visual { get; private set; }
        public ClusterNode SourceNode { get; set; }
        public ClusterNode TargetNode { get; set; }
        private readonly Random random;

        public NetworkConnection(ClusterNode source, ClusterNode target, Random random)
        {
            SourceNode = source;
            TargetNode = target;
            this.random = random;
            CreateVisual();
        }

        private void CreateVisual()
        {
            Visual = new Line
            {
                Stroke = new LinearGradientBrush
                {
                    GradientStops =
                    {
                        new GradientStop(Colors.Transparent, 0),
                        new GradientStop(Color.FromArgb(255, 138, 43, 226), 0.5),
                        new GradientStop(Colors.Transparent, 1)
                    }
                },
                StrokeThickness = 2,
                StrokeDashArray = new DoubleCollection { 4, 2 },
                Effect = new DropShadowEffect
                {
                    Color = Color.FromArgb(255, 138, 43, 226),
                    BlurRadius = 10,
                    ShadowDepth = 0
                }
            };
            UpdateVisual();
        }

        public void UpdateVisual()
        {
            if (Visual == null) return;

            Visual.X1 = SourceNode.Position.X;
            Visual.Y1 = SourceNode.Position.Y;
            Visual.X2 = TargetNode.Position.X;
            Visual.Y2 = TargetNode.Position.Y;
        }

        public void UpdateVisualState()
        {
            // Анимация потока данных
            if (random.NextDouble() < 0.3)
            {
                var anim = new DoubleAnimation
                {
                    From = 0,
                    To = 1,
                    Duration = TimeSpan.FromMilliseconds(1000)
                };
                Visual.Stroke.BeginAnimation(Brush.OpacityProperty, anim);
            }
        }
    }
}