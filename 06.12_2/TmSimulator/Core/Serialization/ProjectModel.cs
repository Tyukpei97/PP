using System.Collections.Generic;

namespace TmSimulator.Core.Serialization;

public class ProjectModel
{
    public List<StateModel> States { get; set; } = new();
    public List<TransitionModel> Transitions { get; set; } = new();
    public List<char> Alphabet { get; set; } = new();
    public char Blank { get; set; } = '_';
    public string? StartState { get; set; }
    public List<string> HaltingStates { get; set; } = new();
    public List<string> TestInputs { get; set; } = new();
    public string? LastTapeInput { get; set; }
}

public class StateModel
{
    public string Name { get; set; } = string.Empty;
    public bool IsStart { get; set; }
    public bool IsHalting { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
}

public class TransitionModel
{
    public string From { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
    public char Read { get; set; }
    public char Write { get; set; }
    public string Move { get; set; } = "S";
}
