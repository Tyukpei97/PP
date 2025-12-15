using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Topology.Core.Models;

namespace Topology.Core;

/// <summary>
/// Основная модель конечного топологического пространства.
/// </summary>
public class TopologySpace
{
    public const int MaxPoints = 12;

    public ObservableCollection<TopologyPoint> Points { get; } = new();
    public ObservableCollection<OpenSet> OpenSets { get; } = new();

    public int FullMask => Points.Aggregate(0, (mask, p) => mask | (1 << p.Id));

    public TopologyPoint AddPoint(string? name = null, double x = 0, double y = 0)
    {
        if (Points.Count >= MaxPoints)
            throw new InvalidOperationException($"Максимум {MaxPoints} точек.");

        var id = GetFreeIndex();
        var point = new TopologyPoint
        {
            Id = id,
            Name = name ?? GeneratePointName(),
            X = x,
            Y = y
        };
        Points.Add(point);
        return point;
    }

    public void RemovePoint(int id)
    {
        var point = Points.FirstOrDefault(p => p.Id == id);
        if (point == null) return;

        Points.Remove(point);
        var mask = ~(1 << id);
        foreach (var open in OpenSets)
        {
            open.Mask &= mask;
        }
        NormalizeOpenSets();
    }

    public void RenamePoint(int id, string newName)
    {
        var point = Points.FirstOrDefault(p => p.Id == id);
        if (point != null)
            point.Name = newName;
    }

    public OpenSet CreateOpenSet(string name, IEnumerable<int> pointIds)
    {
        var mask = ToMask(pointIds);
        return CreateOpenSet(name, mask);
    }

    public OpenSet CreateOpenSet(string name, int mask)
    {
        var existing = OpenSets.FirstOrDefault(o => o.Mask == mask);
        if (existing != null)
        {
            existing.Name = name;
            return existing;
        }

        var openSet = new OpenSet { Name = name, Mask = mask };
        OpenSets.Add(openSet);
        return openSet;
    }

    public void UpdateOpenSet(OpenSet set, IEnumerable<int> pointIds)
    {
        set.Mask = ToMask(pointIds);
        NormalizeOpenSets();
    }

    public void DeleteOpenSet(OpenSet set)
    {
        OpenSets.Remove(set);
    }

    public int ToMask(IEnumerable<int> pointIds) => pointIds.Aggregate(0, (mask, id) => mask | (1 << id));

    public IEnumerable<int> MaskToPoints(int mask) => Points.Where(p => (mask & (1 << p.Id)) != 0).Select(p => p.Id);

    public TopologyValidationResult ComputeClosure(bool mutate = false)
    {
        var result = new TopologyValidationResult();
        var closure = new HashSet<int>(OpenSets.Select(o => o.Mask));

        var empty = 0;
        var full = FullMask;

        if (!closure.Contains(empty))
            result.Missing.Add(empty);
        closure.Add(empty);

        if (!closure.Contains(full))
            result.Missing.Add(full);
        closure.Add(full);

        bool added;
        do
        {
            added = false;
            var current = closure.ToList();
            for (int i = 0; i < current.Count; i++)
            {
                for (int j = i; j < current.Count; j++)
                {
                    var u = current[i];
                    var v = current[j];
                    var union = u | v;
                    var inter = u & v;
                    if (closure.Add(union))
                    {
                        result.Missing.Add(union);
                        added = true;
                    }
                    if (closure.Add(inter))
                    {
                        result.Missing.Add(inter);
                        added = true;
                    }
                }
            }
        } while (added);

        foreach (var missing in result.Missing)
        {
            if (OpenSets.All(o => o.Mask != missing))
                result.AddedByClosure.Add(missing);
        }

        if (mutate && result.Missing.Count > 0)
        {
            foreach (var mask in result.Missing.Distinct())
            {
                if (OpenSets.All(o => o.Mask != mask))
                    OpenSets.Add(new OpenSet { Name = $"S{OpenSets.Count + 1}", Mask = mask, Opacity = 0.35, IsVisible = true });
            }
            NormalizeOpenSets();
        }

        result.IsValid = result.Missing.Count == 0;
        if (!result.IsValid)
        {
            if (!OpenSets.Any(o => o.Mask == empty))
                result.Issues.Add("Отсутствует пустое множество.");
            if (!OpenSets.Any(o => o.Mask == full))
                result.Issues.Add("Отсутствует всё пространство X.");
            if (result.Missing.Any(m => m != empty && m != full))
                result.Issues.Add("Семейство не замкнуто относительно объединений/пересечений.");
        }

        return result;
    }

    public TopologyProperties ComputeProperties()
    {
        var props = new TopologyProperties
        {
            IsCompact = true // все конечные пространства компактны
        };

        props.IsT0 = IsT0();
        props.IsT1 = IsT1();
        props.IsT2 = IsT2();
        props.IsConnected = IsConnected();
        props.MinimalNeighborhoods = Points.ToDictionary(p => p.Id, ComputeMinimalNeighborhood);
        return props;
    }

    public int ComputeMinimalNeighborhood(TopologyPoint point)
    {
        var containing = OpenSets.Where(o => (o.Mask & (1 << point.Id)) != 0).Select(o => o.Mask).ToList();
        if (!containing.Any())
            return 0;
        var intersection = containing.Aggregate(FullMask, (acc, val) => acc & val);
        return intersection;
    }

    public ContinuityResult CheckContinuity(TopologySpace target, IDictionary<int, int> mapping)
    {
        var result = new ContinuityResult();

        foreach (var p in Points)
        {
            if (!mapping.ContainsKey(p.Id))
                result.Issues.Add($"Нет образа для точки {p.Name}.");
        }

        if (result.Issues.Any())
        {
            result.IsContinuous = false;
            return result;
        }

        foreach (var open in target.OpenSets)
        {
            var preimageMask = 0;
            foreach (var kv in mapping)
            {
                if ((open.Mask & (1 << kv.Value)) != 0)
                    preimageMask |= 1 << kv.Key;
            }

            var contains = OpenSets.Any(o => o.Mask == preimageMask);
            if (!contains)
            {
                result.Issues.Add($"Образ открытого множества '{open.Name}' не принадлежит топологии источника.");
            }
        }

        result.IsContinuous = result.Issues.Count == 0;
        return result;
    }

    public TopologyProject ToProject(string? title = null)
    {
        return new TopologyProject
        {
            Title = title,
            Points = Points.Select(p => new TopologyPoint { Id = p.Id, Name = p.Name, X = p.X, Y = p.Y }).ToList(),
            OpenSets = OpenSets.Select(o => new OpenSet
            {
                Name = o.Name,
                Mask = o.Mask,
                ColorHex = o.ColorHex,
                Opacity = o.Opacity,
                IsVisible = o.IsVisible
            }).ToList()
        };
    }

    public static TopologySpace FromProject(TopologyProject project)
    {
        var space = new TopologySpace();
        foreach (var p in project.Points)
        {
            space.Points.Add(new TopologyPoint { Id = p.Id, Name = p.Name, X = p.X, Y = p.Y });
        }

        foreach (var o in project.OpenSets)
        {
            space.OpenSets.Add(new OpenSet
            {
                Name = o.Name,
                Mask = o.Mask,
                ColorHex = o.ColorHex,
                Opacity = o.Opacity,
                IsVisible = o.IsVisible
            });
        }

        space.NormalizeOpenSets();
        return space;
    }

    public void NormalizeOpenSets()
    {
        var seen = new HashSet<int>();
        for (int i = OpenSets.Count - 1; i >= 0; i--)
        {
            var mask = OpenSets[i].Mask;
            if (seen.Contains(mask))
                OpenSets.RemoveAt(i);
            else
                seen.Add(mask);
        }
    }

    private int GetFreeIndex()
    {
        for (int i = 0; i < MaxPoints; i++)
        {
            if (Points.All(p => p.Id != i))
                return i;
        }

        return Points.Count;
    }

    private string GeneratePointName()
    {
        var index = 1;
        while (Points.Any(p => p.Name == $"x{index}"))
            index++;
        return $"x{index}";
    }

    private bool IsT0()
    {
        foreach (var a in Points)
        {
            foreach (var b in Points)
            {
                if (a.Id == b.Id) continue;
                var separates = OpenSets.Any(o => ((o.Mask & (1 << a.Id)) != 0) != ((o.Mask & (1 << b.Id)) != 0));
                if (!separates)
                    return false;
            }
        }
        return true;
    }

    private bool IsT1()
    {
        foreach (var a in Points)
        {
            foreach (var b in Points)
            {
                if (a.Id == b.Id) continue;
                var aNotB = OpenSets.Any(o => (o.Mask & (1 << a.Id)) != 0 && (o.Mask & (1 << b.Id)) == 0);
                var bNotA = OpenSets.Any(o => (o.Mask & (1 << b.Id)) != 0 && (o.Mask & (1 << a.Id)) == 0);
                if (!(aNotB && bNotA))
                    return false;
            }
        }
        return true;
    }

    private bool IsT2()
    {
        foreach (var a in Points)
        {
            foreach (var b in Points)
            {
                if (a.Id == b.Id) continue;

                var hasDisjoint = OpenSets.Any(u =>
                    (u.Mask & (1 << a.Id)) != 0 &&
                    OpenSets.Any(v =>
                        (v.Mask & (1 << b.Id)) != 0 &&
                        (u.Mask & v.Mask) == 0));

                if (!hasDisjoint)
                    return false;
            }
        }
        return true;
    }

    private bool IsConnected()
    {
        foreach (var open in OpenSets)
        {
            if (open.Mask == 0 || open.Mask == FullMask)
                continue;

            var complement = FullMask & ~open.Mask;
            if (OpenSets.Any(o => o.Mask == complement))
                return false;
        }
        return true;
    }
}
