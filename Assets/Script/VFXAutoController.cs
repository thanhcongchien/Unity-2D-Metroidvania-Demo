using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class VFXAutoController : MonoBehaviour
{
    private SpriteRenderer sr;


    [SerializeField]private bool autoDestroy = true;
    [SerializeField]private float destroyTime = 1;

    [Header("Fade effect")]
    [SerializeField] private bool canFade;
    [SerializeField] private float fadeSpeed = 1;



    [Header("Random Rotation")]
    [SerializeField] private float minRotation = 0;
    [SerializeField] private float maxRotation = 360;

    [Header("Random Position")]
    [SerializeField] private bool randomOffset = true;
    [SerializeField] private bool randomRotation = true;
    [SerializeField] private float xMinOffset = -.3f;
    [SerializeField] private float xMaxOffset = .3f;
    [SerializeField] private float yMinOffset = -.3f;
    [SerializeField] private float yMaxOffset = .3f;


    private void Awake()
    {
        sr= GetComponentInChildren<SpriteRenderer>();
    }

    private void Start()
    {
        if(canFade)
            StartCoroutine(FadeCo());
        ApplyRandomOffset();
        ApplyRandomRotation();
        if(autoDestroy)
            Destroy(gameObject,destroyTime);
    }

    private IEnumerator FadeCo()
    {
        Color targetColor=Color.white;
        while(targetColor.a>0)
        {
            targetColor.a-=Time.deltaTime*fadeSpeed;
            sr.color=targetColor;
            yield return null;
        }
        sr.color = targetColor;
    }

    private void ApplyRandomOffset()
    {
        if (randomOffset == false)
            return;
        float xoffset = Random.Range(xMinOffset, xMaxOffset);
        float yoffset = Random.Range(yMinOffset, yMaxOffset);

        transform.position += new Vector3(xoffset, yoffset, 0);

    }
    private void ApplyRandomRotation()
    {
        if (randomRotation == false)
            return;
        float rotation = Random.Range(minRotation, maxRotation);
       transform.Rotate(0, 0, rotation);
    }
}
