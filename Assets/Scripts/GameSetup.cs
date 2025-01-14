using System.Linq;
using UnityEngine;
using System.Collections;

public class GameSetup : MonoBehaviour
{
    [SerializeField]
    GameObject[] walls;
    [SerializeField]
    void Start()
    {
        StartCoroutine(GetWalls());
    }
    IEnumerator GetWalls()
    {
        while (walls.Length == 0)
        {
            yield return new WaitForSeconds(1);
            walls = FindObjectsByType<GameObject>(FindObjectsSortMode.None).Where(obj => obj.name.Contains("EffectMesh")).ToArray();
        }
    }
    public void SetupPong()
    {

    }
}
