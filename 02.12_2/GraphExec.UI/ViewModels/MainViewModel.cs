using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using GraphExec.Core.Graph;
using GraphExec.Core.Library;
using GraphExec.Core.Serialization;
using GraphExec.Core.Types;
using GraphExec.Core.Values;

namespace GraphExec.UI.ViewModels;

public sealed class MainViewModel : ObservableObject
{
    private readonly GraphEvaluator _evaluator = new();
    private readonly GraphValidator _validator = new();
    private readonly GraphSerializer _serializer;
    private GraphEvaluation? _lastEvaluation;
    private readonly List<MacroDefinition> _macros = new();

    public ObservableCollection<NodeDefinition> Palette { get; } = new();
    public ObservableCollection<NodeViewModel> Nodes { get; } = new();
    public ObservableCollection<ConnectionViewModel> Connections { get; } = new();

    private string _status = "Готово";

    public string Status
    {
        get => _status;
        set => SetField(ref _status, value);
    }

    public MainViewModel()
    {
        _serializer = new GraphSerializer();
        foreach (var def in NodeLibrary.All)
            Palette.Add(def);
    }

    public NodeViewModel AddNode(NodeDefinition definition, Point position)
    {
        var node = new NodeViewModel(definition, position.X, position.Y);
        Nodes.Add(node);
        Recalculate();
        return node;
    }

    public bool TryAddConnection(NodeViewModel from, string fromPort, NodeViewModel to, string toPort, out string message)
    {
        message = string.Empty;
        if (from == to)
        {
            message = "Нельзя соединить узел сам с собой";
            return false;
        }

        var inputPort = to.Definition.Inputs.First(p => p.Name == toPort);
        if (!inputPort.AllowMultiple && Connections.Any(c => c.To == to && c.ToPort == toPort))
        {
            message = "Вход уже занят";
            return false;
        }

        var candidateEdges = Connections.Select(c => new GraphEdge(c.From.Id, c.FromPort, c.To.Id, c.ToPort)).ToList();
        candidateEdges.Add(new GraphEdge(from.Id, fromPort, to.Id, toPort));
        var graph = new GraphState(Nodes.Select(n => n.ToNodeInstance()).ToList(), candidateEdges, _macros);
        var errors = _validator.Validate(graph);
        if (errors.Any())
        {
            message = errors.First();
            return false;
        }

        var connection = new ConnectionViewModel(from, fromPort, to, toPort);
        Connections.Add(connection);
        Recalculate();
        return true;
    }

    public void RemoveNode(NodeViewModel node)
    {
        var toRemove = Connections.Where(c => c.From == node || c.To == node).ToList();
        foreach (var c in toRemove)
            Connections.Remove(c);
        Nodes.Remove(node);
        Recalculate();
    }

    public void Clear()
    {
        Nodes.Clear();
        Connections.Clear();
        Status = "Холст очищен";
    }

    public void AutoLayout()
    {
        double x = 40;
        double y = 40;
        int perRow = 4;
        int index = 0;
        foreach (var node in Nodes)
        {
            node.X = x + (index % perRow) * 260;
            node.Y = y + (index / perRow) * 160;
            index++;
        }
        foreach (var c in Connections)
            c.UpdatePath();
    }

    public void ToggleSelection(NodeViewModel node)
    {
        node.IsSelected = !node.IsSelected;
    }

    public void Recalculate(IEnumerable<string>? changed = null)
    {
        var graph = BuildGraphState();
        var eval = _evaluator.Evaluate(graph, changedNodes: changed);
        _lastEvaluation = eval;

        if (eval.GlobalErrors.Any())
        {
            Status = string.Join("; ", eval.GlobalErrors);
            foreach (var node in Nodes)
                node.ApplyOutcome(EvaluationOutcome.Error(Status));
            return;
        }

        foreach (var node in Nodes)
        {
            var outcome = eval.TryGet(node.Id);
            node.ApplyOutcome(outcome);
        }

        foreach (var c in Connections)
            c.Energized = true;

        Status = "Обновлено";
    }

    private GraphState BuildGraphState(IEnumerable<ConnectionViewModel>? connectionsOverride = null)
    {
        var nodeInstances = Nodes.Select(n => n.ToNodeInstance()).ToList();
        var edges = (connectionsOverride ?? Connections).Select(c => new GraphEdge(c.From.Id, c.FromPort, c.To.Id, c.ToPort)).ToList();
        return new GraphState(nodeInstances, edges, _macros);
    }

    public string ExportJson()
    {
        var json = _serializer.Save(BuildGraphState());
        Status = "Сохранено";
        return json;
    }

    public void ImportJson(string json)
    {
        var graph = _serializer.Load(json);
        Nodes.Clear();
        Connections.Clear();
        _macros.Clear();
        foreach (var m in graph.Macros)
        {
            _macros.Add(m);
            Palette.Add(m);
        }

        var nodeMap = new Dictionary<string, NodeViewModel>();
        foreach (var node in graph.Nodes)
        {
            var vm = new NodeViewModel(node.Definition, node.X, node.Y);
            foreach (var setting in node.Settings)
                vm.Settings[setting.Key] = setting.Value;
            if (vm.Mode == EditorMode.Text && vm.Settings.TryGetValue("value", out var textVal))
                vm.TextValue = textVal;
            if (vm.Mode == EditorMode.Slider && vm.Settings.TryGetValue("value", out var sliderVal) && double.TryParse(sliderVal, out var parsed))
                vm.SliderValue = parsed;
            nodeMap[node.Id] = vm;
            Nodes.Add(vm);
        }

        foreach (var edge in graph.Edges)
        {
            if (nodeMap.TryGetValue(edge.FromNode, out var from) && nodeMap.TryGetValue(edge.ToNode, out var to))
                Connections.Add(new ConnectionViewModel(from, edge.FromPort, to, edge.ToPort));
        }

        Recalculate();
        Status = "Граф загружен";
    }

    public MacroDefinition? CreateMacro(string name)
    {
        var selected = Nodes.Where(n => n.IsSelected).ToList();
        if (!selected.Any())
        {
            Status = "Нет выбранных узлов";
            return null;
        }

        var selectedIds = selected.Select(n => n.Id).ToHashSet();
        var internalEdges = Connections.Where(c => selectedIds.Contains(c.From.Id) && selectedIds.Contains(c.To.Id)).ToList();
        var incomingEdges = Connections.Where(c => !selectedIds.Contains(c.From.Id) && selectedIds.Contains(c.To.Id)).ToList();
        var outgoingEdges = Connections.Where(c => selectedIds.Contains(c.From.Id) && !selectedIds.Contains(c.To.Id)).ToList();

        var placeholderNodes = new List<NodeInstance>();
        var macroEdges = new List<GraphEdge>();
        var inputBindings = new List<MacroInputBinding>();
        int inputIndex = 0;
        foreach (var edge in incomingEdges)
        {
            var targetNode = selected.First(n => n.Id == edge.To.Id);
            var targetPort = targetNode.Definition.Inputs.First(p => p.Name == edge.ToPort);
            var placeholderId = $"macro_in_{inputIndex}";
            placeholderNodes.Add(new NodeInstance(placeholderId, new MacroInputDefinition(targetPort.Type, targetPort.Name), 0, 0));
            macroEdges.Add(new GraphEdge(placeholderId, "out", edge.To.Id, edge.ToPort));
            inputBindings.Add(new MacroInputBinding($"in{inputIndex + 1}", placeholderId, targetPort.Type));
            inputIndex++;
        }

        var outputBindings = new List<MacroOutputBinding>();
        foreach (var edge in outgoingEdges)
            outputBindings.Add(new MacroOutputBinding(edge.FromPort, edge.From.Id, edge.FromPort));

        if (!outputBindings.Any() && internalEdges.Any())
        {
            var last = internalEdges.Last();
            outputBindings.Add(new MacroOutputBinding("out", last.From.Id, last.FromPort));
        }

        var internalNodes = selected.Select(n => n.ToNodeInstance()).ToList();
        var internalConnections = internalEdges.Select(e => new GraphEdge(e.From.Id, e.FromPort, e.To.Id, e.ToPort)).ToList();
        internalNodes.AddRange(placeholderNodes);
        var subGraph = new GraphState(internalNodes, internalConnections);
        var macro = new MacroDefinition($"macro.{Guid.NewGuid():N}", name, subGraph, inputBindings, outputBindings);
        _macros.Add(macro);
        Palette.Add(macro);
        Status = $"Макрос {name} создан";
        var center = new Point(selected.Average(n => n.X), selected.Average(n => n.Y));

        // Удаляем исходные узлы и связи
        foreach (var edge in internalEdges.Concat(incomingEdges).Concat(outgoingEdges).ToList())
            Connections.Remove(edge);
        foreach (var node in selected.ToList())
            Nodes.Remove(node);

        var macroNode = AddNode(macro, center);

        for (int i = 0; i < incomingEdges.Count; i++)
        {
            var binding = inputBindings[i];
            var edge = incomingEdges[i];
            TryAddConnection(edge.From, edge.FromPort, macroNode, binding.PortName, out _);
        }

        foreach (var edge in outgoingEdges)
        {
            var binding = outputBindings.FirstOrDefault(b => b.SourceNodeId == edge.From.Id && b.SourcePort == edge.FromPort) ?? outputBindings.First();
            TryAddConnection(macroNode, binding.PortName, edge.To, edge.ToPort, out _);
        }

        Recalculate();
        return macro;
    }

    public void LoadDemo()
    {
        Clear();
        // Пример: [1,2,3,4] -> фильтр (>2) -> умножить на 10 -> сумма
        var list = AddNode(NodeLibrary.Find("source.list")!, new Point(80, 80));
        list.TextValue = "[1,2,3,4]";
        list.Settings["value"] = "[1,2,3,4]";
        var gt = AddNode(NodeLibrary.Find("op.gt")!, new Point(80, 240));
        var threshold = AddNode(NodeLibrary.Find("const.int")!, new Point(80, 360));
        threshold.TextValue = "2";
        threshold.Settings["value"] = "2";
        var filter = AddNode(NodeLibrary.Find("op.filter")!, new Point(360, 140));
        var map = AddNode(NodeLibrary.Find("op.map")!, new Point(600, 140));
        var mul = AddNode(NodeLibrary.Find("op.mul")!, new Point(360, 320));
        var mulConst = AddNode(NodeLibrary.Find("const.int")!, new Point(360, 440));
        mulConst.TextValue = "10";
        mulConst.Settings["value"] = "10";
        var sum = AddNode(NodeLibrary.Find("op.sum")!, new Point(820, 200));

        TryAddConnection(list, "out", filter, "список", out _);
        TryAddConnection(gt, "out", filter, "предикат", out _);
        TryAddConnection(threshold, "out", gt, "порог", out _);

        TryAddConnection(filter, "out", map, "список", out _);
        TryAddConnection(mul, "out", map, "функция", out _);
        TryAddConnection(mulConst, "out", mul, "a", out _);

        TryAddConnection(map, "out", sum, "список", out _);
        Recalculate();
    }
}
