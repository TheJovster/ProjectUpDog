using UnityEngine;

/// Restores one damage state on contact, then disables itself (GDD §18).
[RequireComponent(typeof(Collider2D))]
public class BalloonRepairPickup : MonoBehaviour
{
    [SerializeField] private int _healAmount = 1;

    [Tooltip("Colour/variant id for later art. Gameplay ignores it.")]
    [SerializeField] private int _variantId = 0;

    private GameManager _game;

    public int VariantId => _variantId;

    private void Start() => _game = ServiceRegistry.Instance.Get<GameManager>();

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_game == null) return;
        if (other.GetComponent<PlayerBalloonController>() == null) return;

        _game.Heal(_healAmount);

        // Disabled rather than destroyed so respawn can put it back.
        _game.RegisterConsumed(gameObject);
        gameObject.SetActive(false);
    }
}