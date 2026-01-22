using UnityEngine;
using UnityEngine.UI;

public class InteractionController : MonoBehaviour
{
    [Header("Context")]
    [SerializeField] private Transform player;
    [SerializeField] private Camera cam;
    [SerializeField] private GameObject ultrasoundPrefab;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private LayerMask trashLayerMask;

    [Header("UI")]
    [SerializeField] private HoldButton interactionHoldButton;
    [SerializeField] private Image interactionButtonIcon;
    [SerializeField] private Image holdProgressRing; // 진행률

    private InteractionContext ctx;
    private IInteractionStrategy currentStrategy;
    private readonly InteractionStateMachine fsm = new();

    private bool wasHolding = false;

    private void Awake()
    {
        ctx = new InteractionContext(player, cam)
        {
            ultrasoundPrefab = ultrasoundPrefab,
            shootPoint = shootPoint,
            trashLayerMask = trashLayerMask
        };

        if (interactionHoldButton != null)
        {
            interactionHoldButton.onPressed.AddListener(HandlePress);
            interactionHoldButton.onReleased.AddListener(HandleRelease);
        }

        if (holdProgressRing != null)
            holdProgressRing.fillAmount = 0f;
    }

    private void Update()
    {
        float dt = Time.deltaTime;
        fsm.Tick(dt);

        bool isHolding = interactionHoldButton != null && interactionHoldButton.IsPressed;

        // Hold 처리
        if (fsm.CanAcceptInput && isHolding && currentStrategy != null)
        {
            if (!wasHolding)
            {
                // 안전장치: onPressed 이벤트가 누락될 수 있는 상황 대비
                HandlePress();
            }

            fsm.EnterActive();
            currentStrategy.OnHold(ctx, dt);
        }

        // 진행률 UI
        if (holdProgressRing != null && currentStrategy != null)
        {
            float p = currentStrategy.GetProgress01();
            holdProgressRing.fillAmount = (p < 0f) ? 0f : p;
        }

        wasHolding = isHolding;
    }

    public void SetItem(ItemDefinition def)
    {
        // 기존 전략 해제
        currentStrategy?.OnDeselected(ctx);

        currentStrategy = StrategyFactory.Create(def);
        currentStrategy.OnSelected(ctx);

        // 아이콘 갱신
        if (interactionButtonIcon != null)
            interactionButtonIcon.sprite = currentStrategy.Icon;

        // 진행률 초기화
        if (holdProgressRing != null)
            holdProgressRing.fillAmount = 0f;

    }

    public void HandlePress()
    {
        if (!fsm.CanAcceptInput) return;
        if (currentStrategy == null) return;

        fsm.EnterActive();
        currentStrategy.OnPress(ctx);
    }

    public void HandleRelease()
    {
        if (currentStrategy == null) return;

        // Active 상태 종료
        currentStrategy.OnRelease(ctx);

        // 쿨타임 진입
        float cd = currentStrategy.CooldownSeconds;
        fsm.StartCooldown(cd);

        // 진행률 UI 초기화
        if (holdProgressRing != null)
            holdProgressRing.fillAmount = 0f;

        wasHolding = false;
    }
}
