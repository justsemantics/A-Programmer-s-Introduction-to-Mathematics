using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vertex
{

    static int VertexCount = 0;
    public List<Edge> Edges = new List<Edge>();
    public Color Color = Color.white;
    public Vertex Previous;
    public Vertex Next;

    public Vector2 Position = Vector2.zero;

    public string Name = "v";

    public int Degree
    {
        get
        {
            return Edges.Count;
        }
    }

    public Vertex()
    {
        Name = VertexCount.ToString();
        VertexCount++;
    }

    public Vertex(Vertex _previous)
    {
        Previous = _previous;
        Previous.Next = this;
        Position = Previous.Position;
        Name = Previous.Name;
        Color = Previous.Color;
    }

    public List<Vertex> Neighbors()
    { 
        List<Vertex> neighbors = new List<Vertex>();
        foreach(Edge e in Edges)
        {
            neighbors.Add(e.Traverse(this));
        }

        return neighbors;
    }
}
