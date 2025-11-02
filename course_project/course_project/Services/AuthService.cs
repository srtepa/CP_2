namespace course_project.Services
{
    internal class AuthService
    {
        public static string HashPassword(string password)
        {
            long hash = 17; //начальное значение
            const int prime = 31; //простое число для умножения

            foreach (char c in password)
            {
                //формула: hash = hash * prime + character_code
                hash = hash * prime + (int)c;
            }

            return hash.ToString();
        }

        //функция проверки пароля
        public static bool VerifyPassword(string password, string hashedPassword)
        {
            string newHash = HashPassword(password);
            return newHash == hashedPassword;
        }
    }
}
