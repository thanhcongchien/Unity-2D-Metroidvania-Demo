using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine 
{
    public EntityState currentState { get; private set; }
    public bool canChangeState = true;


    public void Initialize(EntityState startState)
    {
        if (currentState != null || startState == null)
            return;

        currentState = startState;
        currentState.Enter();
    }

    public void ChangeState(EntityState newState)
    {
        if (canChangeState == false || newState == null)
        {
            return;
        }

        if (currentState == newState)
            return;

        if (currentState != null)
            currentState.Exit();

        currentState = newState;
        currentState.Enter();
    }

    public void UpdateActiveState()
    {
        if (currentState == null)
            return;

        currentState.Update();
    }
    public void SwitchOffStateMachine()=>canChangeState = false;


}
