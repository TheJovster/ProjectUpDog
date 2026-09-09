using UnityEngine;

/// Stores the latest checkpoint when touched (GDD §19).
[RequireComponent(typeof(Collider2D))]
public class Checkpoint : MonoBehaviour
{
    [Tooltip("Where the player respawns. Leave empty to use this object's position.")]
    [SerializeField] private Transform _respawnPoint;

    private GameManager _game;
    private bool _claimed;

    private void Start() => _game = ServiceRegistry.Instance.Get<GameManager>();

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_claimed || _game == null) return;
        if (other.GetComponent<PlayerBalloonController>() == null) return;

        // Respawn at the checkpoint itself, not wherever the player happened to
        // clip the trigger — keeps restores predictable.
        Vector3 point = _respawnPoint != null ? _respawnPoint.position : transform.position;
        _game.SetCheckpoint(point);
        _claimed = true;
    }
}