using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace _05._11
{
    public partial class MainWindow : Window
    {
        // ======== Модель ========
        abstract class Organism
        {
            public double X;
            public double Y;
            public double Age;
            public bool Alive = true;
        }

        class Plant : Organism
        {
            // plants are stationary
        }

        class Animal : Organism
        {
            public double Energy;
            public double Speed; // pixels per tick
            public double BreedChance;
            public double MaxAge;
            public double EnergyLossPerTick;
        }

        class Herbivore : Animal { }
        class Predator : Animal { }

        // ======== Состояние симуляции ========
        List<Plant> plants = new List<Plant>();
        List<Herbivore> herbivores = new List<Herbivore>();
        List<Predator> predators = new List<Predator>();

        Random rnd = new Random();
        DispatcherTimer timer;

        // Canvas transform state
        bool isPanning = false;
        Point panStart;
        double initialTranslateX, initialTranslateY;

        // history for graph
        const int MaxHistory = 200;
        Queue<int> histPlants = new Queue<int>();
        Queue<int> histHerb = new Queue<int>();
        Queue<int> histPred = new Queue<int>();

        // world size
        double worldWidth = 800;
        double worldHeight = 600;

        public MainWindow()
        {
            InitializeComponent();

            // Set initial canvas size from XAML defaults (or from controls later)
            WorldCanvas.Width = worldWidth;
            WorldCanvas.Height = worldHeight;

            // timer for simulation (ticks)
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(120); // tick ~8.3 t/s
            timer.Tick += Timer_Tick;

            // initial reset to load controls values
            ApplyControlsToModel();
            ResetSimulation();
        }

        // ========== UI Handlers ==========
        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            timer.Start();
        }

        private void BtnPause_Click(object sender, RoutedEventArgs e)
        {
            timer.Stop();
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            ApplyControlsToModel();
            ResetSimulation();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog { Filter = "JSON files|*.json" };
            if (dlg.ShowDialog() == true)
            {
                var state = new SaveState
                {
                    WorldWidth = worldWidth,
                    WorldHeight = worldHeight,
                    Plants = plants.Select(p => new double[] { p.X, p.Y }).ToArray(),
                    Herbivores = herbivores.Select(h => new double[] { h.X, h.Y, h.Energy }).ToArray(),
                    Predators = predators.Select(p => new double[] { p.X, p.Y, p.Energy }).ToArray()
                };
                File.WriteAllText(dlg.FileName, JsonSerializer.Serialize(state));
            }
        }

        private void BtnLoad_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog { Filter = "JSON files|*.json" };
            if (dlg.ShowDialog() == true)
            {
                try
                {
                    var json = File.ReadAllText(dlg.FileName);
                    var state = JsonSerializer.Deserialize<SaveState>(json);
                    if (state != null)
                    {
                        worldWidth = state.WorldWidth;
                        worldHeight = state.WorldHeight;
                        WorldCanvas.Width = worldWidth;
                        WorldCanvas.Height = worldHeight;
                        plants.Clear();
                        herbivores.Clear();
                        predators.Clear();
                        foreach (var a in state.Plants) plants.Add(new Plant { X = a[0], Y = a[1] });
                        foreach (var a in state.Herbivores) herbivores.Add(new Herbivore { X = a[0], Y = a[1], Energy = a.Length > 2 ? a[2] : 40 });
                        foreach (var a in state.Predators) predators.Add(new Predator { X = a[0], Y = a[1], Energy = a.Length > 2 ? a[2] : 60 });
                        RedrawWorld();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при загрузке: " + ex.Message);
                }
            }
        }

        // ========== Input on Canvas ==========
        private void WorldCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // add plant at position (transformed)
            var p = ScreenToWorld(e.GetPosition(WorldCanvas));
            plants.Add(new Plant { X = p.X, Y = p.Y });
            RedrawWorld();
        }

        private void WorldCanvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var p = ScreenToWorld(e.GetPosition(WorldCanvas));
            if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
            {
                // add predator
                predators.Add(new Predator { X = p.X, Y = p.Y, Energy = 80, Speed = SldrPredSpeed.Value, BreedChance = SldrPredBreed.Value, MaxAge = 200, EnergyLossPerTick = 0.6 });
            }
            else
            {
                // add herbivore
                herbivores.Add(new Herbivore { X = p.X, Y = p.Y, Energy = 50, Speed = SldrHerbSpeed.Value, BreedChance = SldrHerbBreed.Value, MaxAge = 120, EnergyLossPerTick = 0.4 });
            }
            RedrawWorld();
        }

        private void WorldCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (isPanning && e.LeftButton == MouseButtonState.Pressed)
            {
                var pt = e.GetPosition(this);
                var dx = pt.X - panStart.X;
                var dy = pt.Y - panStart.Y;
                Translate.X = initialTranslateX + dx;
                Translate.Y = initialTranslateY + dy;
            }
        }

        private void WorldCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isPanning = false;
            WorldCanvas.ReleaseMouseCapture();
        }

        private void WorldCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e, bool startPan = false)
        {
            // not used (overload)
        }

        private void WorldCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            // zoom to mouse position
            var pos = e.GetPosition(WorldCanvas);
            double oldScale = Scale.ScaleX;
            double delta = e.Delta > 0 ? 1.15 : 1.0 / 1.15;
            double newScale = oldScale * delta;

            // clamp
            newScale = Math.Max(0.2, Math.Min(5, newScale));
            Scale.ScaleX = newScale;
            Scale.ScaleY = newScale;

            // adjust translate so zoom is centered on mouse
            Translate.X = (Translate.X - pos.X) * (newScale / oldScale) + pos.X;
            Translate.Y = (Translate.Y - pos.Y) * (newScale / oldScale) + pos.Y;
        }

        // We'll implement panning by pressing middle mouse or space+drag
        private void WorldCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e, int dummy = 0)
        {
            // workaround overloads: start pan when space pressed
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                isPanning = true;
                panStart = e.GetPosition(this);
                initialTranslateX = Translate.X;
                initialTranslateY = Translate.Y;
                WorldCanvas.CaptureMouse();
            }
        }

        // Proper pan start using window-level events for better UX:
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            // Start panning on middle button
            if (e.MiddleButton == MouseButtonState.Pressed)
            {
                isPanning = true;
                panStart = e.GetPosition(this);
                initialTranslateX = Translate.X;
                initialTranslateY = Translate.Y;
                WorldCanvas.CaptureMouse();
            }
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);
            if (e.MiddleButton == MouseButtonState.Released)
            {
                isPanning = false;
                WorldCanvas.ReleaseMouseCapture();
            }
        }

        // Helper: convert mouse point to world coords (consider transforms)
        private Point ScreenToWorld(Point p)
        {
            // Inverse transform: (p - translate) / scale
            double sx = Scale.ScaleX;
            double wx = (p.X - Translate.X) / sx;
            double wy = (p.Y - Translate.Y) / sx;
            return new Point(wx, wy);
        }

        // ========== Simulation core ==========
        private void Timer_Tick(object sender, EventArgs e)
        {
            StepSimulation();
            RedrawWorld();
            UpdateStatsAndGraph();
        }

        void StepSimulation()
        {
            ApplyControlsToModel(); // allow dynamic parameter tweaks

            // Plants growth: with some random seeding / regrowth
            double plantGrowth = SldrPlantGrowth.Value; // chance to spawn per plant (simplified)
            int currentPlants = plants.Count;
            int attempts = Math.Max(1, (int)(plantGrowth * 50));
            for (int i = 0; i < attempts; i++)
            {
                if (rnd.NextDouble() < plantGrowth)
                {
                    plants.Add(new Plant
                    {
                        X = rnd.NextDouble() * worldWidth,
                        Y = rnd.NextDouble() * worldHeight
                    });
                }
            }

            // Herbivores act
            foreach (var h in herbivores.ToList())
            {
                if (!h.Alive) continue;
                h.Age += 1;
                h.Energy -= h.EnergyLossPerTick;

                // find nearest plant within some radius
                Plant nearest = null;
                double best = double.MaxValue;
                foreach (var p in plants)
                {
                    double d = Dist(h.X, h.Y, p.X, p.Y);
                    if (d < best) { best = d; nearest = p; }
                }

                // move towards plant (or random wander)
                MoveToward(h, nearest != null ? (nearest.X, nearest.Y) : (rnd.NextDouble() * worldWidth, rnd.NextDouble() * worldHeight), h.Speed);

                // if reached plant, eat it
                if (nearest != null && Dist(h.X, h.Y, nearest.X, nearest.Y) < 10)
                {
                    // eat
                    plants.Remove(nearest);
                    h.Energy += 18 + rnd.NextDouble() * 8;
                }

                // reproduce
                if (rnd.NextDouble() < h.BreedChance * 0.5 && h.Energy > 40)
                {
                    h.Energy *= 0.6;
                    var baby = new Herbivore
                    {
                        X = h.X + rnd.NextDouble() * 12 - 6,
                        Y = h.Y + rnd.NextDouble() * 12 - 6,
                        Energy = 25,
                        Speed = h.Speed,
                        BreedChance = h.BreedChance,
                        MaxAge = h.MaxAge,
                        EnergyLossPerTick = h.EnergyLossPerTick
                    };
                    herbivores.Add(baby);
                }

                // death by energy or age
                if (h.Energy <= 0 || h.Age > h.MaxAge)
                {
                    h.Alive = false;
                }
            }
            herbivores.RemoveAll(h => !h.Alive);

            // Predators act
            foreach (var p in predators.ToList())
            {
                if (!p.Alive) continue;
                p.Age += 1;
                p.Energy -= p.EnergyLossPerTick;

                // find nearest herbivore
                Herbivore target = null;
                double best = double.MaxValue;
                foreach (var h in herbivores)
                {
                    double d = Dist(p.X, p.Y, h.X, h.Y);
                    if (d < best) { best = d; target = h; }
                }

                MoveToward(p, target != null ? (target.X, target.Y) : (rnd.NextDouble() * worldWidth, rnd.NextDouble() * worldHeight), p.Speed);

                if (target != null && Dist(p.X, p.Y, target.X, target.Y) < 12)
                {
                    // attempt hunt success chance
                    double success = 0.6 + 0.3 * (p.Energy / 100.0);
                    if (rnd.NextDouble() < success)
                    {
                        herbivores.Remove(target);
                        p.Energy += 35;
                    }
                }

                // reproduce
                if (rnd.NextDouble() < p.BreedChance * 0.3 && p.Energy > 60)
                {
                    p.Energy *= 0.5;
                    predators.Add(new Predator
                    {
                        X = p.X + rnd.NextDouble() * 20 - 10,
                        Y = p.Y + rnd.NextDouble() * 20 - 10,
                        Energy = 40,
                        Speed = p.Speed,
                        BreedChance = p.BreedChance,
                        MaxAge = p.MaxAge,
                        EnergyLossPerTick = p.EnergyLossPerTick
                    });
                }

                if (p.Energy <= 0 || p.Age > p.MaxAge) p.Alive = false;
            }
            predators.RemoveAll(p => !p.Alive);

            // clamp elements to world bounds (simple)
            foreach (var pl in plants) ClampToWorld(pl);
            foreach (var h in herbivores) ClampToWorld(h);
            foreach (var p in predators) ClampToWorld(p);

            // prevent explosion of plants — simple cap
            if (plants.Count > 3000)
            {
                plants = plants.Take(3000).ToList();
            }
        }

        void MoveToward(Animal a, (double X, double Y) target, double speed)
        {
            double dx = target.X - a.X;
            double dy = target.Y - a.Y;
            double d = Math.Sqrt(dx * dx + dy * dy);
            if (d < 0.001) return;
            double step = speed * (timer.Interval.TotalSeconds * 1.0); // allow speed to be intuitive
            double nx = a.X + dx / d * Math.Min(step, d);
            double ny = a.Y + dy / d * Math.Min(step, d);
            a.X = nx + (rnd.NextDouble() - 0.5) * 2; // small jitter
            a.Y = ny + (rnd.NextDouble() - 0.5) * 2;
        }

        double Dist(double x1, double y1, double x2, double y2)
        {
            double dx = x1 - x2, dy = y1 - y2;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        void ClampToWorld(Organism o)
        {
            o.X = Math.Max(0, Math.Min(worldWidth, o.X));
            o.Y = Math.Max(0, Math.Min(worldHeight, o.Y));
        }

        // ========== Rendering ==========
        private void RedrawWorld()
        {
            WorldCanvas.Children.Clear();

            // draw background grid lightly
            double cell = 50;
            for (double gx = 0; gx < worldWidth; gx += cell)
            {
                var line = new Line() { X1 = gx, X2 = gx, Y1 = 0, Y2 = worldHeight, Stroke = new SolidColorBrush(Color.FromArgb(30, 0, 0, 0)), StrokeThickness = 0.5 };
                WorldCanvas.Children.Add(line);
            }
            for (double gy = 0; gy < worldHeight; gy += cell)
            {
                var line = new Line() { X1 = 0, X2 = worldWidth, Y1 = gy, Y2 = gy, Stroke = new SolidColorBrush(Color.FromArgb(30, 0, 0, 0)), StrokeThickness = 0.5 };
                WorldCanvas.Children.Add(line);
            }

            // plants (green small circles)
            foreach (var p in plants)
            {
                var el = new Ellipse() { Width = 6, Height = 6, Fill = Brushes.Green, Stroke = null };
                Canvas.SetLeft(el, p.X - 3);
                Canvas.SetTop(el, p.Y - 3);
                WorldCanvas.Children.Add(el);
            }

            // herbivores
            foreach (var h in herbivores)
            {
                var s = 10.0;
                var el = new Ellipse() { Width = s, Height = s, Fill = Brushes.Gold, Stroke = Brushes.Brown, StrokeThickness = 0.8 };
                Canvas.SetLeft(el, h.X - s / 2);
                Canvas.SetTop(el, h.Y - s / 2);
                WorldCanvas.Children.Add(el);
            }

            // predators
            foreach (var p in predators)
            {
                var s = 14.0;
                var el = new Ellipse() { Width = s, Height = s, Fill = Brushes.IndianRed, Stroke = Brushes.DarkRed, StrokeThickness = 1.0 };
                Canvas.SetLeft(el, p.X - s / 2);
                Canvas.SetTop(el, p.Y - s / 2);
                WorldCanvas.Children.Add(el);
            }
        }

        // ========== Stats & Graph ==========
        void UpdateStatsAndGraph()
        {
            TbCountPlants.Text = plants.Count.ToString();
            TbCountHerb.Text = herbivores.Count.ToString();
            TbCountPred.Text = predators.Count.ToString();

            histPlants.Enqueue(plants.Count);
            histHerb.Enqueue(herbivores.Count);
            histPred.Enqueue(predators.Count);
            if (histPlants.Count > MaxHistory) histPlants.Dequeue();
            if (histHerb.Count > MaxHistory) histHerb.Dequeue();
            if (histPred.Count > MaxHistory) histPred.Dequeue();

            DrawGraph();
        }

        void DrawGraph()
        {
            GraphCanvas.Children.Clear();
            double w = GraphCanvas.ActualWidth;
            double h = GraphCanvas.ActualHeight;
            if (w <= 0) w = GraphCanvas.Width = 600;
            if (h <= 0) h = GraphCanvas.Height = 110;

            var pArr = histPlants.ToArray();
            var hArr = histHerb.ToArray();
            var prArr = histPred.ToArray();
            if (pArr.Length < 2) return;

            int n = pArr.Length;
            int max = Math.Max(1, Math.Max(pArr.Max(), Math.Max(hArr.DefaultIfEmpty(0).Max(), prArr.DefaultIfEmpty(0).Max())));
            // helper to plot array
            void Plot(int[] arr, Brush brush, double stroke)
            {
                var poly = new Polyline { Stroke = brush, StrokeThickness = stroke };
                for (int i = 0; i < arr.Length; i++)
                {
                    double x = (double)i / (n - 1) * w;
                    double y = h - (arr[i] / (double)max) * h;
                    poly.Points.Add(new Point(x, y));
                }
                GraphCanvas.Children.Add(poly);
            }

            Plot(pArr.ToArray(), Brushes.Green, 2);
            Plot(hArr.ToArray(), Brushes.Gold, 2);
            Plot(prArr.ToArray(), Brushes.IndianRed, 2);
        }

        // ========== Initialization & helpers ==========
        void ApplyControlsToModel()
        {
            if (double.TryParse(TbMapWidth.Text, out double mw)) worldWidth = Math.Max(100, mw);
            if (double.TryParse(TbMapHeight.Text, out double mh)) worldHeight = Math.Max(100, mh);
            WorldCanvas.Width = worldWidth;
            WorldCanvas.Height = worldHeight;
        }

        void ResetSimulation()
        {
            timer.Stop();
            plants.Clear();
            herbivores.Clear();
            predators.Clear();
            histPlants.Clear();
            histHerb.Clear();
            histPred.Clear();

            // parse initial counts
            int ip = ParseIntSafe(TbInitPlants.Text, 150);
            int ih = ParseIntSafe(TbInitHerbivores.Text, 40);
            int ipr = ParseIntSafe(TbInitPredators.Text, 12);

            // create entities randomly
            for (int i = 0; i < ip; i++) plants.Add(new Plant { X = rnd.NextDouble() * worldWidth, Y = rnd.NextDouble() * worldHeight });
            for (int i = 0; i < ih; i++) herbivores.Add(new Herbivore
            {
                X = rnd.NextDouble() * worldWidth,
                Y = rnd.NextDouble() * worldHeight,
                Energy = 40 + rnd.NextDouble() * 20,
                Speed = SldrHerbSpeed.Value,
                BreedChance = SldrHerbBreed.Value,
                MaxAge = 120,
                EnergyLossPerTick = 0.35
            });
            for (int i = 0; i < ipr; i++) predators.Add(new Predator
            {
                X = rnd.NextDouble() * worldWidth,
                Y = rnd.NextDouble() * worldHeight,
                Energy = 60 + rnd.NextDouble() * 30,
                Speed = SldrPredSpeed.Value,
                BreedChance = SldrPredBreed.Value,
                MaxAge = 200,
                EnergyLossPerTick = 0.6
            });

            RedrawWorld();
            UpdateStatsAndGraph();
        }

        int ParseIntSafe(string s, int def)
        {
            if (int.TryParse(s, out int v)) return v;
            return def;
        }

        // ========== Persistence structure ==========
        class SaveState
        {
            public double WorldWidth { get; set; }
            public double WorldHeight { get; set; }
            public double[][] Plants { get; set; }
            public double[][] Herbivores { get; set; }
            public double[][] Predators { get; set; }
        }
    }
}
