using course_project.Models;
using System.Text.Json;

namespace course_project.Services;

public class UserService
{
    private readonly string _filePath = "C:\\projects\\CP_2\\course_project\\course_project\\Files\\Users.json";
    private List<User> _users;

    public UserService()
    {
        LoadUsers();
    }

    private void LoadUsers()
    {
        try
        {
            if (!File.Exists(_filePath))
            {
                _users = new List<User>();
                return;
            }

            string jsonData = File.ReadAllText(_filePath);
            _users = JsonSerializer.Deserialize<List<User>>(jsonData);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при загрузке пользователей: {ex.Message}");
            _users = new List<User>();
        }
    }

    public List<User> GetUsers()
    {
        return _users;
    }
}
