using UnityEngine;

/// Damages the player on contact (GDD §16). Works as trigger or collision.
public class Hazard : MonoBehaviour
{
    [SerializeField] private int _damage = 1;

    [Tooltip("Disable this hazard after it lands a hit (one-shot spikes etc).")]
    [SerializeField] private bool _disableOnHit = false;

    private GameManager _game;

    private void Start() => _game = ServiceRegistry.Instance.Get<GameManager>();

    private void OnTriggerEnter2D(Collider2D other) => TryHit(other);
    private void OnCollisionEnter2D(Collision2D collision) => TryHit(collision.collider);

    private void TryHit(Collider2D other)
    {
        if (_game == null) return;
        if (other.GetComponent<PlayerBalloonController>() == null) return;

        _game.Damage(_damage);
        if (_disableOnHit) gameObject.SetActive(false);
    }
}