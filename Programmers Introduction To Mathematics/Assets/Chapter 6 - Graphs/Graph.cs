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
        //only add an edge if it isn't in the graph already. this includes "reversed" edges, where the vertices from and to are switched
        if (!Edges.Contains(e))
        {
            //only add edges between vertices that are in the graph
            if (Vertices.Contains(e.from) && (Vertices.Contains(e.to)))
            {
                Edges.Add(e);

                //let each vertex know about its new neighbor
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

            //remove the edges connecting to the vertex from the graph
            foreach(Edge edge in v.Edges)
            {
                this.Edges.Remove(edge);
            }

            //remove edges connecting to the vertex from its neighbors
            foreach(Vertex neighbor in v.Neighbors())
            {
                neighbor.Edges.Remove(new Edge(v, neighbor));
            }
        } 
    }

    public bool Connected(Vertex a, Vertex b)
    {
        Edge connectingEdge = new Edge(a, b);

        return Edges.Contains(connectingEdge);
    }

    public Graph PlanarFiveColor()
    {
        //recursion end condition, if there are fewer vertices than colors it makes the graph easy to color!
        if (Vertices.Count <= 5)
        {
            //assign the colors in order and call it done
            int currentColor = 0;
            foreach(Vertex v in Vertices)
            {
                v.Color = colors[currentColor];
                currentColor++;
            }

            return Copy();
        }


        //if there are more than 5 vertices, we will call PlanarFiveColor recursively on a "reduced" graph with fewer vertices
        Graph reducedGraph = Copy();

        //this is the result of the PlanarFiveColor call, it will be a graph that is colored in, except for any vertices we remove during this step
        Graph coloredGraph;

        //this is the vertex that will be removed during this step
        Vertex selectedVertex;

        //vertices with degree 4 or less are easy to add to the graph, as there will always be an extra color to use even if none of their neighbors match
        List<Vertex> degAtMost4 = reducedGraph.Vertices.Where(v => v.Degree <= 4).ToList();

        //vertices with 5 neighbors are trickier and require extra steps to ensure they will have two like-colored neighbors
        List<Vertex> degEqual5 = reducedGraph.Vertices.Where(v => v.Degree == 5).ToList();


        //attack easy vertices first
        if(degAtMost4.Count > 0)
        {
            //select the first vertex and remove it, then wait for the graph to come back colored in
            selectedVertex = degAtMost4.First();
            reducedGraph.RemoveVertex(selectedVertex);

            //ok, see you later
            coloredGraph = reducedGraph.PlanarFiveColor();

            //we're back with a colored graph. it's a copy of the old graph, so we have to find the new vertices that match the selected vertex's neighbors
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

            //start with a list of all colors, and remove any that are used by neighbors. because this vertex had degree 4 or less, there will always be one left over
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

            //the selected vertex has edges pointing to old vertices. we have to replace them with edges to its new neighbors
            selectedVertex.Edges.Clear();
            foreach(Vertex neighbor in neighbors)
            {
                coloredGraph.AddEdge(new Edge(selectedVertex, neighbor));
            }

            //graph is colored
            return coloredGraph;
        }
        else
        {
            selectedVertex = degEqual5[0];


            reducedGraph.RemoveVertex(selectedVertex);

            //the selected vertex has five neighbors. we'd be unable to color it in at all if the recursive call were to use up all the colors among its neighbors
            //the solution is to merge two neighbors before continuing, ensuring they share a color when we later "split" that merged vertex back into two
            Vertex w1, w2;

            //the two vertices to merge must not be connected by an edge, or we can't color them the same color later
            reducedGraph.FindTwoNonAdjacent(selectedVertex.Neighbors(), out w1, out w2);

            Vertex mergedVertex = reducedGraph.Merge(w1, w2);

            
            //ok, see you later
            coloredGraph = reducedGraph.PlanarFiveColor();


            //graph is now colored in. first we need to find the new vertex that represents the one we merged earlier and see what color it ended up being
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

            //both vertices that were merged are going to be colored the same
            w1.Color = mergedColor;
            w2.Color = mergedColor;


            //populate a list of new vertices that represent the neighbors of w1
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

            //same with w2
            List<Vertex> w2Neighbors = new List<Vertex>();
            foreach (Vertex oldNeighbor in w2.Neighbors())
            {
                foreach (Vertex newNeighbor in coloredGraph.Vertices)
                {
                    if (oldNeighbor.Name == newNeighbor.Name)
                    {
                        w2Neighbors.Add(newNeighbor);
                    }
                }
            }

            //swap out the merged vertex and the two vertices it replaced
            coloredGraph.RemoveVertex(newMergedVertex);

            coloredGraph.AddVertex(w1);
            coloredGraph.AddVertex(w2);


            //swap out old edges for edges pointing to the new neighbors
            w1.Edges.Clear();
            foreach (Vertex neighbor in w1Neighbors)
            {
                coloredGraph.AddEdge(new Edge(w1, neighbor));
            }

            w2.Edges.Clear();
            foreach (Vertex neighbor in w2Neighbors)
            {
                coloredGraph.AddEdge(new Edge(w2, neighbor));
            }

            //populate a list of vertices that represent the neighbors of the selected vertex. w1 and w2 are going to be in this list, so they had to be added back first
            List<Vertex> neighbors = new List<Vertex>();
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

            //figure out which colors are left over
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

            //reconnect selected vertex with its new neighbors
            selectedVertex.Edges.Clear();
            foreach (Vertex neighbor in neighbors)
            {
                coloredGraph.AddEdge(new Edge(selectedVertex, neighbor));
            }

            //graph is colored
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

        //position is halfway between old vertices
        v.Position = w1.Position + (w2.Position - w1.Position) / 2;

        //new neighbors are simply all the neighbors of both vertices
        foreach(Vertex neighbor in w1.Neighbors()){
            AddEdge(new Edge(v, neighbor));
        }

        foreach(Vertex neighbor in w2.Neighbors())
        {
            AddEdge(new Edge(v, neighbor));
        }

        return v;
    }

    //thought this would be more useful, creates a copy of a graph containing only vertices that satisfy "criteria"
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
