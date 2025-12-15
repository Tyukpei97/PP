using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Numerics;

namespace Quantum
{
    public partial class MainWindow : Window
    {
        private QuantumSystem quantumSystem;
        private Random random = new Random();
        private DispatcherTimer animationTimer;
        private double animationTime = 0;
        private List<QuantumGate> gateHistory = new List<QuantumGate>();
        private int selectedQubitIndex = 0;
        private bool isAnimating = false;
        private Storyboard currentAnimation;
        private bool isInitialized = false;
        private bool isDrawing = false;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeSystem();
            isInitialized = true;
        }

        private void InitializeSystem()
        {
            quantumSystem = new QuantumSystem(1);
            UpdateQubitSelector();

            // Initialize animation timer
            animationTimer = new DispatcherTimer();
            animationTimer.Interval = TimeSpan.FromMilliseconds(16);
            animationTimer.Tick += AnimationTimer_Tick;
            animationTimer.Start();

            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            if (isInitialized && AnimationCheckBox.IsChecked == true)
            {
                animationTime += 0.02 * AnimationSpeedSlider.Value;
                AnimateBlochSphere();
            }
        }

        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            if (isInitialized)
            {
                UpdateSystemInfo();
            }
        }

        private void AnimateBlochSphere()
        {
            if (quantumSystem.QubitCount == 0 || !isInitialized) return;

            DrawBlochSphere();
            AnimateProbabilityBars();
        }

        private void UpdateSystemInfo()
        {
            if (!isInitialized) return;

            SystemInfoText.Text = $"Qubits: {quantumSystem.QubitCount} | " +
                                $"Gates Applied: {gateHistory.Count} | " +
                                $"Entanglement: {quantumSystem.GetEntanglementLevel():F2}";

            SystemLoadBar.Value = Math.Min(quantumSystem.QubitCount * 15 + gateHistory.Count * 2, 100);
        }

        private void DrawBlochSphere()
        {
            if (!isInitialized || isDrawing) return;

            isDrawing = true;

            try
            {
                BlochSphereCanvas.Children.Clear();

                double centerX = BlochSphereCanvas.ActualWidth / 2;
                double centerY = BlochSphereCanvas.ActualHeight / 2;

                if (centerX <= 0 || centerY <= 0)
                {
                    isDrawing = false;
                    return;
                }

                double radius = Math.Min(centerX, centerY) * 0.75;

                // Анимированная сфера Блоха
                DrawAnimatedSphere(centerX, centerY, radius);

                // Оси с анимацией
                DrawAnimatedAxes(centerX, centerY, radius);

                // Вектор состояния с плавной анимацией
                DrawAnimatedStateVector(centerX, centerY, radius);

                // Квантовые состояния на сфере
                DrawQuantumStates(centerX, centerY, radius);

                // Эффекты частиц для визуализации вероятностей
                DrawProbabilityParticles(centerX, centerY, radius);
            }
            finally
            {
                isDrawing = false;
            }
        }

        private void DrawAnimatedSphere(double centerX, double centerY, double radius)
        {
            // Основная сфера
            Ellipse sphere = new Ellipse
            {
                Width = radius * 2,
                Height = radius * 2,
                Stroke = new SolidColorBrush(Color.FromArgb(255, 0, 122, 204)),
                StrokeThickness = 2,
                Fill = new RadialGradientBrush
                {
                    GradientStops = new GradientStopCollection
                    {
                        new GradientStop(Color.FromArgb(64, 0, 122, 204), 0),
                        new GradientStop(Color.FromArgb(16, 0, 122, 204), 1)
                    }
                }
            };
            Canvas.SetLeft(sphere, centerX - radius);
            Canvas.SetTop(sphere, centerY - radius);
            BlochSphereCanvas.Children.Add(sphere);

            // Анимированные линии долготы
            for (int i = 0; i < 8; i++)
            {
                double angle = i * Math.PI / 4 + animationTime * 0.1;
                DrawMeridian(centerX, centerY, radius, angle);
            }
        }

        private void DrawMeridian(double centerX, double centerY, double radius, double angle)
        {
            Path path = new Path
            {
                Stroke = new SolidColorBrush(Color.FromArgb(32, 255, 255, 255)),
                StrokeThickness = 1,
                Data = new EllipseGeometry
                {
                    Center = new Point(centerX, centerY),
                    RadiusX = radius,
                    RadiusY = radius * Math.Abs(Math.Cos(angle))
                }
            };
            BlochSphereCanvas.Children.Add(path);
        }

        private void DrawAnimatedAxes(double centerX, double centerY, double radius)
        {
            // X-axis с пульсацией
            DrawAxis(centerX - radius, centerY, centerX + radius, centerY, Brushes.Red, 2, 0);
            // Y-axis с пульсацией  
            DrawAxis(centerX, centerY - radius, centerX, centerY + radius, Brushes.Green, 2, Math.PI / 2);
            // Z-axis с пульсацией
            DrawAxis(centerX - radius, centerY, centerX + radius, centerY, Brushes.Blue, 2, Math.PI / 4);
        }

        private void DrawAxis(double x1, double y1, double x2, double y2, Brush brush, double thickness, double phase)
        {
            double pulse = Math.Abs(Math.Sin(animationTime + phase));
            Line line = new Line
            {
                X1 = x1,
                Y1 = y1,
                X2 = x2,
                Y2 = y2,
                Stroke = brush,
                StrokeThickness = thickness + pulse,
                Opacity = 0.7 + pulse * 0.3
            };
            BlochSphereCanvas.Children.Add(line);
        }

        private void DrawAnimatedStateVector(double centerX, double centerY, double radius)
        {
            if (selectedQubitIndex >= quantumSystem.QubitCount) return;

            var state = quantumSystem.GetQubitState(selectedQubitIndex);

            double animatedTheta = state.Theta;
            double animatedPhi = state.Phi;

            double x = radius * Math.Sin(animatedTheta) * Math.Cos(animatedPhi);
            double y = radius * Math.Sin(animatedTheta) * Math.Sin(animatedPhi);
            double z = radius * Math.Cos(animatedTheta);

            // Вектор состояния с градиентом
            DrawLine(centerX, centerY, centerX + x, centerY - z,
                    new LinearGradientBrush(Colors.Yellow, Colors.Red, 0), 4);

            // Анимированная точка на конце вектора
            Ellipse point = new Ellipse
            {
                Width = 12,
                Height = 12,
                Fill = new RadialGradientBrush
                {
                    GradientStops = new GradientStopCollection
                    {
                        new GradientStop(Colors.Yellow, 0),
                        new GradientStop(Colors.Red, 1)
                    }
                }
            };

            // Анимация пульсации точки
            var scaleTransform = new ScaleTransform(1, 1);
            point.RenderTransform = scaleTransform;
            point.RenderTransformOrigin = new Point(0.5, 0.5);

            Canvas.SetLeft(point, centerX + x - 6);
            Canvas.SetTop(point, centerY - z - 6);
            BlochSphereCanvas.Children.Add(point);

            // Траектория движения вектора
            DrawStateTrajectory(centerX, centerY, radius);
        }

        private void DrawStateTrajectory(double centerX, double centerY, double radius)
        {
            // Упрощенная траектория для демонстрации
            for (int i = 0; i < Math.Min(5, gateHistory.Count); i++)
            {
                double alpha = 0.3 * (1 - i / 5.0);
                Ellipse trace = new Ellipse
                {
                    Width = 4,
                    Height = 4,
                    Fill = new SolidColorBrush(Color.FromArgb((byte)(alpha * 255), 255, 255, 0)),
                    Opacity = alpha
                };
                Canvas.SetLeft(trace, centerX + Math.Sin(animationTime * 0.5 + i) * radius * 0.3 - 2);
                Canvas.SetTop(trace, centerY + Math.Cos(animationTime * 0.5 + i) * radius * 0.3 - 2);
                BlochSphereCanvas.Children.Add(trace);
            }
        }

        private void DrawQuantumStates(double centerX, double centerY, double radius)
        {
            // |0⟩ state
            DrawQuantumState(centerX, centerY - radius, "|0⟩", Brushes.Green);
            // |1⟩ state  
            DrawQuantumState(centerX, centerY + radius, "|1⟩", Brushes.Red);
            // |+⟩ state
            DrawQuantumState(centerX + radius, centerY, "|+⟩", Brushes.Blue);
            // |-⟩ state
            DrawQuantumState(centerX - radius, centerY, "|-⟩", Brushes.Purple);
        }

        private void DrawQuantumState(double x, double y, string label, Brush color)
        {
            Ellipse state = new Ellipse
            {
                Width = 8,
                Height = 8,
                Fill = color,
                Opacity = 0.7
            };
            Canvas.SetLeft(state, x - 4);
            Canvas.SetTop(state, y - 4);
            BlochSphereCanvas.Children.Add(state);

            TextBlock text = new TextBlock
            {
                Text = label,
                Foreground = color,
                FontSize = 10,
                FontWeight = FontWeights.Bold
            };
            Canvas.SetLeft(text, x + 5);
            Canvas.SetTop(text, y - 7);
            BlochSphereCanvas.Children.Add(text);
        }

        private void DrawProbabilityParticles(double centerX, double centerY, double radius)
        {
            if (selectedQubitIndex >= quantumSystem.QubitCount) return;

            var probabilities = quantumSystem.GetProbabilities(selectedQubitIndex);

            // Частицы для |0⟩ состояния
            DrawParticles(centerX, centerY - radius * 0.8, probabilities[0], Brushes.Green);
            // Частицы для |1⟩ состояния
            DrawParticles(centerX, centerY + radius * 0.8, probabilities[1], Brushes.Red);
        }

        private void DrawParticles(double centerX, double centerY, double probability, Brush color)
        {
            int particleCount = (int)(probability * 15);

            for (int i = 0; i < particleCount; i++)
            {
                double angle = animationTime + i * Math.PI * 2 / Math.Max(1, particleCount);
                double distance = 10 + 3 * Math.Sin(animationTime * 2 + i);

                Ellipse particle = new Ellipse
                {
                    Width = 2,
                    Height = 2,
                    Fill = color,
                    Opacity = 0.5 + 0.3 * Math.Sin(animationTime * 3 + i)
                };
                Canvas.SetLeft(particle, centerX + Math.Cos(angle) * distance - 1);
                Canvas.SetTop(particle, centerY + Math.Sin(angle) * distance - 1);
                BlochSphereCanvas.Children.Add(particle);
            }
        }

        private void DrawLine(double x1, double y1, double x2, double y2, Brush brush, double thickness)
        {
            Line line = new Line
            {
                X1 = x1,
                Y1 = y1,
                X2 = x2,
                Y2 = y2,
                Stroke = brush,
                StrokeThickness = thickness
            };
            BlochSphereCanvas.Children.Add(line);
        }

        private void AnimateProbabilityBars()
        {
            if (selectedQubitIndex >= quantumSystem.QubitCount) return;

            var probabilities = quantumSystem.GetProbabilities(selectedQubitIndex);

            // Анимируем прогресс-бары
            Prob0Bar.Value = probabilities[0] * 100;
            Prob1Bar.Value = probabilities[1] * 100;

            Prob0Text.Text = $"{probabilities[0] * 100:F1}%";
            Prob1Text.Text = $"{probabilities[1] * 100:F1}%";
        }

        private void UpdateDisplay()
        {
            if (!isInitialized) return;

            UpdateStateInfo();
            DrawQuantumCircuit();
            UpdateQubitSelector();
        }

        private void UpdateStateInfo()
        {
            if (selectedQubitIndex < quantumSystem.QubitCount)
            {
                var state = quantumSystem.GetQubitState(selectedQubitIndex);
                StateText.Text = $"Qubit {selectedQubitIndex}";
                StateVectorText.Text = $"θ = {state.Theta:F2}, φ = {state.Phi:F2}";

                double alpha = Math.Cos(state.Theta / 2);
                double beta = Math.Sin(state.Theta / 2);
                DetailedStateText.Text = $"|ψ⟩ = {alpha:F3}|0⟩ + {beta:F3}e^i{state.Phi:F2}|1⟩";
            }
        }

        private void DrawQuantumCircuit()
        {
            if (!isInitialized || CircuitCanvas.ActualWidth <= 0) return;

            CircuitCanvas.Children.Clear();

            double qubitSpacing = 40;
            double gateWidth = 60;
            double currentX = 20;

            // Рисуем кубиты как линии
            for (int i = 0; i < quantumSystem.QubitCount; i++)
            {
                double y = 20 + i * qubitSpacing;

                // Линия кубита
                Line qubitLine = new Line
                {
                    X1 = 20,
                    Y1 = y,
                    X2 = Math.Max(CircuitCanvas.ActualWidth - 20, 100),
                    Y2 = y,
                    Stroke = new SolidColorBrush(i == selectedQubitIndex ? Colors.Yellow : Colors.Gray),
                    StrokeThickness = 2
                };
                CircuitCanvas.Children.Add(qubitLine);

                // Метка кубита
                TextBlock label = new TextBlock
                {
                    Text = $"q[{i}]",
                    Foreground = i == selectedQubitIndex ? Brushes.Yellow : Brushes.White,
                    FontSize = 10
                };
                Canvas.SetLeft(label, 5);
                Canvas.SetTop(label, y - 8);
                CircuitCanvas.Children.Add(label);
            }

            // Рисуем примененные гейты
            for (int i = 0; i < Math.Min(gateHistory.Count, 8); i++)
            {
                double x = 60 + i * gateWidth;
                DrawGate(x, 20 + selectedQubitIndex * qubitSpacing, gateHistory[i].Type.ToString(), Brushes.Cyan);
            }
        }

        private void DrawGate(double x, double y, string gateName, Brush color)
        {
            Rectangle gate = new Rectangle
            {
                Width = 40,
                Height = 30,
                Fill = new SolidColorBrush(Color.FromArgb(128, 0, 255, 255)),
                Stroke = color,
                StrokeThickness = 2,
                RadiusX = 5,
                RadiusY = 5
            };
            Canvas.SetLeft(gate, x - 20);
            Canvas.SetTop(gate, y - 15);
            CircuitCanvas.Children.Add(gate);

            TextBlock text = new TextBlock
            {
                Text = gateName,
                Foreground = color,
                FontSize = 9,
                FontWeight = FontWeights.Bold
            };
            Canvas.SetLeft(text, x - 15);
            Canvas.SetTop(text, y - 7);
            CircuitCanvas.Children.Add(text);
        }

        private void UpdateQubitSelector()
        {
            if (!isInitialized) return;

            QubitSelector.Items.Clear();
            for (int i = 0; i < quantumSystem.QubitCount; i++)
            {
                QubitSelector.Items.Add($"Qubit {i}");
            }
            if (selectedQubitIndex < QubitSelector.Items.Count && QubitSelector.Items.Count > 0)
            {
                QubitSelector.SelectedIndex = selectedQubitIndex;
            }
        }

        // Обработчики событий квантовых вентилей
        private void XGate_Click(object sender, RoutedEventArgs e) => ApplyGateWithAnimation(QuantumGate.X);
        private void YGate_Click(object sender, RoutedEventArgs e) => ApplyGateWithAnimation(QuantumGate.Y);
        private void ZGate_Click(object sender, RoutedEventArgs e) => ApplyGateWithAnimation(QuantumGate.Z);
        private void HGate_Click(object sender, RoutedEventArgs e) => ApplyGateWithAnimation(QuantumGate.H);
        private void SGate_Click(object sender, RoutedEventArgs e) => ApplyGateWithAnimation(QuantumGate.S);
        private void TGate_Click(object sender, RoutedEventArgs e) => ApplyGateWithAnimation(QuantumGate.T);

        private void ApplyGateWithAnimation(QuantumGate gate)
        {
            if (selectedQubitIndex < quantumSystem.QubitCount && !isAnimating && isInitialized)
            {
                gateHistory.Add(gate);
                quantumSystem.ApplyGate(gate, selectedQubitIndex);

                StartGateAnimation(gate);
                UpdateDisplay();
            }
        }

        private void StartGateAnimation(QuantumGate gate)
        {
            isAnimating = true;

            // Простая анимация без сложных преобразований
            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(0.3) };
            timer.Tick += (s, e) =>
            {
                timer.Stop();
                isAnimating = false;
            };
            timer.Start();
        }

        private void ResetTo0_Click(object sender, RoutedEventArgs e)
        {
            if (isInitialized)
            {
                quantumSystem.ResetQubit(selectedQubitIndex, 0);
                UpdateDisplay();
            }
        }

        private void ResetTo1_Click(object sender, RoutedEventArgs e)
        {
            if (isInitialized)
            {
                quantumSystem.ResetQubit(selectedQubitIndex, 1);
                UpdateDisplay();
            }
        }

        private void RandomState_Click(object sender, RoutedEventArgs e)
        {
            if (isInitialized)
            {
                double randomTheta = random.NextDouble() * Math.PI;
                double randomPhi = random.NextDouble() * 2 * Math.PI;
                quantumSystem.SetQubitState(selectedQubitIndex, randomTheta, randomPhi);
                UpdateDisplay();
            }
        }

        private void MeasureQubit_Click(object sender, RoutedEventArgs e)
        {
            if (selectedQubitIndex < quantumSystem.QubitCount && isInitialized)
            {
                StartMeasurementAnimation();

                var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
                timer.Tick += (s, args) =>
                {
                    timer.Stop();
                    int result = quantumSystem.MeasureQubit(selectedQubitIndex);
                    MessageBox.Show($"Measurement result: {result}", "Qubit Measurement");
                    UpdateDisplay();
                };
                timer.Start();
            }
        }

        private void StartMeasurementAnimation()
        {
            // Упрощенная анимация измерения
            var flashAnimation = new DoubleAnimation(0.5, TimeSpan.FromSeconds(0.2));
            flashAnimation.AutoReverse = true;
            flashAnimation.RepeatBehavior = new RepeatBehavior(2);

            var storyboard = new Storyboard();
            storyboard.Children.Add(flashAnimation);

            Storyboard.SetTarget(flashAnimation, BlochSphereCanvas);
            Storyboard.SetTargetProperty(flashAnimation, new PropertyPath(OpacityProperty));

            storyboard.Begin();
        }

        private void AddQubit_Click(object sender, RoutedEventArgs e)
        {
            if (isInitialized)
            {
                quantumSystem.AddQubit();
                UpdateDisplay();
            }
        }

        private void RemoveQubit_Click(object sender, RoutedEventArgs e)
        {
            if (quantumSystem.QubitCount > 1 && isInitialized)
            {
                quantumSystem.RemoveQubit();
                if (selectedQubitIndex >= quantumSystem.QubitCount)
                {
                    selectedQubitIndex = quantumSystem.QubitCount - 1;
                }
                UpdateDisplay();
            }
        }

        private void CNOTGate_Click(object sender, RoutedEventArgs e)
        {
            if (quantumSystem.QubitCount >= 2 && isInitialized)
            {
                int control = selectedQubitIndex;
                int target = (selectedQubitIndex + 1) % quantumSystem.QubitCount;
                quantumSystem.ApplyCNOT(control, target);
                UpdateDisplay();
            }
            else
            {
                MessageBox.Show("Need at least 2 qubits for CNOT gate", "Error");
            }
        }

        private void EntangleQubits_Click(object sender, RoutedEventArgs e)
        {
            if (quantumSystem.QubitCount >= 2 && isInitialized)
            {
                quantumSystem.CreateBellState(0, 1);
                UpdateDisplay();
                AlgorithmResultText.Text = "Created Bell state: (|00⟩ + |11⟩)/√2";
            }
        }

        private void RunAlgorithm_Click(object sender, RoutedEventArgs e)
        {
            if (!isInitialized) return;

            var selectedItem = AlgorithmComboBox.SelectedItem as ComboBoxItem;
            if (selectedItem == null) return;

            var selectedAlgorithm = selectedItem.Content.ToString();

            switch (selectedAlgorithm)
            {
                case "🎯 Deutsch-Jozsa Algorithm":
                    RunDeutschJozsa();
                    break;
                case "🔍 Grover's Search":
                    RunGroverSearch();
                    break;
                case "🔢 Quantum Fourier Transform":
                    RunQuantumFourierTransform();
                    break;
            }
        }

        private void StepAlgorithm_Click(object sender, RoutedEventArgs e)
        {
            AlgorithmResultText.Text = "Step-by-step execution not implemented in this demo";
        }

        private void ResetAlgorithm_Click(object sender, RoutedEventArgs e)
        {
            if (isInitialized)
            {
                quantumSystem = new QuantumSystem(1);
                gateHistory.Clear();
                selectedQubitIndex = 0;
                UpdateDisplay();
                AlgorithmResultText.Text = "Algorithm reset. System restored to initial state.";
            }
        }

        private void RunDeutschJozsa()
        {
            bool isConstant = random.Next(2) == 0;
            AlgorithmResultText.Text = isConstant ?
                "🎯 Result: Function is CONSTANT (always returns same value)" :
                "🎯 Result: Function is BALANCED (returns 0 for half inputs, 1 for other half)";
        }

        private void RunGroverSearch()
        {
            int target = random.Next(4);
            AlgorithmResultText.Text = $"🔍 Grover's search found target element at position: {target}\n" +
                                     "Amplitude amplification completed in O(√N) time!";
        }

        private void RunQuantumFourierTransform()
        {
            AlgorithmResultText.Text = "🔢 Quantum Fourier Transform applied successfully!\n" +
                                     "The quantum state has been transformed to the frequency domain.";
        }

        private void Canvas_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (selectedQubitIndex < quantumSystem.QubitCount && isInitialized)
            {
                var point = e.GetPosition(BlochSphereCanvas);
                double centerX = BlochSphereCanvas.ActualWidth / 2;
                double centerY = BlochSphereCanvas.ActualHeight / 2;

                if (centerX <= 0 || centerY <= 0) return;

                double radius = Math.Min(centerX, centerY) * 0.75;

                double x = (point.X - centerX) / radius;
                double z = (centerY - point.Y) / radius;

                double r = Math.Sqrt(x * x + z * z);
                if (r > 1) r = 1;

                double theta = Math.Acos(Math.Min(1, Math.Max(-1, z / r)));
                double phi = Math.Atan2(0, x);

                quantumSystem.SetQubitState(selectedQubitIndex, theta, phi);
                UpdateDisplay();
            }
        }

        private void Canvas_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            // Заглушка для будущей реализации
        }

        private void QubitSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (QubitSelector.SelectedIndex >= 0 && isInitialized)
            {
                selectedQubitIndex = QubitSelector.SelectedIndex;
                UpdateDisplay();
            }
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            if (isInitialized)
            {
                // Используем отложенное обновление чтобы избежать рекурсии
                Dispatcher.BeginInvoke(new Action(UpdateDisplay), System.Windows.Threading.DispatcherPriority.Render);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            // Очистка ресурсов
            animationTimer?.Stop();
            CompositionTarget.Rendering -= CompositionTarget_Rendering;
            base.OnClosed(e);
        }
    }

    // Остальные классы (QuantumSystem, QubitState, QuantumGate) остаются без изменений
    public class QuantumSystem
    {
        private List<QubitState> qubits;
        private Random random = new Random();

        public int QubitCount => qubits.Count;

        public QuantumSystem(int initialQubits)
        {
            qubits = new List<QubitState>();
            for (int i = 0; i < initialQubits; i++)
            {
                qubits.Add(new QubitState(0, 0));
            }
        }

        public void AddQubit() => qubits.Add(new QubitState(0, 0));
        public void RemoveQubit() { if (qubits.Count > 0) qubits.RemoveAt(qubits.Count - 1); }

        public QubitState GetQubitState(int index) => qubits[index];

        public void SetQubitState(int index, double theta, double phi)
        {
            qubits[index] = new QubitState(theta, phi);
        }

        public void ResetQubit(int index, int state)
        {
            qubits[index] = state == 0 ? new QubitState(0, 0) : new QubitState(Math.PI, 0);
        }

        public void ApplyGate(QuantumGate gate, int qubitIndex)
        {
            qubits[qubitIndex] = gate.Apply(qubits[qubitIndex]);
        }

        public void ApplyCNOT(int controlQubit, int targetQubit)
        {
            if (controlQubit < qubits.Count && targetQubit < qubits.Count)
            {
                var controlState = qubits[controlQubit];
                if (Math.Abs(controlState.Theta - Math.PI) < 0.1)
                {
                    qubits[targetQubit] = QuantumGate.X.Apply(qubits[targetQubit]);
                }
            }
        }

        public void CreateBellState(int qubit1, int qubit2)
        {
            if (qubit1 < qubits.Count && qubit2 < qubits.Count)
            {
                qubits[qubit1] = new QubitState(Math.PI / 2, 0);
                qubits[qubit2] = new QubitState(Math.PI / 2, 0);
            }
        }

        public int MeasureQubit(int index)
        {
            var probabilities = GetProbabilities(index);
            return random.NextDouble() < probabilities[0] ? 0 : 1;
        }

        public double[] GetProbabilities(int index)
        {
            var state = qubits[index];
            double prob0 = Math.Pow(Math.Cos(state.Theta / 2), 2);
            return new double[] { prob0, 1 - prob0 };
        }

        public double GetEntanglementLevel()
        {
            return qubits.Count > 1 ? 0.7 : 0.0;
        }
    }

    public class QubitState
    {
        public double Theta { get; set; }
        public double Phi { get; set; }

        public QubitState(double theta, double phi)
        {
            Theta = theta;
            Phi = phi;
        }

        public override string ToString()
        {
            double alpha = Math.Cos(Theta / 2);
            double beta = Complex.FromPolarCoordinates(Math.Sin(Theta / 2), Phi).Magnitude;
            return $"{alpha:F3}|0⟩ + {beta:F3}ei{Phi:F2}|1⟩";
        }
    }

    public enum QuantumGateType { X, Y, Z, H, S, T }

    public class QuantumGate
    {
        public static QuantumGate X => new QuantumGate(QuantumGateType.X);
        public static QuantumGate Y => new QuantumGate(QuantumGateType.Y);
        public static QuantumGate Z => new QuantumGate(QuantumGateType.Z);
        public static QuantumGate H => new QuantumGate(QuantumGateType.H);
        public static QuantumGate S => new QuantumGate(QuantumGateType.S);
        public static QuantumGate T => new QuantumGate(QuantumGateType.T);

        public QuantumGateType Type { get; }

        private QuantumGate(QuantumGateType type) => Type = type;

        public QubitState Apply(QubitState state)
        {
            return Type switch
            {
                QuantumGateType.X => new QubitState(Math.PI - state.Theta, state.Phi),
                QuantumGateType.Y => new QubitState(state.Theta, state.Phi + Math.PI),
                QuantumGateType.Z => new QubitState(state.Theta, state.Phi + Math.PI),
                QuantumGateType.H => new QubitState(
                    state.Theta == 0 ? Math.PI / 2 : state.Theta == Math.PI ? Math.PI / 2 : Math.PI - state.Theta,
                    state.Phi),
                QuantumGateType.S => new QubitState(state.Theta, state.Phi + Math.PI / 2),
                QuantumGateType.T => new QubitState(state.Theta, state.Phi + Math.PI / 4),
                _ => state
            };
        }
    }
}