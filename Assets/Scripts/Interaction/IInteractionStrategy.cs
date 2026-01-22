using UnityEngine;

public interface IInteractionStrategy
{
    Sprite Icon { get; }
    float CooldownSeconds { get; }

    // 선택/해제 시 필요한 초기화가 있으면 사용
    void OnSelected(InteractionContext ctx);
    void OnDeselected(InteractionContext ctx);

    // 입력 이벤트
    void OnPress(InteractionContext ctx);
    void OnHold(InteractionContext ctx, float deltaTime);
    void OnRelease(InteractionContext ctx);

    // UI 진행률; 필요 없으면 -1 반환
    float GetProgress01();
}
