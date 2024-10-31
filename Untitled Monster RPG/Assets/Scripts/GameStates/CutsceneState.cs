using Utils.StateMachine;

public class CutsceneState : State<GameController>
{
    public static CutsceneState Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public override void Execute()
    {
        PlayerController.Instance.Character.HandleUpdate();
    }
}
