using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public static class Loader
{
    private static GameScene targetScene;

    public static void Load(GameScene targetScene)
    {
        Loader.targetScene = targetScene;
        SceneManager.LoadScene(GameScene.LoadingScene.ToString());
    }
    public static Task LoadNetwork(GameScene targetScene)
    {
        var taskCompletionSource = new TaskCompletionSource<bool>();

        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnLoadComplete;

        void OnLoadComplete(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
        {
            taskCompletionSource.SetResult(true);
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnLoadComplete;
        }

        NetworkManager.Singleton.SceneManager.LoadScene(targetScene.ToString(), LoadSceneMode.Single);
        return taskCompletionSource.Task;
    }

    private static void OnLoadComplete(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut) => throw new NotImplementedException();

    public static void LoaderCallback()
    {
        SceneManager.LoadScene(targetScene.ToString());
    }
}