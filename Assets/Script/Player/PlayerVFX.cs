using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerVFX : EntityVFX
{
   
    public void CreateEffectOf(GameObject effect, Transform target)
    {

        Instantiate(effect, target.position, Quaternion.identity);
    }
}
