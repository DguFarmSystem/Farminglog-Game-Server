// System
using System;
using System.Collections.Generic;

// Fusion
using Fusion;
using Fusion.Sockets;

// Unity
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class MinigameServerRunner : MonoBehaviour, INetworkRunnerCallbacks
{
    public GameObject rpsPlayerPrefab; // 가위바위보 플레이어

    private GameObject playerPrefabToUse;

    async void Start()
    {
        Debug.Log("Starting Server...");

        // 프리팹 선택
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName == "RPS")
            playerPrefabToUse = rpsPlayerPrefab;
        else
            playerPrefabToUse = null;

        var runner = gameObject.AddComponent<NetworkRunner>();
        runner.ProvideInput = false;
        runner.AddCallbacks(this);

        await runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Server,
            SessionName = "TestRoom",
            Scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex),
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log("Player Joined Server");
        var obj = runner.Spawn(playerPrefabToUse, new Vector3(player.RawEncoded % 5, 0, 0), Quaternion.identity, player);
        if (obj != null)
        {
            Debug.Log("Spawn Successful");

            if (runner.GetPlayerObject(player) == null)
            {
                Debug.LogWarning("Force Mapping...");
                runner.SetPlayerObject(player, obj);
            }


        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"Player Left: {player}");

        NetworkObject obj = runner.GetPlayerObject(player);
        if (obj != null)
        {
            runner.Despawn(obj);
            Debug.Log("Player Object Removal Successful");
        }
    }

    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
}
