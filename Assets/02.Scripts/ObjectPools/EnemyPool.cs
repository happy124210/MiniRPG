using System.Collections.Generic;
using UnityEngine;

public class EnemyPool : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    private Queue<Enemy> pool = new();
    
    public Enemy GetEnemy()
    {
        return pool.Count > 0 
            ? pool.Dequeue() 
            : Instantiate(enemyPrefab).GetComponent<Enemy>();
    }
    
    public void ReturnEnemy(Enemy enemy)
    {
        enemy.gameObject.SetActive(false);
        pool.Enqueue(enemy);
    }
}