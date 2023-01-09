using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FiveColorGraph : MonoBehaviour
{
    [SerializeField]
    Transform[] vertexPositions;

    [SerializeField]
    Vector2Int[] edges;

    [SerializeField]
    GraphUI graphUIPrefab;

    List<GraphUI> graphUIs = new List<GraphUI>();

    List<Graph> graphs = new List<Graph>();

    Vector3 offset = new Vector3(-10, 0, 0);

    // Start is called before the first frame update
    void Start()
    {
        Graph startingGraph = new Graph();

        foreach(Transform t in vertexPositions)
        {
            Vertex v = new Vertex();
            v.Position = t.position;
            startingGraph.AddVertex(v);
        }

        foreach(Vector2Int index in edges)
        {
            startingGraph.AddEdge(new Edge(startingGraph.Vertices[index.x], startingGraph.Vertices[index.y]));
        }

        Graph currentGraph = startingGraph;
        currentGraph = currentGraph.PlanarFiveColor();
        Debug.Log("wow");

        GraphUI graphUI = Instantiate(graphUIPrefab);
        graphUI.DrawGraph(currentGraph);
        graphUIs.Add(graphUI);
        /*
        while(currentGraph.Vertices.Count > 5)
        {
            foreach(GraphUI g in graphUIs)
            {
                g.transform.position += offset;
            }

            GraphUI graphUI = Instantiate(graphUIPrefab);
            graphUI.DrawGraph(currentGraph);
            graphUIs.Add(graphUI);

            currentGraph = currentGraph.PlanarFiveColor();
        }
        */
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
