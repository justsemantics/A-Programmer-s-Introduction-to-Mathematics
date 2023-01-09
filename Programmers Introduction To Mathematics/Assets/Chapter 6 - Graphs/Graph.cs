using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.VisualScripting;
using System;

public class Graph
{
    public List<Edge> Edges = new List<Edge>();
    public List<Vertex> Vertices = new List<Vertex>();

    public Color[] colors = new Color[5]
    {
        Color.red,
        Color.blue,
        Color.green,
        Color.cyan,
        Color.magenta
    };


    public void AddEdge(Edge e)
    {
        if (!Edges.Contains(e))
        {
            if (Vertices.Contains(e.from) && (Vertices.Contains(e.to)))
            {
                Edges.Add(e);
                e.from.Edges.Add(e);
                e.to.Edges.Add(e);
            }
            else
            {
                Debug.Log("Edge " + e + " is to a vertex which is not in the graph");
            }
        }
    }

    public void AddEdges(List<Edge> edges)
    {
        foreach (Edge e in edges)
        {
            AddEdge(e);
        }
    }

    public void AddVertex(Vertex v)
    {
        if (!Vertices.Contains(v)) Vertices.Add(v);
    }

    public void AddVertices(List<Vertex> vertices)
    {
        foreach (Vertex v in vertices)
        {
            AddVertex(v);
        }
    }

    public void RemoveVertex(Vertex v)
    {
        if (Vertices.Contains(v))
        {
            Vertices.Remove(v);
            v.Edges.ForEach(edge => Edges.Remove(edge));

            v.Neighbors().ForEach(neighbor =>
            {
                neighbor.Edges.Remove(new Edge(neighbor, v));
            });
        } 
    }

    public bool Connected(Vertex a, Vertex b)
    {
        Edge connectingEdge = new Edge(a, b);

        return Edges.Contains(connectingEdge);
    }

    public Graph PlanarFiveColor()
    {
        if (Vertices.Count <= 5)
        {
            int currentColor = 0;
            foreach(Vertex v in Vertices)
            {
                v.Color = colors[currentColor];
                currentColor++;
            }

            Debug.Log("wowoowwo");
            return Copy();

        }

        Graph reducedGraph = Copy();
        Graph coloredGraph;

        Vertex selectedVertex;

        List<Vertex> degAtMost4 = reducedGraph.Vertices.Where(v => v.Degree <= 4).ToList();
        List<Vertex> degEqual5 = reducedGraph.Vertices.Where(v => v.Degree == 5).ToList();

        if(degAtMost4.Count > 0)
        {
            selectedVertex = degAtMost4[0];

            reducedGraph.RemoveVertex(selectedVertex);

            coloredGraph = reducedGraph.PlanarFiveColor();

            List<Vertex> neighbors = new List<Vertex>();

            foreach(Vertex oldNeighbor in selectedVertex.Neighbors())
            {
                foreach(Vertex newNeighbor in coloredGraph.Vertices)
                {
                    if(oldNeighbor.Name == newNeighbor.Name)
                    {
                        neighbors.Add(newNeighbor);
                    }
                }
            }

            List<Color> availableColors = colors.ToList();

            foreach(Vertex neighbor in neighbors)
            {
                if (availableColors.Contains(neighbor.Color))
                {
                    availableColors.Remove(neighbor.Color);
                }
            }

            selectedVertex.Color = availableColors.First();

            coloredGraph.AddVertex(selectedVertex);

            selectedVertex.Edges.Clear();

            foreach(Vertex neighbor in neighbors)
            {
                coloredGraph.AddEdge(new Edge(selectedVertex, neighbor));
            }

            return coloredGraph;
        }
        else
        {
            selectedVertex = degEqual5[0];

            List<Vertex> neighbors = selectedVertex.Neighbors();

            reducedGraph.RemoveVertex(selectedVertex);

            Vertex w1, w2;

            reducedGraph.FindTwoNonAdjacent(neighbors, out w1, out w2);

            Vertex mergedVertex = reducedGraph.Merge(w1, w2);


            coloredGraph = reducedGraph.PlanarFiveColor();


            Vertex newMergedVertex = mergedVertex;
            Color mergedColor = Color.black;
            foreach(Vertex v in coloredGraph.Vertices)
            {
                if(v.Name == mergedVertex.Name)
                {
                    newMergedVertex = v;
                    mergedColor = v.Color;
                    break;
                }
            }

            w1.Color = mergedColor;
            List<Vertex> w1Neighbors = new List<Vertex>();
            foreach (Vertex oldNeighbor in w1.Neighbors())
            {
                foreach (Vertex newNeighbor in coloredGraph.Vertices)
                {
                    if (oldNeighbor.Name == newNeighbor.Name)
                    {
                        w1Neighbors.Add(newNeighbor);
                    }
                }
            }

            w2.Color = mergedColor;
            List<Vertex> w2Neighbors = new List<Vertex>();
            foreach (Vertex oldNeighbor in w1.Neighbors())
            {
                foreach (Vertex newNeighbor in coloredGraph.Vertices)
                {
                    if (oldNeighbor.Name == newNeighbor.Name)
                    {
                        w2Neighbors.Add(newNeighbor);
                    }
                }
            }

            coloredGraph.RemoveVertex(newMergedVertex);

            coloredGraph.AddVertex(w1);
            coloredGraph.AddVertex(w2);

            w1.Edges.Clear();
            w2.Edges.Clear();

            foreach (Vertex neighbor in w1Neighbors)
            {
                coloredGraph.AddEdge(new Edge(w1, neighbor));
            }

            foreach (Vertex neighbor in w2Neighbors)
            {
                coloredGraph.AddEdge(new Edge(w2, neighbor));
            }

            neighbors = new List<Vertex>();

            foreach (Vertex oldNeighbor in selectedVertex.Neighbors())
            {
                foreach (Vertex newNeighbor in coloredGraph.Vertices)
                {
                    if (oldNeighbor.Name == newNeighbor.Name)
                    {
                        neighbors.Add(newNeighbor);
                    }
                }
            }

            List<Color> availableColors = colors.ToList();

            foreach (Vertex neighbor in neighbors)
            {
                if (availableColors.Contains(neighbor.Color))
                {
                    availableColors.Remove(neighbor.Color);
                }
            }

            selectedVertex.Color = availableColors.First();


            coloredGraph.AddVertex(selectedVertex);

            selectedVertex.Edges.Clear();
            foreach (Vertex neighbor in neighbors)
            {
                coloredGraph.AddEdge(new Edge(selectedVertex, neighbor));
            }

            return coloredGraph;

        }
    }

    public void FindTwoNonAdjacent(List<Vertex> vertices, out Vertex w1, out Vertex w2)
    {
        foreach(Vertex a in vertices)
        {
            foreach(Vertex b in vertices)
            {
                if (a == b) continue;

                if (!Connected(a, b))
                {
                    w1 = a;
                    w2 = b;
                    return;
                }
            }
        }

        throw new Exception("uh oh, couldn't find any");
    }

    public Vertex Merge(Vertex w1, Vertex w2)
    {
        RemoveVertex(w1);
        RemoveVertex(w2);

        Vertex v = new Vertex();
        AddVertex(v);

        v.Position = w1.Position + (w2.Position - w1.Position) / 2;

        foreach(Vertex neighbor in w1.Neighbors()){
            AddEdge(new Edge(v, neighbor));
        }

        foreach(Vertex neighbor in w2.Neighbors())
        {
            AddEdge(new Edge(v, neighbor));
        }

        return v;
    }

    public Graph SubGraph(Func<Vertex, bool> criteria)
    {
        Graph subGraph = new Graph();

        Vertices.Where(criteria).ToList().ForEach((v) =>
        {
            subGraph.AddVertex(new Vertex(v));
        });

        foreach(Vertex v in subGraph.Vertices)
        {
            foreach(Vertex u in subGraph.Vertices)
            {
                if (u == v) continue;

                if(Connected(u.Previous, v.Previous))
                {
                    subGraph.AddEdge(new Edge(u, v));
                }
            }
        }

        return subGraph;
    }

    public Graph()
    {
    }

    public Graph Copy()
    {
        return SubGraph(vertex => true);
    }

    public Graph(List<Vertex> vertices)
    {
        Vertices = vertices;
    }

    public delegate bool VertexRule(Vertex v);

    public static List<Vertex> SelectVertices(Graph graph, VertexRule rule)
    {
        List<Vertex> vertices = new List<Vertex>();

        foreach(Vertex v in graph.Vertices)
        {
            if(rule(v))
            {
                vertices.Add(v);
            }
        }

        return vertices;
    }
}
