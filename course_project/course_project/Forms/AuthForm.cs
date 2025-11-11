using System.Text.Json;
using course_project.Forms;
using course_project.Models;
using course_project.Services;

namespace course_project
{
    public partial class AuthForm : Form
    {
        public AuthForm()
        {
            InitializeComponent();
            this.MaximizeBox = false;
            this.MinimizeBox = false;
        }

        private void AuthForm_Load(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            string loginFromUser = textBoxLogin.Text;
            string passwordFromUser = textBoxPassword.Text;
            
            //ошибка: если пуста строка
            if (string.IsNullOrWhiteSpace(loginFromUser) || string.IsNullOrWhiteSpace(passwordFromUser))
            {
                MessageBox.Show("Пожалуйста, введите логин и пароль.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            
            try
            {
                string jsonFilePath = 
                    "C:\\\\Users\\\\stepankonon\\\\Documents\\\\CP_2\\\\course_project\\\\course_project\\\\Files\\\\Users.json";
                
                if (!File.Exists(jsonFilePath))
                {
                    MessageBox.Show("Файл данных пользователей не найден!", "Критическая ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string jsonData = File.ReadAllText(jsonFilePath);
                List<User> users = JsonSerializer.Deserialize<List<User>>(jsonData);
                
                User foundUser = users.FirstOrDefault(user => user.UserName == loginFromUser);
                
                if (foundUser != null && AuthService.VerifyPassword(passwordFromUser, foundUser.HashedPassword))
                {
                    MessageBox.Show($"Добро пожаловать, {foundUser.UserName}!", "Авторизация успешна", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    this.Hide();
                    
                    SessionManager.SetCurrentUser(foundUser);
                    
                    if (foundUser.UserName == "admin")
                    {
                        var managerForm = new ManagerMenuForm();
                        managerForm.Show();
                    }
                    else if (foundUser.UserName == "seller")
                    {
                        var sellerForm = new SellerMenuForm();
                        sellerForm.Show();
                    }
                }
                else
                {
                    MessageBox.Show("Неверный логин или пароль.", "Ошибка авторизации", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            
        }

        private void textBoxLogin_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsLetter(e.KeyChar) && !char.IsControl(e.KeyChar) && !char.IsWhiteSpace(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void textBoxPassword_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar) && !char.IsWhiteSpace(e.KeyChar))
            {
                e.Handled = true;
            }
        }
    }
}
