using UnityEngine;

public class BGMPlayer : MonoBehaviour
{
    [Header("Resources/Sounds ���� ���� ���ϸ� (Ȯ���� ����)")]
    public string bgmName;
    // public string effectName;

    private void Start()
    {
        var _ = Managers.Sound;
        Managers.Sound.Play(bgmName, Define.Sound.Bgm);
    }
}
