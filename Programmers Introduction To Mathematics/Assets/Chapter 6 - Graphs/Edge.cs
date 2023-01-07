using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Edge : IEquatable<Edge>
{
    public Vertex from, to;

    public bool Connects(Vertex vertexA, Vertex vertexB)
    {
        return (vertexA == from && vertexB == to) || (vertexA == to && vertexB == from);
    }

    public Vertex Traverse(Vertex start)
    {
        if(from != start && to != start)
        {
            throw new ArgumentException(string.Format("Edge {0} does not touch Vertex {1}", this, start));
        }

        if(from == start)
        {
            return to;
        }
        else
        {
            return from;
        }
    }

    public bool Equals(Edge other)
    {
        return ((this.from == other.from) && (this.to == other.to)) || ((this.from == other.to) && (this.to == other.from));
    }

    public Edge(Vertex _from, Vertex _to)
    {
        this.from = _from;
        this.to = _to;
    }

    public void Replace(Vertex newVertex, Vertex oldVertex)
    {
        if(oldVertex == from)
        {
            from = newVertex;
        }

        if(oldVertex == to)
        {
            to = newVertex;
        }
    }
}
