using BCrypt.Net;
using System;

namespace course_project.Services
{
    internal class AuthService
    {
        public static string HashPassword(string password)
        {
            //ошибка: если пуста строка
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("Пароль не может быть пустым.", nameof(password));
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
