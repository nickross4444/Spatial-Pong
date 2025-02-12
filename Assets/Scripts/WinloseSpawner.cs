using UnityEngine;

public class WinloseSpawner : MonoBehaviour
{
    [SerializeField] private GameObject winEffect;
    [SerializeField] private GameObject loseEffect;
    [SerializeField] private GameObject finalMenu;
    
    private GameObject goal1;
    private GameObject goal2;
    private Transform cameraTransform;
    private bool hasSpawnedWinEffect = false;
    private bool hasSpawnedLoseEffect = false;

    private GameObject currentWinEffect;
    private GameObject currentLoseEffect;

    private void SpawnObjectAtCenter(GameObject effect, ref bool hasSpawned, ref GameObject instance)
    {
        if (hasSpawned) return;

        GameObject[] goals = GameObject.FindGameObjectsWithTag("Goal");

        if (goals.Length < 2)
        {
            Debug.LogError("Not enough 'Goal' objects found!");
            return;
        }

        goal1 = goals[0]; 
        goal2 = goals[1]; 
        
        if (cameraTransform == null)
        {
            if (Camera.main != null)
                cameraTransform = Camera.main.transform;
            else
            {
                Debug.LogError("No main camera found!");
                return;
            }
        }
        
        Vector3 centerPosition = (goal1.transform.position + goal2.transform.position) / 2f;
        centerPosition.y = 1.6f;
        
        centerPosition.x = cameraTransform.position.x;
        
        Quaternion lookRotation = Quaternion.LookRotation(goal1.transform.position - centerPosition);
        
        instance = Instantiate(effect, centerPosition, lookRotation);
        hasSpawned = true;
    }

    public void WinEffect()
    {
        SpawnObjectAtCenter(winEffect, ref hasSpawnedWinEffect, ref currentWinEffect);
        Invoke("SpawnFinalMenu", 4f);
    }

    public void LoseEffect()
    {
        SpawnObjectAtCenter(loseEffect, ref hasSpawnedLoseEffect, ref currentLoseEffect);
        Invoke("SpawnFinalMenu", 4f);
    }

    private void SpawnFinalMenu()
    {
        finalMenu.SetActive(true);
    }

    public void ResetGame()
    {
        if (currentWinEffect != null)
        {
            Destroy(currentWinEffect);
            currentWinEffect = null;
        }

        if (currentLoseEffect != null)
        {
            Destroy(currentLoseEffect);
            currentLoseEffect = null;
        }

        hasSpawnedWinEffect = false;
        hasSpawnedLoseEffect = false;
    }
}






