using System;
using System.Linq;
using Topology.Core.Models;
using Topology.Core;

namespace Topology.Core.Examples;

public static class TopologyExamples
{
    public static TopologySpace CreateIndiscrete(int count)
    {
        var space = CreateSpaceWithPoints(count);
        space.OpenSets.Add(new OpenSet { Name = "∅", Mask = 0 });
        space.OpenSets.Add(new OpenSet { Name = "X", Mask = space.FullMask });
        return space;
    }

    public static TopologySpace CreateDiscrete(int count)
    {
        var space = CreateSpaceWithPoints(count);
        space.OpenSets.Add(new OpenSet { Name = "∅", Mask = 0 });
        space.OpenSets.Add(new OpenSet { Name = "X", Mask = space.FullMask });
        foreach (var p in space.Points)
        {
            space.OpenSets.Add(new OpenSet { Name = $"{{{p.Name}}}", Mask = 1 << p.Id });
        }
        return space;
    }

    public static TopologySpace CreateSierpinski()
    {
        var space = CreateSpaceWithPoints(2);
        space.OpenSets.Add(new OpenSet { Name = "∅", Mask = 0 });
        space.OpenSets.Add(new OpenSet { Name = $"{{{space.Points[1].Name}}}", Mask = 1 << space.Points[1].Id });
        space.OpenSets.Add(new OpenSet { Name = "X", Mask = space.FullMask });
        return space;
    }

    public static TopologySpace CreateRandom(int count, Random? random = null)
    {
        random ??= new Random();
        var space = CreateSpaceWithPoints(count);
        space.OpenSets.Add(new OpenSet { Name = "∅", Mask = 0 });
        space.OpenSets.Add(new OpenSet { Name = "X", Mask = space.FullMask });

        var attempts = 0;
        while (space.OpenSets.Count < Math.Min(10, 1 << count) && attempts < 64)
        {
            attempts++;
            var mask = random.Next(0, 1 << count);
            if (mask == 0 || mask == space.FullMask) continue;
            if (space.OpenSets.Any(o => o.Mask == mask)) continue;
            space.OpenSets.Add(new OpenSet { Name = $"U{space.OpenSets.Count}", Mask = mask });
        }

        space.ComputeClosure(mutate: true);
        space.NormalizeOpenSets();
        return space;
    }

    private static TopologySpace CreateSpaceWithPoints(int count)
    {
        var space = new TopologySpace();
        var radius = 150.0;
        var centerX = 200.0;
        var centerY = 200.0;
        for (int i = 0; i < count; i++)
        {
            var angle = 2 * Math.PI * i / Math.Max(1, count);
            var x = centerX + radius * Math.Cos(angle);
            var y = centerY + radius * Math.Sin(angle);
            space.AddPoint(null, x, y);
        }
        return space;
    }
}
