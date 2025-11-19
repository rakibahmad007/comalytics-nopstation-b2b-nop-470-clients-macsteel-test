using JWT;
using JWT.Algorithms;
using JWT.Serializers;
namespace Nop.Plugin.Misc.ErpWebhook.Extensions;

public class JwtHelper
{
    public static IJwtEncoder JwtEncoder
    {
        get
        {
            var algorithm = new HMACSHA256Algorithm();
            var serializer = new JsonNetSerializer();
            var urlEncoder = new JwtBase64UrlEncoder();
            return new JwtEncoder(algorithm, serializer, urlEncoder);
        }
    }

    public static IJwtDecoder JwtDecoder
    {
        get
        {
            var serializer = new JsonNetSerializer();
            var provider = new UtcDateTimeProvider();
            var validator = new JwtValidator(serializer, provider);
            var urlEncoder = new JwtBase64UrlEncoder();
            var algorithm = new HMACSHA256Algorithm();
            return new JwtDecoder(serializer, validator, urlEncoder);
        }
    }
}
