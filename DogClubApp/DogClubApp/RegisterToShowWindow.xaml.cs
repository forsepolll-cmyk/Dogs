using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;

namespace DogClubApp
{
    public partial class RegisterToShowWindow : Window
    {
        private string _connStr;
        private string _login;

        public RegisterToShowWindow(string connStr, string login)
        {
            InitializeComponent();
            _connStr = connStr;
            _login = login;
            LoadCombos();
        }

        private void LoadCombos()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connStr))
                {
                    conn.Open();

                    string searchFio = _login;
                    if (_login.ToLower() == "ivanov") searchFio = "Иванов Иван Иванович";
                    else if (_login.ToLower() == "petrova") searchFio = "Петрова Анна Сергеевна";
                    else if (_login.ToLower() == "sidorov") searchFio = "Сидоров Петр Алексеевич";
                    else if (_login.ToLower() == "smirnova") searchFio = "Смирнова Ольга Викторовна";
                    else if (_login.ToLower() == "kuznetsov") searchFio = "Кузнецов Дмитрий Олегович";

                    SqlCommand cmdDogs = new SqlCommand(@"SELECT s.Kod_sobaki, s.Klichka FROM KURS_Sobaka s 
                                                  LEFT JOIN KURS_Vladelec v ON s.Kod_vladelca = v.Kod_vladelca 
                                                  WHERE v.FIO LIKE @fio", conn);
                    cmdDogs.Parameters.AddWithValue("@fio", "%" + searchFio + "%");
                    SqlDataAdapter daDogs = new SqlDataAdapter(cmdDogs);
                    DataTable dtDogs = new DataTable();
                    daDogs.Fill(dtDogs);
                    CbDog.ItemsSource = dtDogs.DefaultView;
                    CbDog.DisplayMemberPath = "Klichka";
                    CbDog.SelectedValuePath = "Kod_sobaki";

                    SqlDataAdapter daShows = new SqlDataAdapter("SELECT Kod_vystavki, Nazvanie FROM KURS_Vystavka", conn);
                    DataTable dtShows = new DataTable();
                    daShows.Fill(dtShows);
                    CbShow.ItemsSource = dtShows.DefaultView;
                    CbShow.DisplayMemberPath = "Nazvanie";
                    CbShow.SelectedValuePath = "Kod_vystavki";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            if (CbDog.SelectedValue == null || CbShow.SelectedValue == null) { MessageBox.Show("Выберите собаку и выставку!"); return; }

            try
            {
                using (SqlConnection conn = new SqlConnection(_connStr))
                {
                    conn.Open();
                    SqlCommand check = new SqlCommand("SELECT COUNT(*) FROM KURS_Uchastnik WHERE Kod_sobaki = @dog AND Kod_vystavki = @show", conn);
                    check.Parameters.AddWithValue("@dog", CbDog.SelectedValue);
                    check.Parameters.AddWithValue("@show", CbShow.SelectedValue);
                    int count = (int)check.ExecuteScalar();

                    if (count > 0) { MessageBox.Show("Эта собака уже записана на выставку!"); return; }

                    SqlCommand cmd = new SqlCommand("INSERT INTO KURS_Uchastnik (Kod_sobaki, Kod_vystavki) VALUES (@dog, @show)", conn);
                    cmd.Parameters.AddWithValue("@dog", CbDog.SelectedValue);
                    cmd.Parameters.AddWithValue("@show", CbShow.SelectedValue);
                    cmd.ExecuteNonQuery();
                }
                MessageBox.Show("Вы успешно записаны!");
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex) { MessageBox.Show("Ошибка записи: " + ex.Message); }
        }
    }
}