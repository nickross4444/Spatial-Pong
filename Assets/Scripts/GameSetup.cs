using System.Linq;
using UnityEngine;
using System.Collections;
using Oculus.Interaction;
using Oculus.Interaction.Surfaces;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.Events;
using Unity.XR.Oculus;

public class GameSetup : MonoBehaviour
{
    GameObject[] walls;
    GameObject floor;
    List<GameObject> courtWalls = new List<GameObject>();
    [SerializeField]
    GameObject SceneMesh, pongAnchorPrefabSpawner, PongPassthrough, WorldPassthrough, ball, botPaddle, playerPaddle;
    [SerializeField]
    Material goalMaterial, paddleControlAreaMaterial;
    [SerializeField] private GameObject playZonePrefab;
    [SerializeField]
    UnityEvent AfterWallSetup;
    GameObject player;
    float spawnHeight = 1f;
    float goalOffset = 0.25f;
    float playerPaddleOffset = 1.15f;
    float botPaddleOffset = 1f;
    float paddlePlaneWidthScaler = 5f;
    [SerializeField]
    bool usePassthrough = true;
    [SerializeField]
    float gameStartDelay = 3f;
    AudioSource SetupAudioSource;
    [Header("Audio")]

    [SerializeField] AudioClip wallSelectAudio;
    [SerializeField] AudioClip wallHoverAudio;
    [SerializeField] AudioClip courtSpawnAudio;

    SystemHeadset headsetType;

    void Start()
    {
        headsetType = Utils.GetSystemHeadsetType();
        Debug.Log("Current Headset Type: " + headsetType);

        player = GameObject.FindGameObjectWithTag("MainCamera");
        WorldPassthrough.SetActive(usePassthrough);
        PongPassthrough.SetActive(false);
        StartCoroutine(GetWalls());
        SetupAudioSource = GetComponent<AudioSource>();
    }
    public IEnumerator GetWalls()
    {
        do
        {
            yield return null;
            walls = FindObjectsByType<GameObject>(FindObjectsSortMode.None).Where(obj => obj.name.Contains("EffectMesh")).ToArray();
            floor = GameObject.Find("FLOOR");
        } while (walls.Length == 0 || floor == null);

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

        // Spawn PlayZone
        Vector3 size = paddlePlane.GetComponent<Renderer>().bounds.size;
        Vector3 rightVector = paddlePlane.transform.right;
        float paddleWidth;
        if (Mathf.Abs(Vector3.Dot(rightVector, Vector3.right)) > 0.9f)
        {
            paddleWidth = size.x;
        }
        else if (Mathf.Abs(Vector3.Dot(rightVector, Vector3.up)) > 0.9f)
        {
            paddleWidth = size.y;
        }
        else
        {
            paddleWidth = size.z;
        }
        Quaternion playZoneRotation = paddlePlane.transform.rotation * Quaternion.Euler(90, 0, 0);
        Vector3 spawnPosition = paddlePlane.transform.position + paddlePlane.transform.forward * 0.55f;
        spawnPosition.y = 0.1f;
        GameObject playZone = Instantiate(playZonePrefab, spawnPosition, playZoneRotation);
        Transform quad = playZone.transform.Find("Emission");

        if (quad != null)
        {
            Vector3 originalScale = quad.localScale;
            float originalWidth = originalScale.x;
            float newWidth = Mathf.Max(originalWidth, paddleWidth);
            quad.localScale = new Vector3(newWidth, originalScale.y, originalScale.z);
        }

        //After spawning playzone, scale the paddle plane to be larger:
        paddlePlane.transform.localScale = Vector3.Scale(paddlePlane.transform.localScale, new Vector3(paddlePlaneWidthScaler, 1, 1));

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
        PongPassthrough.SetActive(active && usePassthrough);
        WorldPassthrough.SetActive(!active && usePassthrough);
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
        if (courtWalls.Count < 2 && !courtWalls.Contains(wall))
        {
            courtWalls.Add(wall);
            MakeGoal(wall);
            if (courtWalls.Count == 2)
            {
                if (headsetType == SystemHeadset.Oculus_Link_Quest_2 || headsetType == SystemHeadset.Oculus_Quest_2)
                {
                    Debug.Log("Device name : Quest 2");
                    pongAnchorPrefabSpawner.SetActive(true);
                }
                else
                {
                    Debug.Log("Device name : Quest 3 or 3s or other");
                    SceneMesh.SetActive(true);
                }
                StartCoroutine(CallTransitionWhenReady());
                AfterWallSetup.Invoke();
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
