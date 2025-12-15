using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace NeuralMuslim
{
    public partial class MainWindow : Window
    {
        private List<Ellipse> neurons = new List<Ellipse>();
        private List<Line> connections = new List<Line>();
        private List<Particle> particles = new List<Particle>();
        private DispatcherTimer animationTimer;
        private DispatcherTimer statsTimer;
        private Random random = new Random();
        private double simulationTime = 0;
        private int activeConnections = 0;
        private string currentMode = "Signal Propagation";

        public MainWindow()
        {
            InitializeComponent();
            InitializeSimulation();
            DrawBackgroundGrid();
            CreateNeuralNetwork();
            StartStatsTimer();

            // Подписываемся на изменение режима
            cmbMode.SelectionChanged += CmbMode_SelectionChanged;
        }

        private void CmbMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbMode.SelectedItem != null)
            {
                currentMode = ((ComboBoxItem)cmbMode.SelectedItem).Content.ToString();
                txtStats.Text = $"Mode changed to: {currentMode}";
            }
        }

        private void InitializeSimulation()
        {
            // Таймер для анимации
            animationTimer = new DispatcherTimer();
            animationTimer.Interval = TimeSpan.FromMilliseconds(50);
            animationTimer.Tick += AnimationTimer_Tick;

            // Начальные настройки
            txtStats.Text = "Network initialized\n3 layers ready\nClick START to begin";
        }

        private void DrawBackgroundGrid()
        {
            double spacing = 40;
            for (double x = 0; x < gridCanvas.ActualWidth; x += spacing)
            {
                Line line = new Line
                {
                    X1 = x,
                    X2 = x,
                    Y1 = 0,
                    Y2 = gridCanvas.ActualHeight,
                    Stroke = new SolidColorBrush(Color.FromArgb(20, 139, 0, 139)),
                    StrokeThickness = 0.5
                };
                gridCanvas.Children.Add(line);
            }

            for (double y = 0; y < gridCanvas.ActualHeight; y += spacing)
            {
                Line line = new Line
                {
                    X1 = 0,
                    X2 = gridCanvas.ActualWidth,
                    Y1 = y,
                    Y2 = y,
                    Stroke = new SolidColorBrush(Color.FromArgb(20, 139, 0, 139)),
                    StrokeThickness = 0.5
                };
                gridCanvas.Children.Add(line);
            }
        }

        private void CreateNeuralNetwork()
        {
            networkCanvas.Children.Clear();
            neurons.Clear();
            connections.Clear();

            int layers = 5;
            double canvasWidth = networkCanvas.ActualWidth;
            double canvasHeight = networkCanvas.ActualHeight;

            if (canvasWidth == 0 || canvasHeight == 0) return;

            double layerSpacing = canvasWidth / (layers + 1);

            // Создаем нейроны для каждого слоя
            for (int layer = 0; layer < layers; layer++)
            {
                int neuronsInLayer = layer == 0 ? 4 : layer == layers - 1 ? 3 : 6;
                double neuronSpacing = canvasHeight / (neuronsInLayer + 1);

                for (int neuron = 0; neuron < neuronsInLayer; neuron++)
                {
                    double x = (layer + 1) * layerSpacing;
                    double y = (neuron + 1) * neuronSpacing;

                    // Создаем нейрон с градиентной заливкой
                    Ellipse neuronEllipse = new Ellipse
                    {
                        Width = 24,
                        Height = 24,
                        Fill = CreateNeuronGradient(),
                        Stroke = new SolidColorBrush(Color.FromArgb(150, 138, 43, 226)),
                        StrokeThickness = 1.5
                    };

                    // Добавляем свечение
                    if (chkGlow.IsChecked == true)
                    {
                        neuronEllipse.Effect = new DropShadowEffect
                        {
                            BlurRadius = 15,
                            Color = Color.FromArgb(150, 138, 43, 226),
                            ShadowDepth = 0
                        };
                    }

                    Canvas.SetLeft(neuronEllipse, x - 12);
                    Canvas.SetTop(neuronEllipse, y - 12);
                    networkCanvas.Children.Add(neuronEllipse);
                    neurons.Add(neuronEllipse);

                    // Создаем связи с предыдущим слоем
                    if (layer > 0)
                    {
                        int prevLayerNeurons = layer == 1 ? 4 : 6;
                        for (int prevNeuron = 0; prevNeuron < prevLayerNeurons; prevNeuron++)
                        {
                            if (random.NextDouble() > 0.7)
                            {
                                double prevX = layer * layerSpacing;
                                double prevY = (prevNeuron + 1) * (canvasHeight / (prevLayerNeurons + 1));

                                Line connection = new Line
                                {
                                    X1 = prevX,
                                    Y1 = prevY,
                                    X2 = x,
                                    Y2 = y,
                                    Stroke = new SolidColorBrush(Color.FromArgb(40, 218, 112, 214)),
                                    StrokeThickness = 0.8,
                                    Opacity = 0.3
                                };
                                networkCanvas.Children.Add(connection);
                                connections.Add(connection);
                            }
                        }
                    }
                }
            }
        }

        private RadialGradientBrush CreateNeuronGradient()
        {
            return new RadialGradientBrush
            {
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromArgb(255, 218, 112, 214), 0.0),
                    new GradientStop(Color.FromArgb(200, 186, 85, 211), 0.7),
                    new GradientStop(Color.FromArgb(150, 148, 0, 211), 1.0)
                }
            };
        }

        private void StartStatsTimer()
        {
            statsTimer = new DispatcherTimer();
            statsTimer.Interval = TimeSpan.FromSeconds(2);
            statsTimer.Tick += (s, e) => UpdateStats();
            statsTimer.Start();
        }

        private void UpdateStats()
        {
            if (animationTimer.IsEnabled)
            {
                simulationTime += 2;
                txtStats.Text = $"Active: {activeConnections} connections\n" +
                              $"Time: {simulationTime}s\n" +
                              $"Particles: {particles.Count}\n" +
                              $"Mode: {currentMode}";
            }
        }

        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            // Разная анимация в зависимости от выбранного режима
            switch (currentMode)
            {
                case "Signal Propagation":
                    AnimateSignalPropagation();
                    break;
                case "Training Process":
                    AnimateTrainingProcess();
                    break;
                case "Pattern Recognition":
                    AnimatePatternRecognition();
                    break;
                case "Data Clustering":
                    AnimateDataClustering();
                    break;
            }

            if (chkParticles.IsChecked == true)
            {
                CreateParticles();
                UpdateParticles();
            }
            UpdateChart();
        }

        private void AnimateSignalPropagation()
        {
            activeConnections = 0;
            // Анимация последовательного распространения сигнала
            int activationLayer = (int)(simulationTime % connections.Count / 10);

            foreach (var connection in connections)
            {
                if (random.NextDouble() > 0.5)
                {
                    activeConnections++;
                    connection.Stroke = new SolidColorBrush(Color.FromArgb(150, 138, 43, 226));
                    connection.StrokeThickness = 2.0;
                    connection.Opacity = 0.9;

                    var animation = new DoubleAnimation(0.3, TimeSpan.FromMilliseconds(800));
                    connection.BeginAnimation(Line.OpacityProperty, animation);
                }
            }

            // Анимируем нейроны последовательно по слоям
            foreach (var neuron in neurons)
            {
                if (random.NextDouble() > 0.7)
                {
                    PulseNeuron(neuron, Colors.Purple);
                }
            }
        }

        private void AnimateTrainingProcess()
        {
            activeConnections = 0;
            // Анимация интенсивного "обучения" - больше активности
            foreach (var connection in connections)
            {
                if (random.NextDouble() > 0.2) // Более высокая активность
                {
                    activeConnections++;
                    connection.Stroke = new SolidColorBrush(Color.FromArgb(200, 218, 112, 214));
                    connection.StrokeThickness = 2.5;
                    connection.Opacity = 1.0;

                    var animation = new DoubleAnimation(0.4, TimeSpan.FromMilliseconds(500));
                    connection.BeginAnimation(Line.OpacityProperty, animation);
                }
            }

            // Более интенсивная пульсация нейронов
            foreach (var neuron in neurons)
            {
                if (random.NextDouble() > 0.5)
                {
                    PulseNeuron(neuron, Colors.HotPink);
                }
            }
        }

        private void AnimatePatternRecognition()
        {
            activeConnections = 0;
            // Анимация распознавания паттернов - кластерная активность
            bool activateCluster = random.NextDouble() > 0.5;

            foreach (var connection in connections)
            {
                if (activateCluster && random.NextDouble() > 0.4)
                {
                    activeConnections++;
                    connection.Stroke = new SolidColorBrush(Color.FromArgb(180, 148, 0, 211));
                    connection.StrokeThickness = 2.2;
                    connection.Opacity = 0.8;

                    var animation = new DoubleAnimation(0.3, TimeSpan.FromMilliseconds(700));
                    connection.BeginAnimation(Line.OpacityProperty, animation);
                }
            }

            // Активация кластеров нейронов
            foreach (var neuron in neurons)
            {
                if (activateCluster && random.NextDouble() > 0.3)
                {
                    PulseNeuron(neuron, Colors.DarkViolet);
                }
            }
        }

        private void AnimateDataClustering()
        {
            activeConnections = 0;
            // Анимация кластеризации - медленные, ритмичные пульсации
            double cycle = Math.Sin(simulationTime * 2);

            foreach (var connection in connections)
            {
                if (cycle > 0.5 && random.NextDouble() > 0.6)
                {
                    activeConnections++;
                    connection.Stroke = new SolidColorBrush(Color.FromArgb(160, 186, 85, 211));
                    connection.StrokeThickness = 1.8;
                    connection.Opacity = 0.7;

                    var animation = new DoubleAnimation(0.2, TimeSpan.FromMilliseconds(1200));
                    connection.BeginAnimation(Line.OpacityProperty, animation);
                }
            }

            // Медленная ритмичная пульсация нейронов
            foreach (var neuron in neurons)
            {
                if (cycle > 0.7 && random.NextDouble() > 0.8)
                {
                    PulseNeuron(neuron, Colors.MediumOrchid);
                }
            }
        }

        private void PulseNeuron(Ellipse neuron, Color pulseColor)
        {
            var scaleAnimation = new DoubleAnimation(1.5, TimeSpan.FromMilliseconds(300));
            scaleAnimation.AutoReverse = true;
            neuron.RenderTransform = new ScaleTransform();
            neuron.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
            neuron.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);

            // Изменение цвета при активации
            var colorAnimation = new ColorAnimation(pulseColor, TimeSpan.FromMilliseconds(200));
            colorAnimation.AutoReverse = true;
            ((RadialGradientBrush)neuron.Fill).GradientStops[0].BeginAnimation(
                GradientStop.ColorProperty, colorAnimation);
        }

        private void CreateParticles()
        {
            if (neurons.Count == 0) return;

            // Разное количество частиц в зависимости от режима
            int particleChance = currentMode switch
            {
                "Training Process" => 3, // Больше частиц при обучении
                "Signal Propagation" => 2,
                _ => 1
            };

            for (int i = 0; i < particleChance; i++)
            {
                if (random.NextDouble() > 0.8)
                {
                    var randomNeuron = neurons[random.Next(neurons.Count)];
                    double x = Canvas.GetLeft(randomNeuron) + 12;
                    double y = Canvas.GetTop(randomNeuron) + 12;

                    Color particleColor = currentMode switch
                    {
                        "Signal Propagation" => Color.FromArgb(180, 138, 43, 226),
                        "Training Process" => Color.FromArgb(180, 218, 112, 214),
                        "Pattern Recognition" => Color.FromArgb(180, 148, 0, 211),
                        "Data Clustering" => Color.FromArgb(180, 186, 85, 211),
                        _ => Color.FromArgb(180, 138, 43, 226)
                    };

                    Particle particle = new Particle
                    {
                        Shape = new Ellipse
                        {
                            Width = 4,
                            Height = 4,
                            Fill = new SolidColorBrush(particleColor)
                        },
                        X = x,
                        Y = y,
                        VX = (random.NextDouble() - 0.5) * 4,
                        VY = (random.NextDouble() - 0.5) * 4,
                        Life = 1.0
                    };

                    Canvas.SetLeft(particle.Shape, x);
                    Canvas.SetTop(particle.Shape, y);
                    particleCanvas.Children.Add(particle.Shape);
                    particles.Add(particle);
                }
            }
        }

        private void UpdateParticles()
        {
            for (int i = particles.Count - 1; i >= 0; i--)
            {
                var particle = particles[i];
                particle.X += particle.VX;
                particle.Y += particle.VY;
                particle.Life -= 0.02;

                Canvas.SetLeft(particle.Shape, particle.X);
                Canvas.SetTop(particle.Shape, particle.Y);
                particle.Shape.Opacity = particle.Life;

                if (particle.Life <= 0)
                {
                    particleCanvas.Children.Remove(particle.Shape);
                    particles.RemoveAt(i);
                }
            }
        }

        private void UpdateChart()
        {
            chartCanvas.Children.Clear();

            double width = chartCanvas.ActualWidth;
            double height = chartCanvas.ActualHeight;

            if (width == 0 || height == 0) return;

            // Разный вид графика в зависимости от режима
            Polyline line = new Polyline
            {
                StrokeThickness = 2,
                Points = new PointCollection()
            };

            Color chartColor = currentMode switch
            {
                "Signal Propagation" => Color.FromArgb(255, 138, 43, 226),
                "Training Process" => Color.FromArgb(255, 218, 112, 214),
                "Pattern Recognition" => Color.FromArgb(255, 148, 0, 211),
                "Data Clustering" => Color.FromArgb(255, 186, 85, 211),
                _ => Color.FromArgb(255, 138, 43, 226)
            };

            line.Stroke = new SolidColorBrush(chartColor);

            for (int i = 0; i < 20; i++)
            {
                double x = (i / 19.0) * width;
                double noise = currentMode switch
                {
                    "Signal Propagation" => Math.Sin(simulationTime * 0.5 + i * 0.3) * 0.3 + 0.7,
                    "Training Process" => Math.Sin(simulationTime * 2 + i * 0.5) * 0.2 + 0.8,
                    "Pattern Recognition" => Math.Cos(simulationTime * 0.8 + i * 0.4) * 0.4 + 0.6,
                    "Data Clustering" => Math.Sin(simulationTime * 0.3 + i * 0.6) * 0.5 + 0.5,
                    _ => Math.Sin(simulationTime * 0.5 + i * 0.3) * 0.3 + 0.7
                };
                double y = height * (0.2 + 0.6 * noise);
                line.Points.Add(new Point(x, y));
            }

            chartCanvas.Children.Add(line);
        }

        // Обработчики кнопок
        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            if (!animationTimer.IsEnabled)
            {
                animationTimer.Start();
                btnStart.Content = "STOP FLOW";
                txtStats.Text = $"{currentMode} simulation running...";
            }
            else
            {
                animationTimer.Stop();
                btnStart.Content = "START FLOW";
                txtStats.Text = "Simulation paused";
            }
        }

        private void BtnPulse_Click(object sender, RoutedEventArgs e)
        {
            // Запускаем волну активации
            foreach (var neuron in neurons)
            {
                if (random.NextDouble() > 0.3)
                {
                    Color pulseColor = currentMode switch
                    {
                        "Signal Propagation" => Colors.Purple,
                        "Training Process" => Colors.HotPink,
                        "Pattern Recognition" => Colors.DarkViolet,
                        "Data Clustering" => Colors.MediumOrchid,
                        _ => Colors.Purple
                    };
                    PulseNeuron(neuron, pulseColor);
                }
            }
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            animationTimer.Stop();
            particleCanvas.Children.Clear();
            particles.Clear();
            simulationTime = 0;
            CreateNeuralNetwork();
            btnStart.Content = "START FLOW";
            txtStats.Text = $"Network reset\nReady for {currentMode} simulation";
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            if (gridCanvas != null)
            {
                gridCanvas.Children.Clear();
                networkCanvas.Children.Clear();
                DrawBackgroundGrid();
                CreateNeuralNetwork();
            }
        }
    }

    public class Particle
    {
        public Ellipse Shape { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double VX { get; set; }
        public double VY { get; set; }
        public double Life { get; set; }
    }
}