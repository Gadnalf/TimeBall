using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CooldownTimer : MonoBehaviour
{
    [SerializeField]
    private Image coolDown;

    private void Start() {
        if (gameObject.name == "P1DashCD") {
            transform.position = new Vector3(30, 30, 0);
        }
        else {
            var height = FindObjectOfType<Canvas>().GetComponent<RectTransform>().rect.height;
            transform.position = new Vector3(30, height / 2 + 30, 0);
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
            yield return new WaitForSeconds((float)1 / 50);
            coolDown.fillAmount -= 1 / (50 * seconds);
        }
    }
}

