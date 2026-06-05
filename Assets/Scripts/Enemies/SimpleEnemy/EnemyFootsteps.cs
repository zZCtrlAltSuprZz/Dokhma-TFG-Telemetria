using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(AudioSource))]
public class EnemyFootsteps : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioClip[] footstepClips;
    [SerializeField] private float volume = 0.45f;
    [SerializeField] private float minPitch = 0.95f;
    [SerializeField] private float maxPitch = 1.05f;

    [Header("Steps")]
    [SerializeField] private float stepInterval = 0.55f;
    [SerializeField] private float minMoveSpeed = 0.15f;

    [Header("Optimization")]
    [SerializeField] private float maxHearDistance = 12f;
    [SerializeField, Range(0f, 1f)] private float playChance = 0.45f;

    private AudioSource audioSource;
    private NavMeshAgent agent;
    private Transform player;

    private float stepTimer;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        agent = GetComponent<NavMeshAgent>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    private void Update()
    {
        if (agent == null || player == null)
            return;

        // Evita error si el agente aún no está activo o no está en NavMesh
        if (!agent.enabled || !agent.isOnNavMesh)
            return;

        if (Vector3.Distance(transform.position, player.position) > maxHearDistance)
            return;

        bool isMoving =
            !agent.isStopped &&
            agent.velocity.magnitude > minMoveSpeed;

        if (!isMoving)
        {
            stepTimer = 0f;
            return;
        }

        stepTimer += Time.deltaTime;

        if (stepTimer >= stepInterval)
        {
            PlayFootstep();
            stepTimer = 0f;
        }
    }

    private void PlayFootstep()
    {
        if (footstepClips == null || footstepClips.Length == 0)
            return;

        if (UnityEngine.Random.value > playChance)
            return;

        AudioClip clip = footstepClips[
            UnityEngine.Random.Range(0, footstepClips.Length)
        ];

        audioSource.pitch = UnityEngine.Random.Range(minPitch, maxPitch);
        audioSource.PlayOneShot(clip, volume);
    }
}