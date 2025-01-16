#define DEBUG
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
    GameObject floor;
    List<GameObject> courtWalls = new List<GameObject>();
    [SerializeField]
    GameObject SceneMesh, PongPassthrough, WorldPassthrough, ball, botPaddle, playerPaddle;
    [SerializeField]
    Material goalMaterial;
    GameObject Player;
    float spawnHeight = 1f;
    float goalOffset = 0.5f;
    bool usePassthrough = false;

    void Start()
    {
        Player = GameObject.FindGameObjectWithTag("MainCamera");
        StartCoroutine(GetWalls());
    }
    IEnumerator GetWalls()
    {
        while (walls.Length == 0 || floor == null)
        {
            yield return new WaitForSeconds(1);
            walls = FindObjectsByType<GameObject>(FindObjectsSortMode.None).Where(obj => obj.name.Contains("EffectMesh")).ToArray();
            floor = GameObject.Find("FLOOR");
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
    public void SetupPong()
    {
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
#if DEBUG
        //spawn ball and paddle right in front of the player
        Vector3 ballPos = Player.transform.position + Player.transform.forward * 0.5f;
        ballPos.y = floor.transform.position.y + spawnHeight;
        ball = Instantiate(ball, ballPos, Quaternion.identity);
        Vector3 paddlePos = ballPos - Player.transform.forward * 0.1f;
        paddlePos.y = floor.transform.position.y + spawnHeight;
        botPaddle = Instantiate(botPaddle, paddlePos, Player.transform.rotation);
#else
        //instantiate ball at the center of the court
        Vector3 ballPos = (courtWalls[0].transform.position + courtWalls[1].transform.position) / 2;
        ballPos.y = floor.transform.position.y + spawnHeight;
        ball = Instantiate(ball, ballPos, Quaternion.identity);
        //instantiate bot paddle 1 unit in front of the second wall, with the same rotation as the second wall
        Vector3 botPos = courtWalls[1].transform.position + courtWalls[1].transform.forward * 1;
        botPos.y = floor.transform.position.y + spawnHeight;
        botPaddle = Instantiate(botPaddle, botPos, courtWalls[1].transform.rotation);
#endif
        //instantiate player paddle 1 unit in front of the first wall, with the same rotation as the first wall
        Vector3 playerPos = courtWalls[0].transform.position + courtWalls[0].transform.forward * 1;
        playerPos.y = floor.transform.position.y + spawnHeight;
        playerPaddle = Instantiate(playerPaddle, playerPos, courtWalls[0].transform.rotation);

        //set up goals
        foreach (GameObject wall in courtWalls)
        {
            wall.GetComponent<MeshRenderer>().material = goalMaterial;
            wall.transform.position += wall.transform.forward * goalOffset;
        }
        //start the game
        GetComponent<GameManager>().StartGame(ball, playerPaddle, botPaddle, courtWalls[0], courtWalls[1]);
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
            SetupPong();
        }
    }
}
