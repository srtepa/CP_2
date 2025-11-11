using course_project.Models;

namespace course_project.Services
{
    public static class SessionManager
    {
        public static User CurrentUser { get; private set; }
        
        // НОВОЕ: Свойство для хранения временного доступа
        public static bool HasTemporaryAdminAccess { get; private set; }

        public static void SetCurrentUser(User user)
        {
            CurrentUser = user;
        }

        // НОВЫЕ МЕТОДЫ: для управления временным доступом
        public static void GrantTemporaryAdminAccess()
        {
            HasTemporaryAdminAccess = true;
        }

        public static void RevokeTemporaryAdminAccess()
        {
            HasTemporaryAdminAccess = false;
        }

        public static void ClearSession()
        {
            CurrentUser = null;
            // Сбрасываем временный доступ при выходе из системы
            HasTemporaryAdminAccess = false; 
        }

        public static bool IsLoggedIn => CurrentUser != null;
    }
}