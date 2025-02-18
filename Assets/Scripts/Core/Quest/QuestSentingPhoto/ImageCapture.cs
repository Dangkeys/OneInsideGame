using UnityEngine;

[CreateAssetMenu(fileName = "NewImage", menuName = "ScriptableObjects/ImageCapture")]
public class ImageCapture : ScriptableObject
{
    [SerializeField] private string imageName;
    [SerializeField] private Sprite image;

    public string GetName()
    {
        return imageName;
    }

    public Sprite GetImage()
    {
        return image;
    }
}
