using System.Linq;
using System.Net;
using System.Net.Sockets;
using Microsoft.AspNetCore.Http;
using PensionMS.Core.Constants;

namespace PensionMS.API.Helpers;

public static class RequestHelper
{
    public static string GetClientIp(HttpContext context)
    {
        var ip = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (string.IsNullOrEmpty(ip))
            ip = context.Connection.RemoteIpAddress?.ToString();
            
        if (ip == InfrastructureConstants.LoopbackIpv6 || ip == InfrastructureConstants.LoopbackIpv4)
        {
            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                var localIp = host.AddressList
                    .FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(a));
                ip = localIp?.ToString() ?? InfrastructureConstants.LoopbackIpv4;
            }
            catch { ip = InfrastructureConstants.LoopbackIpv4; }
        }
        return ip ?? InfrastructureConstants.LoopbackIpv4;
    }
}
