using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Suitor: RoundParticipant
{
    public int id;
    public List<Suited> preferences;

    public int currentChoice = 0;

    public Action OnSelected, OnRejected;

    public Action RejectedByAll;

    bool rejectedThisRound = false;

    public void PrepareForRound()
    {
        //if we were rejected, choose a new target to propose to
        if (rejectedThisRound)
        {
            currentChoice++;

            if(currentChoice == preferences.Count)
            {
                RejectedByAll();
            }
        }

        rejectedThisRound = false;
    }
    public void OnRoundStart()
    {
        preferences[currentChoice].AddToProposals(this);
    }

    public void OnRoundEnd()
    {

    }

    public void OnSelectionEnd()
    {

    }

    public void Select()
    {
        OnSelected();
    }

    public void Reject()
    {
        rejectedThisRound = true;
        OnRejected();
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
