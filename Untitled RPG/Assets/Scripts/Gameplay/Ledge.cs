using UnityEngine;

public class Ledge : MonoBehaviour
{
    [SerializeField] private int _xDir;
    [SerializeField] private int _yDir;

    private SpriteRenderer _spriteRenderer;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_spriteRenderer != null)
        {
            _spriteRenderer.enabled = false;
        }
    }

    public bool CanJump(Vector2 moveDir)
    {
        return moveDir.x == _xDir && moveDir.y == _yDir;
    }
}