using System;
using System.Data.SqlClient;
using System.Windows;

namespace DogClubApp
{
    public partial class RegisterWindow : Window
    {
        private string _connectionString = "data source=stud-mssql.sttec.yar.ru,38325;initial catalog=user91_db;persist security info=True;user id=user91_db;password=user91;multipleactiveresultsets=True;encrypt=True;trustservercertificate=True;application name=DogClubApp";

        public RegisterWindow()
        {
            InitializeComponent();
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TbFio.Text) || string.IsNullOrWhiteSpace(TbPhone.Text) ||
                string.IsNullOrWhiteSpace(TbLogin.Text) || string.IsNullOrWhiteSpace(PbPassword.Password))
            {
                MessageBox.Show("Заполните обязательные поля (ФИО, Телефон, Логин, Пароль)!");
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();

                    SqlCommand checkLogin = new SqlCommand("SELECT COUNT(*) FROM KURS_User WHERE Login = @login", conn);
                    checkLogin.Parameters.AddWithValue("@login", TbLogin.Text);
                    if ((int)checkLogin.ExecuteScalar() > 0)
                    {
                        MessageBox.Show("Пользователь с таким логином уже существует!");
                        return;
                    }

                    SqlCommand cmdVladelec = new SqlCommand(
                        "INSERT INTO KURS_Vladelec (FIO, Telefon, Email) VALUES (@fio, @phone, @email)", conn);
                    cmdVladelec.Parameters.AddWithValue("@fio", TbLogin.Text);
                    cmdVladelec.Parameters.AddWithValue("@phone", TbPhone.Text);
                    cmdVladelec.Parameters.AddWithValue("@email", TbEmail.Text);
                    cmdVladelec.ExecuteNonQuery();

                    SqlCommand cmdUser = new SqlCommand(
                        "INSERT INTO KURS_User (Login, Parol, Role) VALUES (@login, @pass, 'Visitor')", conn);
                    cmdUser.Parameters.AddWithValue("@login", TbLogin.Text);
                    cmdUser.Parameters.AddWithValue("@pass", PbPassword.Password);
                    cmdUser.ExecuteNonQuery();
                }

                MessageBox.Show("Регистрация успешна! Теперь вы можете войти.");
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка регистрации: " + ex.Message);
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}