using UnityEngine;
using UnityEngine.Events;

// Generic block that can be hit, damaged, and destroyed. Is worth a certain
// number of points, sends an event when it is destroyed with the score it was
// worth and who destroyed it
public class DestructibleScoringBlock : MonoBehaviour
{
    public UnityEvent<float, GameObject> scoringBlockDestroyed;

    [SerializeField] private float Score = 1;
    [SerializeField] private float Health = 1;

    public void Damage(float damage, GameObject damageDealer)
    {
        Health -= damage;
        if (Health < 0) {
            scoringBlockDestroyed.Invoke(Score, damageDealer);
        }
    }
}