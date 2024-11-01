using Utils.StateMachine;

public class DialogueState : State<GameController>
{
    public static DialogueState Instance { get; private set; }

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
}
