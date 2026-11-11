using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace DogClubApp
{
    public partial class MainWindow : Window
    {
        private string _connectionString = "data source=stud-mssql.sttec.yar.ru,38325;initial catalog=user91_db;persist security info=True;user id=user91_db;password=user91;multipleactiveresultsets=True;encrypt=True;trustservercertificate=True;application name=DogClubApp";
        private string _currentUserRole;
        private string _currentUserLogin;

        public MainWindow(string login, string role)
        {
            InitializeComponent();
            _currentUserRole = role;
            _currentUserLogin = login;
            TbUserInfo.Text = $"Пользователь: {login} ({role})";

            ApplyPermissions();
            LoadAllData();
        }

        private void ApplyPermissions()
        {
            if (_currentUserRole != "Admin")
            {
                TabSobaki.Visibility = Visibility.Collapsed;
                TabVladelcy.Visibility = Visibility.Collapsed;
                TabPorody.Visibility = Visibility.Collapsed;
                TabVystavki.Visibility = Visibility.Collapsed;
                TabUsers.Visibility = Visibility.Collapsed;
                TabReports.Visibility = Visibility.Collapsed;

                BtnMenuSobaki.Visibility = Visibility.Collapsed;
                BtnMenuVladelcy.Visibility = Visibility.Collapsed;
                BtnMenuPorody.Visibility = Visibility.Collapsed;
                BtnMenuVystavki.Visibility = Visibility.Collapsed;
                BtnMenuUsers.Visibility = Visibility.Collapsed;
                BtnMenuReports.Visibility = Visibility.Collapsed;

                PanelAddSobaka.Visibility = Visibility.Collapsed;
                PanelAddPoroda.Visibility = Visibility.Collapsed;
                PanelDeletePoroda.Visibility = Visibility.Collapsed;
                PanelAddVystavka.Visibility = Visibility.Collapsed;
                PanelDeleteVystavka.Visibility = Visibility.Collapsed;

                BtnDeleteUchastnik.Visibility = Visibility.Collapsed;
            }
            else
            {
                BtnRegisterToShow.Visibility = Visibility.Collapsed;

                TabMyDogs.Visibility = Visibility.Collapsed;
                BtnMenuMyDogs.Visibility = Visibility.Collapsed;
            }
        }

        private void LoadAllData()
        {
            LoadData("Sobaki", "SELECT s.Kod_sobaki AS [Код], s.Klichka AS [Кличка], p.Nazvanie AS [Порода], v.FIO AS [Владелец], s.Data_rozhdeniya AS [Дата рождения], s.Pol AS [Пол], s.Nomer_rodoslovnoy AS [Родословная] FROM KURS_Sobaka s LEFT JOIN KURS_Poroda p ON s.Kod_porody = p.Kod_porody LEFT JOIN KURS_Vladelec v ON s.Kod_vladelca = v.Kod_vladelca");
            LoadData("Vladelcy", "SELECT * FROM KURS_Vladelec");
            LoadData("Porody", "SELECT * FROM KURS_Poroda");
            LoadData("Vystavki", "SELECT * FROM KURS_Vystavka");
            LoadData("Uchastniki", "SELECT u.Kod_uchastnika AS [Код], s.Klichka AS [Собака], v.Nazvanie AS [Выставка], u.Ocenka AS [Оценка], u.Titul AS [Титул], u.Mesto AS [Место] FROM KURS_Uchastnik u LEFT JOIN KURS_Sobaka s ON u.Kod_sobaki = s.Kod_sobaki LEFT JOIN KURS_Vystavka v ON u.Kod_vystavki = v.Kod_vystavki");
            LoadData("Users", "SELECT * FROM KURS_User");

            LoadReportCombo();
            LoadMyDogs();
        }

        private void LoadData(string tableName, string query)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    switch (tableName)
                    {
                        case "Sobaki": GridSobaki.ItemsSource = dt.DefaultView; break;
                        case "Vladelcy": GridVladelcy.ItemsSource = dt.DefaultView; break;
                        case "Porody": GridPorody.ItemsSource = dt.DefaultView; break;
                        case "Vystavki": GridVystavki.ItemsSource = dt.DefaultView; break;
                        case "Uchastniki": GridUchastniki.ItemsSource = dt.DefaultView; break;
                        case "Users": GridUsers.ItemsSource = dt.DefaultView; break;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Ошибка загрузки данных: " + ex.Message); }
        }

        private void LoadReportCombo()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter("SELECT Kod_vystavki, Nazvanie FROM KURS_Vystavka", conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    CbReportVystavka.ItemsSource = dt.DefaultView;
                    CbReportVystavka.DisplayMemberPath = "Nazvanie";
                    CbReportVystavka.SelectedValuePath = "Kod_vystavki";
                }
            }
            catch (Exception ex) { MessageBox.Show("Ошибка загрузки выставок: " + ex.Message); }
        }

        private void LoadMyDogs()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();

                    string searchFio = _currentUserLogin;
                    if (_currentUserLogin.ToLower() == "ivanov") searchFio = "Иванов Иван Иванович";
                    else if (_currentUserLogin.ToLower() == "petrova") searchFio = "Петрова Анна Сергеевна";

                    string query = @"SELECT s.Kod_sobaki AS [Код], s.Klichka AS [Кличка], p.Nazvanie AS [Порода], s.Nomer_rodoslovnoy AS [Родословная] 
                             FROM KURS_Sobaka s 
                             LEFT JOIN KURS_Poroda p ON s.Kod_porody = p.Kod_porody
                             LEFT JOIN KURS_Vladelec v ON s.Kod_vladelca = v.Kod_vladelca
                             WHERE v.FIO LIKE @fio";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@fio", "%" + searchFio + "%");

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    GridMyDogs.ItemsSource = dt.DefaultView;
                }
            }
            catch (Exception ex) { MessageBox.Show("Ошибка загрузки 'Мои собаки': " + ex.Message); }
        }

        private void DeleteSobaka_Click(object sender, RoutedEventArgs e)
        {
            if (GridSobaki.SelectedItem == null) { MessageBox.Show("Выберите собаку!"); return; }
            DataRowView row = (DataRowView)GridSobaki.SelectedItem;
            int id = (int)row["Код"];
            if (MessageBox.Show("Удалить собаку?", "Внимание", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                ExecuteQuery("DELETE FROM KURS_Sobaka WHERE Kod_sobaki = @id", id);
                LoadData("Sobaki", "SELECT s.Kod_sobaki AS [Код], s.Klichka AS [Кличка], p.Nazvanie AS [Порода], v.FIO AS [Владелец], s.Data_rozhdeniya AS [Дата рождения], s.Pol AS [Пол], s.Nomer_rodoslovnoy AS [Родословная] FROM KURS_Sobaka s LEFT JOIN KURS_Poroda p ON s.Kod_porody = p.Kod_porody LEFT JOIN KURS_Vladelec v ON s.Kod_vladelca = v.Kod_vladelca");
            }
        }

        private void DeletePoroda_Click(object sender, RoutedEventArgs e)
        {
            if (GridPorody.SelectedItem == null) { MessageBox.Show("Выберите породу!"); return; }
            DataRowView row = (DataRowView)GridPorody.SelectedItem;
            int id = (int)row["Kod_porody"];
            if (MessageBox.Show("Удалить породу?", "Внимание", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    ExecuteQuery("DELETE FROM KURS_Poroda WHERE Kod_porody = @id", id);
                    LoadData("Porody", "SELECT * FROM KURS_Poroda");
                }
                catch { MessageBox.Show("Нельзя удалить породу, к которой привязаны собаки!"); }
            }
        }

        private void DeleteVystavka_Click(object sender, RoutedEventArgs e)
        {
            if (GridVystavki.SelectedItem == null) { MessageBox.Show("Выберите выставку!"); return; }
            DataRowView row = (DataRowView)GridVystavki.SelectedItem;
            int id = (int)row["Kod_vystavki"];
            if (MessageBox.Show("Удалить выставку?", "Внимание", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    ExecuteQuery("DELETE FROM KURS_Vystavka WHERE Kod_vystavki = @id", id);
                    LoadData("Vystavki", "SELECT * FROM KURS_Vystavka");
                    LoadReportCombo();
                }
                catch { MessageBox.Show("Нельзя удалить выставку, на которую есть участники!"); }
            }
        }

        private void DeleteUchastnik_Click(object sender, RoutedEventArgs e)
        {
            if (GridUchastniki.SelectedItem == null) { MessageBox.Show("Выберите запись!"); return; }
            DataRowView row = (DataRowView)GridUchastniki.SelectedItem;
            int id = (int)row["Код"];
            if (MessageBox.Show("Удалить запись участника?", "Внимание", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                ExecuteQuery("DELETE FROM KURS_Uchastnik WHERE Kod_uchastnika = @id", id);
                LoadData("Uchastniki", "SELECT u.Kod_uchastnika AS [Код], s.Klichka AS [Собака], v.Nazvanie AS [Выставка], u.Ocenka AS [Оценка], u.Titul AS [Титул], u.Mesto AS [Место] FROM KURS_Uchastnik u LEFT JOIN KURS_Sobaka s ON u.Kod_sobaki = s.Kod_sobaki LEFT JOIN KURS_Vystavka v ON u.Kod_vystavki = v.Kod_vystavki");
            }
        }

        private void ExecuteQuery(string query, int id)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex) { MessageBox.Show("Ошибка удаления: " + ex.Message); }
        }

        private void RegisterToShow_Click(object sender, RoutedEventArgs e)
        {
            RegisterToShowWindow reg = new RegisterToShowWindow(_connectionString, _currentUserLogin);
            if (reg.ShowDialog() == true)
            {
                LoadData("Uchastniki", "SELECT u.Kod_uchastnika AS [Код], s.Klichka AS [Собака], v.Nazvanie AS [Выставка], u.Ocenka AS [Оценка], u.Titul AS [Титул], u.Mesto AS [Место] FROM KURS_Uchastnik u LEFT JOIN KURS_Sobaka s ON u.Kod_sobaki = s.Kod_sobaki LEFT JOIN KURS_Vystavka v ON u.Kod_vystavki = v.Kod_vystavki");
            }
        }

        private void AddPoroda_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TbPorodaName.Text)) { MessageBox.Show("Введите название породы!"); return; }
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("INSERT INTO KURS_Poroda (Nazvanie, Opisanie) VALUES (@name, @desc)", conn);
                    cmd.Parameters.AddWithValue("@name", TbPorodaName.Text);
                    cmd.Parameters.AddWithValue("@desc", TbPorodaDesc.Text);
                    cmd.ExecuteNonQuery();
                }
                MessageBox.Show("Порода добавлена!");
                TbPorodaName.Text = ""; TbPorodaDesc.Text = "";
                LoadData("Porody", "SELECT * FROM KURS_Poroda");
            }
            catch (Exception ex) { MessageBox.Show("Ошибка: " + ex.Message); }
        }

        private void AddVystavka_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TbVystName.Text) || string.IsNullOrWhiteSpace(TbVystDate.Text)) { MessageBox.Show("Введите название и дату!"); return; }
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("INSERT INTO KURS_Vystavka (Nazvanie, Data_provedeniya, Mesto) VALUES (@name, @date, @place)", conn);
                    cmd.Parameters.AddWithValue("@name", TbVystName.Text);
                    cmd.Parameters.AddWithValue("@date", TbVystDate.Text);
                    cmd.Parameters.AddWithValue("@place", TbVystPlace.Text);
                    cmd.ExecuteNonQuery();
                }
                MessageBox.Show("Выставка создана!");
                TbVystName.Text = ""; TbVystDate.Text = ""; TbVystPlace.Text = "";
                LoadData("Vystavki", "SELECT * FROM KURS_Vystavka");
                LoadReportCombo();
            }
            catch (Exception ex) { MessageBox.Show("Ошибка: " + ex.Message); }
        }

        private void AddSobaka_Click(object sender, RoutedEventArgs e)
        {
            AddDogWindow addDog = new AddDogWindow(_connectionString);
            if (addDog.ShowDialog() == true)
            {
                LoadData("Sobaki", "SELECT s.Kod_sobaki AS [Код], s.Klichka AS [Кличка], p.Nazvanie AS [Порода], v.FIO AS [Владелец], s.Data_rozhdeniya AS [Дата рождения], s.Pol AS [Пол], s.Nomer_rodoslovnoy AS [Родословная] FROM KURS_Sobaka s LEFT JOIN KURS_Poroda p ON s.Kod_porody = p.Kod_porody LEFT JOIN KURS_Vladelec v ON s.Kod_vladelca = v.Kod_vladelca");
                LoadMyDogs();
            }
        }

        private void GenerateReport_Click(object sender, RoutedEventArgs e)
        {
            if (CbReportVystavka.SelectedValue == null) { MessageBox.Show("Выберите выставку!"); return; }
            try
            {
                int id = (int)CbReportVystavka.SelectedValue;
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(@"SELECT s.Klichka AS [Собака], p.Nazvanie AS [Порода], u.Ocenka AS [Оценка], u.Titul AS [Титул], u.Mesto AS [Место]
                                                      FROM KURS_Uchastnik u
                                                      LEFT JOIN KURS_Sobaka s ON u.Kod_sobaki = s.Kod_sobaki
                                                      LEFT JOIN KURS_Poroda p ON s.Kod_porody = p.Kod_porody
                                                      WHERE u.Kod_vystavki = @id", conn);
                    cmd.Parameters.AddWithValue("@id", id);
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    GridReport.ItemsSource = dt.DefaultView;
                }
            }
            catch (Exception ex) { MessageBox.Show("Ошибка: " + ex.Message); }
        }

        private void Menu_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null && int.TryParse(btn.Tag.ToString(), out int index)) MainTabControl.SelectedIndex = index;
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            new LoginWindow().Show();
            this.Close();
        }
    }
}