using System.Collections.Generic;
using UnityEngine;

public class Az_Hitbox : MonoBehaviour
{
     //----------------------------------------------------------------------------------------

     /// <summary>
     /// Gets all colliders touching the given hitbox or hitboxes, filtered by an optional tag.
     /// </summary>
     /// <param name="hitboxes">A single BoxCollider or an array of BoxColliders.</param>
     /// <param name="tag">Optional tag to filter colliders. If null, all colliders are returned.</param>
     /// <returns>A list of colliders currently touching the hitbox or hitboxes.</returns>
     public static List<Collider> Get_Touching_Colliders(BoxCollider[] hitboxes, string tag = null)
     {
          List<Collider> touchingColliders = new List<Collider>();

          foreach (BoxCollider hitbox in hitboxes)
          {
               Vector3 center = hitbox.transform.TransformPoint(hitbox.center);
               Vector3 size = hitbox.size * 0.5f; // OverlapBox takes half-extents

               Collider[] colliders = Physics.OverlapBox(center, size, hitbox.transform.rotation);

               foreach (Collider collider in colliders)
               {
                    if (collider != hitbox && (string.IsNullOrEmpty(tag) || collider.CompareTag(tag)))
                    {
                         touchingColliders.Add(collider);
                    }
               }
          }

          return touchingColliders;
     }

     public static List<Collider> Get_Touching_Colliders(BoxCollider hitbox, string tag = null)
     {
          return Get_Touching_Colliders(new BoxCollider[] { hitbox }, tag);
     }

     //----------------------------------------------------------------------------------------
}