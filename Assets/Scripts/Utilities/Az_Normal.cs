using System.Collections.Generic;
using UnityEngine;

public class Az_Normal : MonoBehaviour
{
     /// <summary>
     /// Returns the closest target to the given source position.
     /// </summary>
     /// <param name="Src_Position">The source position.</param>
     /// <param name="Targets">The targets.</param>
     /// <returns>The closest target.</returns>
     public static GameObject Get_Closet_Target(Vector3 Src_Position, List<GameObject> Targets)
     {
          GameObject Closest_Target = null;
          float Closest_Distance = Mathf.Infinity;

          foreach (GameObject Target in Targets)
          {
               float Distance = Vector3.Distance(Target.transform.position, Src_Position);
               if (Distance < Closest_Distance)
               {
                    Closest_Distance = Distance;
                    Closest_Target = Target;
               }
          }

          return Closest_Target;
     }
}
