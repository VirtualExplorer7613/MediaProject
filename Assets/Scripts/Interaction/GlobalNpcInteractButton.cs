using System.Collections;
using UnityEngine;

public class GlobalNpcInteractButton : MonoBehaviour
{
    [SerializeField] private string clickSfx = "SFX/Click";
    //버튼 누르면 플레이어 기준 가장 가까운 NPC에게 상호작용 전달

    [Header("Tutorial Gate")]
    [SerializeField] private TutorialRunner tutorialRunner;
    [SerializeField] private bool lockUntilTutorialStep = true;
    [SerializeField] private int requiredTutorialStepIndex = 0;

    [Tooltip("한 번 정상적으로 NPC 대화를 시작하면 이후엔 튜토리얼 검사 없이 항상 허용")]
    [SerializeField] private bool unlockForeverAfterFirstTalk = true;

    [Header("Timing")]
    [SerializeField] private float firstTalkDelay = 0.05f;

    private bool tutorialGateReleased = false;
    private bool pendingTalk = false;

    public void OnClickInteract()
    {
        if (pendingTalk) return;

        if (!CanUseNpcTalkButton())
        {
            Debug.Log("[NPC Talk Button] 아직 이 튜토리얼 단계에서는 사용할 수 없습니다.");
            return;
        }

        Managers.Sound.Play(clickSfx);
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        TalkableNPC[] npcs = FindObjectsByType<TalkableNPC>(FindObjectsSortMode.None);
        TalkableNPC best = null;
        float bestDist = float.MaxValue;

        Vector3 p = player.transform.position;

        foreach (var npc in npcs)
        {
            if (npc == null) continue;

            /*loat d = Vector3.Distance(npc.transform.position, p);*/
            float d3 = Vector3.Distance(npc.transform.position, p);
            float dy = Mathf.Abs(npc.transform.position.y - p.y);

            Vector2 npcXZ = new Vector2(npc.transform.position.x, npc.transform.position.z);
            Vector2 playerXZ = new Vector2(p.x, p.z);
            float d2 = Vector2.Distance(npcXZ, playerXZ);

            // NPC의 interactionDistance 기준 안에 들어온 것만 후보
            if (d3 <= npc.interactionDistance && d3 < bestDist)
            {
                bestDist = d3;
                best = npc;
            }
        }

        if (best == null)
        {
            Debug.Log("근처에 상호작용 가능한 NPC가 없습니다.");
            return;
        }

        bool firstTutorialUnlockClick =
            lockUntilTutorialStep &&
            !tutorialGateReleased &&
            tutorialRunner != null &&
            tutorialRunner.IsCurrentStep(requiredTutorialStepIndex);

        if (firstTutorialUnlockClick)
        {
            if (unlockForeverAfterFirstTalk)
                tutorialGateReleased = true;

            StartCoroutine(DelayedTalk(best));
            return;
        }

        best.OnInteractButtonPressed();

        if (unlockForeverAfterFirstTalk)
            tutorialGateReleased = true;


        /*if (best != null)
        {
            best.OnInteractButtonPressed();

            if (unlockForeverAfterFirstTalk)
            {
                tutorialGateReleased = true;
            }
        }
        else
        {
            Debug.Log("근처에 상호작용 가능한 NPC가 없습니다.");
        }*/
    }

    private bool CanUseNpcTalkButton()
    {
        if (!lockUntilTutorialStep)
            return true;

        if (tutorialGateReleased)
            return true;

        if (tutorialRunner == null)
        {
            tutorialRunner = FindFirstObjectByType<TutorialRunner>();
            if (tutorialRunner == null)
            {
                Debug.LogWarning("[NPC Talk Button] TutorialRunner를 찾지 못했습니다.");
                return false;
            }
        }

        return tutorialRunner.IsCurrentStep(requiredTutorialStepIndex)
            || tutorialRunner.IsStepOrLater(requiredTutorialStepIndex);
    }

    private IEnumerator DelayedTalk(TalkableNPC npc)
    {
        pendingTalk = true;

        yield return null; // 다음 프레임까지 대기
        yield return new WaitForSecondsRealtime(firstTalkDelay);

        if (npc != null)
            npc.OnInteractButtonPressed();

        pendingTalk = false;
    }
}
