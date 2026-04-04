using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : EntityCombat
{
    [SerializeField] private LayerMask whatIsCounterable;
   public bool CounterAttackPeformed()
    {
        bool hasCountered=false;

        foreach(var target in getDetectedColliders(whatIsCounterable))
        {
            ICounterable counterable=target.GetComponent<ICounterable>();
             

            if (counterable == null)
                continue;

           if(counterable.CanBeCountered)
            {
                counterable.HandleCounter();
                hasCountered=true;
            }

        }

        return hasCountered;
    }
}
