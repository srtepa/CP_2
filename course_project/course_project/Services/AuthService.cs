using BCrypt.Net;

namespace course_project.Services
{
    internal class AuthService
    {
        public static string HashPassword(string password)
        {
            //ошибка: если пуста строка
            if (string.IsNullOrEmpty(password) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Пожалуйста, введите пароль.");
            }

            return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 10);
        }

        //функция проверки пароля
        public static bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
    }
}
