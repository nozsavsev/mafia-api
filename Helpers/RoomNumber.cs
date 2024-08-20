namespace _Mafia_API.Helpers
{
    public class RoomNumber
    {

        public static string Generate()
        {
            Random random = new Random();
            int randomNumber = random.Next(100000, 1000000);
            return randomNumber.ToString();
        }

    }
}
