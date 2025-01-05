using System.Collections.Generic;
using UnityEngine;

public class Az_Hitbox : MonoBehaviour
{
     public enum Collider_Type
     {
          Any, // All collider types
          CharacterController, // Only CharacterControllers
          Other // Any collider that is not a CharacterController
     }

     public class HitboxParams
     {
          public BoxCollider[] Hitboxs { get; set; } // "Hitboxs" instead of "Hitboxes"
          public string Tag { get; set; } = null;
          public Collider_Type Type { get; set; } = Collider_Type.Any; // "Type" instead of "ColliderType"
          public Collider[] Exclude { get; set; } = null; // Colliders to exclude
     }

     //----------------------------------------------------------------------------------------

     /// <summary>
     /// Gets all colliders touching the given hitbox or hitboxes, filtered by parameters inside HitboxParams.
     /// </summary>
     /// <param name="parameters">Encapsulates hitboxes, tag, and collider type as parameters.</param>
     /// <returns>A list of colliders currently touching the hitboxes.</returns>
     public static List<Collider> Get_Touching_Colliders(HitboxParams parameters)
     {
          List<Collider> touchingColliders = new List<Collider>();
          HashSet<Collider> excludeSet = parameters.Exclude != null ? new HashSet<Collider>(parameters.Exclude) : null;

          foreach (BoxCollider hitbox in parameters.Hitboxs)
          {
               Vector3 center = hitbox.transform.TransformPoint(hitbox.center);
               Vector3 size = hitbox.size * 0.5f; // OverlapBox takes half-extents

               Collider[] colliders = Physics.OverlapBox(center, size, hitbox.transform.rotation);

               foreach (Collider collider in colliders)
               {
                    if (collider != hitbox &&
                        (excludeSet == null || !excludeSet.Contains(collider)) && // Exclude check
                        (string.IsNullOrEmpty(parameters.Tag) || collider.CompareTag(parameters.Tag)))
                    {
                         // Filter by collider type
                         if (parameters.Type == Collider_Type.Any ||
                             (parameters.Type == Collider_Type.CharacterController && collider.GetComponent<CharacterController>() != null) ||
                             (parameters.Type == Collider_Type.Other && collider.GetComponent<CharacterController>() == null))
                         {
                              touchingColliders.Add(collider);
                         }
                    }
               }
          }

          return touchingColliders;
     }

     /// <summary>
     /// Overload for a single BoxCollider using HitboxParams.
     /// </summary>
     /// <param name="hitbox">A single BoxCollider to check.</param>
     /// <param name="tag">Optional tag to filter colliders. If null, all colliders are returned.</param>
     /// <param name="colliderType">Specifies the type of collider to filter for.</param>
     /// <param name="exclude">Colliders to exclude from the results.</param>
     /// <returns>A list of colliders currently touching the hitbox.</returns>
     public static List<Collider> Get_Touching_Colliders(BoxCollider hitbox, string tag = null, Collider_Type colliderType = Collider_Type.Any, Collider exclude = null)
     {
          return Get_Touching_Colliders(new HitboxParams
          {
               Hitboxs = new BoxCollider[] { hitbox },
               Tag = tag,
               Type = colliderType,
               Exclude = exclude != null ? new Collider[] { exclude } : null
          });
     }

     /// <summary>
     /// Overload for a single BoxCollider using HitboxParams with multiple exclusions.
     /// </summary>
     /// <param name="hitbox">A single BoxCollider to check.</param>
     /// <param name="tag">Optional tag to filter colliders. If null, all colliders are returned.</param>
     /// <param name="colliderType">Specifies the type of collider to filter for.</param>
     /// <param name="exclude">Array of colliders to exclude from the results.</param>
     /// <returns>A list of colliders currently touching the hitbox.</returns>
     public static List<Collider> Get_Touching_Colliders(BoxCollider hitbox, string tag = null, Collider_Type colliderType = Collider_Type.Any, Collider[] exclude = null)
     {
          return Get_Touching_Colliders(new HitboxParams
          {
               Hitboxs = new BoxCollider[] { hitbox },
               Tag = tag,
               Type = colliderType,
               Exclude = exclude
          });
     }

     public static List<GameObject> Get_Touching_Objects(HitboxParams parameters)
     {
          List<GameObject> Objects_List = new List<GameObject>();

          foreach (Collider This_Collider in Get_Touching_Colliders(parameters).ToArray())
          {
               Objects_List.Add(This_Collider.gameObject);
          }

          return Objects_List;
     }

     //----------------------------------------------------------------------------------------
}
