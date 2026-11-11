using Microsoft.AspNetCore.Mvc;
using DogClubAPI.Data;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;

namespace DogClubAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExhibitionsController : ControllerBase
    {
        private readonly DbHelper _db;
        public ExhibitionsController(DbHelper db) => _db = db;

        [HttpGet]
        public IActionResult GetAll()
        {
            var dt = _db.ExecuteQuery("SELECT * FROM KURS_Vystavka");

            var list = new List<Dictionary<string, object>>();
            foreach (System.Data.DataRow row in dt.Rows)
            {
                var dict = new Dictionary<string, object>();
                foreach (System.Data.DataColumn col in dt.Columns)
                {
                    dict[col.ColumnName] = row[col];
                }
                list.Add(dict);
            }

            return Ok(list);
        }

        [HttpGet("{id}/results")]
        public IActionResult GetResults(int id)
        {
            string query = @"SELECT s.Klichka, p.Nazvanie AS Poroda, u.Ocenka, u.Titul, u.Mesto 
                     FROM KURS_Uchastnik u
                     LEFT JOIN KURS_Sobaka s ON u.Kod_sobaki = s.Kod_sobaki
                     LEFT JOIN KURS_Poroda p ON s.Kod_porody = p.Kod_porody
                     WHERE u.Kod_vystavki = @id";

            var param = new SqlParameter("@id", id);
            var dt = _db.ExecuteQuery(query, new[] { param });

            var list = new List<Dictionary<string, object>>();
            foreach (System.Data.DataRow row in dt.Rows)
            {
                var dict = new Dictionary<string, object>();
                foreach (System.Data.DataColumn col in dt.Columns)
                {
                    dict[col.ColumnName] = row[col];
                }
                list.Add(dict);
            }

            return Ok(list);
        }

        [HttpPost("{id}/register")]
        public IActionResult RegisterDog(int id, [FromBody] RegisterDto dto)
        {
            string checkQuery = "SELECT COUNT(*) FROM KURS_Uchastnik WHERE Kod_vystavki = @vyst AND Kod_sobaki = @sob";
            SqlParameter[] checkParams = {
                new SqlParameter("@vyst", id),
                new SqlParameter("@sob", dto.DogId)
            };
            var dt = _db.ExecuteQuery(checkQuery, checkParams);

            if (Convert.ToInt32(dt.Rows[0][0]) > 0)
            {
                return Conflict(new { error = "Собака уже зарегистрирована на эту выставку", code = "DOG_ALREADY_REGISTERED" });
            }

            string query = "INSERT INTO KURS_Uchastnik (Kod_vystavki, Kod_sobaki) VALUES (@vyst, @sob)";
            SqlParameter[] parameters = {
                new SqlParameter("@vyst", id),
                new SqlParameter("@sob", dto.DogId)
            };

            _db.ExecuteNonQuery(query, parameters);
            return Created("", new { message = "Собака зарегистрирована" });
        }
    }

    public class RegisterDto
    {
        public int DogId { get; set; }
    }
}