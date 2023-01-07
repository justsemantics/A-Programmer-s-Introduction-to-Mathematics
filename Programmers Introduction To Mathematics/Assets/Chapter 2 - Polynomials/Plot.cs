using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plot : MonoBehaviour
{
    private int currentID = 0;

    [SerializeField]
    float xMin, xMax, yMin, yMax;

    [SerializeField]
    float xScale, yScale;

    [SerializeField]
    int resolution;

    [SerializeField]
    Line linePrefab;

    Dictionary<int, Line> Lines = new Dictionary<int, Line>();

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ShowEquation(int equationID, bool show)
    {
        Line line;

        if (Lines.TryGetValue(equationID, out line))
        {
            line.Show(show);
        }
    }

    public int AddEquation(Equation equation, bool show = true)
    {
        int id = currentID;
        currentID++;

        Line line = Instantiate<Line>(linePrefab, transform, false);
        line.transform.localScale = new Vector3(xScale, yScale, 1);
        line.Init(equation, new Vector2(xMin, xMax), resolution);
        line.Show(show);

        Lines.Add(id, line);

        return id;
    }

    public int[] AddEquations(Equation[] equations, bool show = true)
    {
        int[] ids = new int[equations.Length];

        for (int i = 0; i < equations.Length; i++)
        {
            ids[i] = AddEquation(equations[i], show);
        }

        return ids;
    }

    public void RemoveEquations(int[] equationIDs)
    {
        foreach (int id in equationIDs)
        {
            RemoveEquation(id);
        }
    }

    public int AddEquation(Equation equation, Color color)
    {
        int id = this.AddEquation(equation);
        this.SetColor(id, color);

        return id;
    }

    public void SetColor(int equationID, Color color)
    {
        Line line;
        if (Lines.TryGetValue(equationID, out line))
        {
            line.SetColor(color);
        }
    }

    public void RemoveEquation(int equationID)
    {
        Line line;
        if (Lines.TryGetValue(equationID, out line))
        {
            Lines.Remove(equationID);
            Destroy(line);
        }
    }
}
