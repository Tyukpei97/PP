namespace TmSimulator.Core.Machine;

public class State
{
    public string Name { get; set; }
    public bool IsStart { get; set; }
    public bool IsHalting { get; set; }
    public double X { get; set; }
    public double Y { get; set; }

    public State(string name, bool isStart = false, bool isHalting = false, double x = 0, double y = 0)
    {
        Name = name;
        IsStart = isStart;
        IsHalting = isHalting;
        X = x;
        Y = y;
    }
}
