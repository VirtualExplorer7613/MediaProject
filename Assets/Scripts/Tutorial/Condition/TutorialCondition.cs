using System;
using UnityEngine;

public abstract class TutorialCondition : MonoBehaviour
{
    public event Action Completed;

    protected bool isActive;
    protected bool isCompleted;

    public virtual void Activate(TutorialContext context)
    {
        isActive = true;
        isCompleted = false;
    }

    public virtual void Deactivate()
    {
        isActive = false;
    }

    protected void Complete()
    {
        if (!isActive || isCompleted) return;
        isCompleted = true;
        Completed?.Invoke();
    }
}
