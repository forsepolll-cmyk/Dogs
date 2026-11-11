using System;
using System.Data.SqlClient;
using System.Windows;

namespace DogClubApp
{
    public partial class LoginWindow : Window
    {
        private string _connectionString = "data source=stud-mssql.sttec.yar.ru,38325;initial catalog=user91_db;persist security info=True;user id=user91_db;password=user91;multipleactiveresultsets=True;encrypt=True;trustservercertificate=True;application name=DogClubApp";

        public LoginWindow()
        {
            InitializeComponent();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string login = TbLogin.Text.Trim();
            string pass = PbPassword.Password.Trim();

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(pass))
            {
                MessageBox.Show("Введите логин и пароль!");
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = "SELECT Role FROM KURS_User WHERE Login = @login AND Parol = @pass";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@login", login);
                    cmd.Parameters.AddWithValue("@pass", pass);

                    object roleObj = cmd.ExecuteScalar();
                    if (roleObj != null)
                    {
                        string role = roleObj.ToString();
                        MainWindow main = new MainWindow(login, role);
                        main.Show();
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Неверный логин или пароль!");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка подключения к БД: " + ex.Message);
            }
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            RegisterWindow reg = new RegisterWindow();
            reg.Owner = this;
            reg.ShowDialog();
        }
    }
}