using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    #region UIMove
    public LTDescr UIMove(RectTransform rectTransform, Vector3 to, float time)
    {
        LTDescr leanTweenValue = LeanTween.value(0, 1, time);
        leanTweenValue.AddOnStart(() =>
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
        leanTweenValue.AddOnStart(() =>
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
        leanTweenValue.AddOnStart(() =>
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
        leanTweenValue.AddOnStart(() =>
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
    public LTDescr UIShake(RectTransform rectTransform, float magnitude, float speed, float time)
    {
        LTDescr leanTweenValue = LeanTween.value(0, 1, time);
        leanTweenValue.AddOnStart(() =>
        {
            StartCoroutine(UIShakeCoroutine(leanTweenValue, rectTransform, magnitude, speed, time));
        });
        return leanTweenValue;
    }
    IEnumerator UIShakeCoroutine(LTDescr leanTweenValue, RectTransform rectTransform, float magnitude, float speed, float time)
    {
        float elapsedTime = 0f;
        float tweeningValue = 0;
        leanTweenValue.setOnUpdate((float value) => tweeningValue = value);
        Vector3 distanceToOriginalPosition = Vector3.zero;


        Vector3 randomMove = new Vector3(Random.Range(0f, 1f), Random.Range(0f, 1f), 0).normalized;
        while (LeanTween.isTweening(leanTweenValue.id) && rectTransform != null && elapsedTime < time + leanTweenValue.delay + 2)
        {
            float deltaTime = Time.deltaTime;
            elapsedTime += deltaTime;

            if ((distanceToOriginalPosition + randomMove * speed * deltaTime).magnitude > magnitude)
            {
                randomMove = new Vector3(Random.Range(0f, 1f), Random.Range(0f, 1f), 0).normalized;
                if (Vector3.Angle(randomMove, distanceToOriginalPosition) < 90 )
                {
                    randomMove *= -1;
                }                
            }
            if ((time - elapsedTime) * speed < distanceToOriginalPosition.magnitude)
            {
                randomMove = distanceToOriginalPosition.normalized * -1;
            }
            distanceToOriginalPosition += randomMove * speed * deltaTime;
            rectTransform.position += randomMove * speed * deltaTime;
            yield return null;
        }
        if (rectTransform != null)
        {
            rectTransform.position -= distanceToOriginalPosition;
        }
    }
    #endregion
}
