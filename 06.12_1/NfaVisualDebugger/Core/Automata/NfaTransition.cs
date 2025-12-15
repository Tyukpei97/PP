namespace NfaVisualDebugger.Core.Automata
{
    public class NfaTransition
    {
        public int FromStateId { get; }
        public int ToStateId { get; }
        public string Label { get; set; }

        public NfaTransition(int fromStateId, int toStateId, string label)
        {
            FromStateId = fromStateId;
            ToStateId = toStateId;
            Label = label;
        }

        public bool IsEpsilon(string epsilonSymbol) => Label == epsilonSymbol;
    }
}
