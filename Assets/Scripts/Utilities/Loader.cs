using Unity.Netcode;
using UnityEngine.SceneManagement;

public static class Loader {
    private static GameScene targetScene;

    public static void Load(GameScene targetScene)
    {
        Loader.targetScene = targetScene;
        SceneManager.LoadScene(GameScene.LoadingScene.ToString());
    }
    public static void LoadNetwork(GameScene targetScene)
    {
        NetworkManager.Singleton.SceneManager.LoadScene(targetScene.ToString(), LoadSceneMode.Single);
    }
    public static void LoaderCallback()
    {
        SceneManager.LoadScene(targetScene.ToString());
    }
}