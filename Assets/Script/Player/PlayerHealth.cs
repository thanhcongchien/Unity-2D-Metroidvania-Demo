using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : EntityHealth
{
    private Player player;

    protected override void Awake()
    {
        base.Awake();
        player = GetComponent<Player>();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.N))
            Die();
    }
    protected override void Die()
    {
        base.Die();


        player.ui.SwitchDeathScreen();

        //GameManager.Instance.SetLastPlayerPosition(transform.position);
        //GameManager.Instance.RestartScene();
    }
}
