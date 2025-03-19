using Utils.StateMachine;

public class CutsceneState : State<GameController>
{
    public static CutsceneState Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    public override void Execute()
    {
        if (PlayerController.Instance != null && PlayerController.Instance.Character != null)
        {
            PlayerController.Instance.Character.UpdateAnimator();
        }
    }
}