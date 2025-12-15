using System;
using System.Collections.ObjectModel;
using TmSimulator.Core.Analysis;
using TmSimulator.Core.Machine;

namespace TmSimulator.UI.ViewModels;

public class MainViewModel : ObservableObject
{
    private readonly ComparisonEngine _comparisonEngine = new();
    private string _globalStatus = "Готово";

    public MachineWorkspaceViewModel MachineA { get; }
    public MachineWorkspaceViewModel MachineB { get; }
    public ObservableCollection<string> TestInputs { get; } = new();
    public ObservableCollection<ComparisonResultViewModel> ComparisonResults { get; } = new();

    public RelayCommand AddTestInputCommand { get; }
    public RelayCommand RemoveTestInputCommand { get; }
    public RelayCommand CompareCommand { get; }

    public string GlobalStatus
    {
        get => _globalStatus;
        set => SetProperty(ref _globalStatus, value);
    }

    public MainViewModel()
    {
        MachineA = new MachineWorkspaceViewModel("Машина A");
        MachineB = new MachineWorkspaceViewModel("Машина B");

        TestInputs.Add(string.Empty);
        TestInputs.Add("0");
        TestInputs.Add("1");

        AddTestInputCommand = new RelayCommand(p => AddTestInput(p as string ?? string.Empty));
        RemoveTestInputCommand = new RelayCommand(p => RemoveTestInput(p as string ?? string.Empty));
        CompareCommand = new RelayCommand(_ => CompareMachines());
    }

    private void AddTestInput(string value)
    {
        TestInputs.Add(value);
        GlobalStatus = "Тестовое значение добавлено.";
    }

    private void RemoveTestInput(string value)
    {
        TestInputs.Remove(value);
        GlobalStatus = "Тестовое значение удалено.";
    }

    private void CompareMachines()
    {
        ComparisonResults.Clear();
        var limit = Math.Min(MachineA.StepLimit, MachineB.StepLimit);
        var results = _comparisonEngine.Compare(MachineA.GetDefinition(), MachineB.GetDefinition(), TestInputs, limit);
        foreach (var r in results)
        {
            ComparisonResults.Add(ComparisonResultViewModel.FromResult(r));
        }

        GlobalStatus = "Сравнение завершено.";
    }
}
