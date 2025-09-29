namespace _Mafia_API.Helpers
{
    public static class DotEnv
    {
        public static void Load(string filePath)
        {

            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", "googleUser.json");

            if (!File.Exists(filePath))
                return;

            foreach (var line in File.ReadAllLines(filePath))
            {
                if (line.Trim().StartsWith("#"))
                    continue;

                var parts = line.Trim().Split('=');

                if (parts.Length != 2)
                    continue;

                if (parts[1].StartsWith("\"") && parts[1].EndsWith("\""))
                    parts[1] = parts[1].Substring(1, parts[1].Length - 2);

                Environment.SetEnvironmentVariable(parts[0], parts[1]);
            }
        }
    }
}
