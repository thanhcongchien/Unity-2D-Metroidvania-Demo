using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallDeathArea : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerHealth playerHealth = collision.GetComponentInParent<PlayerHealth>();
        if (playerHealth == null)
            return;

        playerHealth.ForceDie();
    }
}
