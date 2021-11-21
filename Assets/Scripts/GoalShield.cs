using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalShield : MonoBehaviour
{
    private Material material;
    private float alphaIncrease;

    private BallScript ball;
    private GameObject text;

    // Start is called before the first frame update
    void Start()
    {
        material = gameObject.GetComponent<Renderer>().material;
        material.color = new Color(0, 0, 0, 0.5f);
        alphaIncrease = 1f;
        // StartAlphaChange();

        ball = FindObjectOfType<BallScript>().GetComponent<BallScript>();
        text = transform.GetChild(0).gameObject;
    }

    private void Update() {
        var charge = ball.GetCharge();

        if (charge == 0) {
            material.color = new Color(material.color.r, material.color.g, material.color.b, 0.9f);
            text.gameObject.SetActive(true);
        }
        else {
            material.color = new Color(material.color.r, material.color.g, material.color.b, 0.1f);
            text.gameObject.SetActive(false);
        }
            
    }

    private void StartAlphaChange() {
        IEnumerator coroutine = alphaChange();
        StartCoroutine(coroutine);
    }

    private IEnumerator alphaChange() {
        while (true) {
            yield return new WaitForSeconds((float)1 / 50);
            float newAlpha = material.color.a + alphaIncrease * ((float)1 / 100);
            Color newColor = new Color(material.color.r, material.color.g, material.color.b, newAlpha);
            material.color = newColor;
            if (newAlpha <= 0.25f || newAlpha >= 0.75f) {
                alphaIncrease = -alphaIncrease;
            }
        }
    }
}
