using UnityEngine;

public class TriggerZone : MonoBehaviour
{
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private GameObject[] enemyPrefabs;
    
    private bool isTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isTriggered) return;
        if (!other.CompareTag("Player")) return;
        
        isTriggered = true;

        foreach (Transform point in spawnPoints)
        {
            int rand = Random.Range(0, enemyPrefabs.Length);
            Instantiate(enemyPrefabs[rand], point.position, Quaternion.identity);
        }
    }
}
