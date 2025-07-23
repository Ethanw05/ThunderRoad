// Decompiled with JetBrains decompiler
// Type: ThunderRoad.AnimatorParamController
// Assembly: ThunderRoad, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 440ED6B9-B010-414D-A078-366B18988020
// Assembly location: A:\BAS DEV\ThunderRoad.dll
// XML documentation location: A:\BAS DEV\ThunderRoad.xml

using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace ThunderRoad;

[RequireComponent(typeof (Animator))]
[AddComponentMenu("ThunderRoad/Animator Param Controller")]
[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Event-Linkers/AnimatorParamController.html")]
public class AnimatorParamController : MonoBehaviour
{
  [Tooltip("Only works if this controller is on an item!")]
  public bool saveValuesOnStore;
  [Tooltip("Only works if this controller is on an item!")]
  public bool loadValuesOnLoad;
  private Dictionary<string, AnimatorParamController.ParsedOperation> preParsedOperations = new Dictionary<string, AnimatorParamController.ParsedOperation>();
  private Dictionary<string, AnimatorControllerParameterType> animatorParams = new Dictionary<string, AnimatorControllerParameterType>();

  public Animator animator { get; protected set; }

  public Item item { get; protected set; }

  protected void Start()
  {
    this.animator = this.GetComponent<Animator>();
    if ((UnityEngine.Object) this.animator == (UnityEngine.Object) null)
    {
      Debug.LogError((object) $"AnimatorParamController is on GameObject \"{this.gameObject.name}\" and has no animator on it. This shouldn't happen. The script will not function.");
    }
    else
    {
      this.item = this.GetComponentInParent<Item>();
      if ((UnityEngine.Object) this.item != (UnityEngine.Object) null)
      {
        if (this.saveValuesOnStore)
          this.item.OnContainerAddEvent += new Item.ContainerEvent(this.StoreParamValues);
        if (!this.loadValuesOnLoad)
          return;
        ContentCustomDataAnimatorParams customData;
        this.item.TryGetCustomData<ContentCustomDataAnimatorParams>(out customData);
        if (customData != null)
        {
          foreach (ContentCustomDataAnimatorParams.AnimatorParam savedParam in customData.savedParams)
          {
            switch (savedParam.type)
            {
              case AnimatorControllerParameterType.Float:
                this.animator.SetFloat(savedParam.name, savedParam.floatVal);
                continue;
              case AnimatorControllerParameterType.Int:
                this.animator.SetInteger(savedParam.name, savedParam.intVal);
                continue;
              case AnimatorControllerParameterType.Bool:
                this.animator.SetBool(savedParam.name, savedParam.boolVal);
                continue;
              default:
                continue;
            }
          }
        }
        else
          this.StoreParamValues((Container) null);
      }
      else
      {
        if (!this.saveValuesOnStore && !this.loadValuesOnLoad)
          return;
        Debug.LogError((object) $"AnimatorParamController on GameObject \"{this.gameObject.name}\" is set to store or load values, but isn't on an item to be able to store or load values!");
      }
    }
  }

  private void StoreParamValues(Container _)
  {
    if (this.animator == null)
      this.animator = this.GetComponent<Animator>();
    if ((UnityEngine.Object) this.animator == (UnityEngine.Object) null)
      Debug.LogError((object) (this.gameObject.name + " has no Animator on it!"));
    if (this.item == null)
      this.item = this.transform.root.GetComponentInChildren<Item>();
    if ((UnityEngine.Object) this.item == (UnityEngine.Object) null)
      Debug.LogError((object) (this.gameObject.name + " has an Animator, but no Item on it to be able to save parameter values!"));
    if (this.item.HasCustomData<ContentCustomDataAnimatorParams>())
      this.item.RemoveCustomData<ContentCustomDataAnimatorParams>();
    this.item.AddCustomData<ContentCustomDataAnimatorParams>(new ContentCustomDataAnimatorParams(this.animator));
  }

  public void SetTrigger(string triggerName)
  {
    this.animator.SetTrigger(Animator.StringToHash(triggerName));
  }

  public void BoolOperation(string input)
  {
    if ((UnityEngine.Object) this.animator == (UnityEngine.Object) null)
      return;
    AnimatorParamController.ParsedOperation parsedOperation;
    if (this.ParseInput(input, out parsedOperation))
    {
      if (" &|#".Contains(parsedOperation.oper.ToString()))
      {
        bool flag1;
        if (!this.StringToBool(parsedOperation.lhs, out flag1))
        {
          if (parsedOperation.lhs == "~")
          {
            flag1 = UnityEngine.Random.Range(0, 2) == 0;
          }
          else
          {
            Debug.LogError((object) ("Type error: First value in operation is not a valid boolean\nCould not process input bool operation\nError from: " + parsedOperation.originalOperation));
            return;
          }
        }
        if (parsedOperation.oper == ' ')
        {
          this.animator.SetBool(parsedOperation.paramHash, flag1);
        }
        else
        {
          bool flag2;
          if (!this.StringToBool(parsedOperation.rhs, out flag2))
          {
            Debug.LogError((object) ("Type error: Second value in operation is not a valid boolean\nCould not process input bool operation\nError from: " + parsedOperation.originalOperation));
          }
          else
          {
            switch (parsedOperation.oper)
            {
              case '#':
                this.animator.SetBool(parsedOperation.paramHash, flag1 | flag2 && !(flag1 & flag2));
                break;
              case '&':
                this.animator.SetBool(parsedOperation.paramHash, flag1 & flag2);
                break;
              case '|':
                this.animator.SetBool(parsedOperation.paramHash, flag1 | flag2);
                break;
            }
          }
        }
      }
      else
      {
        float a;
        if (!this.StringToFloat(parsedOperation.lhs, out a))
        {
          Debug.LogError((object) ("Type error: First value in comparison is not a valid number\nCould not process input bool operation\nError from: " + parsedOperation.originalOperation));
        }
        else
        {
          float b;
          if (!this.StringToFloat(parsedOperation.rhs, out b))
          {
            Debug.LogError((object) ("Type error: Second value in comparison is not a valid number\nCould not process input bool operation\nError from: " + parsedOperation.originalOperation));
          }
          else
          {
            switch (parsedOperation.oper)
            {
              case '<':
                this.animator.SetBool(parsedOperation.paramHash, (double) a < (double) b);
                break;
              case '=':
                this.animator.SetBool(parsedOperation.paramHash, Mathf.Approximately(a, b));
                break;
              case '>':
                this.animator.SetBool(parsedOperation.paramHash, (double) a > (double) b);
                break;
            }
          }
        }
      }
    }
    else
      Debug.LogError((object) ("Could not process input bool operation\nError from: " + parsedOperation.originalOperation));
  }

  private bool StringToBool(string input, out bool value)
  {
    bool flag = input.Contains("!");
    string name = input.Replace("!", "");
    if (name.ToLower() == "true" || name.ToLower() == "false")
    {
      value = name.ToLower() == "true";
      if (flag)
        value = !value;
      return true;
    }
    if (this.CheckForParam(name) == 1)
    {
      value = this.animator.GetBool(Animator.StringToHash(name));
      if (flag)
        value = !value;
      return true;
    }
    value = false;
    return false;
  }

  public void IntegerOperation(string input)
  {
    if ((UnityEngine.Object) this.animator == (UnityEngine.Object) null)
      return;
    AnimatorParamController.ParsedOperation parsedOperation;
    if (this.ParseInput(input, out parsedOperation))
    {
      int num1;
      if (!this.StringToInt(parsedOperation.lhs, out num1))
        Debug.LogError((object) ("Type error: First value in operation is not a valid integer\nCould not process input integer operation\nError from: " + parsedOperation.originalOperation));
      else if (parsedOperation.oper == ' ')
      {
        this.animator.SetInteger(parsedOperation.paramHash, num1);
      }
      else
      {
        int num2;
        if (!this.StringToInt(parsedOperation.rhs, out num2))
        {
          Debug.LogError((object) ("Type error: Second value in operation is not a valid integer\nCould not process input integer operation\nError from: " + parsedOperation.originalOperation));
        }
        else
        {
          switch (parsedOperation.oper)
          {
            case '%':
              this.animator.SetInteger(parsedOperation.paramHash, num1 % num2);
              break;
            case '*':
              this.animator.SetInteger(parsedOperation.paramHash, num1 * num2);
              break;
            case '+':
              this.animator.SetInteger(parsedOperation.paramHash, num1 + num2);
              break;
            case '-':
              this.animator.SetInteger(parsedOperation.paramHash, num1 - num2);
              break;
            case '/':
              this.animator.SetInteger(parsedOperation.paramHash, num1 / num2);
              break;
            case '?':
              this.animator.SetInteger(parsedOperation.paramHash, UnityEngine.Random.Range(num1, num2));
              break;
            case '[':
              this.animator.SetInteger(parsedOperation.paramHash, Mathf.Max(num1, num2));
              break;
            case ']':
              this.animator.SetInteger(parsedOperation.paramHash, Mathf.Min(num1, num2));
              break;
            case '^':
              this.animator.SetInteger(parsedOperation.paramHash, (int) Mathf.Pow((float) num1, (float) num2));
              break;
          }
        }
      }
    }
    else
      Debug.LogError((object) ("Could not process input integer operation\nError from: " + parsedOperation.originalOperation));
  }

  private bool StringToInt(string input, out int value)
  {
    if (int.TryParse(input, out value))
      return true;
    if (this.CheckForParam(input) == 2)
    {
      value = this.animator.GetInteger(Animator.StringToHash(input));
      return true;
    }
    value = 0;
    return false;
  }

  public void FloatOperation(string input)
  {
    if ((UnityEngine.Object) this.animator == (UnityEngine.Object) null)
      return;
    AnimatorParamController.ParsedOperation parsedOperation;
    if (this.ParseInput(input, out parsedOperation))
    {
      float num1;
      if (!this.StringToFloat(parsedOperation.lhs, out num1))
        Debug.LogError((object) ("Type error: First value in operation is not a valid float\nCould not process input float operation\nError from: " + parsedOperation.originalOperation));
      else if (parsedOperation.oper == ' ')
      {
        this.animator.SetFloat(parsedOperation.paramHash, num1);
      }
      else
      {
        float num2;
        if (!this.StringToFloat(parsedOperation.rhs, out num2))
        {
          Debug.LogError((object) ("Type error: Second value in operation is not a valid float\nCould not process input float operation\nError from: " + parsedOperation.originalOperation));
        }
        else
        {
          switch (parsedOperation.oper)
          {
            case '%':
              this.animator.SetFloat(parsedOperation.paramHash, num1 % num2);
              break;
            case '*':
              this.animator.SetFloat(parsedOperation.paramHash, num1 * num2);
              break;
            case '+':
              this.animator.SetFloat(parsedOperation.paramHash, num1 + num2);
              break;
            case '-':
              this.animator.SetFloat(parsedOperation.paramHash, num1 - num2);
              break;
            case '/':
              this.animator.SetFloat(parsedOperation.paramHash, num1 / num2);
              break;
            case '?':
              this.animator.SetFloat(parsedOperation.paramHash, UnityEngine.Random.Range(num1, num2));
              break;
            case '[':
              this.animator.SetFloat(parsedOperation.paramHash, Mathf.Max(num1, num2));
              break;
            case ']':
              this.animator.SetFloat(parsedOperation.paramHash, Mathf.Min(num1, num2));
              break;
            case '^':
              this.animator.SetFloat(parsedOperation.paramHash, Mathf.Pow(num1, num2));
              break;
          }
        }
      }
    }
    else
      Debug.LogError((object) ("Could not process input float operation\nError from: " + parsedOperation.originalOperation));
  }

  private bool StringToFloat(string input, out float value)
  {
    if (float.TryParse(input, out value))
      return true;
    int num = this.CheckForParam(input);
    if (num > 1)
    {
      if (num == 2)
        value = (float) this.animator.GetInteger(Animator.StringToHash(input));
      if (num == 3)
        value = this.animator.GetFloat(Animator.StringToHash(input));
      return true;
    }
    value = 0.0f;
    return false;
  }

  public bool ParseInput(
    string input,
    out AnimatorParamController.ParsedOperation parsedOperation)
  {
    parsedOperation = new AnimatorParamController.ParsedOperation();
    if ((UnityEngine.Object) this.animator == (UnityEngine.Object) null)
      return false;
    if (this.preParsedOperations.TryGetValue(input, out parsedOperation))
      return true;
    parsedOperation = new AnimatorParamController.ParsedOperation();
    string str1 = input.Replace(" ", "");
    parsedOperation.originalOperation = input;
    if (!str1.Contains("="))
    {
      Debug.LogError((object) "Synax error in operation: Missing assignment (=)");
      return false;
    }
    string[] strArray1 = str1.Split(new char[1]{ '=' }, 2);
    if (strArray1.Length < 2)
    {
      Debug.LogError((object) "Synax error in operation: Assignment is missing left or right side");
      return false;
    }
    if (string.IsNullOrEmpty(strArray1[0]) || string.IsNullOrEmpty(strArray1[1]))
    {
      Debug.LogError((object) "Synax error in operation: Left or right side of operation is empty");
      return false;
    }
    int num = this.CheckForParam(strArray1[0]);
    if (num == 0)
    {
      Debug.LogError((object) "Synax error in operation: Right hand side is not a valid bool, int, or float animator parameter name");
      return false;
    }
    string str2 = "&|#<>=";
    string str3 = "+-*/%^[]?";
    char ch1 = ' ';
    for (int index = 0; index < strArray1[1].Length; ++index)
    {
      string str4 = str2;
      char ch2 = strArray1[1][index];
      string str5 = ch2.ToString();
      bool flag1 = str4.Contains(str5);
      string str6 = str3;
      ch2 = strArray1[1][index];
      string str7 = ch2.ToString();
      bool flag2 = str6.Contains(str7);
      if (flag1 | flag2)
      {
        if (ch1 != ' ')
        {
          Debug.LogError((object) "Synax error in operation: Too many operators");
          return false;
        }
        if (flag1 && num != 1)
        {
          Debug.LogError((object) "Synax error in operation: Bool operator in int or float assignment");
          return false;
        }
        if (flag2 && num == 1)
        {
          Debug.LogError((object) "Synax error in operation: Number operator in bool assignment");
          return false;
        }
        ch1 = strArray1[1][index];
      }
    }
    parsedOperation.paramHash = Animator.StringToHash(strArray1[0]);
    parsedOperation.oper = ch1;
    if (ch1 != ' ')
    {
      string[] strArray2 = strArray1[1].Split(ch1, StringSplitOptions.None);
      parsedOperation.lhs = strArray2[0];
      parsedOperation.rhs = strArray2[1];
    }
    else
      parsedOperation.lhs = strArray1[1];
    this.preParsedOperations.Add(input, parsedOperation);
    return true;
  }

  public int CheckForParam(string name)
  {
    if ((UnityEngine.Object) this.animator == (UnityEngine.Object) null)
      return 0;
    if (this.animatorParams.Count == 0)
    {
      foreach (AnimatorControllerParameter parameter in this.animator.parameters)
      {
        if (!this.animatorParams.ContainsKey(parameter.name))
          this.animatorParams.Add(parameter.name, parameter.type);
      }
    }
    AnimatorControllerParameterType controllerParameterType;
    if (!this.animatorParams.TryGetValue(name.Replace("!", ""), out controllerParameterType))
      return 0;
    switch (controllerParameterType)
    {
      case AnimatorControllerParameterType.Float:
        return 3;
      case AnimatorControllerParameterType.Int:
        return 2;
      case AnimatorControllerParameterType.Bool:
        return 1;
      default:
        return 0;
    }
  }

  public class ParsedOperation
  {
    public int paramHash;
    public string lhs = "";
    public char oper = ' ';
    public string rhs = "";
    public string originalOperation = "";
  }
}
