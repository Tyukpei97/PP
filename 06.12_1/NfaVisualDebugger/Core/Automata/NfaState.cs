using System;

namespace NfaVisualDebugger.Core.Automata
{
    public class NfaState
    {
        public int Id { get; }
        public string Name { get; set; }
        public bool IsStart { get; set; }
        public bool IsAccept { get; set; }
        public double X { get; set; }
        public double Y { get; set; }

        public NfaState(int id, string name, bool isStart = false, bool isAccept = false, double x = 0, double y = 0)
        {
            Id = id;
            Name = name;
            IsStart = isStart;
            IsAccept = isAccept;
            X = x;
            Y = y;
        }

        public (double x, double y) Position => (X, Y);

        public NfaState CloneWithPosition(double x, double y) =>
            new(Id, Name, IsStart, IsAccept, x, y);

        public override string ToString() => $"{Name} ({Id})";
    }
}
