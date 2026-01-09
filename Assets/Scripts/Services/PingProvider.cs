using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

namespace Test.Services
{
    public class PingProvider
    {
        public string GetPingForClient(ulong clientId)
        {
            var nm = NetworkManager.Singleton;
            if (nm == null)
                return "-";

            var transport = nm.GetComponent<UnityTransport>();
            if (transport == null)
                return "-";

            return transport.GetCurrentRtt(clientId).ToString();
        }
    }
}
