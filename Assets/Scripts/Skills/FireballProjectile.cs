using UnityEngine;

public class FireballProjectile : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField]
    private SpriteRenderer _renderer;

    [SerializeField]
    private float _speed = 9f;

    [SerializeField]
    private float _maxLifetime = 3f;

    #endregion

    #region Private Fields

    private Vector2 _direction;
    private float _damage;
    private float _elapsed;
    private bool _hit;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        if (_renderer == null)
            _renderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (_hit)
            return;

        transform.position += (Vector3)(_direction * (_speed * Time.deltaTime));

        _elapsed += Time.deltaTime;
        if (_elapsed >= _maxLifetime)
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_hit)
            return;

        EnemyHealth enemy = other.GetComponent<EnemyHealth>()
            ?? other.GetComponentInParent<EnemyHealth>();

        if (enemy == null)
            return;

        _hit = true;
        enemy.TakeDamage(Mathf.RoundToInt(_damage));
        Destroy(gameObject);
    }

    #endregion

    #region Public Methods

    public void Init(Vector2 direction, float damage)
    {
        _direction = direction.normalized;
        _damage = damage;

        if (_renderer != null)
            _renderer.flipX = direction.x < 0f;
    }

    #endregion
}
