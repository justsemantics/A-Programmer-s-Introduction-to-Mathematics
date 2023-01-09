using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VertexUI : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI Text;

    [SerializeField]
    RectTransform rectTransform;

    [SerializeField]
    Image image;

    GraphUI graphUI;

    Vertex v;

    float startingAngle = 0f;
    float minAngle = 30f;

    public void Init(Vertex _v, GraphUI _graphUI)
    {
        v = _v;
        graphUI = _graphUI;

        Text.text = v.Name;

        rectTransform.anchoredPosition = v.Position;

        image.color = v.Color;

        CalculateEdges();
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void CalculateEdges()
    {
        v.Edges.Sort(CompareEdgesByDistance);

        startingAngle = edgeAngle(v.Edges[0]);

        v.Edges.Sort(CompareEdgesByAngle);

        float offsetCW = 0, offsetCCW = 0;

        foreach (Edge e in v.Edges)
        {
            EdgeUI edgeUI = graphUI.Edges[e];

            float targetAngle = signedEdgeAngle(e);

            //angle is positive, i.e. CCW from the closest node
            if(targetAngle >= 0)
            {
                //angle is too small, make it the offset and increment
                if(targetAngle <= offsetCCW)
                {
                    targetAngle = offsetCCW;
                    offsetCCW += minAngle;
                }
                //angle is fine as it is, block off the range up to that angle
                else
                {
                    offsetCCW = targetAngle + minAngle;
                }
            }
            //angle is negative, i.e. CW
            else
            {
                //angle is too "small," not negative enough
                if(targetAngle >= offsetCW)
                {
                    targetAngle = offsetCW;
                    offsetCW -= minAngle;
                }
                //angle is fine, block off the range up to that angle
                else
                {
                    offsetCW = targetAngle - minAngle;
                }
            }

            float controlPointDistance = edgeDistance(e) / 3;
            float theta = (targetAngle + startingAngle);

            Vector3 controlPointPosition = (Vector3)v.Position + new Vector3(Mathf.Cos(theta * Mathf.Deg2Rad) * controlPointDistance, Mathf.Sin(theta * Mathf.Deg2Rad) * controlPointDistance, 0);

            if (e.from == v)
            {
                edgeUI.Control1 = controlPointPosition;
            }
            else
            {
                edgeUI.Control2 = controlPointPosition;
            }

            edgeUI.Refresh();
        }
    }

    int CompareEdgesByAngle(Edge edge1, Edge edge2)
    {
        return CompareEdgesByFloat(edge1, edge2, edgeAngle);
    }

    int CompareEdgesByDistance(Edge edge1, Edge edge2)
    {
        return CompareEdgesByFloat(edge1, edge2, edgeDistance);
    }

    int CompareEdgesByFloat(Edge edge1, Edge edge2, Func<Edge, float> calculate)
    {
        float float1 = calculate(edge1);
        float float2 = calculate(edge2);

        if (float1 < float2)
        {
            return -1;
        }
        else if (float1 > float2)
        {
            return 1;
        }
        else return 0;
    }

    Vector2 vectorAlongEdge(Edge e)
    {
        return e.Traverse(v).Position - v.Position;
    }

    float edgeDistance(Edge e)
    {
        return vectorAlongEdge(e).magnitude;
    }

    float edgeAngle(Edge e)
    {
        Vector2 baseDirection = new Vector2(Mathf.Cos(startingAngle * Mathf.Deg2Rad), Mathf.Sin(startingAngle * Mathf.Deg2Rad));
        return Vector2.Angle(baseDirection, vectorAlongEdge(e)); ;
    }

    float signedEdgeAngle(Edge e)
    {
        Vector2 baseDirection = new Vector2(Mathf.Cos(startingAngle * Mathf.Deg2Rad), Mathf.Sin(startingAngle * Mathf.Deg2Rad));
        return Vector2.SignedAngle(baseDirection, vectorAlongEdge(e));
    }
}