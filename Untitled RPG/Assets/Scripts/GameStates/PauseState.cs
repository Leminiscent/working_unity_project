using Utils.StateMachine;

public class PauseState : State<GameController>
{
    public static PauseState Instance { get; private set; }

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