using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static LTDescr;

public class TweenAnimation
{
    public LTDescr tween { get; }
    public Guid guid { get; } = Guid.NewGuid();

    public TweenAnimation(LTDescr tween)
    {
        this.tween = tween;
    }

    #region LTDescr Variables
    public bool toggle
    {
        get
        {
            return this.tween.toggle;
        }
        set
        {
            this.tween.toggle = value;
        }
    }
    public bool useEstimatedTime
    {
        get
        {
            return this.tween.useEstimatedTime;
        }
        set
        {
            this.tween.useEstimatedTime = value;
        }
    }
    public bool useFrames
    {
        get
        {
            return this.tween.useFrames;
        }
        set
        {
            this.tween.useFrames = value;
        }
    }
    public bool useManualTime
    {
        get
        {
            return this.tween.useManualTime;
        }
        set
        {
            this.tween.useManualTime = value;
        }
    }
    public bool usesNormalDt
    {
        get
        {
            return this.tween.usesNormalDt;
        }
        set
        {
            this.tween.usesNormalDt = value;
        }
    }
    public bool hasInitiliazed
    {
        get
        {
            return this.tween.hasInitiliazed;
        }
        set
        {
            this.tween.hasInitiliazed = value;
        }
    }
    public bool hasExtraOnCompletes
    {
        get
        {
            return this.tween.hasExtraOnCompletes;
        }
        set
        {
            this.tween.hasExtraOnCompletes = value;
        }
    }
    public bool hasPhysics
    {
        get
        {
            return this.tween.hasPhysics;
        }
        set
        {
            this.tween.hasPhysics = value;
        }
    }
    public bool onCompleteOnRepeat
    {
        get
        {
            return this.tween.onCompleteOnRepeat;
        }
        set
        {
            this.tween.onCompleteOnRepeat = value;
        }
    }
    public bool onCompleteOnStart
    {
        get
        {
            return this.tween.onCompleteOnStart;
        }
        set
        {
            this.tween.onCompleteOnStart = value;
        }
    }
    public bool useRecursion
    {
        get
        {
            return this.tween.useRecursion;
        }
        set
        {
            this.tween.useRecursion = value;
        }
    }
    public float ratioPassed
    {
        get
        {
            return this.tween.ratioPassed;
        }
        set
        {
            this.tween.ratioPassed = value;
        }
    }
    public float passed
    {
        get
        {
            return this.tween.passed;
        }
        set
        {
            this.tween.passed = value;
        }
    }
    public float delay
    {
        get
        {
            return this.tween.delay;
        }
        set
        {
            this.tween.delay = value;
        }
    }
    public float time
    {
        get
        {
            return this.tween.time;
        }
        set
        {
            this.tween.time = value;
        }
    }
    public float speed
    {
        get
        {
            return this.tween.speed;
        }
        set
        {
            this.tween.speed = value;
        }
    }
    public float lastVal
    {
        get
        {
            return this.tween.lastVal;
        }
        set
        {
            this.tween.lastVal = value;
        }
    }
    public int loopCount
    {
        get
        {
            return this.tween.loopCount;
        }
        set
        {
            this.tween.loopCount = value;
        }
    }
    public float direction
    {
        get
        {
            return this.tween.direction;
        }
        set
        {
            this.tween.direction = value;
        }
    }
    public float directionLast
    {
        get
        {
            return this.tween.directionLast;
        }
        set
        {
            this.tween.directionLast = value;
        }
    }
    public float overshoot
    {
        get
        {
            return this.tween.overshoot;
        }
        set
        {
            this.tween.overshoot = value;
        }
    }
    public float period
    {
        get
        {
            return this.tween.period;
        }
        set
        {
            this.tween.period = value;
        }
    }
    public float scale
    {
        get
        {
            return this.tween.scale;
        }
        set
        {
            this.tween.scale = value;
        }
    }
    public bool destroyOnComplete
    {
        get
        {
            return this.tween.destroyOnComplete;
        }
        set
        {
            this.tween.destroyOnComplete = value;
        }
    }
    public Transform trans
    {
        get
        {
            return this.tween.trans;
        }
        set
        {
            this.tween.trans = value;
        }
    }
    public TweenAction type
    {
        get
        {
            return this.tween.type;
        }
        set
        {
            this.tween.type = value;
        }
    }
    public LeanTweenType loopType
    {
        get
        {
            return this.tween.loopType;
        }
        set
        {
            this.tween.loopType = value;
        }
    }
    public bool hasUpdateCallback
    {
        get
        {
            return this.tween.hasUpdateCallback;
        }
        set
        {
            this.tween.hasUpdateCallback = value;
        }
    }
    public EaseTypeDelegate easeMethod
    {
        get
        {
            return this.tween.easeMethod;
        }
        set
        {
            this.tween.easeMethod = value;
        }
    }
    public SpriteRenderer spriteRen
    {
        get
        {
            return this.tween.spriteRen;
        }
        set
        {
            this.tween.spriteRen = value;
        }
    }
    public RectTransform rectTransform
    {
        get
        {
            return this.tween.rectTransform;
        }
        set
        {
            this.tween.rectTransform = value;
        }
    }
    #endregion

    #region AutoGenerated LTDescr Overrides
    public LTDescr setFollow()
    {
        return this.tween.setFollow();
    }
    public LTDescr setMoveX()
    {
        return this.tween.setMoveX();
    }
    public LTDescr setMoveY()
    {
        return this.tween.setMoveY();
    }
    public LTDescr setMoveZ()
    {
        return this.tween.setMoveZ();
    }
    public LTDescr setMoveLocalX()
    {
        return this.tween.setMoveLocalX();
    }
    public LTDescr setMoveLocalY()
    {
        return this.tween.setMoveLocalY();
    }
    public LTDescr setMoveLocalZ()
    {
        return this.tween.setMoveLocalZ();
    }
    public LTDescr setOffset(Vector3 offset)
    {
        return this.tween.setOffset(offset);
    }
    public LTDescr setMoveCurved()
    {
        return this.tween.setMoveCurved();
    }
    public LTDescr setMoveCurvedLocal()
    {
        return this.tween.setMoveCurvedLocal();
    }
    public LTDescr setMoveSpline()
    {
        return this.tween.setMoveSpline();
    }
    public LTDescr setMoveSplineLocal()
    {
        return this.tween.setMoveSplineLocal();
    }
    public LTDescr setScaleX()
    {
        return this.tween.setScaleX();
    }
    public LTDescr setScaleY()
    {
        return this.tween.setScaleY();
    }
    public LTDescr setScaleZ()
    {
        return this.tween.setScaleZ();
    }
    public LTDescr setRotateX()
    {
        return this.tween.setRotateX();
    }
    public LTDescr setRotateY()
    {
        return this.tween.setRotateY();
    }
    public LTDescr setRotateZ()
    {
        return this.tween.setRotateZ();
    }
    public LTDescr setRotateAround()
    {
        return this.tween.setRotateAround();
    }
    public LTDescr setRotateAroundLocal()
    {
        return this.tween.setRotateAroundLocal();
    }
    public LTDescr setAlpha()
    {
        return this.tween.setAlpha();
    }
    public LTDescr setTextAlpha()
    {
        return this.tween.setTextAlpha();
    }
    public LTDescr setAlphaVertex()
    {
        return this.tween.setAlphaVertex();
    }
    public LTDescr setColor()
    {
        return this.tween.setColor();
    }
    public LTDescr setCallbackColor()
    {
        return this.tween.setCallbackColor();
    }
    public LTDescr setTextColor()
    {
        return this.tween.setTextColor();
    }
    public LTDescr setCanvasAlpha()
    {
        return this.tween.setCanvasAlpha();
    }
    public LTDescr setCanvasGroupAlpha()
    {
        return this.tween.setCanvasGroupAlpha();
    }
    public LTDescr setCanvasColor()
    {
        return this.tween.setCanvasColor();
    }
    public LTDescr setCanvasMoveX()
    {
        return this.tween.setCanvasMoveX();
    }
    public LTDescr setCanvasMoveY()
    {
        return this.tween.setCanvasMoveY();
    }
    public LTDescr setCanvasMoveZ()
    {
        return this.tween.setCanvasMoveZ();
    }
    public LTDescr setCanvasRotateAround()
    {
        return this.tween.setCanvasRotateAround();
    }
    public LTDescr setCanvasRotateAroundLocal()
    {
        return this.tween.setCanvasRotateAroundLocal();
    }
    public LTDescr setCanvasPlaySprite()
    {
        return this.tween.setCanvasPlaySprite();
    }
    public LTDescr setCanvasMove()
    {
        return this.tween.setCanvasMove();
    }
    public LTDescr setCanvasScale()
    {
        return this.tween.setCanvasScale();
    }
    public LTDescr setCanvasSizeDelta()
    {
        return this.tween.setCanvasSizeDelta();
    }
    public LTDescr setCallback()
    {
        return this.tween.setCallback();
    }
    public LTDescr setValue3()
    {
        return this.tween.setValue3();
    }
    public LTDescr setMove()
    {
        return this.tween.setMove();
    }
    public LTDescr setMoveLocal()
    {
        return this.tween.setMoveLocal();
    }
    public LTDescr setMoveToTransform()
    {
        return this.tween.setMoveToTransform();
    }
    public LTDescr setRotate()
    {
        return this.tween.setRotate();
    }
    public LTDescr setRotateLocal()
    {
        return this.tween.setRotateLocal();
    }
    public LTDescr setScale()
    {
        return this.tween.setScale();
    }
    public LTDescr setGUIMove()
    {
        return this.tween.setGUIMove();
    }
    public LTDescr setGUIMoveMargin()
    {
        return this.tween.setGUIMoveMargin();
    }
    public LTDescr setGUIScale()
    {
        return this.tween.setGUIScale();
    }
    public LTDescr setGUIAlpha()
    {
        return this.tween.setGUIAlpha();
    }
    public LTDescr setGUIRotate()
    {
        return this.tween.setGUIRotate();
    }
    public LTDescr setDelayedSound()
    {
        return this.tween.setDelayedSound();
    }
    public LTDescr setTarget(Transform trans)
    {
        return this.tween.setTarget(trans);
    }
    public LTDescr updateNow()
    {
        return this.tween.updateNow();
    }
    public LTDescr setFromColor(Color col)
    {
        return this.tween.setFromColor(col);
    }
    public LTDescr pause()
    {
        return this.tween.pause();
    }
    public LTDescr resume()
    {
        return this.tween.resume();
    }
    public LTDescr setAxis(Vector3 axis)
    {
        return this.tween.setAxis(axis);
    }
    public LTDescr setDelay(float delay)
    {
        return this.tween.setDelay(delay);
    }
    public LTDescr setEase(LeanTweenType easeType)
    {
        return this.tween.setEase(easeType);
    }
    public LTDescr setEaseLinear()
    {
        return this.tween.setEaseLinear();
    }
    public LTDescr setEaseSpring()
    {
        return this.tween.setEaseSpring();
    }
    public LTDescr setEaseInQuad()
    {
        return this.tween.setEaseInQuad();
    }
    public LTDescr setEaseOutQuad()
    {
        return this.tween.setEaseOutQuad();
    }
    public LTDescr setEaseInOutQuad()
    {
        return this.tween.setEaseInOutQuad();
    }
    public LTDescr setEaseInCubic()
    {
        return this.tween.setEaseInCubic();
    }
    public LTDescr setEaseOutCubic()
    {
        return this.tween.setEaseOutCubic();
    }
    public LTDescr setEaseInOutCubic()
    {
        return this.tween.setEaseInOutCubic();
    }
    public LTDescr setEaseInQuart()
    {
        return this.tween.setEaseInQuart();
    }
    public LTDescr setEaseOutQuart()
    {
        return this.tween.setEaseOutQuart();
    }
    public LTDescr setEaseInOutQuart()
    {
        return this.tween.setEaseInOutQuart();
    }
    public LTDescr setEaseInQuint()
    {
        return this.tween.setEaseInQuint();
    }
    public LTDescr setEaseOutQuint()
    {
        return this.tween.setEaseOutQuint();
    }
    public LTDescr setEaseInOutQuint()
    {
        return this.tween.setEaseInOutQuint();
    }
    public LTDescr setEaseInSine()
    {
        return this.tween.setEaseInSine();
    }
    public LTDescr setEaseOutSine()
    {
        return this.tween.setEaseOutSine();
    }
    public LTDescr setEaseInOutSine()
    {
        return this.tween.setEaseInOutSine();
    }
    public LTDescr setEaseInExpo()
    {
        return this.tween.setEaseInExpo();
    }
    public LTDescr setEaseOutExpo()
    {
        return this.tween.setEaseOutExpo();
    }
    public LTDescr setEaseInOutExpo()
    {
        return this.tween.setEaseInOutExpo();
    }
    public LTDescr setEaseInCirc()
    {
        return this.tween.setEaseInCirc();
    }
    public LTDescr setEaseOutCirc()
    {
        return this.tween.setEaseOutCirc();
    }
    public LTDescr setEaseInOutCirc()
    {
        return this.tween.setEaseInOutCirc();
    }
    public LTDescr setEaseInBounce()
    {
        return this.tween.setEaseInBounce();
    }
    public LTDescr setEaseOutBounce()
    {
        return this.tween.setEaseOutBounce();
    }
    public LTDescr setEaseInOutBounce()
    {
        return this.tween.setEaseInOutBounce();
    }
    public LTDescr setEaseInBack()
    {
        return this.tween.setEaseInBack();
    }
    public LTDescr setEaseOutBack()
    {
        return this.tween.setEaseOutBack();
    }
    public LTDescr setEaseInOutBack()
    {
        return this.tween.setEaseInOutBack();
    }
    public LTDescr setEaseInElastic()
    {
        return this.tween.setEaseInElastic();
    }
    public LTDescr setEaseOutElastic()
    {
        return this.tween.setEaseOutElastic();
    }
    public LTDescr setEaseInOutElastic()
    {
        return this.tween.setEaseInOutElastic();
    }
    public LTDescr setEasePunch()
    {
        return this.tween.setEasePunch();
    }
    public LTDescr setEaseShake()
    {
        return this.tween.setEaseShake();
    }
    public LTDescr setOvershoot(float overshoot)
    {
        return this.tween.setOvershoot(overshoot);
    }
    public LTDescr setPeriod(float period)
    {
        return this.tween.setPeriod(period);
    }
    public LTDescr setScale(float scale)
    {
        return this.tween.setScale(scale);
    }
    public LTDescr setEase(AnimationCurve easeCurve)
    {
        return this.tween.setEase(easeCurve);
    }
    public LTDescr setTo(Vector3 to)
    {
        return this.tween.setTo(to);
    }
    public LTDescr setTo(Transform to)
    {
        return this.tween.setTo(to);
    }
    public LTDescr setFrom(Vector3 from)
    {
        return this.tween.setFrom(from);
    }
    public LTDescr setFrom(float from)
    {
        return this.tween.setFrom(from);
    }
    public LTDescr setDiff(Vector3 diff)
    {
        return this.tween.setDiff(diff);
    }
    public LTDescr setHasInitialized(bool has)
    {
        return this.tween.setHasInitialized(has);
    }
    public LTDescr setId(uint id, uint global_counter)
    {
        return this.tween.setId(id, global_counter);
    }
    public LTDescr setPassed(float passed)
    {
        return this.tween.setPassed(passed);
    }
    public LTDescr setTime(float time)
    {
        return this.tween.setTime(time);
    }
    public LTDescr setSpeed(float speed)
    {
        return this.tween.setSpeed(speed);
    }
    public LTDescr setRepeat(int repeat)
    {
        return this.tween.setRepeat(repeat);
    }
    public LTDescr setLoopType(LeanTweenType loopType)
    {
        return this.tween.setLoopType(loopType);
    }
    public LTDescr setUseEstimatedTime(bool useEstimatedTime)
    {
        return this.tween.setUseEstimatedTime(useEstimatedTime);
    }
    public LTDescr setIgnoreTimeScale(bool useUnScaledTime)
    {
        return this.tween.setIgnoreTimeScale(useUnScaledTime);
    }
    public LTDescr setUseFrames(bool useFrames)
    {
        return this.tween.setUseFrames(useFrames);
    }
    public LTDescr setUseManualTime(bool useManualTime)
    {
        return this.tween.setUseManualTime(useManualTime);
    }
    public LTDescr setLoopCount(int loopCount)
    {
        return this.tween.setLoopCount(loopCount);
    }
    public LTDescr setLoopOnce()
    {
        return this.tween.setLoopOnce();
    }
    public LTDescr setLoopClamp()
    {
        return this.tween.setLoopClamp();
    }
    public LTDescr setLoopClamp(int loops)
    {
        return this.tween.setLoopClamp(loops);
    }
    public LTDescr setLoopPingPong()
    {
        return this.tween.setLoopPingPong();
    }
    public LTDescr setLoopPingPong(int loops)
    {
        return this.tween.setLoopPingPong(loops);
    }
    public LTDescr setOnComplete(Action onComplete)
    {
        return this.tween.setOnComplete(onComplete);
    }
    public LTDescr setOnComplete(Action<object> onComplete)
    {
        return this.tween.setOnComplete(onComplete);
    }
    public LTDescr setOnComplete(Action<object> onComplete, object onCompleteParam)
    {
        return this.tween.setOnComplete(onComplete, onCompleteParam);
    }
    public LTDescr setOnCompleteParam(object onCompleteParam)
    {
        return this.tween.setOnCompleteParam(onCompleteParam);
    }
    public LTDescr setOnUpdate(Action<float> onUpdate)
    {
        return this.tween.setOnUpdate(onUpdate);
    }
    public LTDescr setOnUpdateRatio(Action<float, float> onUpdate)
    {
        return this.tween.setOnUpdateRatio(onUpdate);
    }
    public LTDescr setOnUpdateObject(Action<float, object> onUpdate)
    {
        return this.tween.setOnUpdateObject(onUpdate);
    }
    public LTDescr setOnUpdateVector2(Action<Vector2> onUpdate)
    {
        return this.tween.setOnUpdateVector2(onUpdate);
    }
    public LTDescr setOnUpdateVector3(Action<Vector3> onUpdate)
    {
        return this.tween.setOnUpdateVector3(onUpdate);
    }
    public LTDescr setOnUpdateColor(Action<Color> onUpdate)
    {
        return this.tween.setOnUpdateColor(onUpdate);
    }
    public LTDescr setOnUpdateColor(Action<Color, object> onUpdate)
    {
        return this.tween.setOnUpdateColor(onUpdate);
    }
    public LTDescr setOnUpdate(Action<Color> onUpdate)
    {
        return this.tween.setOnUpdate(onUpdate);
    }
    public LTDescr setOnUpdate(Action<Color, object> onUpdate)
    {
        return this.tween.setOnUpdate(onUpdate);
    }
    public LTDescr setOnUpdate(Action<float, object> onUpdate, object onUpdateParam = null)
    {
        return this.tween.setOnUpdate(onUpdate, onUpdateParam);
    }
    public LTDescr setOnUpdate(Action<Vector3, object> onUpdate, object onUpdateParam = null)
    {
        return this.tween.setOnUpdate(onUpdate, onUpdateParam);
    }
    public LTDescr setOnUpdate(Action<Vector2> onUpdate, object onUpdateParam = null)
    {
        return this.tween.setOnUpdate(onUpdate, onUpdateParam);
    }
    public LTDescr setOnUpdate(Action<Vector3> onUpdate, object onUpdateParam = null)
    {
        return this.tween.setOnUpdate(onUpdate, onUpdateParam);
    }
    public LTDescr setOnUpdateParam(object onUpdateParam)
    {
        return this.tween.setOnUpdateParam(onUpdateParam);
    }
    public LTDescr setOrientToPath(bool doesOrient)
    {
        return this.tween.setOrientToPath(doesOrient);
    }
    public LTDescr setOrientToPath2d(bool doesOrient2d)
    {
        return this.tween.setOrientToPath2d(doesOrient2d);
    }
    public LTDescr setRect(LTRect rect)
    {
        return this.tween.setRect(rect);
    }
    public LTDescr setRect(Rect rect)
    {
        return this.tween.setRect(rect);
    }
    public LTDescr setPath(LTBezierPath path)
    {
        return this.tween.setPath(path);
    }
    public LTDescr setPoint(Vector3 point)
    {
        return this.tween.setPoint(point);
    }
    public LTDescr setDestroyOnComplete(bool doesDestroy)
    {
        return this.tween.setDestroyOnComplete(doesDestroy);
    }
    public LTDescr setAudio(object audio)
    {
        return this.tween.setAudio(audio);
    }
    public LTDescr setOnCompleteOnRepeat(bool isOn)
    {
        return this.tween.setOnCompleteOnRepeat(isOn);
    }
    public LTDescr setOnCompleteOnStart(bool isOn)
    {
        return this.tween.setOnCompleteOnStart(isOn);
    }
    public LTDescr setRect(RectTransform rect)
    {
        return this.tween.setRect(rect);
    }
    public LTDescr setSprites(UnityEngine.Sprite[] sprites)
    {
        return this.tween.setSprites(sprites);
    }
    public LTDescr setFrameRate(float frameRate)
    {
        return this.tween.setFrameRate(frameRate);
    }
    public LTDescr setOnStart(Action onStart)
    {
        return this.tween.setOnStart(onStart);
    }
    public LTDescr setDirection(float direction)
    {
        return this.tween.setDirection(direction);
    }
    public LTDescr setRecursive(bool useRecursion)
    {
        return this.tween.setRecursive(useRecursion);
    }
    #endregion
}
