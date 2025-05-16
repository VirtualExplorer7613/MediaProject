using UnityEngine;

public class BGMPlayer : MonoBehaviour
{
    [Header("Resources/Sounds 폴더 안의 파일명 (확장자 제외)")]
    public string bgmName = "game_bgm";
    public string effectName;

    private void Start()
    {
        var _ = Managers.Sound;
        Managers.Sound.Play(bgmName, Define.Sound.Bgm);
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            Managers.Sound.Play(effectName, Define.Sound.Effect);
        }
    }
}
