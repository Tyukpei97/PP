using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Windows.Media.Effects;

namespace CosmicSystem
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer simulationTimer;
        private DispatcherTimer fpsTimer;
        private List<CosmicObject> cosmicObjects;
        private CosmicObject selectedObject;
        private double simulationTime = 0;
        private bool isSimulationRunning = false;
        private const double G = 6.67430e-11;
        private const double SCALE_FACTOR = 2e9;
        private Point lastMousePosition;
        private bool isDragging = false;
        private int frameCount = 0;
        private double currentFps = 0;
        private List<Ellipse> stars = new List<Ellipse>();

        public MainWindow()
        {
            InitializeComponent();
            InitializeSimulation();
            CreateStarField();
        }

        private void InitializeSimulation()
        {
            cosmicObjects = new List<CosmicObject>();

            // Таймер симуляции
            simulationTimer = new DispatcherTimer();
            simulationTimer.Interval = TimeSpan.FromMilliseconds(16);
            simulationTimer.Tick += SimulationTimer_Tick;

            // Таймер FPS
            fpsTimer = new DispatcherTimer();
            fpsTimer.Interval = TimeSpan.FromSeconds(1);
            fpsTimer.Tick += FpsTimer_Tick;
            fpsTimer.Start();

            CreateSolarSystem();
            UpdateInfo();
        }

        private void CreateStarField()
        {
            Random rand = new Random();
            StarFieldCanvas.Children.Clear();
            stars.Clear();

            for (int i = 0; i < 200; i++)
            {
                Ellipse star = new Ellipse
                {
                    Width = rand.NextDouble() * 2 + 0.5,
                    Height = rand.NextDouble() * 2 + 0.5,
                    Fill = new SolidColorBrush(Color.FromRgb(
                        (byte)(200 + rand.Next(55)),
                        (byte)(200 + rand.Next(55)),
                        (byte)(200 + rand.Next(55))
                    )),
                    Opacity = rand.NextDouble() * 0.7 + 0.3
                };

                Canvas.SetLeft(star, rand.NextDouble() * SimulationCanvas.ActualWidth);
                Canvas.SetTop(star, rand.NextDouble() * SimulationCanvas.ActualHeight);

                StarFieldCanvas.Children.Add(star);
                stars.Add(star);
            }
        }

        private void CreateSolarSystem()
        {
            // Солнце с улучшенной визуализацией
            AddStar("Солнце", 1.989e30, 0, 0, Colors.Yellow, 696340 / 2000);

            // Планеты с реальными цветами и размерами
            AddPlanet("Меркурий", 3.301e23, 57.9e9, 0, Color.FromRgb(147, 149, 152), 2439.7, 0.8);
            AddPlanet("Венера", 4.867e24, 108.2e9, 0, Color.FromRgb(237, 162, 73), 6051.8, 0.9);
            AddPlanet("Земля", 5.972e24, 149.6e9, 0, Color.FromRgb(89, 125, 206), 6371, 1.0);
            AddPlanet("Марс", 6.417e23, 227.9e9, 0, Color.FromRgb(193, 68, 14), 3389.5, 0.7);

            // Начальные скорости для орбитального движения
            cosmicObjects[1].Velocity = new Vector(0, 47.4e3);
            cosmicObjects[2].Velocity = new Vector(0, 35.0e3);
            cosmicObjects[3].Velocity = new Vector(0, 29.8e3);
            cosmicObjects[4].Velocity = new Vector(0, 24.1e3);
        }

        private void AddStar(string name, double mass, double x, double y, Color color, double radius)
        {
            var star = new CosmicObject
            {
                Name = name,
                Mass = mass,
                Position = new Vector(x, y),
                Velocity = new Vector(0, 0),
                BaseColor = color,
                Radius = Math.Max(15, radius / 1000),
                ObjectType = CosmicObjectType.Star,
                GlowIntensity = 0.8
            };
            cosmicObjects.Add(star);
        }

        private void AddPlanet(string name, double mass, double x, double y, Color color, double radius, double detailScale)
        {
            var planet = new CosmicObject
            {
                Name = name,
                Mass = mass,
                Position = new Vector(x, y),
                Velocity = new Vector(0, 0),
                BaseColor = color,
                Radius = Math.Max(8, radius / 2000),
                ObjectType = CosmicObjectType.Planet,
                HasAtmosphere = true,
                DetailScale = detailScale
            };
            cosmicObjects.Add(planet);
        }

        private void SimulationTimer_Tick(object sender, EventArgs e)
        {
            if (!isSimulationRunning) return;

            frameCount++;
            double timeStep = 3600 * 24 * TimeScaleSlider.Value;

            CalculatePhysics(timeStep);
            UpdateDisplay();
            simulationTime += timeStep;

            TimeText.Text = $"Время: {simulationTime / (3600 * 24):F1} дней";
            UpdateInfo();
        }

        private void FpsTimer_Tick(object sender, EventArgs e)
        {
            currentFps = frameCount;
            frameCount = 0;
            FpsText.Text = $"FPS: {currentFps}";
        }

        private void CalculatePhysics(double timeStep)
        {
            for (int i = 0; i < cosmicObjects.Count; i++)
            {
                for (int j = i + 1; j < cosmicObjects.Count; j++)
                {
                    CalculateGravity(cosmicObjects[i], cosmicObjects[j], timeStep);
                }
            }

            foreach (var obj in cosmicObjects)
            {
                obj.Position += obj.Velocity * timeStep;
            }
        }

        private void CalculateGravity(CosmicObject obj1, CosmicObject obj2, double timeStep)
        {
            Vector direction = obj2.Position - obj1.Position;
            double distance = direction.Length;

            if (distance < (obj1.Radius + obj2.Radius) * 1000)
            {
                HandleCollision(obj1, obj2);
                return;
            }

            double force = G * obj1.Mass * obj2.Mass / (distance * distance);
            Vector forceDirection = direction.Normalized();

            obj1.Velocity += forceDirection * (force / obj1.Mass) * timeStep;
            obj2.Velocity -= forceDirection * (force / obj2.Mass) * timeStep;
        }

        private void HandleCollision(CosmicObject obj1, CosmicObject obj2)
        {
            if (obj1.Mass >= obj2.Mass)
            {
                Vector totalMomentum = obj1.Mass * obj1.Velocity + obj2.Mass * obj2.Velocity;
                obj1.Mass += obj2.Mass;
                obj1.Velocity = totalMomentum / obj1.Mass;
                obj1.Radius = Math.Sqrt(obj1.Radius * obj1.Radius + obj2.Radius * obj2.Radius);

                cosmicObjects.Remove(obj2);
            }
            else
            {
                HandleCollision(obj2, obj1);
            }
        }

        private void UpdateDisplay()
        {
            SimulationCanvas.Children.Clear();
            SimulationCanvas.Children.Add(StarFieldCanvas);

            // Сортируем объекты по размеру для правильного отображения
            var sortedObjects = new List<CosmicObject>(cosmicObjects);
            sortedObjects.Sort((a, b) => a.Radius.CompareTo(b.Radius));

            foreach (var obj in sortedObjects)
            {
                DrawCosmicObject(obj);
            }
        }

        private void DrawCosmicObject(CosmicObject obj)
        {
            double centerX = SimulationCanvas.ActualWidth / 2;
            double centerY = SimulationCanvas.ActualHeight / 2;
            double scale = ScaleSlider.Value;

            double scaledX = obj.Position.X / SCALE_FACTOR * scale + centerX;
            double scaledY = obj.Position.Y / SCALE_FACTOR * scale + centerY;
            double displayRadius = Math.Max(3, obj.Radius / 1000 * scale);

            // Основное тело
            Ellipse mainBody = CreateMainBody(obj, displayRadius);
            Canvas.SetLeft(mainBody, scaledX - displayRadius);
            Canvas.SetTop(mainBody, scaledY - displayRadius);
            SimulationCanvas.Children.Add(mainBody);

            // Атмосфера для планет
            if (obj.HasAtmosphere && AtmosphereEffectCheck.IsChecked == true)
            {
                Ellipse atmosphere = CreateAtmosphere(obj, displayRadius);
                Canvas.SetLeft(atmosphere, scaledX - displayRadius * 1.3);
                Canvas.SetTop(atmosphere, scaledY - displayRadius * 1.3);
                SimulationCanvas.Children.Add(atmosphere);
            }

            // Свечение для звезд и больших планет
            if (GlowEffectCheck.IsChecked == true && (obj.ObjectType == CosmicObjectType.Star || obj.Radius > 20))
            {
                Ellipse glow = CreateGlowEffect(obj, displayRadius);
                Canvas.SetLeft(glow, scaledX - displayRadius * 2);
                Canvas.SetTop(glow, scaledY - displayRadius * 2);
                SimulationCanvas.Children.Add(glow);
            }

            // Траектория
            if (ShowTrajectoryCheck.IsChecked == true && obj.Trajectory.Count > 1)
            {
                DrawTrajectory(obj, scale, centerX, centerY);
            }

            // Выделение выбранного объекта
            if (obj == selectedObject)
            {
                DrawSelectionIndicator(scaledX, scaledY, displayRadius);
            }

            // Обновляем траекторию
            obj.UpdateTrajectory();
        }

        private Ellipse CreateMainBody(CosmicObject obj, double radius)
        {
            RadialGradientBrush gradient = new RadialGradientBrush
            {
                GradientOrigin = new Point(0.3, 0.3),
                Center = new Point(0.3, 0.3)
            };

            // Создаем объем с помощью градиента
            Color baseColor = obj.BaseColor;
            Color lightColor = Color.FromArgb(255,
                (byte)Math.Min(255, baseColor.R + 60),
                (byte)Math.Min(255, baseColor.G + 60),
                (byte)Math.Min(255, baseColor.B + 60));

            Color darkColor = Color.FromArgb(255,
                (byte)Math.Max(0, baseColor.R - 40),
                (byte)Math.Max(0, baseColor.G - 40),
                (byte)Math.Max(0, baseColor.B - 40));

            gradient.GradientStops.Add(new GradientStop(lightColor, 0.0));
            gradient.GradientStops.Add(new GradientStop(baseColor, 0.5));
            gradient.GradientStops.Add(new GradientStop(darkColor, 1.0));

            // Добавляем текстуру для планет
            if (obj.ObjectType == CosmicObjectType.Planet)
            {
                gradient.Opacity = 0.9;
            }

            return new Ellipse
            {
                Width = radius * 2,
                Height = radius * 2,
                Fill = gradient,
                Stroke = obj == selectedObject ? Brushes.Cyan : Brushes.Transparent,
                StrokeThickness = obj == selectedObject ? 2 : 0,
                Effect = obj == selectedObject ? new DropShadowEffect
                {
                    Color = Colors.Cyan,
                    BlurRadius = 10,
                    ShadowDepth = 0
                } : null
            };
        }

        private Ellipse CreateAtmosphere(CosmicObject obj, double radius)
        {
            return new Ellipse
            {
                Width = radius * 2.6,
                Height = radius * 2.6,
                Fill = new RadialGradientBrush
                {
                    GradientStops =
                    {
                        new GradientStop(Color.FromArgb(80, obj.BaseColor.R, obj.BaseColor.G, obj.BaseColor.B), 0.0),
                        new GradientStop(Color.FromArgb(20, obj.BaseColor.R, obj.BaseColor.G, obj.BaseColor.B), 0.7),
                        new GradientStop(Colors.Transparent, 1.0)
                    }
                },
                Opacity = 0.3
            };
        }

        private Ellipse CreateGlowEffect(CosmicObject obj, double radius)
        {
            Color glowColor = obj.ObjectType == CosmicObjectType.Star ?
                Colors.Yellow : Color.FromArgb(100, obj.BaseColor.R, obj.BaseColor.G, obj.BaseColor.B);

            return new Ellipse
            {
                Width = radius * 4,
                Height = radius * 4,
                Fill = new RadialGradientBrush
                {
                    GradientStops =
                    {
                        new GradientStop(glowColor, 0.0),
                        new GradientStop(Color.FromArgb(50, glowColor.R, glowColor.G, glowColor.B), 0.5),
                        new GradientStop(Colors.Transparent, 1.0)
                    }
                },
                Opacity = obj.GlowIntensity
            };
        }

        private void DrawTrajectory(CosmicObject obj, double scale, double centerX, double centerY)
        {
            if (obj.Trajectory.Count < 2) return;

            Polyline trajectory = new Polyline
            {
                Stroke = new SolidColorBrush(Color.FromArgb(150, obj.BaseColor.R, obj.BaseColor.G, obj.BaseColor.B)),
                StrokeThickness = 1,
                StrokeDashArray = obj.ObjectType == CosmicObjectType.Star ?
                    new DoubleCollection { 5, 2 } : new DoubleCollection() // Пунктир для звезд
            };

            foreach (var point in obj.Trajectory)
            {
                double trajX = point.X / SCALE_FACTOR * scale + centerX;
                double trajY = point.Y / SCALE_FACTOR * scale + centerY;
                trajectory.Points.Add(new Point(trajX, trajY));
            }

            SimulationCanvas.Children.Add(trajectory);
        }

        private void DrawSelectionIndicator(double x, double y, double radius)
        {
            Ellipse indicator = new Ellipse
            {
                Width = radius * 2.5,
                Height = radius * 2.5,
                Stroke = Brushes.Cyan,
                StrokeThickness = 1,
                StrokeDashArray = new DoubleCollection { 2, 2 },
                Fill = Brushes.Transparent
            };

            Canvas.SetLeft(indicator, x - radius * 1.25);
            Canvas.SetTop(indicator, y - radius * 1.25);
            SimulationCanvas.Children.Add(indicator);
        }

        private void UpdateInfo()
        {
            ObjectCountText.Text = $"Объектов: {cosmicObjects.Count}";

            if (selectedObject != null)
            {
                SelectedObjectInfo.Text = $"🎯 {selectedObject.Name}\n" +
                                         $"📊 Масса: {selectedObject.Mass / 1.989e30:F6} M☉\n" +
                                         $"🚀 Скорость: {selectedObject.Velocity.Length / 1000:F1} км/с\n" +
                                         $"📍 Позиция: ({selectedObject.Position.X / 1e9:F0}, {selectedObject.Position.Y / 1e9:F0}) млн км\n" +
                                         $"🔵 Радиус: {selectedObject.Radius:F0} км";
            }
            else
            {
                SelectedObjectInfo.Text = "👆 Выберите объект щелчком";
            }
        }

        // Обработчики событий UI
        private void StartBtn_Click(object sender, RoutedEventArgs e)
        {
            isSimulationRunning = true;
            simulationTimer.Start();
            StatusText.Text = "🚀 Симуляция запущена";
            StartBtn.IsEnabled = false;
            PauseBtn.IsEnabled = true;
        }

        private void PauseBtn_Click(object sender, RoutedEventArgs e)
        {
            isSimulationRunning = false;
            simulationTimer.Stop();
            StatusText.Text = "⏸ Симуляция на паузе";
            StartBtn.IsEnabled = true;
            PauseBtn.IsEnabled = false;
        }

        private void ResetBtn_Click(object sender, RoutedEventArgs e)
        {
            isSimulationRunning = false;
            simulationTimer.Stop();
            cosmicObjects.Clear();
            simulationTime = 0;
            CreateSolarSystem();
            UpdateDisplay();
            StatusText.Text = "🔄 Симуляция сброшена";
            StartBtn.IsEnabled = true;
            PauseBtn.IsEnabled = false;
        }

        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var clickPoint = e.GetPosition(SimulationCanvas);

            if (e.ChangedButton == MouseButton.Left)
            {
                if (ObjectTypeCombo.SelectedIndex == 0) // Космический аппарат
                {
                    CreateSpacecraft(clickPoint);
                }
                else
                {
                    SelectObject(clickPoint);
                }
            }
            else if (e.ChangedButton == MouseButton.Right)
            {
                lastMousePosition = e.GetPosition(this);
                isDragging = true;
                Mouse.Capture(SimulationCanvas);
            }
        }

        private void Canvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            double zoomFactor = e.Delta > 0 ? 1.1 : 0.9;
            ScaleSlider.Value = Math.Max(0.1, Math.Min(5, ScaleSlider.Value * zoomFactor));
        }

        private void CreateSpacecraft(Point clickPoint)
        {
            double centerX = SimulationCanvas.ActualWidth / 2;
            double centerY = SimulationCanvas.ActualHeight / 2;
            double scale = ScaleSlider.Value;

            double worldX = (clickPoint.X - centerX) * SCALE_FACTOR / scale;
            double worldY = (clickPoint.Y - centerY) * SCALE_FACTOR / scale;

            var spacecraft = new CosmicObject
            {
                Name = "Аппарат",
                Mass = MassSlider.Value * 1e21,
                Position = new Vector(worldX, worldY),
                Velocity = new Vector(VelocitySlider.Value * 1000, 0),
                BaseColor = Colors.LightGray,
                Radius = 2,
                ObjectType = CosmicObjectType.Spacecraft
            };

            cosmicObjects.Add(spacecraft);
            selectedObject = spacecraft;
            UpdateDisplay();
            UpdateInfo();
        }

        private void SelectObject(Point clickPoint)
        {
            double centerX = SimulationCanvas.ActualWidth / 2;
            double centerY = SimulationCanvas.ActualHeight / 2;
            double scale = ScaleSlider.Value;

            double worldX = (clickPoint.X - centerX) * SCALE_FACTOR / scale;
            double worldY = (clickPoint.Y - centerY) * SCALE_FACTOR / scale;
            Vector clickPos = new Vector(worldX, worldY);

            double minDistance = double.MaxValue;
            CosmicObject closestObject = null;

            foreach (var obj in cosmicObjects)
            {
                double distance = (obj.Position - clickPos).Length;
                double selectionRadius = obj.Radius * 1000 * 2; // Увеличиваем зону выбора
                if (distance < minDistance && distance < selectionRadius)
                {
                    minDistance = distance;
                    closestObject = obj;
                }
            }

            selectedObject = closestObject;
            UpdateDisplay();
            UpdateInfo();
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            isDragging = false;
            Mouse.Capture(null);
        }
    }

    public enum CosmicObjectType
    {
        Star,
        Planet,
        Spacecraft,
        Asteroid
    }

    public class CosmicObject
    {
        public string Name { get; set; } = "Объект";
        public double Mass { get; set; }
        public Vector Position { get; set; }
        public Vector Velocity { get; set; }
        public Color BaseColor { get; set; } = Colors.White;
        public double Radius { get; set; }
        public CosmicObjectType ObjectType { get; set; }
        public List<Vector> Trajectory { get; private set; } = new List<Vector>();
        public bool HasAtmosphere { get; set; } = false;
        public double GlowIntensity { get; set; } = 0.5;
        public double DetailScale { get; set; } = 1.0;
        private const int MAX_TRAJECTORY_POINTS = 200;

        public void UpdateTrajectory()
        {
            Trajectory.Add(new Vector(Position.X, Position.Y));
            if (Trajectory.Count > MAX_TRAJECTORY_POINTS)
            {
                Trajectory.RemoveAt(0);
            }
        }
    }

    public struct Vector
    {
        public double X { get; set; }
        public double Y { get; set; }

        public Vector(double x, double y)
        {
            X = x;
            Y = y;
        }

        public double Length => Math.Sqrt(X * X + Y * Y);

        public Vector Normalized()
        {
            double len = Length;
            return len > 0 ? new Vector(X / len, Y / len) : new Vector(0, 0);
        }

        public static Vector operator +(Vector a, Vector b) => new Vector(a.X + b.X, a.Y + b.Y);
        public static Vector operator -(Vector a, Vector b) => new Vector(a.X - b.X, a.Y - b.Y);
        public static Vector operator *(Vector v, double scalar) => new Vector(v.X * scalar, v.Y * scalar);
        public static Vector operator *(double scalar, Vector v) => v * scalar;
        public static Vector operator /(Vector v, double scalar) => new Vector(v.X / scalar, v.Y / scalar);
    }
}