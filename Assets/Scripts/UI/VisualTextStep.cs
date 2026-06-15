using UnityEngine;

public enum EpilogueStepLayout
{
    Illustration, // 일러스트 + 설명
    Dialogue,     // 일러스트 없는 짧은 대화
    Monologue     // 일러스트 없는 긴 독백
}


[System.Serializable]
public class VisualTextStep
{
    public EpilogueStepLayout layout = EpilogueStepLayout.Illustration;

    public Sprite image;

    [TextArea(2, 5)]
    public string text;

    [Tooltip("-1이면 Epilogue의 기본 최소 대기 시간을 사용")]
    public float minReadTime = -1f;
}