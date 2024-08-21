using UniqueIdGenerator.Net;

namespace _Mafia_API.Helpers
{
    public class SnowflakeGlobal
    {

        private static Mutex mutex = new Mutex();


        public static string Generate()
        {
            Generator SnowflakeGen = new Generator((short)Thread.CurrentThread.ManagedThreadId, new DateTime(2023, 1, 1));
            mutex.WaitOne();
            string result = SnowflakeGen.NextLong().ToString();
            mutex.ReleaseMutex();

            return result;
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
