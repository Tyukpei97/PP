using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using NfaVisualDebugger.Core.Automata;
using NfaVisualDebugger.Core.Serialization;
using NfaVisualDebugger.UI.Dialogs;
using NfaVisualDebugger.UI.ViewModels;

namespace NfaVisualDebugger
{
    public partial class MainWindow : Window
    {
        private StateViewModel? _draggedState;
        private Point _dragOffset;
        private AutomatonViewModel? _dragVm;

        private StateViewModel? _transitionStart;
        private AutomatonViewModel? _transitionVm;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnAutoLayoutClick(object sender, RoutedEventArgs e)
        {
            if (GetVmFromSender(sender) is { } vm && (sender as FrameworkElement)?.Tag is Canvas canvas)
            {
                var width = canvas.ActualWidth > 10 ? canvas.ActualWidth : 900;
                var height = canvas.ActualHeight > 10 ? canvas.ActualHeight : 500;
                vm.AutoLayout(width, height);
            }
        }

        private void OnDeleteStateClick(object sender, RoutedEventArgs e)
        {
            if (GetVmFromSender(sender) is { SelectedState: { } state } vm)
            {
                vm.RemoveState(state);
            }
        }

        private void OnDeleteTransitionClick(object sender, RoutedEventArgs e)
        {
            if (GetVmFromSender(sender) is { SelectedTransition: { } trans } vm)
            {
                vm.Transitions.Remove(trans);
            }
        }

        private void OnSaveAutomatonClick(object sender, RoutedEventArgs e)
        {
            if (GetVmFromSender(sender) is not { } vm)
                return;

            var dialog = new SaveFileDialog
            {
                Filter = "JSON (*.json)|*.json",
                FileName = "automaton.json"
            };

            if (dialog.ShowDialog() == true)
            {
                var nfa = vm.ToModel();
                JsonAutomataSerializer.Save(dialog.FileName, nfa);
                vm.Status = $"Сохранено в {dialog.FileName}";
            }
        }

        private void OnLoadAutomatonClick(object sender, RoutedEventArgs e)
        {
            if (GetVmFromSender(sender) is not { } vm)
                return;

            var dialog = new OpenFileDialog
            {
                Filter = "JSON (*.json)|*.json"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var nfa = JsonAutomataSerializer.Load(dialog.FileName);
                    vm.LoadFromModel(nfa);
                    vm.Status = $"Загружено из {dialog.FileName}";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Не удалось загрузить автомат: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void OnCanvasMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is not Canvas canvas || !ReferenceEquals(e.Source, canvas))
            {
                return;
            }

            var vm = GetVmFromSender(sender);
            if (vm == null)
            {
                return;
            }

            var pos = e.GetPosition(canvas);
            vm.AddState(pos.X - 30, pos.Y - 30);
        }

        private void OnCanvasMouseMove(object sender, MouseEventArgs e)
        {
            if (_draggedState == null || _dragVm == null)
            {
                return;
            }

            var canvas = FindCanvas(sender as DependencyObject);
            if (canvas == null || e.LeftButton != MouseButtonState.Pressed)
            {
                return;
            }

            var pos = e.GetPosition(canvas);
            _draggedState.X = pos.X + _dragOffset.X;
            _draggedState.Y = pos.Y + _dragOffset.Y;
        }

        private void OnCanvasMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _draggedState = null;
            _dragVm = null;
        }

        private void OnCanvasRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            _transitionStart = null;
            _transitionVm = null;
        }

        private void OnStateMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is not FrameworkElement element || element.DataContext is not StateViewModel state)
            {
                return;
            }

            var vm = GetVmFromSender(sender);
            var canvas = FindCanvas(sender as DependencyObject);
            if (vm == null || canvas == null)
            {
                return;
            }

            vm.SelectedState = state;
            _draggedState = state;
            _dragVm = vm;
            var pos = e.GetPosition(canvas);
            _dragOffset = new Point(state.X - pos.X, state.Y - pos.Y);
            e.Handled = true;
        }

        private void OnStateMouseUp(object sender, MouseButtonEventArgs e)
        {
            _draggedState = null;
            _dragVm = null;
        }

        private void OnStateRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement { DataContext: StateViewModel state })
            {
                _transitionStart = state;
                _transitionVm = GetVmFromSender(sender);
                e.Handled = true;
            }
        }

        private void OnStateRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_transitionStart == null || _transitionVm == null)
            {
                return;
            }

            if (sender is not FrameworkElement { DataContext: StateViewModel target })
            {
                _transitionStart = null;
                _transitionVm = null;
                return;
            }

            var dialog = new InputDialog("Метка перехода", Nfa.Epsilon)
            {
                Owner = this
            };
            if (dialog.ShowDialog() == true)
            {
                _transitionVm.AddTransition(_transitionStart, target, dialog.ResultText);
            }
            _transitionStart = null;
            _transitionVm = null;
            e.Handled = true;
        }

        private void OnTransitionMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is not FrameworkElement { DataContext: TransitionViewModel transition })
            {
                return;
            }

            var vm = GetVmFromSender(sender);
            if (vm == null)
            {
                return;
            }

            vm.SelectedTransition = transition;
            if (e.ClickCount == 2)
            {
                var dialog = new InputDialog("Метка перехода", transition.Label) { Owner = this };
                if (dialog.ShowDialog() == true)
                {
                    vm.UpdateTransitionLabel(transition, dialog.ResultText);
                }
            }
            e.Handled = true;
        }

        private void OnSaveProjectClick(object sender, RoutedEventArgs e)
        {
            if (DataContext is not MainViewModel mainVm)
            {
                return;
            }

            var dialog = new SaveFileDialog
            {
                Filter = "Проект (*.json)|*.json",
                FileName = "project.json"
            };

            if (dialog.ShowDialog() == true)
            {
                var nfaA = mainVm.AutomatonA.ToModel();
                var nfaB = mainVm.AutomatonB.ToModel();
                var project = new ProjectDto
                {
                    AutomatonA = new AutomatonDto { States = nfaA.States.ToList(), Transitions = nfaA.Transitions.ToList() },
                    AutomatonB = new AutomatonDto { States = nfaB.States.ToList(), Transitions = nfaB.Transitions.ToList() },
                    RegexTextA = mainVm.AutomatonA.RegexText,
                    RegexTextB = mainVm.AutomatonB.RegexText,
                    LastInput = mainVm.AutomatonA.InputText
                };

                JsonAutomataSerializer.SaveProject(dialog.FileName, project);
                mainVm.EquivalenceMessage = $"Проект сохранён: {dialog.FileName}";
            }
        }

        private void OnLoadProjectClick(object sender, RoutedEventArgs e)
        {
            if (DataContext is not MainViewModel mainVm)
            {
                return;
            }

            var dialog = new OpenFileDialog
            {
                Filter = "Проект (*.json)|*.json"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var project = JsonAutomataSerializer.LoadProject(dialog.FileName);
                    if (project.AutomatonA != null)
                    {
                        mainVm.AutomatonA.LoadFromModel(ToNfa(project.AutomatonA));
                    }

                    if (project.AutomatonB != null)
                    {
                        mainVm.AutomatonB.LoadFromModel(ToNfa(project.AutomatonB));
                    }

                    if (project.RegexTextA != null)
                    {
                        mainVm.AutomatonA.RegexText = project.RegexTextA;
                    }

                    if (project.RegexTextB != null)
                    {
                        mainVm.AutomatonB.RegexText = project.RegexTextB;
                    }

                    if (project.LastInput != null)
                    {
                        mainVm.AutomatonA.InputText = project.LastInput;
                    }

                    mainVm.EquivalenceMessage = $"Проект загружен: {dialog.FileName}";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Не удалось загрузить проект: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private AutomatonViewModel? GetVmFromSender(object? sender)
        {
            var current = sender as DependencyObject;
            while (current != null)
            {
                if (current is FrameworkElement fe && fe.DataContext is AutomatonViewModel vm)
                {
                    return vm;
                }
                current = VisualTreeHelper.GetParent(current);
            }
            return null;
        }

        private Canvas? FindCanvas(DependencyObject? element)
        {
            if (element is FrameworkElement { Tag: Canvas tagCanvas })
            {
                return tagCanvas;
            }

            while (element != null)
            {
                if (element is Canvas canvas && canvas.Name == "AutomatonCanvas")
                {
                    return canvas;
                }
                element = VisualTreeHelper.GetParent(element);
            }
            return null;
        }

        private static Nfa ToNfa(AutomatonDto dto)
        {
            var nfa = new Nfa();
            foreach (var s in dto.States.OrderBy(s => s.Id))
            {
                nfa.States.Add(new NfaState(s.Id, s.Name, s.IsStart, s.IsAccept, s.X, s.Y));
            }

            foreach (var t in dto.Transitions)
            {
                nfa.Transitions.Add(new NfaTransition(t.FromStateId, t.ToStateId, t.Label));
            }

            return nfa;
        }
    }
}
