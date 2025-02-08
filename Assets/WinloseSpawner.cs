using UnityEngine;

public class WinloseSpawner : MonoBehaviour
{
    [SerializeField] private GameObject winEffect;
    [SerializeField] private GameObject loseEffect;
    private GameObject goal1;
    private GameObject goal2;
    private bool hasSpawnedWinEffect = false;
    private bool hasSpawnedLoseEffect = false;

    private void SpawnObjectAtCenter(GameObject effect, ref bool hasSpawned)
    {
        // Prevent spawning multiple times
        if (hasSpawned) return;

        GameObject[] goals = GameObject.FindGameObjectsWithTag("Goal");

        if (goals.Length < 2)
        {
            Debug.LogError("Not enough 'Goal' objects found!");
            return;
        }

        goal1 = goals[0]; // First goal found
        goal2 = goals[1]; // Second goal found

        Vector3 centerPosition = (goal1.transform.position + goal2.transform.position) / 2f;
        centerPosition.y = 1.5f;

        // Calculate rotation so the object faces goal1
        Quaternion lookRotation = Quaternion.LookRotation(goal1.transform.position - centerPosition);

        // Instantiate the effect with correct rotation
        Instantiate(effect, centerPosition, lookRotation);

        // Mark as spawned
        hasSpawned = true;
    }

    public void WinEffect()
    {
        SpawnObjectAtCenter(winEffect, ref hasSpawnedWinEffect);
    }

    public void LoseEffect()
    {
        SpawnObjectAtCenter(loseEffect, ref hasSpawnedLoseEffect);
    }
}




