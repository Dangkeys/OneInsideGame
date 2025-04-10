using System.Collections.Generic;
using UnityEngine;

public class HitboxUtilities : MonoBehaviour
{
    public enum ColliderType
    {
        Any, // All collider types
        CharacterController, // Only CharacterControllers
        Other // Any collider that is not a CharacterController
    }

    public class HitboxParams
    {
        public BoxCollider[] Hitboxs { get; set; } // "Hitboxs" instead of "Hitboxes"
        public string Tag { get; set; } = null;
        public ColliderType Type { get; set; } = ColliderType.Any; // "Type" instead of "ColliderType"
        public Collider[] Exclude { get; set; } = null; // Colliders to exclude
    }

    //----------------------------------------------------------------------------------------

    /// <summary>
    /// Gets all colliders touching the given hitbox or hitboxes, filtered by parameters inside HitboxParams.
    /// </summary>
    /// <param name="parameters">Encapsulates hitboxes, tag, and collider type as parameters.</param>
    /// <returns>A list of colliders currently touching the hitboxes.</returns>
    public static List<Collider> GetTouchingColliders(HitboxParams parameters)
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
                    if (parameters.Type == ColliderType.Any ||
                        (parameters.Type == ColliderType.CharacterController && collider.GetComponent<CharacterController>() != null) ||
                        (parameters.Type == ColliderType.Other && collider.GetComponent<CharacterController>() == null))
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
    public static List<Collider> GetTouchingColliders(BoxCollider hitbox, string tag = null, ColliderType colliderType = ColliderType.Any, Collider exclude = null)
    {
        return GetTouchingColliders(new HitboxParams
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
    public static List<Collider> GetTouchingColliders(BoxCollider hitbox, string tag = null, ColliderType colliderType = ColliderType.Any, Collider[] exclude = null)
    {
        return GetTouchingColliders(new HitboxParams
        {
            Hitboxs = new BoxCollider[] { hitbox },
            Tag = tag,
            Type = colliderType,
            Exclude = exclude
        });
    }

    public static List<GameObject> GetTouchingObjects(HitboxParams parameters)
    {
        List<GameObject> objectsList = new List<GameObject>();

        foreach (Collider thisCollider in GetTouchingColliders(parameters).ToArray())
        {
            objectsList.Add(thisCollider.gameObject);
        }

        return objectsList;
    }

    //----------------------------------------------------------------------------------------
}
