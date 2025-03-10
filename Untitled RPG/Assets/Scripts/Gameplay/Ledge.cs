using UnityEngine;

public class Ledge : MonoBehaviour
{
    [SerializeField] private int _xDir;
    [SerializeField] private int _yDir;

    private void Awake()
    {
        GetComponent<SpriteRenderer>().enabled = false;
    }

    public bool CanJump(Vector2 moveDir)
    {
        return moveDir.x == _xDir && moveDir.y == _yDir;
    }

}
