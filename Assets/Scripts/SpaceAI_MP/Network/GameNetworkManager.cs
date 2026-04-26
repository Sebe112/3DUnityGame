using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace AI.Network
{
    public class GameNetworkManager : MonoBehaviour
    {
        private void Start()
        {
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetConnectionData("127.0.0.1", 7777);
            transport.UseWebSockets = true;

#if UNITY_EDITOR
            NetworkManager.Singleton.StartHost();
#else
            NetworkManager.Singleton.StartClient();
#endif
        }
    }
}