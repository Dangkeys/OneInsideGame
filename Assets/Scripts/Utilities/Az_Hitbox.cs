using System.Collections.Generic;
using System.Linq;
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

          // Create exclude set only if we have exclusions
          HashSet<Collider> excludeSet = parameters.Exclude?.ToHashSet();

          // Pre-check if we need to do tag comparison
          bool checkTag = !string.IsNullOrEmpty(parameters.Tag);

          foreach (BoxCollider hitbox in parameters.Hitboxs)
          {
               if (hitbox == null)
                    continue;  // Skip if hitbox is null

               // Transform hitbox properties to world space
               Vector3 worldCenter = hitbox.transform.TransformPoint(hitbox.center);
               Vector3 worldHalfExtents = Vector3.Scale(hitbox.size * 0.5f, hitbox.transform.lossyScale);
               Quaternion worldRotation = hitbox.transform.rotation;

               // Get all overlapping colliders
               Collider[] colliders = Physics.OverlapBox(worldCenter, worldHalfExtents, worldRotation);

               foreach (Collider collider in colliders)
               {
                    // Skip invalid colliders or self-collision
                    if (collider == null || collider == hitbox)
                         continue;

                    // Skip if collider is in exclude list
                    if (excludeSet != null && excludeSet.Contains(collider))
                         continue;

                    // Skip if tag doesn't match (when tag checking is needed)
                    if (checkTag && !collider.CompareTag(parameters.Tag))
                         continue;

                    // Handle collider type filtering
                    bool shouldAdd = parameters.Type switch
                    {
                         Collider_Type.Any => true,
                         Collider_Type.CharacterController => collider.TryGetComponent<CharacterController>(out _),
                         Collider_Type.Other => !collider.TryGetComponent<CharacterController>(out _),
                         _ => false
                    };

                    if (shouldAdd && !touchingColliders.Contains(collider))
                    {
                         touchingColliders.Add(collider);
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
