using UnityEngine;

public class InteractionStateMachine
{
    public enum State
    {
        Idle,
        Active,
        Cooldown,
        Disabled
    }

    public State Current { get; private set; } = State.Idle;

    private float cooldownRemaining = 0f;

    public bool CanAcceptInput =>
        Current == State.Idle || Current == State.Active;

    public void Tick(float dt)
    {
        if (Current == State.Cooldown)
        {
            cooldownRemaining -= dt;
            if (cooldownRemaining <= 0f)
            {
                cooldownRemaining = 0f;
                Current = State.Idle;
            }
        }
    }

    public void EnterActive()
    {
        if (Current == State.Cooldown || Current == State.Disabled) return;
        Current = State.Active;
    }

    public void ExitActiveToIdle()
    {
        if (Current == State.Active) Current = State.Idle;
    }

    public void StartCooldown(float seconds)
    {
        if (seconds <= 0f)
        {
            Current = State.Idle;
            return;
        }
        cooldownRemaining = seconds;
        Current = State.Cooldown;
    }
}
