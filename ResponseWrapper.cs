using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace _Mafia_API
{
    [JsonConverter(typeof(JsonStringEnumMemberConverter))]
    public enum AuthFailureReasons
    {

        [JsonPropertyName("NoUser")]
        SessionExpired = 0,

    }

    [JsonConverter(typeof(JsonStringEnumMemberConverter))]
    public enum WrResponseStatus
    {
        [JsonPropertyName("InternalError")]
        InternalError = 0,

        [JsonPropertyName("Ok")]
        Ok,

        [JsonPropertyName("ServerDown")]
        ServerDown,

        [JsonPropertyName("NotFound")]
        NotFound,

        [JsonPropertyName("InvalidValues")]
        InvalidValues,

        [JsonPropertyName("InvalidAction")]
        InvalidAction,

    }

    public class ResponseWrapper<R> where R : class?
    {
        public ResponseWrapper(WrResponseStatus status, [AllowNull] R response = null, List<string>? AuthenticationFailureReasons = null)
        {
            this.status = status;
            this.response = response;

            if (AuthenticationFailureReasons != null)
            {
                this.authenticationFailureReasons = new List<AuthFailureReasons>();
                foreach (var reason in AuthenticationFailureReasons)
                {
                    this.authenticationFailureReasons.Add((AuthFailureReasons)Enum.Parse(typeof(AuthFailureReasons), reason));
                }
            }
        }
        public ResponseWrapper(string status, [AllowNull] R response = null)
        {
            this.status = (WrResponseStatus)Enum.Parse(typeof(WrResponseStatus), status);
            this.response = response;
        }

        public WrResponseStatus status { get; set; }
        public List<AuthFailureReasons>? authenticationFailureReasons { get; set; } = null;
        public R? response { get; set; }
    }
}