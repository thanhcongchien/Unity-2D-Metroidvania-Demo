using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamagable
{
    public bool takeDamage(float damage,float elementalDamage,ElementType element, Transform damageDealer);


}
