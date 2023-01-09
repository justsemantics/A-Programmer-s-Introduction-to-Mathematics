using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphUI : MonoBehaviour
{
    [SerializeField]
    RectTransform VertexParent, EdgeParent;

    [SerializeField]
    VertexUI vertexUIPrefab;

    [SerializeField]
    EdgeUI edgeUIPrefab;

    public Dictionary<Edge, EdgeUI> Edges = new Dictionary<Edge, EdgeUI>();


    public void DrawGraph(Graph graph)
    {
        foreach(Edge e in graph.Edges)
        {
            EdgeUI edgeUI = Instantiate<EdgeUI>(edgeUIPrefab, EdgeParent, false);
            edgeUI.Init(e.from.Position, e.to.Position, 20);
            Edges.Add(e, edgeUI);
        }

        foreach(Vertex v in graph.Vertices)
        {
            VertexUI vertexUI = Instantiate<VertexUI>(vertexUIPrefab, VertexParent, false);
            vertexUI.Init(v, this);
        }

    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
