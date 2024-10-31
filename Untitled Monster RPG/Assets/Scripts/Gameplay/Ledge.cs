using UnityEngine;

public class Ledge : MonoBehaviour
{
    [SerializeField] private int xDir;
    [SerializeField] private int yDir;

    private void Awake()
    {
        GetComponent<SpriteRenderer>().enabled = false;
    }

    public bool CanJump(Vector2 moveDir)
    {
        return moveDir.x == xDir && moveDir.y == yDir;
    }

}
