using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CooldownTimer : MonoBehaviour
{
    [SerializeField]
    private Camera playerCamera;
    [SerializeField]
    private Image coolDown;

    private void Start() {
        if (playerCamera.name == "P1Camera") {
            transform.position = new Vector3(30, 30, 0);
        }
        else {
            // canvas.height is 776
            transform.position = new Vector3(30, 776/2 + 30, 0);
        }

        coolDown.fillAmount = 0f;
    }

    public void AbilityDisabled() {
        coolDown.fillAmount = 1f;
        gameObject.SetActive(false);
    }

    public void AbilityEnabled() {
        coolDown.fillAmount = 0f;
        gameObject.SetActive(true);
    }

    public void StartCooldown(float seconds) {
        coolDown.fillAmount = 1f;
        IEnumerator coroutine = cooldownFill(seconds);
        StartCoroutine(coroutine);
    }

    private IEnumerator cooldownFill(float seconds) {
        while (coolDown.fillAmount > 0) {
            yield return new WaitForSeconds(seconds / 50);
            coolDown.fillAmount -= seconds / 50;
        }
    }
}

