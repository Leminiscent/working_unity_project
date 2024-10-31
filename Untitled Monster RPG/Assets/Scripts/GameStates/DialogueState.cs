using Utils.StateMachine;

public class DialogueState : State<GameController>
{
    public static DialogueState Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }
}
