using System.Collections;
using UnityEngine;

public class EnemyMemoryPool : MonoBehaviour
{

   [SerializeField]
   private Transform target; 
   [SerializeField]
   private GameObject enemySpawnPointPrefab;
   [SerializeField]
   private GameObject enemyPrefab;
   [SerializeField]
   private float enemySpawnTime = 1;
   [SerializeField]
   private float enemySpawnLatency = 1;

   private MemoryPool spawnPointMemoryPool;
   private MemoryPool enemyMemoryPool;

   private int numberOfEnemiesSpawnedAtOnce = 1;
   private Vector2Int mapSize = new Vector2Int(100, 100);

   private void Awake()
   {
     spawnPointMemoryPool = new MemoryPool(enemySpawnPointPrefab);
     enemyMemoryPool = new MemoryPool(enemyPrefab);

     StartCoroutine("SpawnTile");
   }
   
   private IEnumerator SpawnTile()
   {
     int currentNumber = 0;
     int maximumNumber = 50;

     while ( true )
     {
       for ( int i = 0; i < numberOfEnemiesSpawnedAtOnce; ++ i )
       {
        GameObject item = spawnPointMemoryPool.ActivatePoolItem(); 

        item.transform.position = new Vector3(Random.Range(-mapSize.x*0.49f,mapSize.x*0.49f),1,Random.Range(-mapSize.x*0.49f,mapSize.x*0.49f)); 

        StartCoroutine("SpawnEnemy", item);  

        currentNumber ++;

        if ( currentNumber >= maximumNumber )
        {
            currentNumber = 0;
            numberOfEnemiesSpawnedAtOnce ++;
        } 

        yield return new WaitForSeconds(enemySpawnTime);
       }     
     }
   }

     private IEnumerator SpawnEnemy(GameObject point)
   {
    yield return new WaitForSeconds(enemySpawnLatency);
   
    GameObject item = enemyMemoryPool.ActivatePoolItem();
    item.transform.position = point.transform.position;

    item.GetComponent<EnemyFSM>().Setup(target);

    spawnPointMemoryPool.DeactivatePoolItem(point);
   }
}
