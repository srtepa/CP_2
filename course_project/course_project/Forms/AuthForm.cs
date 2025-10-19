using course_project.Services;

namespace course_project
{
    public partial class AuthForm : Form
    {
        private string _username;
        private string _hashPassword;
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
            string passwordFromUser = textBox2.Text;
            _hashPassword = AuthService.HashPassword(passwordFromUser);

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
