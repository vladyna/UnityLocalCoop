using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Test.Services
{
    public class LanIpService
    {
        public string GetBestLanIpv4Address()
        {
            foreach (var ip in GetLanIpv4Addresses())
            {
                return ip;
            }

            return "127.0.0.1";
        }

        public IEnumerable<string> GetLanIpv4Addresses()
        {
            var nics = NetworkInterface.GetAllNetworkInterfaces();

            foreach (var nic in nics)
            {
                if (nic.OperationalStatus != OperationalStatus.Up)
                    continue;

                if (nic.NetworkInterfaceType is NetworkInterfaceType.Loopback or NetworkInterfaceType.Tunnel)
                    continue;

                var ipProps = nic.GetIPProperties();
                foreach (var ua in ipProps.UnicastAddresses)
                {
                    if (ua.Address.AddressFamily != AddressFamily.InterNetwork)
                        continue;

                    var ip = ua.Address.ToString();
                    if (ip.StartsWith("169.254.")) 
                        continue;

                    if (ip == "127.0.0.1")
                        continue;

                    yield return ip;
                }
            }
        }
    }
}
