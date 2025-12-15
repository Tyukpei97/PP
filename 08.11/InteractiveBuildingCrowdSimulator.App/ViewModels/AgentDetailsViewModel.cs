using InteractiveBuildingCrowdSimulator.App.Infrastructure;
using InteractiveBuildingCrowdSimulator.App.Models;

namespace InteractiveBuildingCrowdSimulator.App.ViewModels;

public class AgentDetailsViewModel : ObservableObject
{
    private AgentRenderState? _agent;

    public AgentRenderState? Agent
    {
        get => _agent;
        set => SetProperty(ref _agent, value);
    }
}
