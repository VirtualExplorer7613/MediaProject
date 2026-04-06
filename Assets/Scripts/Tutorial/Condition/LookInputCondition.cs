using UnityEngine;

public class LookInputCondition : TutorialCondition
{
    [SerializeField] private float requiredLookAmount = 150f;

    private float accumulated;

    public override void Activate(TutorialContext context)
    {
        base.Activate(context);
        accumulated = 0f;
    }

    private void Update()
    {
        if (!isActive || isCompleted) return;

/*#if UNITY_EDITOR || UNITY_STANDALONE
        float mouseX = Mathf.Abs(Input.GetAxis("Mouse X"));
        float mouseY = Mathf.Abs(Input.GetAxis("Mouse Y"));
        accumulated += (mouseX + mouseY) * 100f * Time.unscaledDeltaTime;
#endif*/

        if (Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Moved)
                accumulated += t.deltaPosition.magnitude;
        }

        if (accumulated >= requiredLookAmount)
            Complete();
    }
}