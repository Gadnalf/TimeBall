using UnityEngine;
using UnityEngine.UI;

public class ChargeBorderScript : MonoBehaviour
{
    public PlayerThrowBall playerBallScript;
    private Image border;

    // Start is called before the first frame update
    void Start()
    {
        border = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        if (playerBallScript.CheckIfHasBall())
        {
            if (playerBallScript.GetCurrentCharge() >= GameConfigurations.goalShieldBreakableCharge)
            {
                border.enabled = true;
            }
            else
            {
                border.enabled = false;
            }
        }
        else
        {
            border.enabled = false;
        }
    }
}