using UnityEngine;

public class BGMPlayer : MonoBehaviour
{
    [Header("Resources/Sounds 폴더 안의 파일명 (확장자 제외)")]
    public string bgmName;
    // public string effectName;

    private void Start()
    {
        var _ = Managers.Sound;
        Managers.Sound.Play(bgmName, Define.Sound.Bgm);
    }
}
