// Decompiled with JetBrains decompiler
// Type: ThunderRoad.FaceAnimator
// Assembly: ThunderRoad, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 440ED6B9-B010-414D-A078-366B18988020
// Assembly location: A:\BAS DEV\ThunderRoad.dll
// XML documentation location: A:\BAS DEV\ThunderRoad.xml

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace ThunderRoad;

[RequireComponent(typeof (Animator))]
public class FaceAnimator : ThunderBehaviour
{
  private static FaceAnimator.Expression[] _expressionList;
  public float expressionChangeSpeedMax = 15f;
  public float varianceChangeSpeedMax = 1f;
  public float smoothTime = 0.15f;
  public bool autoUpdate;
  public AnimatorOverrideController animatorOverrideController;
  public AnimationClip overrideA;
  public AnimationClip overrideB;
  private FaceAnimator.Expression _currentPrimaryExpression;
  [NonSerialized]
  public bool brainDriven;
  protected AnimatorOverrideController runtimeAnimatorOverrideController;
  protected KeyValuePair<AnimationClip, AnimationClip>[] animationClipOverrides;
  protected static int[] expressionHashes;
  protected int hashDynamicExpression;
  protected int hashLoopDynamic;
  protected float animationEndTime;
  protected float? neutralExpressionTime;

  public static FaceAnimator.Expression[] expressionList
  {
    get
    {
      if (Utils.IsNullOrEmpty(FaceAnimator._expressionList))
        FaceAnimator._expressionList = (FaceAnimator.Expression[]) Enum.GetValues(typeof (FaceAnimator.Expression));
      return FaceAnimator._expressionList;
    }
  }

  public bool animated { get; protected set; }

  public FaceAnimator.Expression currentPrimaryExpression
  {
    get => this._currentPrimaryExpression;
    protected set
    {
      this._currentPrimaryExpression = value;
      this.expressionValues.target = new float[FaceAnimator.expressionList.Length];
      this.expressionValues.target[(int) value] = 1f;
    }
  }

  public FaceAnimator.Expression lastPrimaryExpression { get; protected set; }

  public bool customExpression { get; protected set; }

  public FaceAnimator.SmoothDampTarget expressionValues { get; protected set; }

  public FaceAnimator.SmoothDampTarget varianceValues { get; protected set; }

  public Animator animator { get; protected set; }

  private void Start()
  {
    this.animator = this.GetComponent<Animator>();
    this.runtimeAnimatorOverrideController = new AnimatorOverrideController(this.animatorOverrideController.runtimeAnimatorController);
    this.runtimeAnimatorOverrideController.name = "FacialOverrideAnimator";
    List<KeyValuePair<AnimationClip, AnimationClip>> overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
    this.animatorOverrideController.GetOverrides(overrides);
    List<KeyValuePair<AnimationClip, AnimationClip>> keyValuePairList = new List<KeyValuePair<AnimationClip, AnimationClip>>()
    {
      new KeyValuePair<AnimationClip, AnimationClip>(this.overrideA, this.overrideA),
      new KeyValuePair<AnimationClip, AnimationClip>(this.overrideB, this.overrideB)
    };
    foreach (KeyValuePair<AnimationClip, AnimationClip> keyValuePair in overrides)
    {
      if (!((UnityEngine.Object) keyValuePair.Key == (UnityEngine.Object) this.overrideA) && !((UnityEngine.Object) keyValuePair.Key == (UnityEngine.Object) this.overrideB))
        keyValuePairList.Add(keyValuePair);
    }
    this.animationClipOverrides = keyValuePairList.ToArray();
    this.animator.runtimeAnimatorController = (RuntimeAnimatorController) this.runtimeAnimatorOverrideController;
    this.runtimeAnimatorOverrideController.ApplyOverrides((IList<KeyValuePair<AnimationClip, AnimationClip>>) overrides);
    this.expressionValues = new FaceAnimator.SmoothDampTarget(FaceAnimator.expressionList.Length);
    this.varianceValues = new FaceAnimator.SmoothDampTarget(FaceAnimator.expressionList.Length);
    if (FaceAnimator.expressionHashes == null)
      FaceAnimator.expressionHashes = new int[FaceAnimator.expressionList.Length];
    for (int index = 0; index < FaceAnimator.expressionList.Length; ++index)
    {
      FaceAnimator.Expression expression = FaceAnimator.expressionList[index];
      FaceAnimator.expressionHashes[(int) expression] = Animator.StringToHash(expression.ToString());
      this.expressionValues.target[(int) expression] = this.animator.GetFloat(FaceAnimator.expressionHashes[(int) expression]);
      this.expressionValues.current[(int) expression] = this.animator.GetFloat(FaceAnimator.expressionHashes[(int) expression]);
    }
    this.hashDynamicExpression = Animator.StringToHash("DynamicExpression");
    this.hashLoopDynamic = Animator.StringToHash("LoopDynamic");
    this.currentPrimaryExpression = FaceAnimator.Expression.Neutral;
  }

  public event FaceAnimator.ExpressionChange OnExpressionChanged;

  public void SetExpression(FaceAnimator.Expression expression)
  {
    this.SetExpression(expression, new float?());
  }

  public bool SetExpression(FaceAnimator.Expression expression, float? resetNeutralTime = null)
  {
    if (expression == this.currentPrimaryExpression)
      return false;
    this.lastPrimaryExpression = this.currentPrimaryExpression;
    this.currentPrimaryExpression = expression;
    float? nullable1;
    if (!resetNeutralTime.HasValue)
    {
      nullable1 = new float?();
    }
    else
    {
      float time = Time.time;
      float? nullable2 = resetNeutralTime;
      nullable1 = nullable2.HasValue ? new float?(time + nullable2.GetValueOrDefault()) : new float?();
    }
    this.neutralExpressionTime = nullable1;
    this.customExpression = false;
    FaceAnimator.ExpressionChange expressionChanged = this.OnExpressionChanged;
    if (expressionChanged != null)
      expressionChanged(this.lastPrimaryExpression, expression);
    return true;
  }

  public void SetCustomExpressionValues(float[] targets)
  {
    if (Utils.IsNullOrEmpty(targets) || targets.Length != this.expressionValues.target.Length)
      return;
    this.expressionValues.target = targets;
    this.customExpression = true;
  }

  public void PlayAnimation(AnimationClip clip) => this.PlayAnimation(clip, false);

  public void PlayAnimation(AnimationClip clip, bool loop)
  {
    this.animator.SetBool(this.hashLoopDynamic, loop);
    int num = this.animator.GetInteger(this.hashDynamicExpression) == 1 ? 2 : 1;
    this.animationClipOverrides[num - 1] = new KeyValuePair<AnimationClip, AnimationClip>(this.animationClipOverrides[num - 1].Key, clip);
    this.runtimeAnimatorOverrideController.ApplyOverrides((IList<KeyValuePair<AnimationClip, AnimationClip>>) this.animationClipOverrides);
    this.animator.SetInteger(this.hashDynamicExpression, num);
    this.animated = true;
    this.animationEndTime = Time.time + clip.length;
    this.StartCoroutine(this.AnimationEnd(this.animationEndTime));
  }

  protected IEnumerator AnimationEnd(float endTime)
  {
    yield return (object) Yielders.ForSeconds(endTime - Time.time);
    if (endTime.IsApproximately(this.animationEndTime) || (double) endTime >= (double) this.animationEndTime)
      this.animated = false;
  }

  public void StopAnimation()
  {
    this.animator.SetInteger(this.hashDynamicExpression, 0);
    this.animator.SetBool(this.hashLoopDynamic, false);
    this.animated = false;
  }

  public void SmoothDampChange(FaceAnimator.SmoothDampTarget values, float maxChangeSpeed)
  {
    this.smoothTime = Mathf.Max(0.0001f, this.smoothTime);
    float num1 = 2f / this.smoothTime;
    float num2 = num1 * Time.deltaTime;
    float num3 = (float) (1.0 / (1.0 + (double) num2 + 0.47999998927116394 * (double) num2 * (double) num2 + 0.23499999940395355 * (double) num2 * (double) num2 * (double) num2));
    values.ZeroEphemerals();
    for (int index = 0; index < values.floatCount; ++index)
    {
      values.modified_target[index] = values.target[index];
      values.change_values[index] = values.current[index] - values.target[index];
    }
    float num4 = maxChangeSpeed * this.smoothTime;
    float num5 = num4 * num4;
    float d = 0.0f;
    for (int index = 0; index < values.change_values.Length; ++index)
      d += values.change_values[index] * values.change_values[index];
    float num6 = 0.0f;
    float num7 = (float) Math.Sqrt((double) d);
    float deltaTime = Time.deltaTime;
    for (int index = 0; index < values.floatCount; ++index)
    {
      if ((double) d > (double) num5)
        values.change_values[index] = values.change_values[index] / num7 * num4;
      values.modified_target[index] = values.current[index] - values.change_values[index];
      values.temp_values[index] = (values.velocity[index] + num1 * values.change_values[index]) * deltaTime;
      values.velocity[index] = (values.velocity[index] - num1 * values.temp_values[index]) * num3;
      values.output_values[index] = values.modified_target[index] + (values.change_values[index] + values.temp_values[index]) * num3;
      values.orig_minus_current[index] = values.target[index] - values.current[index];
      values.out_minus_orig[index] = values.output_values[index] - values.target[index];
      num6 += values.orig_minus_current[index] * values.out_minus_orig[index];
    }
    for (int index = 0; index < values.floatCount; ++index)
    {
      if ((double) num6 > 0.0)
      {
        values.output_values[index] = values.target[index];
        values.velocity[index] = (values.output_values[index] - values.target[index]) / Time.deltaTime;
      }
      values.current[index] = values.output_values[index];
    }
  }

  public override ManagedLoops EnabledManagedLoops => ManagedLoops.Update;

  protected internal override void ManagedUpdate()
  {
    base.ManagedUpdate();
    this.AutoUpdates();
  }

  public void AutoUpdates()
  {
    if (!this.autoUpdate)
      return;
    if (this.neutralExpressionTime.HasValue && (double) Time.time >= (double) this.neutralExpressionTime.Value)
      this.SetExpression(FaceAnimator.Expression.Neutral);
    if (this.brainDriven)
      return;
    this.SmoothDampChange(this.expressionValues, this.expressionChangeSpeedMax);
    this.UpdateAnimator();
  }

  public void UpdateAnimator()
  {
    for (int index = 0; index < FaceAnimator.expressionList.Length; ++index)
    {
      FaceAnimator.Expression expression = FaceAnimator.expressionList[index];
      float num = this.expressionValues.current[(int) expression] + this.varianceValues.current[(int) expression];
      this.animator.SetFloat(FaceAnimator.expressionHashes[(int) expression], num);
    }
  }

  private void OnValidate()
  {
  }

  public enum Expression
  {
    Angry,
    Attack,
    Confusion,
    Death,
    Fear,
    Happy,
    Intrigue,
    Neutral,
    Pain,
    Sad,
    Surprise,
    Tired,
  }

  public class SmoothDampTarget
  {
    public float[] target;
    public float[] current;
    public float[] velocity;
    public float[] change_values;
    public float[] modified_target;
    public float[] temp_values;
    public float[] output_values;
    public float[] orig_minus_current;
    public float[] out_minus_orig;

    public int floatCount { get; protected set; }

    public SmoothDampTarget(int count)
    {
      this.floatCount = count;
      this.target = new float[count];
      this.current = new float[count];
      this.velocity = new float[count];
      this.change_values = new float[count];
      this.modified_target = new float[count];
      this.temp_values = new float[count];
      this.output_values = new float[count];
      this.orig_minus_current = new float[count];
      this.out_minus_orig = new float[count];
    }

    public void ZeroEphemerals()
    {
      for (int index = 0; index < this.floatCount; ++index)
      {
        this.change_values[index] = 0.0f;
        this.modified_target[index] = 0.0f;
        this.temp_values[index] = 0.0f;
        this.output_values[index] = 0.0f;
        this.orig_minus_current[index] = 0.0f;
        this.out_minus_orig[index] = 0.0f;
      }
    }
  }

  public delegate void ExpressionChange(
    FaceAnimator.Expression previousExpression,
    FaceAnimator.Expression newExpression);
}
