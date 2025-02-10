using System.Linq;
using UnityEngine;
using System.Collections;
using Oculus.Interaction;
using Oculus.Interaction.Surfaces;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.Events;

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
    [SerializeField]
    UnityEvent AfterWallSetup;
    GameObject player;
    float spawnHeight = 1f;
    float goalOffset = 0.25f;
    float playerPaddleOffset = 1.15f;
    float botPaddleOffset = 1f;
    float paddleSpeed = 2f;
    [SerializeField]
    bool usePassthrough = true;
    [SerializeField]
    float gameStartDelay = 3f;
    AudioSource SetupAudioSource;
    [Header("Audio")]
    [SerializeField] AudioClip wallSelectAudio;
    [SerializeField] AudioClip wallHoverAudio;
    [SerializeField] AudioClip courtSpawnAudio;


    void Start()
    {
        player = GameObject.FindGameObjectWithTag("MainCamera");
        WorldPassthrough.SetActive(usePassthrough);
        PongPassthrough.SetActive(false);
        StartCoroutine(GetWalls());
        // Set default paddle speed if not already set
        PlayerPrefs.SetFloat("PaddleSpeed", paddleSpeed);
        SetupAudioSource = GetComponent<AudioSource>();
    }
    public IEnumerator GetWalls()
    {
        while (walls.Length == 0 || floor == null)
        {
            yield return null;
            walls = FindObjectsByType<GameObject>(FindObjectsSortMode.None).Where(obj => obj.name.Contains("EffectMesh")).ToArray();
            floor = GameObject.Find("FLOOR");
        }
        foreach (GameObject wall in walls)
        {
            wall.GetComponent<MeshRenderer>().enabled = false;
        }
    }
    public void SetupWalls()
    {
        foreach (GameObject wall in walls)
        {
            RayInteractable rayInteractable = wall.AddComponent<RayInteractable>();
            ColliderSurface colliderSurface = wall.AddComponent<ColliderSurface>();
            colliderSurface.InjectCollider(wall.GetComponent<Collider>());
            rayInteractable.InjectSurface(colliderSurface);

            // Add event handlers
            rayInteractable.WhenPointerEventRaised += (args) => HandleStateChanged(args, wall);
        }
    }
    public void SetupPong()
    {
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
        AfterWallSetup.Invoke();
        SetPongPassthrough(true);
        SetupAudioSource.PlayOneShot(courtSpawnAudio);
        StartCoroutine(StartBallAfterDelay());
    }



    IEnumerator CallTransitionWhenReady()
    {
        //it takes some time for the scene mesh to be generated. This starts the transition when it's ready
        Transition[] transitionComponents;
        do
        {
            transitionComponents = FindObjectsByType<Transition>(FindObjectsSortMode.None);
            yield return null;
        } while (transitionComponents.Length == 0);

        for (int i = 0; i < transitionComponents.Length; i++)
        {
            transitionComponents[i].StartTransition(i == 0 ? () => SetupPong() : null);    //set passthrough to true after transition is complete
        }

    }
    IEnumerator StartBallAfterDelay()
    {
        yield return new WaitForSeconds(gameStartDelay);
        GetComponent<GameManager>().StartBall(ball, playerPaddle, botPaddle, courtWalls[0], courtWalls[1]);
    }



    void SetPongPassthrough(bool active)
    {
        WorldPassthrough.SetActive(!active && usePassthrough);
        PongPassthrough.SetActive(active && usePassthrough);
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
            SetupAudioSource.PlayOneShot(wallHoverAudio);
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
                SceneMesh.SetActive(true);
                StartCoroutine(CallTransitionWhenReady());
            }
        }
    }
    void MakeGoal(GameObject wall)
    {
        wall.GetComponent<MeshRenderer>().material = goalMaterial;
        wall.transform.position += wall.transform.forward * goalOffset;
        wall.tag = "Goal";
        SetupAudioSource.PlayOneShot(wallSelectAudio);
    }
}
