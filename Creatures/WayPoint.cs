// Decompiled with JetBrains decompiler
// Type: ThunderRoad.WayPoint
// Assembly: ThunderRoad, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 440ED6B9-B010-414D-A078-366B18988020
// Assembly location: A:\BAS DEV\ThunderRoad.dll
// XML documentation location: A:\BAS DEV\ThunderRoad.xml

using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

#nullable disable
namespace ThunderRoad;

[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Levels/WayPoint.html")]
public class WayPoint : MonoBehaviour
{
  [Tooltip("When ticked, does the creature turn to the direction of the Z axis/Blue arrow.")]
  [Header("Turn")]
  public bool turnToDirection;
  [Tooltip("The speed of which the creature turns to direction.")]
  public float turnSpeedRatio = 1f;
  [Tooltip("How long the NPC waits for at the waypoint.")]
  [Header("Wait")]
  public UnityEngine.Vector2 waitMinMaxDuration = new UnityEngine.Vector2(0.0f, 0.0f);
  [Header("Animation")]
  [Tooltip("When enabled, the creature will play the listed animation when at the waypoint")]
  public bool playAnimation;
  [Tooltip("ID of the animation you want the creature to play")]
  public string animationId;
  [Tooltip("The minimum angle of which the creature turns during the animation")]
  public float animationTurnMinAngle = 30f;
  [Tooltip("Specify the delay before playing an animation at the waypoint.")]
  public UnityEngine.Vector2 animationRandomMinMaxDelay = new UnityEngine.Vector2(0.0f, 0.0f);
  [Tooltip("Sets a behaviour tree the creature tries to complete while standing at this waypoint")]
  [Header("Action subtree")]
  public string actionBehaviorTreeID = "";
  [Tooltip("Sets an object for the creature to target if the behaviour tree needs one")]
  public Transform target;
  [Tooltip("How many times the creature needs to succeed the behaviour tree to proceed")]
  public int requiredTreeSuccessCount = 3;
  [Tooltip("How many times the creature should be able to fail the behaviour tree before it stops trying")]
  public int failuresToSkip = 10;
  protected static NavMeshPath navMeshPath;
  [NonSerialized]
  public AnimationData animationData;

  public List<ValueDropdownItem<string>> GetAllAnimationID()
  {
    return Catalog.GetDropdownAllID(Category.Animation);
  }

  private void Awake()
  {
    if (this.animationId == null || !(this.animationId != ""))
      return;
    this.animationData = Catalog.GetData<AnimationData>(this.animationId);
  }

  private void OnValidate()
  {
    if (!this.gameObject.activeInHierarchy)
      return;
    WayPoint.navMeshPath = new NavMeshPath();
  }

  public void OnDrawGizmos()
  {
    if (!(bool) (UnityEngine.Object) this.transform.parent)
      return;
    int siblingIndex = this.transform.GetSiblingIndex();
    Transform child = this.transform.parent.GetChild(siblingIndex < this.transform.parent.childCount - 1 ? siblingIndex + 1 : 0);
    if (!(bool) (UnityEngine.Object) child)
      return;
    NavMesh.CalculatePath(this.transform.position, child.transform.position, -1, WayPoint.navMeshPath);
    if (WayPoint.navMeshPath.status == NavMeshPathStatus.PathComplete)
    {
      float num1 = 0.5f;
      float num2 = 0.8f;
      int num3 = 40;
      UnityEngine.Color white = UnityEngine.Color.white;
      UnityEngine.Color magenta = UnityEngine.Color.magenta;
      Gizmos.color = WayPoint.navMeshPath.status == NavMeshPathStatus.PathPartial ? UnityEngine.Color.yellow : white;
      Gizmos.DrawSphere(this.transform.position, 0.15f);
      Gizmos.DrawLine(this.transform.position, WayPoint.navMeshPath.corners[0]);
      if (this.turnToDirection)
        Common.DrawGizmoArrow(this.transform.position, this.transform.forward * 0.5f, Gizmos.color);
      for (int index = 0; index < WayPoint.navMeshPath.corners.Length; ++index)
      {
        UnityEngine.Vector3 vector3_1 = new UnityEngine.Vector3();
        if (index < WayPoint.navMeshPath.corners.Length - 1)
          vector3_1 = WayPoint.navMeshPath.corners[index + 1] - WayPoint.navMeshPath.corners[index];
        if (vector3_1 != UnityEngine.Vector3.zero)
        {
          Gizmos.color = magenta;
          Gizmos.DrawRay(WayPoint.navMeshPath.corners[index], vector3_1);
          UnityEngine.Vector3 vector3_2 = Quaternion.LookRotation(vector3_1) * Quaternion.Euler(0.0f, (float) (180 + num3), 0.0f) * UnityEngine.Vector3.forward;
          UnityEngine.Vector3 vector3_3 = Quaternion.LookRotation(vector3_1) * Quaternion.Euler(0.0f, (float) (180 - num3), 0.0f) * UnityEngine.Vector3.forward;
          Gizmos.DrawRay(WayPoint.navMeshPath.corners[index] + vector3_1 * num2, vector3_2 * num1);
          Gizmos.DrawRay(WayPoint.navMeshPath.corners[index] + vector3_1 * num2, vector3_3 * num1);
        }
      }
    }
    else
    {
      if (WayPoint.navMeshPath.status != NavMeshPathStatus.PathInvalid)
        return;
      Gizmos.color = UnityEngine.Color.red;
      Gizmos.DrawSphere(this.transform.position, 0.15f);
    }
  }

  public void OnDrawGizmosSelected()
  {
    if ((bool) (UnityEngine.Object) this.transform.parent)
      this.name = "Waypoint" + this.transform.GetSiblingIndex().ToString();
    else
      this.name = "Waypoint_PleaseINeedAParent";
  }

  public static void SpawnerDrawGizmos(Transform spawner, Transform waypoints)
  {
    if ((bool) (UnityEngine.Object) waypoints && waypoints.childCount > 0)
    {
      NavMesh.CalculatePath(spawner.position, waypoints.GetChild(0).transform.position, -1, WayPoint.navMeshPath ?? (WayPoint.navMeshPath = new NavMeshPath()));
      if (WayPoint.navMeshPath.status == NavMeshPathStatus.PathComplete)
      {
        if ((double) spawner.position.y > (double) WayPoint.navMeshPath.corners[0].y)
        {
          Gizmos.color = WayPoint.navMeshPath.status == NavMeshPathStatus.PathPartial ? UnityEngine.Color.yellow : UnityEngine.Color.gray;
          Gizmos.DrawSphere(spawner.position, 0.15f);
          Gizmos.color = WayPoint.navMeshPath.status == NavMeshPathStatus.PathPartial ? UnityEngine.Color.yellow : UnityEngine.Color.green;
          Gizmos.DrawLine(spawner.position, WayPoint.navMeshPath.corners[0]);
          for (int index = 0; index < WayPoint.navMeshPath.corners.Length; ++index)
          {
            UnityEngine.Vector3 direction = new UnityEngine.Vector3();
            if (index < WayPoint.navMeshPath.corners.Length - 1)
              direction = WayPoint.navMeshPath.corners[index + 1] - WayPoint.navMeshPath.corners[index];
            if (direction != UnityEngine.Vector3.zero)
            {
              Gizmos.color = UnityEngine.Color.gray;
              Gizmos.DrawRay(WayPoint.navMeshPath.corners[index], direction);
            }
          }
        }
        else
        {
          Gizmos.color = UnityEngine.Color.red;
          Gizmos.DrawSphere(spawner.position, 0.15f);
        }
      }
      else
      {
        if (WayPoint.navMeshPath.status != NavMeshPathStatus.PathInvalid)
          return;
        Gizmos.color = UnityEngine.Color.red;
        Gizmos.DrawSphere(spawner.position, 0.15f);
      }
    }
    else
    {
      NavMeshHit hit;
      if (NavMesh.SamplePosition(spawner.position, out hit, 2f, -1) && (double) spawner.position.y > (double) hit.position.y)
      {
        Gizmos.color = UnityEngine.Color.gray;
        Gizmos.DrawSphere(spawner.position, 0.15f);
        Gizmos.color = UnityEngine.Color.green;
        Gizmos.DrawLine(spawner.position, hit.position);
      }
      else
      {
        Gizmos.color = UnityEngine.Color.red;
        Gizmos.DrawSphere(spawner.position, 0.15f);
      }
    }
  }
}
