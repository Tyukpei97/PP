using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Molekuli
{
    public partial class MainWindow : Window
    {
        private List<Particle> particles = new List<Particle>();
        private Random random = new Random();
        private DispatcherTimer simulationTimer;
        private DateTime lastUpdateTime;
        private int frameCount = 0;
        private double currentFPS = 0;
        private Dictionary<Ellipse, Particle> visualParticles = new Dictionary<Ellipse, Particle>();

        // Simulation parameters
        private const double ContainerWidth = 830;
        private const double ContainerHeight = 550;
        private bool isRunning = false;
        private int collisionChecksPerFrame = 0;

        // Lilac color palette for particles
        private Color[] particleColors = new Color[]
        {
            Color.FromRgb(155, 107, 204), // Lilac
            Color.FromRgb(182, 132, 227), // Light Lilac
            Color.FromRgb(107, 75, 140),  // Dark Lilac
            Color.FromRgb(204, 153, 255), // Pale Lilac
            Color.FromRgb(140, 90, 182),  // Medium Lilac
        };

        public MainWindow()
        {
            InitializeComponent();
            InitializeSimulation();
        }

        private void InitializeSimulation()
        {
            // Setup simulation timer
            simulationTimer = new DispatcherTimer();
            simulationTimer.Interval = TimeSpan.FromMilliseconds(20);
            simulationTimer.Tick += SimulationTimer_Tick;
            lastUpdateTime = DateTime.Now;

            // Set canvas and overlay size
            simulationCanvas.Width = ContainerWidth;
            simulationCanvas.Height = ContainerHeight;
            overlayRect.Width = ContainerWidth;
            overlayRect.Height = ContainerHeight;

            // Create initial particles
            ResetSimulation();
        }

        private void ResetSimulation()
        {
            particles.Clear();
            visualParticles.Clear();
            simulationCanvas.Children.Clear();

            // Re-add overlay
            simulationCanvas.Children.Add(overlayRect);

            int particleCount = int.TryParse(txtParticleCount.Text, out int count) ? Math.Min(count, 200) : 50;

            for (int i = 0; i < particleCount; i++)
            {
                AddRandomParticle();
            }

            UpdateStatusInfo();
        }

        private void AddRandomParticle()
        {
            double radius = random.NextDouble() * 8 + 4;
            double mass = radius * 2;
            double temperature = sliderTemperature.Value;

            // Get random color from lilac palette
            Color baseColor = particleColors[random.Next(particleColors.Length)];

            Particle particle = new Particle
            {
                X = random.NextDouble() * (ContainerWidth - radius * 2) + radius,
                Y = random.NextDouble() * (ContainerHeight - radius * 2) + radius,
                VX = (random.NextDouble() - 0.5) * temperature * 0.3,
                VY = (random.NextDouble() - 0.5) * temperature * 0.3,
                Radius = radius,
                Mass = mass,
                BaseColor = baseColor,
                CurrentColor = baseColor
            };

            particles.Add(particle);
            DrawParticle(particle);
        }

        private void DrawParticle(Particle particle)
        {
            // Create particle with lilac theme
            Ellipse mainSphere = new Ellipse
            {
                Width = particle.Radius * 2,
                Height = particle.Radius * 2,
                Fill = new SolidColorBrush(particle.CurrentColor),
                Opacity = 0.9
            };

            // Add subtle glow effect
            mainSphere.Effect = new System.Windows.Media.Effects.DropShadowEffect
            {
                Color = particle.CurrentColor,
                ShadowDepth = 0,
                BlurRadius = 8,
                Opacity = 0.4
            };

            Canvas.SetLeft(mainSphere, particle.X - particle.Radius);
            Canvas.SetTop(mainSphere, particle.Y - particle.Radius);

            simulationCanvas.Children.Add(mainSphere);
            particle.Shape = mainSphere;
            visualParticles[mainSphere] = particle;
        }

        private void SimulationTimer_Tick(object sender, EventArgs e)
        {
            UpdatePhysics();
            UpdateVisuals();
            UpdateStatusInfo();
            CalculateFPS();
        }

        private void UpdatePhysics()
        {
            double deltaTime = 0.02;
            double gravity = sliderGravity.Value * 0.08;
            collisionChecksPerFrame = 0;

            // Spatial partitioning for collision optimization
            var spatialGrid = new Dictionary<(int, int), List<Particle>>();
            int gridSize = 50;

            foreach (var particle in particles)
            {
                int gridX = (int)(particle.X / gridSize);
                int gridY = (int)(particle.Y / gridSize);
                var key = (gridX, gridY);

                if (!spatialGrid.ContainsKey(key))
                    spatialGrid[key] = new List<Particle>();
                spatialGrid[key].Add(particle);
            }

            foreach (var particle in particles)
            {
                // Apply gravity
                if (sliderGravity.Value > 0)
                {
                    particle.VY += gravity;
                }

                // Apply damping
                particle.VX *= 0.999;
                particle.VY *= 0.999;

                // Update position
                particle.X += particle.VX * deltaTime;
                particle.Y += particle.VY * deltaTime;

                // Boundary collisions
                if (particle.X - particle.Radius < 0)
                {
                    particle.X = particle.Radius;
                    particle.VX = Math.Abs(particle.VX) * 0.9;
                }
                else if (particle.X + particle.Radius > ContainerWidth)
                {
                    particle.X = ContainerWidth - particle.Radius;
                    particle.VX = -Math.Abs(particle.VX) * 0.9;
                }

                if (particle.Y - particle.Radius < 0)
                {
                    particle.Y = particle.Radius;
                    particle.VY = Math.Abs(particle.VY) * 0.9;
                }
                else if (particle.Y + particle.Radius > ContainerHeight)
                {
                    particle.Y = ContainerHeight - particle.Radius;
                    particle.VY = -Math.Abs(particle.VY) * 0.9;
                }

                // Particle-particle collisions
                if (chkCollisions.IsChecked == true)
                {
                    int gridX = (int)(particle.X / gridSize);
                    int gridY = (int)(particle.Y / gridSize);

                    for (int dx = -1; dx <= 1; dx++)
                    {
                        for (int dy = -1; dy <= 1; dy++)
                        {
                            var neighborKey = (gridX + dx, gridY + dy);
                            if (spatialGrid.TryGetValue(neighborKey, out var cellParticles))
                            {
                                foreach (var other in cellParticles)
                                {
                                    if (particle != other)
                                    {
                                        collisionChecksPerFrame++;
                                        HandleParticleCollision(particle, other);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void HandleParticleCollision(Particle p1, Particle p2)
        {
            double dx = p2.X - p1.X;
            double dy = p2.Y - p1.Y;
            double distanceSquared = dx * dx + dy * dy;
            double minDistance = p1.Radius + p2.Radius;
            double minDistanceSquared = minDistance * minDistance;

            if (distanceSquared < minDistanceSquared && distanceSquared > 0)
            {
                double distance = Math.Sqrt(distanceSquared);
                double overlap = minDistance - distance;

                double nx = dx / distance;
                double ny = dy / distance;

                p1.X -= nx * overlap * 0.5;
                p1.Y -= ny * overlap * 0.5;
                p2.X += nx * overlap * 0.5;
                p2.Y += ny * overlap * 0.5;

                double tempVX = p1.VX;
                double tempVY = p1.VY;
                p1.VX = p2.VX * 0.95;
                p1.VY = p2.VY * 0.95;
                p2.VX = tempVX * 0.95;
                p2.VY = tempVY * 0.95;
            }
        }

        private void UpdateVisuals()
        {
            foreach (var particle in particles)
            {
                if (particle.Shape != null)
                {
                    // Update position
                    Canvas.SetLeft(particle.Shape, particle.X - particle.Radius);
                    Canvas.SetTop(particle.Shape, particle.Y - particle.Radius);

                    // Update color based on speed (lilac theme)
                    double speed = Math.Sqrt(particle.VX * particle.VX + particle.VY * particle.VY);
                    particle.CurrentColor = GetColorBySpeed(particle.BaseColor, speed);

                    // Update fill color
                    if (particle.Shape.Fill is SolidColorBrush brush)
                    {
                        brush.Color = particle.CurrentColor;
                    }

                    // Update glow effect
                    if (particle.Shape.Effect is System.Windows.Media.Effects.DropShadowEffect glow)
                    {
                        glow.Color = particle.CurrentColor;
                        glow.BlurRadius = 6 + speed * 1.5;
                    }

                    // Update opacity based on speed
                    particle.Shape.Opacity = 0.7 + Math.Min(0.3, speed * 0.03);
                }
            }
        }

        private Color GetColorBySpeed(Color baseColor, double speed)
        {
            // Modify lilac color based on speed - brighter for faster particles
            double intensity = Math.Min(1.0, speed * 0.1);
            byte r = (byte)Math.Min(255, baseColor.R + intensity * 50);
            byte g = (byte)Math.Min(255, baseColor.G + intensity * 30);
            byte b = (byte)Math.Min(255, baseColor.B + intensity * 40);

            return Color.FromRgb(r, g, b);
        }

        private void UpdateStatusInfo()
        {
            double totalEnergy = 0;
            double totalSpeed = 0;
            int particleCount = particles.Count;

            foreach (var particle in particles)
            {
                double speedSquared = particle.VX * particle.VX + particle.VY * particle.VY;
                totalEnergy += 0.5 * particle.Mass * speedSquared;
                totalSpeed += Math.Sqrt(speedSquared);
            }

            txtParticleInfo.Text = $"Particles: {particleCount}";
            txtEnergyInfo.Text = $"Energy: {totalEnergy:F0}";
            txtSpeedInfo.Text = $"Speed: {(totalSpeed / Math.Max(1, particleCount)):F1}";
            txtFPSInfo.Text = $"FPS: {currentFPS:F0}";
        }

        private void CalculateFPS()
        {
            frameCount++;
            DateTime currentTime = DateTime.Now;
            double elapsed = (currentTime - lastUpdateTime).TotalSeconds;

            if (elapsed >= 1.0)
            {
                currentFPS = frameCount / elapsed;
                frameCount = 0;
                lastUpdateTime = currentTime;
            }
        }

        // Event handlers
        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            if (!isRunning)
            {
                simulationTimer.Start();
                isRunning = true;
                btnStart.Content = "🚀 RUNNING";
            }
        }

        private void BtnPause_Click(object sender, RoutedEventArgs e)
        {
            if (isRunning)
            {
                simulationTimer.Stop();
                isRunning = false;
                btnStart.Content = "🚀 START";
            }
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            simulationTimer.Stop();
            isRunning = false;
            btnStart.Content = "🚀 START";
            ResetSimulation();
        }

        private void SimulationCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (particles.Count < 300)
            {
                Point position = e.GetPosition(simulationCanvas);

                Color randomColor = particleColors[random.Next(particleColors.Length)];

                Particle particle = new Particle
                {
                    X = position.X,
                    Y = position.Y,
                    VX = (random.NextDouble() - 0.5) * 15,
                    VY = (random.NextDouble() - 0.5) * 15,
                    Radius = random.NextDouble() * 10 + 5,
                    Mass = 12,
                    BaseColor = randomColor,
                    CurrentColor = randomColor
                };

                particles.Add(particle);
                DrawParticle(particle);
                UpdateStatusInfo();
            }
        }
    }

    // Enhanced Particle class with color properties
    public class Particle
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double VX { get; set; }
        public double VY { get; set; }
        public double Radius { get; set; }
        public double Mass { get; set; }
        public Color BaseColor { get; set; }
        public Color CurrentColor { get; set; }
        public Ellipse Shape { get; set; }
    }
}