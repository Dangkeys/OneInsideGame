using System.Collections.Generic;

public static class ShuffleUtility
{
    public static List<T> GetShuffledList<T>(IEnumerable<T> source)
    {
        var list = new List<T>(source);
        Shuffle(list);
        return list;
    }

    private static void Shuffle<T>(IList<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = UnityEngine.Random.Range(0, i + 1);
            (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
        }
    }
}