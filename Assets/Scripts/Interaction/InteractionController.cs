using UnityEngine;
using UnityEngine.UI;

public class InteractionController : MonoBehaviour
{
    [Header("Context")]
    [SerializeField] private Transform player;
    [SerializeField] private Camera cam;
    [SerializeField] private GameObject ultrasoundPrefab;
    [SerializeField] private GameObject magnetVisual;
    [SerializeField] private HammerVisual hammerVisual;
    [SerializeField] private ScissorVisual scissorVisual;
    [SerializeField] private GameObject pullVisual;
    [SerializeField] private GameObject spongeVisual;
    [SerializeField] private LayerMask spongeSurfaceMask;
    [SerializeField] private LayerMask oilStainMask;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private LayerMask trashLayerMask;
    [SerializeField] private ItemUseBroadcaster itemUseBroadcaster;

    [Header("Default Item")]
    [SerializeField] private ItemDefinition defaultItem;

    [Header("UI")]
    [SerializeField] private HoldButton interactionHoldButton;
    [SerializeField] private Image interactionButtonIcon;
    [SerializeField] private Image holdProgressRing; // 진행률

    private InteractionContext ctx;
    public InteractionContext CTX => ctx;
    private IInteractionStrategy currentStrategy;
    private bool isDialogueOpen = false;
    private ItemDefinition currentItemDef;
    public ItemDefinition CurrentItemDef => currentItemDef;
    
    private readonly InteractionStateMachine fsm = new();

    private bool wasHolding = false;
    private bool lastCanAcceptInput = true;
    private bool pressAccepted = false;

    private void Awake()
    {
        ctx = new InteractionContext(player, cam)
        {
            ultrasoundPrefab = ultrasoundPrefab,
            shootPoint = shootPoint,
            trashLayerMask = trashLayerMask,
            magnetVisual = magnetVisual,
            pullVisual = pullVisual,
            itemUseBroadcaster = itemUseBroadcaster,
            hammerVisual = hammerVisual,
            scissorVisual = scissorVisual,
            spongeVisual = spongeVisual,
            spongeSurfaceMask = spongeSurfaceMask,
            oilStainMask = oilStainMask,
        };

        ctx.SetMagnetVisual(false);
        ctx.SetPullVisual(false);
        ctx.hammerVisual?.SetVisible(false);
        ctx.SetSpongeVisual(false);


        if (interactionHoldButton != null)
        {
            interactionHoldButton.onPressed.AddListener(HandlePress);
            interactionHoldButton.onReleased.AddListener(HandleRelease);
        }

        if (holdProgressRing != null)
            holdProgressRing.fillAmount = 0f;
    }

    private void Start()
    {
        // default item이 있으면 설정
        if (defaultItem != null)
        {
            SetItem(defaultItem);
        }
    }

    private void Update()
    {
        float dt = Time.deltaTime;
        fsm.Tick(dt);

        bool canAccept = fsm.CanAcceptInput;
        bool isHolding = interactionHoldButton != null && interactionHoldButton.IsPressed;

        if (canAccept && currentStrategy is SpongeStrategy)
        {
            currentStrategy.OnHold(ctx, dt);
        }
        else if (canAccept && isHolding && currentStrategy != null)
        {
            /*if (!wasHolding || !lastCanAcceptInput)
            {
                // 안전장치: onPressed 이벤트가 누락될 수 있는 상황 대비
                HandlePress();
            }*/

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
        lastCanAcceptInput = canAccept;
    }

    public void SetItem(ItemDefinition def)
    {
        // 기존 전략 해제
        currentStrategy?.OnDeselected(ctx);

        currentItemDef = def;
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
        Debug.Log($"[InteractionController] HandlePress called at frame={Time.frameCount}, time={Time.time}");

        if (!fsm.CanAcceptInput) return;
        if (currentStrategy == null) return;

        pressAccepted = true; // 현재 입력이 유효한지 체크하는 변수
        fsm.EnterActive();
        currentStrategy.OnPress(ctx);
        Debug.Log("Handle Press!!");
        Debug.Log($"[Interaction] HandlePress currentStrategy = {currentStrategy?.GetType().Name ?? "NULL"}");

    }

    public void HandleRelease()
    {
        if (currentStrategy == null) return;

        if (currentStrategy is SpongeStrategy)
            return;

        // 쿨타임 중에 눌렀다가 다시 떼는 경우는 무효처리
        if (!pressAccepted)
        {
            wasHolding = false;
            return;
        }
        Debug.Log("Handle Release!!");
        // Active 상태 종료
        currentStrategy.OnRelease(ctx);

        // 쿨타임 진입
        float cd = currentStrategy.CooldownSeconds;
        fsm.StartCooldown(cd);

        // 진행률 UI 초기화
        if (holdProgressRing != null)
            holdProgressRing.fillAmount = 0f;

        wasHolding = false;
        pressAccepted = false;
    }

    public void OnDialogueOpened()
    {
        isDialogueOpen = true;

        if (currentStrategy is HammerStrategy)
            hammerVisual?.SetVisible(false);
    }

    public void OnDialogueClosed()
    {
        isDialogueOpen = false;

        if (currentStrategy is HammerStrategy)
            hammerVisual?.SetVisible(true);
    }
}
