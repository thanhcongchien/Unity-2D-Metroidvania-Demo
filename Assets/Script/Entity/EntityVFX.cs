using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityVFX : MonoBehaviour
{
    protected SpriteRenderer sr;
    private Entity entity;

    [Header("On Taking Damage VFX")]
    [SerializeField] private Material onDamageMaterial;
    [SerializeField] private float onDamageVfxDuration = .2f;
    private Material originalMaterial;
    private Coroutine onDamageVfxCoroutine;

    [Header("On Doing Damage VFX")]
    [SerializeField] private GameObject HitVFX;
    [SerializeField] private Color hitvfxColor = Color.white;
    [SerializeField] private GameObject CritHitVFX;

    [Header("Element Color")]
    [SerializeField] private Color chillvfx = Color.cyan;
    [SerializeField]private Color burnvfx = Color.red;
    [SerializeField] private Color electrifyvfx = Color.yellow;
    private Color originalHitVFXColor;
    [Header("Image Echo VFX")]
    [Range(.01f, .2f)]
    [SerializeField] private float imageEchoInterval = .05f;
    [SerializeField] private GameObject imageEchoPrefab;
    private Coroutine imageEchoCo;

    private void Awake()
    {
        entity = GetComponent<Entity>();
        sr = GetComponentInChildren<SpriteRenderer>();
        originalMaterial = sr.material;
        originalHitVFXColor = hitvfxColor;
    }
    



    public void DoImageEchoEffect(float duration)
    {
        if (imageEchoPrefab == null || entity == null || entity.anim == null || sr == null)
            return;

        StopImageEchoEffect();

        imageEchoCo = StartCoroutine(ImageEchoCo(duration));
    }

    public void StopImageEchoEffect()
    {
        if (imageEchoCo != null)
        {
            StopCoroutine(imageEchoCo);
            imageEchoCo = null;
        }
    }
    private IEnumerator ImageEchoCo(float duration)
    {
        float time = 0;
        while (time < duration)
        {
            CreateImageEcho();

            yield return new WaitForSeconds(imageEchoInterval);
            time += imageEchoInterval;
        }
    }

    private void CreateImageEcho()
    {
        if (imageEchoPrefab == null || entity == null || entity.anim == null || sr == null)
            return;

        Vector3 position = entity.anim.transform.position;
        float scale = entity.anim.transform.localScale.x;


        GameObject imageEcho = Instantiate(imageEchoPrefab, position, transform.rotation);

        imageEcho.transform.localScale = new Vector3(scale, scale, scale);

        imageEcho.GetComponentInChildren<SpriteRenderer>().sprite = sr.sprite;
    }
    public Color getElementColor(ElementType element)
    {
        if (element == ElementType.Ice)
            return chillvfx;
        if(element==ElementType.None)
           return Color.white;
        if (element == ElementType.Fire)
            return burnvfx;
        if(element==ElementType.Lightning)
            return electrifyvfx;

        return Color.white;
    }

    public void PlayStatusVFX(ElementType element,float duration)
    {
        if(element== ElementType.Ice)
            StartCoroutine(PlayStatusVFXCo(chillvfx,duration));
        if(element== ElementType.Fire)
            StartCoroutine(PlayStatusVFXCo(burnvfx,duration));
        if(element== ElementType.Lightning)
            StartCoroutine(PlayStatusVFXCo(electrifyvfx,duration));
    }

    public void StopAllVFX()
    {
        StopAllCoroutines();
        sr.color=Color.white;
        sr.material=originalMaterial;
    }




    private IEnumerator PlayStatusVFXCo(Color effectcolor,float duration)
    {
        float tickInterval = .25f;
        float timer = 0;

        Color lightColor = effectcolor * 1.2f;
        Color darkColor= effectcolor * .8f;

        bool toggle = false;

        while ( timer < duration)
        {
            sr.color=toggle ? lightColor : darkColor;
            toggle = !toggle;

            yield return new WaitForSeconds(tickInterval);
            timer+=tickInterval;
        }

        sr.color=Color.white;
    }



    public void PlayHitVFX(Transform target,bool isCrit,ElementType element)
    {
        GameObject hitPrefab = isCrit ? CritHitVFX : HitVFX;

       GameObject vfx= Instantiate(hitPrefab, target.position, Quaternion.identity);
        //vfx.GetComponentInChildren<SpriteRenderer>().color = getElementColor(element);

        if (entity.facingDir == -1 && isCrit)
            vfx.transform.Rotate(0, 180, 0);

    }

    public void PlayOnDamageVfx()
    {
        if (onDamageVfxCoroutine != null)
            StopCoroutine(onDamageVfxCoroutine);

        onDamageVfxCoroutine = StartCoroutine(OnDamageVfxCo());
    }

    private IEnumerator OnDamageVfxCo()
    {
        sr.material = onDamageMaterial;

        yield return new WaitForSeconds(onDamageVfxDuration);
        sr.material = originalMaterial;
    }
}
