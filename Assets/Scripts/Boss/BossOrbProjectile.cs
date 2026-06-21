using UnityEngine;

public class BossOrbProjectile : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField]
    private float _speed = 6f;

    [SerializeField]
    private float _lifetime = 5f;

    [SerializeField]
    private int _damage = 40;

    [SerializeField]
    private SpriteRenderer _renderer;

    #endregion

    #region Private Fields

    private Vector2 _direction;
    private bool _hit;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        if (_renderer == null)
            _renderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        Destroy(gameObject, _lifetime);
    }

    private void Update()
    {
        if (_hit) return;
        transform.position += (Vector3)(_direction * (_speed * Time.deltaTime));
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_hit) return;
        if (!other.CompareTag("Player")) return;

        _hit = true;
        GameManager.Instance.PlayerHealth.TakeDamage(_damage);
        Destroy(gameObject);
    }

    #endregion

    #region Public Methods

    public void Init(Vector2 direction)
    {
        _direction = direction.normalized;
        if (_renderer != null)
            _renderer.flipX = _direction.x < 0f;
    }

    #endregion
}
