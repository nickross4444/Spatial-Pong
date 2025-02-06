using System.Linq;
using UnityEngine;
using System.Collections;
using Oculus.Interaction;
using Oculus.Interaction.Surfaces;
using System.Collections.Generic;
using Unity.VisualScripting;

public class GameSetup : MonoBehaviour
{
    [SerializeField]
    GameObject[] walls;
    GameObject floor;
    List<GameObject> courtWalls = new List<GameObject>();
    [SerializeField]
    GameObject SceneMesh, PongPassthrough, WorldPassthrough, ball, botPaddle, playerPaddle;
    [SerializeField]
    Material goalMaterial, paddleControlAreaMaterial;
    GameObject player;
    float spawnHeight = 1f;
    float goalOffset = 0.25f;
    float playerPaddleOffset = 1.15f;
    float botPaddleOffset = 1f;
    float paddleSpeed = 2f;
    [SerializeField]
    bool usePassthrough = true;
    AudioSource wallAudio;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("MainCamera");
        WorldPassthrough.SetActive(usePassthrough);
        PongPassthrough.SetActive(false);
        StartCoroutine(GetWalls());
        // Set default paddle speed if not already set
        PlayerPrefs.SetFloat("PaddleSpeed", paddleSpeed);
        wallAudio = GetComponent<AudioSource>();
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
        //foreach (RayInteractor interactor in FindObjectsByType<RayInteractor>(FindObjectsSortMode.None))
        //{
        //    interactor.gameObject.SetActive(false);     //turn off pointers
        //}
        WorldPassthrough.SetActive(false);
        PongPassthrough.SetActive(usePassthrough);

        SceneMesh.SetActive(true);
        //sort courtWalls by distance to player
        courtWalls.Sort((a, b) => Vector3.Distance(a.transform.position, player.transform.position).CompareTo(Vector3.Distance(b.transform.position, player.transform.position)));
        //create paddlePlane
        Vector3 paddlePlanePos = courtWalls[0].transform.position + courtWalls[0].transform.forward * playerPaddleOffset;
        Quaternion paddlePlaneRot = courtWalls[0].transform.rotation * Quaternion.Euler(0, 180, 0); //face opposite direction
        GameObject paddlePlane = Instantiate(courtWalls[0], paddlePlanePos, paddlePlaneRot);
        paddlePlane.GetComponent<MeshRenderer>().material = paddleControlAreaMaterial;
        paddlePlane.layer = LayerMask.NameToLayer("NoBall");
        PaddlePlane paddlePlaneComponent = paddlePlane.AddComponent<PaddlePlane>();

        //instantiate ball at the center of the court
        Vector3 ballPos = (courtWalls[0].transform.position + courtWalls[1].transform.position) / 2;
        ballPos.y = floor.transform.position.y + spawnHeight;
        ball = Instantiate(ball, ballPos, Quaternion.identity);
        //instantiate player paddle 1 unit in front of the first wall, with the same rotation as the first wall
        Vector3 playerPos = courtWalls[0].transform.position + courtWalls[0].transform.forward * playerPaddleOffset;
        playerPos.y = floor.transform.position.y + spawnHeight;
        playerPaddle = Instantiate(playerPaddle, playerPos, courtWalls[0].transform.rotation);

        //instantiate bot paddle 1 unit in front of the second wall, with the same rotation as the second wall
        Vector3 botPos = courtWalls[1].transform.position + courtWalls[1].transform.forward * botPaddleOffset;
        botPos.y = floor.transform.position.y + spawnHeight;
        botPaddle = Instantiate(botPaddle, botPos, courtWalls[1].transform.rotation);
        //start the game
        paddlePlaneComponent.Initialize(playerPaddle);
        GetComponent<GameManager>().StartGame(ball, playerPaddle, botPaddle, courtWalls[0], courtWalls[1]);
    }

    // Define handler methods
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
        if (courtWalls.Count < 2 && renderer != null && !courtWalls.Contains(wall))
        {
            renderer.enabled = true;
        }
    }

    private void OnUnhover(GameObject wall)
    {
        MeshRenderer renderer = wall.GetComponent<MeshRenderer>();
        if (renderer != null && !courtWalls.Contains(wall))
        {
            RayInteractable rayInteractable = wall.GetComponent<RayInteractable>();
            if (rayInteractable.Interactors.Count == 0)     //only hide if not being interacted with by another hand
            {
                renderer.enabled = false;
            }
        }
    }

    private void OnSelect(GameObject wall)
    {
        if (courtWalls.Count < 2)
        {
            courtWalls.Add(wall);
            MakeGoal(wall);
            if (courtWalls.Count == 2)
            {
                //RayInteractable rayInteractable = wall.GetComponent<RayInteractable>();
                //rayInteractable.WhenPointerEventRaised -= (args) => HandleStateChanged(args, wall);
                SetupPong();
            }
        }
    }
    void MakeGoal(GameObject wall)
    {
        wall.GetComponent<MeshRenderer>().material = goalMaterial;
        wall.transform.position += wall.transform.forward * goalOffset;
        wall.tag = "Goal";
        wallAudio.Play();
    }
}
