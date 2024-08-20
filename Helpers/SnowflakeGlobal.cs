using UniqueIdGenerator.Net;

namespace _Mafia_API.Helpers
{
    public class SnowflakeGlobal
    {
        private static Generator SnowflakeGen = new Generator(0, new DateTime(2023, 1, 1));

        public static string Generate()
        {
            return SnowflakeGen.NextLong().ToString();
        }


        public static string ToBase64(string snowflakeId)
        {
            return ToBase64(ulong.Parse(snowflakeId));
        }

        public static string ToBase64(ulong snowflakeId)
        {
            const string chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            var result = string.Empty;

            while (snowflakeId > 0)
            {
                result = chars[(int)(snowflakeId % 62)] + result;
                snowflakeId /= 62;
            }

            return result;
        }

        public static string FromBase64(string base64String)
        {
            const string chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            ulong result = 0;
            foreach (var c in base64String)
            {
                result = result * 62 + (ulong)chars.IndexOf(c);
            }

            return result.ToString();
        }

    }
}
