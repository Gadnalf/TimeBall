using UnityEngine;

public class BallScript : MonoBehaviour
{
    public Vector3 spawnLocation;
    public float turnRate = 1f;
    public GameObject shield;

    private Rigidbody rb;
    private PlayerData playerData;
    private Rigidbody target;
    private bool homing;
    private int charge;

    private AudioManager audioManager;

    // Start is called before the first frame update
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerData = GetComponent<PlayerData>();
        audioManager = FindObjectOfType<AudioManager>();
    }

    private void FixedUpdate()
    {
        if (target && homing)
        {
            float magnitude = Mathf.Pow(Mathf.Pow(rb.velocity.x, 2) + Mathf.Pow(rb.velocity.z, 2), 0.5f);
            Vector3 direction = (1f-turnRate)*(rb.velocity) + turnRate*(target.transform.position - transform.position);
            Vector2 scaledDirection = new Vector2(direction.x, direction.z).normalized * magnitude;
            rb.velocity = new Vector3(scaledDirection.x, rb.velocity.y, scaledDirection.y);
        }
    }

    private void Update()
    {
        UpdateShield();
        if (transform.position.y >= 30) {
            Reset();
        }
    }

    private void UpdateShield()
    {
        switch (charge >= GameConfigurations.maxBallCharge) {
            case true: 
                shield.GetComponent<Renderer>().sharedMaterial.SetVector("_PulseOffset", Vector3.one * 0.3f);
                break;
            default:
                shield.GetComponent<Renderer>().sharedMaterial.SetVector("_PulseOffset", Vector3.zero);
                break;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        homing = false;
        var collide = collision.gameObject;

        if (collide.tag == "Arena" || collide.layer == 9)
            audioManager.GetAudio("BallBounce").Play();
    }

    public void SetHomingTarget(Rigidbody target = null)
    {
        this.target = target;
        homing = target;
    }

    public Rigidbody GetHomingTarget()
    {
        return target;
    }

    public bool IsHomingTarget(Rigidbody rb)
    {
        return target == rb;
    }

    public void AddCharge()
    {
        if (charge < GameConfigurations.maxBallCharge)
        {
            charge++;
        }
        if (charge >= GameConfigurations.maxBallCharge) {
            gameObject.layer = 8;
        }
    }

    public void SetMaxCharge() {
        charge = GameConfigurations.maxBallCharge;
        gameObject.layer = 8;
    }

    public void ClearCharge()
    {
        charge = 0;
        gameObject.layer = 0;
    }

    public int GetCharge() {
        return charge;
    }

    public PlayerData.PlayerNumber GetPlayerNumber () {
        return playerData.playerNumber;
    }

    public void Reset()
    {
        rb.isKinematic = false;
        rb.transform.position = spawnLocation;
        rb.transform.rotation = Quaternion.identity;
        rb.transform.parent = null;
        rb.velocity = Vector3.zero;
        playerData.playerNumber = PlayerData.PlayerNumber.NoPlayer;
        SetHomingTarget(null);
        ClearCharge();
        shield.GetComponent<Renderer>().sharedMaterial.SetVector("_PulseOffset", Vector3.zero);
    }
}
