using System.Collections.ObjectModel;
using InteractiveBuildingCrowdSimulator.App.Infrastructure;
using InteractiveBuildingCrowdSimulator.App.Models;

namespace InteractiveBuildingCrowdSimulator.App.ViewModels;

public record ChartPoint(double X, double Y);

/// <summary>
/// Данные для диаграмм и текстовых показателей.
/// </summary>
public class AnalyticsViewModel : ObservableObject
{
    private string _problemAreas = "Нет узких мест";

    public ObservableCollection<ChartPoint> AgentsRemaining { get; } = new();
    public ObservableCollection<ChartPoint> AverageSpeed { get; } = new();
    public ObservableCollection<ChartPoint> MaxDensity { get; } = new();

    public string ProblemAreas
    {
        get => _problemAreas;
        set => SetProperty(ref _problemAreas, value);
    }

    public void AddSnapshot(StatisticsSnapshot snapshot)
    {
        var timeSec = snapshot.SimulationTime.TotalSeconds;
        AgentsRemaining.Add(new ChartPoint(timeSec, snapshot.AgentsRemaining));
        AverageSpeed.Add(new ChartPoint(timeSec, snapshot.AverageSpeed));
        MaxDensity.Add(new ChartPoint(timeSec, snapshot.MaxDensity));

        ProblemAreas = snapshot.ProblemAreas.Count > 0
            ? string.Join("; ", snapshot.ProblemAreas)
            : "Нет опасных плотностей";

        Trim(AgentsRemaining);
        Trim(AverageSpeed);
        Trim(MaxDensity);
    }

    public void Clear()
    {
        AgentsRemaining.Clear();
        AverageSpeed.Clear();
        MaxDensity.Clear();
        ProblemAreas = "Нет данных";
    }

    private static void Trim(ObservableCollection<ChartPoint> series)
    {
        if (series.Count <= 600)
        {
            return;
        }

        while (series.Count > 600)
        {
            series.RemoveAt(0);
        }
    }
}
