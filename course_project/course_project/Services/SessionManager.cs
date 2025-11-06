using course_project.Models;

namespace course_project.Services
{
    public static class SessionManager
    {
        public static User CurrentUser { get; private set; }

        public static void SetCurrentUser(User user)
        {
            CurrentUser = user;
        }

        public static void ClearSession()
        {
            CurrentUser = null;
        }

        public static bool IsLoggedIn => CurrentUser != null;
    }
}