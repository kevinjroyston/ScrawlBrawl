 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LeanTweenHelper : MonoBehaviour
{
    public static LeanTweenHelper Singleton;
    private void Awake()
    {
        Singleton = this;
    }

    public Vector3 UnBoundedLerp(Vector3 a, Vector3 b, float t)
    {
        return (b - a) * t + a;
    }
    #region ColorChange
    /*public LTDescr ColorChange(Image image, Color to, float time)
    {
        LTDescr leanTweenValue = LeanTween.value(0, 1, time);
        leanTweenValue.addOnStart(() =>
        {
            StartCoroutine(ColorChangeCoroutine(leanTweenValue, image, to, time));
        });
        return leanTweenValue;
    }
    IEnumerator ColorChangeCoroutine(LTDescr leanTweenValue, Image image, Color to, float time)
    {
       /* float elapsedTime = 0f;
        float tweeningValue = 0;
        leanTweenValue.setOnUpdate((float value) => tweeningValue = value);
        Color originalColor =
        Vector3 originalPosition = rectTransform.position;
        bool isTweening = LeanTween.isTweening(leanTweenValue.id);
        bool isFinished = leanTweenValue.isFinished;
        while (LeanTween.isTweening(leanTweenValue.id) && rectTransform != null && elapsedTime < time + leanTweenValue.delay + 2)
        {
            elapsedTime += Time.deltaTime;
            rectTransform.position = UnBoundedLerp(
                originalPosition,
                to,
                tweeningValue);
            yield return null;
        }
        if (rectTransform != null)
        {
            rectTransform.position = to;
        }
    }*/
    #endregion
    #region UIMove
    public LTDescr UIMove(RectTransform rectTransform, Vector3 to, float time)
    {
        LTDescr leanTweenValue = LeanTween.value(0, 1, time);
        leanTweenValue.addOnStart(() =>
        {
            StartCoroutine(UIMoveCoroutine(leanTweenValue, rectTransform, to, time));
        });    
        return leanTweenValue;
    }
    IEnumerator UIMoveCoroutine(LTDescr leanTweenValue, RectTransform rectTransform, Vector3 to, float time)
    {
        float elapsedTime = 0f;
        float tweeningValue = 0;
        leanTweenValue.setOnUpdate((float value) => tweeningValue = value);
        Vector3 originalPosition = rectTransform.position;
        bool isTweening = LeanTween.isTweening(leanTweenValue.id);
        bool isFinished = leanTweenValue.isFinished;
        while (LeanTween.isTweening(leanTweenValue.id) && rectTransform != null && elapsedTime < time + leanTweenValue.delay + 2)
        {
            elapsedTime += Time.deltaTime;
            rectTransform.position = UnBoundedLerp(
                originalPosition,
                to,
                tweeningValue);  
            yield return null;
        }
        if (rectTransform != null)
        {
            rectTransform.position = to;
        }      
    }
    #endregion

    #region UIMoveRelative
    public LTDescr UIMoveRelative(RectTransform rectTransform, Vector3 to, float time)
    {
        LTDescr leanTweenValue = LeanTween.value(0, 1, time);
        leanTweenValue.addOnStart(() =>
        {
            StartCoroutine(UIMoveRelativeCoroutine(leanTweenValue, rectTransform, to, time));
        });
        return leanTweenValue;
    }
    IEnumerator UIMoveRelativeCoroutine(LTDescr leanTweenValue, RectTransform rectTransform, Vector3 to, float time)
    {
        float elapsedTime = 0f;
        float tweeningValue = 0;
        float lastTweenValue = 0;
        leanTweenValue.setOnUpdate((float value) => tweeningValue = value);
        Vector3 originalPosition = rectTransform.position;
        while (LeanTween.isTweening(leanTweenValue.id) && rectTransform != null && elapsedTime < time + leanTweenValue.delay + 2)
        {
            float deltaTween = tweeningValue - lastTweenValue;
            elapsedTime += Time.deltaTime;
            rectTransform.position = rectTransform.position + to * deltaTween;
            lastTweenValue = tweeningValue;
            yield return null;
        }
    }
    #endregion

    #region OrbitAroundPoint
    public LTDescr OrbitAroundPoint(RectTransform rectTransform, Vector3 center, float radians, float time, float? startingRadians)
    {
        LTDescr leanTweenValue = LeanTween.value(0, 1, time);
        leanTweenValue.addOnStart(() =>
        {
            StartCoroutine(OrbitCoroutine(leanTweenValue, rectTransform, center, radians, time, startingRadians));
        });
        
        return leanTweenValue;
    }

    IEnumerator OrbitCoroutine(LTDescr leanTweenValue, RectTransform rectTransform, Vector3 center, float radians, float time, float? startingRadians)
    {
        float tweeningValue = 0f;
        float elapsedTime = 0f;
        leanTweenValue.setOnUpdate((float value) => tweeningValue = value);
        float radius = Mathf.Sqrt((rectTransform.position.x - center.x) * (rectTransform.position.x - center.x) + (rectTransform.position.x - center.x) * (rectTransform.position.y - center.y));
       
        float startingRadiansAssigned = 0;
        if (startingRadians == null)
        {       
            Vector2 differenceVector = rectTransform.position - center;
            if (radius > 0)
            {
                startingRadiansAssigned = Mathf.Asin(differenceVector.normalized.y / 1);
                if ((rectTransform.position.x - center.x) < 0)
                {

                    startingRadiansAssigned = Mathf.PI - startingRadiansAssigned;
                }
            }
        }
        else
        {
            startingRadiansAssigned = (float)startingRadians;
        }
        
        while (LeanTween.isTweening(leanTweenValue.id) && rectTransform != null && elapsedTime < time + leanTweenValue.delay + 2)
        {
            radius = Vector3.Distance(rectTransform.position, center);
            elapsedTime += Time.deltaTime;
            float currentRadians = tweeningValue * radians + startingRadiansAssigned;
            float xPos = radius * Mathf.Cos(currentRadians);
            float yPos = radius * Mathf.Sin(currentRadians);
            rectTransform.position = new Vector3(center.x + xPos,center.y + yPos, rectTransform.position.z);
            yield return null;
        }
    }
    #endregion

    #region DynamicOrbitAroundPoint
    /// <summary>
    /// Orbits with a changing radius defined by radiusValueTween. 
    /// Only pass in a LeanTween.value(...) tween into radiusValueTween.
    /// </summary>
    /// <param name="rectTransform"></param>
    /// <param name="center"></param>
    /// <param name="radiusValueTween"></param>
    /// <param name="radians"></param>
    /// <param name="time"></param>
    /// <returns></returns>
    public LTDescr DynamicOrbitAroundPoint(RectTransform rectTransform, Vector3 center, LTDescr radiusValueTween, float radians, float time, float? startingRadians = null)
    {
        LTDescr leanTweenValue = LeanTween.value(0, 1, time);
        leanTweenValue.addOnStart(() =>
        {
            StartCoroutine(DynamicOrbitCoroutine(leanTweenValue, rectTransform, center, radiusValueTween, radians, time, startingRadians));
        });

        return leanTweenValue;
    }

    IEnumerator DynamicOrbitCoroutine(LTDescr leanTweenValue, RectTransform rectTransform, Vector3 center, LTDescr radiusValueTween, float radians, float time, float? startingRadians)
    {
        float tweeningValue = 0f;
        float elapsedTime = 0f;
        float radius = Vector2.Distance(rectTransform.position, center);
        leanTweenValue.setOnUpdate((float value) => tweeningValue = value);
        radiusValueTween.setOnUpdate((float value) => radius = value);

        float startingRadiansAssigned = 0;
        if (startingRadians == null)
        {
            Vector2 differenceVector = rectTransform.position - center;
            if (radius > 0)
            {
                startingRadiansAssigned = Mathf.Asin(differenceVector.normalized.y / 1);
                if ((rectTransform.position.x - center.x) < 0)
                {

                    startingRadiansAssigned = Mathf.PI - startingRadiansAssigned;
                }
            }
        }
        else
        {
            startingRadiansAssigned = (float)startingRadians;
        }

        while (LeanTween.isTweening(leanTweenValue.id) && rectTransform != null && elapsedTime < time + leanTweenValue.delay + 2)
        {
            elapsedTime += Time.deltaTime;
            float currentRadians = tweeningValue * radians + startingRadiansAssigned;
            float xPos = radius * Mathf.Cos(currentRadians);
            float yPos = radius * Mathf.Sin(currentRadians);
            rectTransform.position = new Vector3(center.x + xPos, center.y + yPos, rectTransform.position.z);
            yield return null;
        }
    }
    #endregion

    #region UIShake
    public LTDescr UIShake(RectTransform rectTransform, float magnitude, float rateOfChange, float time, float frequency = 10)
    {
        LTDescr leanTweenValue = LeanTween.value(0, frequency * time, time);
        leanTweenValue.addOnStart(() =>
        {
            StartCoroutine(UIShakeCoroutine(leanTweenValue, rectTransform, magnitude, rateOfChange, time));
        });
        return leanTweenValue;
    }
    IEnumerator UIShakeCoroutine(LTDescr leanTweenValue, RectTransform rectTransform, float magnitude, float rateOfChange, float time)
    {
        float elapsedTime = 0f;
        float tweeningValue = 0;
        leanTweenValue.setOnUpdate((float value) => tweeningValue = value);
        float lastValX = 0;
        float lastValY = 0;
        float sin1ValX = Random.Range(0, 10);
        float sin2ValX = Random.Range(0, 10);
        float sin1ValY = Random.Range(0, 10);
        float sin2ValY = Random.Range(0, 10);
        Vector2 distanceFromStart = Vector2.zero;

        while (LeanTween.isTweening(leanTweenValue.id) && rectTransform != null && elapsedTime < time + leanTweenValue.delay + 2)
        {
            elapsedTime += Time.deltaTime;
            sin1ValX += Random.Range(-rateOfChange, rateOfChange);
            sin2ValX += Random.Range(-rateOfChange, rateOfChange);
            sin1ValY += Random.Range(-rateOfChange, rateOfChange);
            sin2ValY += Random.Range(-rateOfChange, rateOfChange);

            float newValX = (Mathf.Sin(sin1ValX * tweeningValue) + Mathf.Sin(sin2ValX * tweeningValue)) * magnitude;
            float deltaX = newValX - lastValX;
            lastValX = newValX;
            float newValY = (Mathf.Sin(sin1ValY * tweeningValue) + Mathf.Sin(sin2ValY * tweeningValue)) *magnitude;
            float deltaY = newValY - lastValY;
            lastValY = newValY;

            rectTransform.position += new Vector3(deltaX, deltaY);
            distanceFromStart += new Vector2(deltaX, deltaY);
            yield return null;
        }
        if (rectTransform != null)
        {
            rectTransform.position -= new Vector3(distanceFromStart.x, distanceFromStart.y);
        }
    }
    #endregion
}
