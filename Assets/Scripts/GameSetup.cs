using System.Linq;
using UnityEngine;
using System.Collections;
using Oculus.Interaction;
using Oculus.Interaction.Surfaces;
using System.Collections.Generic;

public class GameSetup : MonoBehaviour
{
    [SerializeField]
    GameObject[] walls;
    List<GameObject> courtWalls = new List<GameObject>();
    [SerializeField]
    GameObject SceneMesh, PongPassthrough, WorldPassthrough, Ball, BotPaddle, PlayerPaddle;
    GameObject Player;
    bool usePassthrough = false;
    void Start()
    {
        Player = GameObject.FindGameObjectWithTag("MainCamera");
        StartCoroutine(GetWalls());
    }
    IEnumerator GetWalls()
    {
        while (walls.Length == 0)
        {
            yield return new WaitForSeconds(1);
            walls = FindObjectsByType<GameObject>(FindObjectsSortMode.None).Where(obj => obj.name.Contains("EffectMesh")).ToArray();
        }
        SetupWalls();
    }
    void SetupWalls()
    {
        foreach (GameObject wall in walls)
        {
            RayInteractable rayInteractable = wall.AddComponent<RayInteractable>();
            ColliderSurface colliderSurface = wall.AddComponent<ColliderSurface>();
            colliderSurface.InjectCollider(wall.GetComponent<Collider>());
            rayInteractable.InjectSurface(colliderSurface);

            // Add event handlers
            rayInteractable.WhenPointerEventRaised += (args) => HandleStateChanged(args, wall);
            wall.GetComponent<MeshRenderer>().enabled = false;
        }
    }
    public void Test(GameObject wall)
    {
        Debug.Log("Test from " + wall.name);
    }
    public void SetupPong(GameObject wall)
    {
        Debug.Log("SetupPong from " + wall.name);
        foreach (RayInteractor interactor in FindObjectsByType<RayInteractor>(FindObjectsSortMode.None))
        {
            interactor.gameObject.SetActive(false);     //turn off pointers
        }
        if (usePassthrough)
        {
            WorldPassthrough.SetActive(false);
            PongPassthrough.SetActive(true);
        }
        SceneMesh.SetActive(true);
        //sort courtWalls by distance to player
        courtWalls.Sort((a, b) => Vector3.Distance(a.transform.position, Player.transform.position).CompareTo(Vector3.Distance(b.transform.position, Player.transform.position)));
        //instantiate ball at the center of the court
        Instantiate(Ball, (courtWalls[0].transform.position + courtWalls[1].transform.position) / 2, Quaternion.identity);
        //instantiate bot paddle 1 unit in front of the second wall, with the same rotation as the second wall
        Instantiate(BotPaddle, courtWalls[1].transform.position + courtWalls[1].transform.forward * 1, courtWalls[1].transform.rotation);
        //instantiate player paddle 1 unit in front of the first wall, with the same rotation as the first wall
        Instantiate(PlayerPaddle, courtWalls[0].transform.position + courtWalls[0].transform.forward * 1, courtWalls[0].transform.rotation);

    }

    // Define your handler methods
    private void HandleStateChanged(PointerEvent args, GameObject wall)
    {
        switch (args.Type)
        {
            case PointerEventType.Hover: OnHover(wall); break;
            case PointerEventType.Unhover: OnUnhover(wall); break;
            case PointerEventType.Select: OnSelect(wall); break;
        }
    }

    private void OnHover(GameObject wall)
    {
        MeshRenderer renderer = wall.GetComponent<MeshRenderer>();
        if (renderer != null && !courtWalls.Contains(wall))
        {
            renderer.enabled = true;
        }
    }

    private void OnUnhover(GameObject wall)
    {
        MeshRenderer renderer = wall.GetComponent<MeshRenderer>();
        if (renderer != null && !courtWalls.Contains(wall))
        {
            renderer.enabled = false;
        }
    }

    private void OnSelect(GameObject wall)
    {
        courtWalls.Add(wall);
        if (courtWalls.Count == 2)
        {
            SetupPong(wall);
        }
    }
}
