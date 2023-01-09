using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EdgeUI : MonoBehaviour
{
    [SerializeField]
    LineRenderer lineRenderer;

    [SerializeField]
    Transform control1Transform, control2Transform;

    public Vector3 
        StartPoint = Vector3.zero, 
        EndPoint = Vector3.zero, 
        Control1 = Vector3.zero, 
        Control2 = Vector3.zero;
    public int Resolution = 20;

    public void Init(Vector3 start, Vector3 end, int resolution)
    {
        StartPoint = start;
        EndPoint = end;
        Resolution = resolution;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void Refresh()
    {
        control1Transform.position = Control1;
        control2Transform.position = Control2;
        lineRenderer.positionCount = Resolution + 1;

        Vector3[] positions = new Vector3[Resolution + 1];

        for (int i = 0; i <= Resolution; i++)
        {
            float t = i / (float)Resolution;

            positions[i] = Bezier(StartPoint, EndPoint, Control1, Control2, t);
        }

        lineRenderer.SetPositions(positions);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Vector3 Bezier(Vector3 start, Vector3 end, Vector3 control1, Vector3 control2, float t)
    {
        List<Vector3> points = new List<Vector3>() { start, control1, control2, end };

        do
        {
            points = LerpStep(points, t);
        }while(points.Count > 1);

        return points[0];
    }

    public List<Vector3> LerpStep(List<Vector3> points, float t)
    {
        List<Vector3> newPoints = new List<Vector3>();
        Vector3 previousPoint = points[0];

        for(int i = 1; i < points.Count; i++)
        {
            newPoints.Add(Vector3.Lerp(previousPoint, points[i], t));

            previousPoint = points[i];
        }

        return newPoints;
    }
}
