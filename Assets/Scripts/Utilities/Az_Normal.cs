using System.Collections.Generic;
using UnityEngine;

public class Az_Normal : MonoBehaviour
{
     /// <summary>
     /// Returns the closest target to the given source position.
     /// </summary>
     /// <param name="srcPosition">The source position.</param>
     /// <param name="targets">The targets.</param>
     /// <returns>The closest target.</returns>
     public static GameObject Get_Closet_Target(Vector3 srcPosition, List<GameObject> targets)
     {
          GameObject closestTarget = null;
          float closestDistance = Mathf.Infinity;

          foreach (GameObject target in targets)
          {
               float Distance = Vector3.Distance(target.transform.position, srcPosition);
               if (Distance < closestDistance)
               {
                    closestDistance = Distance;
                    closestTarget = target;
               }
          }

          return closestTarget;
     }
}
