using UnityEngine;

public class GlobalNpcInteractButton : MonoBehaviour
{
    //버튼 누르면 플레이어 기준 가장 가까운 NPC에게 상호작용 전달
    public void OnClickInteract()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        TalkableNPC[] npcs = FindObjectsByType<TalkableNPC>(FindObjectsSortMode.None);
        TalkableNPC best = null;
        float bestDist = float.MaxValue;

        Vector3 p = player.transform.position;

        foreach (var npc in npcs)
        {
            if (npc == null) continue;

            float d = Vector3.Distance(npc.transform.position, p);

            // NPC의 interactionDistance 기준 안에 들어온 것만 후보
            if (d <= npc.interactionDistance && d < bestDist)
            {
                bestDist = d;
                best = npc;
            }
        }

        if (best != null)
        {
            best.OnInteractButtonPressed();
        }
        else
        {
            Debug.Log("근처에 상호작용 가능한 NPC가 없습니다.");
        }
    }
}
