using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StableMarriages : MonoBehaviour
{
    System.Action BeforeRound;
    System.Action StartRound;
    System.Action EndRound;
    System.Action EndSelection;

    List<Suitor> suitors;
    List<Suited> suiteds;
    List<Suitor> unassignedSuitors;

    [SerializeField]
    int numPairs = 10;

    int numSuitors, numSuiteds;

    // Start is called before the first frame update
    void Start()
    {
        CreateParticipants();


        do
        {
            unassignedSuitors = new List<Suitor>();

            BeforeRound();
            StartRound();
            EndRound();
        } while (unassignedSuitors.Count > 0);

        EndSelection();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void CreateParticipants()
    {
        suitors = new List<Suitor>();
        suiteds = new List<Suited>();
        for (int i = 0; i < numPairs; i++)
        {
            Suitor suitor = new Suitor();
            AddSuitor(suitor);

            Suited suited = new Suited();
            AddSuited(suited);
        }

        foreach(Suitor suitor in suitors)
        {
            List<Suited> preferences = new List<Suited>(suiteds);
            preferences.Shuffle();
            suitor.preferences = preferences;
        }

        foreach(Suited suited in suiteds)
        {
            List<Suitor> preferences = new List<Suitor>(suitors);
            preferences.Shuffle();
            suited.preferences = preferences;
        }
    }

    void SuitorSelected(Suitor suitor)
    {
        if (unassignedSuitors.Contains(suitor))
        {
            unassignedSuitors.Remove(suitor);
        }
    }

    void SuitorRejected(Suitor suitor)
    {
        unassignedSuitors.Add(suitor);
    }

    void AddSuitor(Suitor suitor)
    {
        BeforeRound += suitor.PrepareForRound;
        StartRound += suitor.OnRoundStart;
        EndRound += suitor.OnRoundEnd;


        suitor.OnSelected = () => { SuitorSelected(suitor); };
        suitor.OnRejected = () => { SuitorRejected(suitor); };
        suitor.RejectedByAll = () => { SuitorRejectedByAll(suitor); };

        suitor.id = numSuitors;
        numSuitors++;

        suitors.Add(suitor);
    }

    void AddSuited(Suited suited)
    {
        BeforeRound += suited.PrepareForRound;
        StartRound += suited.OnRoundStart;
        EndRound += suited.OnRoundEnd;
        EndSelection += suited.OnSelectionEnd;

        suited.id = numSuiteds;
        numSuiteds++;

        suiteds.Add(suited);
    }

    void SuitorRejectedByAll(Suitor suitor)
    {
        Debug.Log("OUCH");
    }



}
static class ListExtension
{
    //from https://stackoverflow.com/questions/273313/randomize-a-listt
    private static System.Random rng = new System.Random();
    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}

public interface RoundParticipant
{
    public void PrepareForRound();
    public void OnRoundStart();
    public void OnRoundEnd();

    public void OnSelectionEnd();
}
