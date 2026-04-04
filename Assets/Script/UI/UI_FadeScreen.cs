using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_FadeScreen : MonoBehaviour
{
    private Image fadeImage;
    public Coroutine fadeEffectCo { get; private set;}

    private void Awake()
    {
        fadeImage = GetComponent<Image>();

      //fadeImage.color = new Color(0, 0, 0, 1);
    }
    public void FadeIn(float duration = 1.5f)
    {
        SetAlpha(1f);


        FadeEffect(0f, duration);
    }
    public void FadeOut(float duration = 1.5f)
    {
        SetAlpha(0f);

        FadeEffect(1f, duration);
    }

    public void SetBlackScreen(bool visible)
    {
        SetAlpha(visible ? 1f : 0f);
    }

    private void SetAlpha(float alpha)
    {
        if (fadeImage == null)
            return;

        fadeImage.color = new Color(0f, 0f, 0f, alpha);
    }

    private void FadeEffect(float targetAlpha, float duration)
    {
        if (fadeImage == null)
        {
            return;
        }

        if (fadeEffectCo != null)
        {
            StopCoroutine(fadeEffectCo);
            fadeEffectCo = null;
        }


        fadeEffectCo = StartCoroutine(FadeEffectCo(targetAlpha, duration));
    }

    private IEnumerator FadeEffectCo(float targetAlpha, float duration)
    { 
        float startAlpha = fadeImage.color.a;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            var color = fadeImage.color;

            color.a = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
          
            fadeImage.color = color;
            yield return null;
        }

        fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, targetAlpha);
        fadeEffectCo = null;

    }

}
