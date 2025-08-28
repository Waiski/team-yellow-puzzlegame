// =============================
// WeightedPicker.cs
// Utility for weighted random selection with deterministic seeding support.
// =============================
using System.Collections.Generic;
using UnityEngine;

public class WeightedPicker<T>
{
    private struct Node { public T item; public float cumulative; }
    private readonly List<Node> _nodes = new();
    private float _total;

    public void Add(T item, float weight)
    {
        _total += weight;
        _nodes.Add(new Node { item = item, cumulative = _total });
    }

    public T Pick(System.Random rng)
    {
        if (_nodes.Count == 0) return default;
        var r = (float)(rng.NextDouble() * _total);
        // Binary search could be used, list is small so linear is fine
        for (int i = 0; i < _nodes.Count; i++)
        {
            if (r <= _nodes[i].cumulative) return _nodes[i].item;
        }
        return _nodes[^1].item; // Fallback
    }
}