using Unity.Netcode;

namespace Test.Services
{
    public class PingProvider
    {
        public string GetPingForClient(ulong clientId)
        {
            var nm = NetworkManager.Singleton;
            if (nm == null) return "-";

            try
            {
                var transportComp = nm.GetComponent("UnityTransport");

                if (transportComp != null)
                {
                    var mi = transportComp.GetType().GetMethod("GetCurrentRtt");
                    if (mi != null)
                    {
                        var val = mi.Invoke(transportComp, new object[] { clientId });
                        if (val != null)
                            return val.ToString();
                    }
                }
                return "-";
            }
            catch
            {
                return "-";
            }
        }
    }
}
