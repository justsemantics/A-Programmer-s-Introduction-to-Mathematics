using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Line : MonoBehaviour
{
    [SerializeField]
    LineRenderer LineRenderer;

    public Equation Equation
    {
        get; private set;
    }

    public Vector2 Domain
    {
        get; private set;
    }

    public int Resolution
    {
        get; private set;
    }

    bool initialized = false;

    public void Init(Equation _equation, Vector2 _domain, int _resolution)
    {
        Equation = _equation;
        Domain = _domain;
        Resolution = _resolution;

        initialized = true;
    }

    public void SetColor(Color color)
    {
        LineRenderer.startColor = color;
        LineRenderer.endColor = color;
    }

    public void Show(bool show)
    {
        LineRenderer.enabled = show;
    }

    public void Start()
    {
        
    }

    private void Update()
    {
        if (initialized)
        {
            Vector3[] points = new Vector3[Resolution];

            float dx = (Domain.y - Domain.x) / Resolution;
            
            for(int i = 0; i < Resolution; i++)
            {
                float x = Domain.x + dx * i;
                Vector3 point = new Vector3(x, Equation.Evaluate(x), 0);

                points[i] = point;
            }

            LineRenderer.positionCount = Resolution;

            LineRenderer.SetPositions(points);
        }
    }
}
