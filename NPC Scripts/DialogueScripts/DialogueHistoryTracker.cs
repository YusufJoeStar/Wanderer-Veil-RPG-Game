using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueHistoryTracker : MonoBehaviour
{
   
    private readonly HashSet<ActorSO> spokenNPCs = new HashSet<ActorSO>();



    public void RecordNPC(ActorSO actorSO)
    {
        if (actorSO != null && !spokenNPCs.Contains(actorSO))
            spokenNPCs.Add(actorSO);
    }

    public bool HasSpokenWith(ActorSO actorSO)
    {
        return actorSO != null && spokenNPCs.Contains(actorSO);
    }
}