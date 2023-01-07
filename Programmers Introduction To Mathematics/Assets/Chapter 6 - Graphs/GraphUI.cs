using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphUI : MonoBehaviour
{
    [SerializeField]
    RectTransform Vertices, Edges;

    [SerializeField]
    VertexUI vertexUIPrefab;

    [SerializeField]
    EdgeUI edgeUIPrefab;


    public void DrawGraph(Graph graph)
    {
        foreach(Vertex v in graph.Vertices)
        {
            VertexUI vertexUI = Instantiate<VertexUI>(vertexUIPrefab, Vertices, false);
        }

        foreach(Edge e in graph.Edges)
        {
            EdgeUI edgeUI = Instantiate<EdgeUI>(edgeUIPrefab, Edges, false);
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
