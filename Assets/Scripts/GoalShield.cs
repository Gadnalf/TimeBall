using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalShield : MonoBehaviour
{
    private Material material;
    private Collider shieldCollider;
    private float alphaIncrease;

    // Start is called before the first frame update
    void Start()
    {
        material = gameObject.GetComponent<Renderer>().material;
        material.color = new Color(0, 0, 0, 0.5f);
        shieldCollider = GetComponent<Collider>();
        alphaIncrease = 1f;
        StartAlphaChange();
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
