using System.Collections.Generic;
using UnityEngine;
using System;

public class GameUtilities : MonoBehaviour
{
    /// <summary>
    /// Returns the closest target to the given source position.
    /// </summary>
    /// <param name="srcPosition">The source position.</param>
    /// <param name="targets">The targets.</param>
    /// <returns>The closest target.</returns>
    public static GameObject GetClosetTarget(Vector3 srcPosition, List<GameObject> targets)
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

    /// <summary>
    /// Generates a unique id string which is a combination of:
    /// - The last 6 characters of the current UTC datetime in hexadecimal format.
    /// - A random number between 0 and 999999.
    ///
    /// The generated id is in the format: [PREFIX]XXXXXX_XXXXXX
    ///
    /// If a default prefix is provided and the current prefix is null, the default prefix will be used.
    /// If the current prefix is not null, it will be used.
    /// If both the default prefix and the current prefix are null, the prefix will be an empty string.
    /// </summary>
    /// <param name="defaultPrefix">The default prefix to use if the current prefix is null.</param>
    /// <param name="currentPrefix">The current prefix to use if it is not null.</param>
    /// <returns>The generated id string.</returns>
    public static string GenerateID(string defaultPrefix = null, string currentPrefix = null)
    {
        string timestamp = DateTime.UtcNow.Ticks.ToString("X");
        string random = UnityEngine.Random.Range(0, 1000000).ToString("D6");

        string prefix = currentPrefix?.ToUpper() ?? defaultPrefix;
        if (!string.IsNullOrEmpty(prefix))
        {
            prefix += "_";
        }

        return $"{prefix}{timestamp.Substring(timestamp.Length - 6)}_{random}";
    }
}