using System;
using System.Globalization;
using System.Windows.Data;
using TmSimulator.Core.Simulation;

namespace TmSimulator.UI.Rendering;

public class SimulationStatusConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not SimulationStatus status) return string.Empty;
        return status switch
        {
            SimulationStatus.Running => "Выполняется",
            SimulationStatus.HaltedAccepting => "Останов (достигнуто конечное)",
            SimulationStatus.HaltedNoRule => "Останов (нет правила)",
            SimulationStatus.LoopDetected => "Обнаружен цикл",
            SimulationStatus.StepLimitReached => "Достигнут лимит шагов",
            _ => status.ToString()
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
