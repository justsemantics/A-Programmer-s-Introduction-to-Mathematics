using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Tests : MonoBehaviour
{

    [SerializeField]
    Graph graph;

    [SerializeField]
    Transform[] targets;

    EquationWrapper[] equations;

    int[] equationIDs;

    Polynomial basePolynomial;

    Polynomial solution;

    bool updating = true;

    // Start is called before the first frame update
    void Start()
    {
        RefreshGraph();
        AnimateGraph();
    }

    void AnimateGraph()
    {
        updating = false;
        System.Action nextStep = () => {
            updating = true;
            Debug.Log("EOL"); 
        };


        System.Action firstStep = () => { Debug.Log("First Step, should not see this lol"); };

        System.Action[] displayActions = new System.Action[targets.Length * 3 + 1];

        displayActions[displayActions.Length - 1] = nextStep;

        int baseEquation = equations.Length - 1;

        for (int i = targets.Length - 1; i >= 0; i--)
        {
            //id of actions in displayActions array
            int step1ID = i * 3;
            int step2ID = i * 3 + 1;
            int step3ID = i * 3 + 2;

            int nextStep1ID = (i + 1) * 3;

            //id of equations in graph
            int equationFromRoots = step1ID;
            int equationFromRootsID = equationIDs[step1ID];
            int equationAfterScaling = step2ID;
            int equationAfterScalingID = equationIDs[step2ID];
            int displayEquation = step3ID;
            int displayEquationID = equationIDs[step3ID];

            //a random color
            UnityEngine.Color color = Random.ColorHSV();

            //steps are in reverse order because each step needs to refer to its "following" step

            //Last step for this target, set the equation after scaling to the random color and hide the lerping display equation
            System.Action step3 = () =>
            {
                graph.SetColor(equationAfterScalingID, color);
                graph.ShowEquation(displayEquationID, false);

                displayActions[nextStep1ID]();
            };

            displayActions[step3ID] = step3;

            //Second step for this target, set the equation after scaling to be visible and lerp towards it from the equation from roots
            System.Action step2 = () =>
            {
                graph.ShowEquation(equationFromRootsID, false);

                graph.SetColor(equationAfterScalingID, new UnityEngine.Color(0, 0, 0, 0.2f));
                graph.ShowEquation(equationAfterScalingID, true);

                StartCoroutine(LerpPolynomial(equationFromRoots, equationAfterScaling, displayEquation, 3, displayActions[step3ID]));
            };

            displayActions[step2ID] = step2;


            //First step for this target, show the equation from roots and lerp towards it from zero
            System.Action step1 = () =>
            {
                graph.SetColor(equationFromRootsID, new UnityEngine.Color(0, 0, 0, 0.2f));
                graph.ShowEquation(equationFromRootsID, true);

                graph.SetColor(displayEquationID, color);
                graph.ShowEquation(displayEquationID, true);

                StartCoroutine(LerpPolynomial(baseEquation, equationFromRoots, displayEquation, 3, displayActions[step2ID]));
            };

            displayActions[step1ID] = step1;


            if (step1ID == 0)
            {
                firstStep = step1;
            }
        }

        graph.ShowEquation(equationIDs[equationIDs.Length - 1], false);

        firstStep();
    }

    void RefreshGraph()
    {
        if(equationIDs != null)
        {
            graph.RemoveEquations(equationIDs);
        }

        equations = new EquationWrapper[targets.Length * 3 + 2];
        equationIDs = new int[targets.Length * 3 + 1];

        float[] baseCoefficients = new float[targets.Length];
        basePolynomial = new Polynomial(baseCoefficients, false);
        EquationWrapper ew0 = new EquationWrapper();
        ew0.equation = basePolynomial;
        equations[equations.Length - 1] = ew0;

        EquationWrapper ew = new EquationWrapper();
        solution = new Polynomial();
        ew.equation = solution;
        equations[equations.Length - 2] = ew;
        


        Vector2[] targetPositions = new Vector2[targets.Length];
        for(int currentTarget = 0; currentTarget < targets.Length; currentTarget++)
        {
            targetPositions[currentTarget] = targets[currentTarget].localPosition;
        }

        for(int currentTarget = 0; currentTarget < targets.Length; currentTarget++)
        {

            //roots are the x positions of targets other than the current target
            float[] roots = new float[targets.Length - 1];
            int rootIndex = 0;
            for (int i = 0; i < targets.Length; i++)
            {
                if (i != currentTarget)
                {
                    roots[rootIndex] = targetPositions[i].x;
                    rootIndex++;
                }
            }

            //set up indexing into the equation array
            int currentEquationIndex = currentTarget * 3;


            //first equation is simply a polynomial with roots at the target locations
            Polynomial equationFromRoots = Polynomial.FromRoots(roots);

            EquationWrapper ew2 = new EquationWrapper();
            ew2.equation = equationFromRoots;

            equations[currentEquationIndex] = ew2;
            equationIDs[currentEquationIndex] = graph.AddEquation(ew2, false);


            //second equation is the first one, scaled so that the result at x = currentTarget.x is equal to currentTarget.y
            float divisor = 1;
            foreach (float x in roots)
            {
                divisor *= (targetPositions[currentTarget].x - x);
            }

            Polynomial equationAfterScaling = (equationFromRoots / divisor) * targetPositions[currentTarget].y;

            EquationWrapper ew3 = new EquationWrapper();
            ew3.equation = equationAfterScaling;

            equations[currentEquationIndex + 1] = ew3;
            equationIDs[currentEquationIndex + 1] = graph.AddEquation(ew3, false);

            solution += equationAfterScaling;

            //third equation is going to be used for display purposes
            Polynomial displayEquation = basePolynomial.CloneViaSerialization();

            EquationWrapper ew4 = new EquationWrapper();
            ew4.equation = displayEquation;

            equations[currentEquationIndex + 2] = ew4;
            equationIDs[currentEquationIndex + 2] = graph.AddEquation(ew4, false);
        }

        equationIDs[equationIDs.Length - 1] = graph.AddEquation(ew);
    }

    void UpdateGraph()
    {
        solution = new Polynomial();
        Vector2[] targetPositions = new Vector2[targets.Length];
        for (int currentTarget = 0; currentTarget < targets.Length; currentTarget++)
        {
            targetPositions[currentTarget] = targets[currentTarget].localPosition;
        }

        for (int currentTarget = 0; currentTarget < targets.Length; currentTarget++)
        {

            //roots are the x positions of targets other than the current target
            float[] roots = new float[targets.Length - 1];
            int rootIndex = 0;
            for (int i = 0; i < targets.Length; i++)
            {
                if (i != currentTarget)
                {
                    roots[rootIndex] = targetPositions[i].x;
                    rootIndex++;
                }
            }

            //set up indexing into the equation array
            int currentEquationIndex = currentTarget * 3;


            //first equation is simply a polynomial with roots at the target locations
            Polynomial equationFromRoots = Polynomial.FromRoots(roots);

            equations[currentEquationIndex].equation = equationFromRoots;

            //second equation is the first one, scaled so that the result at x = currentTarget.x is equal to currentTarget.y
            float divisor = 1;
            foreach (float x in roots)
            {
                divisor *= (targetPositions[currentTarget].x - x);
            }

            Polynomial equationAfterScaling = (equationFromRoots / divisor) * targetPositions[currentTarget].y;

            equations[currentEquationIndex + 1].equation = equationAfterScaling;

            solution += equationAfterScaling;

        }

        equations[equations.Length - 1].equation = solution;
    }

    IEnumerator LerpPolynomial(int fromID, int toID, int outputID, float duration, System.Action callback = null)
    {
        float timeElapsed = 0;

        while(timeElapsed <= duration)
        {
            float progress = timeElapsed / duration;

            equations[outputID].equation = Polynomial.Interpolate(equations[fromID].equation, equations[toID].equation, progress);

            Debug.Log(equations[outputID].equation.ToString());

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        if(callback != null)
        {
            callback();
        }
    }

    Polynomial DrawEquationSet(int selectedTarget, UnityEngine.Color color)
    {
        Vector2[] targetPositions = new Vector2[targets.Length];
        float[] roots = new float[targets.Length - 1];
        int rootIndex = 0;
        for (int i = 0; i < targets.Length; i++)
        {
            targetPositions[i] = targets[i].localPosition;

            if(i != selectedTarget)
            {
                roots[rootIndex] = targetPositions[i].x;
                rootIndex++;
            }
        }

        Polynomial a = Polynomial.FromRoots(roots);

        float divisor = 1;
        foreach (float x in roots)
        {
            divisor *= (targetPositions[selectedTarget].x - x);
        }
        Polynomial b = a / divisor;
        Polynomial c = b * targetPositions[selectedTarget].y;

        graph.AddEquation(a, color);
        graph.AddEquation(b, color);
        graph.AddEquation(c, color);

        return c;
    }

    void FindSolution()
    {
        Vector2[] targetPositions = new Vector2[targets.Length];

        for(int i = 0; i < targets.Length; i++)
        {
            targetPositions[i] = targets[i].localPosition;
        }

        
    }

    // Update is called once per frame
    void Update()
    {
        if (updating)
        {
            UpdateGraph();
        }
    }
}
