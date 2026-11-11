using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace DogClubApp
{
    public partial class AddDogWindow : Window
    {
        private string _connectionString;

        public AddDogWindow(string connectionString)
        {
            InitializeComponent();
            _connectionString = connectionString;
            LoadCombos();
        }

        private void LoadCombos()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();

                    SqlDataAdapter adapterPoroda = new SqlDataAdapter("SELECT Kod_porody, Nazvanie FROM KURS_Poroda", conn);
                    DataTable dtPoroda = new DataTable();
                    adapterPoroda.Fill(dtPoroda);
                    CbPoroda.ItemsSource = dtPoroda.DefaultView;
                    CbPoroda.DisplayMemberPath = "Nazvanie";
                    CbPoroda.SelectedValuePath = "Kod_porody";

                    SqlDataAdapter adapterVladelec = new SqlDataAdapter("SELECT Kod_vladelca, FIO FROM KURS_Vladelec", conn);
                    DataTable dtVladelec = new DataTable();
                    adapterVladelec.Fill(dtVladelec);
                    CbVladelec.ItemsSource = dtVladelec.DefaultView;
                    CbVladelec.DisplayMemberPath = "FIO";
                    CbVladelec.SelectedValuePath = "Kod_vladelca";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки списков: " + ex.Message);
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TbKlichka.Text) || CbPoroda.SelectedValue == null ||
                CbVladelec.SelectedValue == null || string.IsNullOrWhiteSpace(TbRod.Text))
            {
                MessageBox.Show("Заполните обязательные поля (Кличка, Порода, Владелец, Родословная)!");
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = @"INSERT INTO KURS_Sobaka (Klichka, Kod_porody, Kod_vladelca, Data_rozhdeniya, Pol, Nomer_rodoslovnoy, Okras) 
                                     VALUES (@klichka, @poroda, @vladelec, @date, @pol, @rod, @okras)";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@klichka", TbKlichka.Text);
                    cmd.Parameters.AddWithValue("@poroda", CbPoroda.SelectedValue);
                    cmd.Parameters.AddWithValue("@vladelec", CbVladelec.SelectedValue);
                    cmd.Parameters.AddWithValue("@date", TbDate.Text);
                    cmd.Parameters.AddWithValue("@pol", (CbPol.SelectedItem as ComboBoxItem)?.Content.ToString());
                    cmd.Parameters.AddWithValue("@rod", TbRod.Text);
                    cmd.Parameters.AddWithValue("@okras", TbOkras.Text);

                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Собака добавлена!");
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка добавления: " + ex.Message);
            }
        }
    }
}