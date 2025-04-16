using Nop.Core.Configuration;

namespace Nop.Web.API;

public class JwtConfig : IConfig
{
    public string Issuer { get; set; }
    public string Audience { get; set; }
    public string Key { get; set; }
    public int ExpireMinutes { get; set; }
}
