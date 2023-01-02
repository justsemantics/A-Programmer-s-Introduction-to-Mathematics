using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Suited : RoundParticipant
{
    public int id;
    public List<Suitor> preferences;

    public int currentChoice = int.MaxValue;

    public List<Suitor> proposalsThisRound;
    public void PrepareForRound()
    {
        //empty the list so we can acccept new proposals
        proposalsThisRound = new List<Suitor>();
    }

    public void OnRoundStart()
    {
        //suitors are proposing during this time
    }

    public void OnRoundEnd()
    {
        if(proposalsThisRound.Count == 0)
        {
            return;
        }

        //assess proposals and find most preferred suitor
        foreach(Suitor suitor in proposalsThisRound)
        {
            int value = preferences.IndexOf(suitor);
            if(value <= currentChoice)
            {
                currentChoice = value;
            }
        }

        //remove the highest ranked suitor from the list and reject all the others
        proposalsThisRound.Remove(preferences[currentChoice]);

        foreach(Suitor rejectedSuitor in proposalsThisRound)
        {
            rejectedSuitor.Reject();
        }
    }

    public void AddToProposals(Suitor suitor)
    {
        proposalsThisRound.Add(suitor);
    }

    public void OnSelectionEnd()
    {
        Debug.Log(string.Format("Suited {0} chose Suitor {1}. They got preference number {2} and {3}, respectively.",
            id, preferences[currentChoice].id, currentChoice, preferences[currentChoice].currentChoice));
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
