using Utils.StateMachine;

public class PauseState : State<GameController>
{
    public static PauseState Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }
}
