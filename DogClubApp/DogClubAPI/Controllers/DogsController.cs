using Microsoft.AspNetCore.Mvc;
using DogClubAPI.Data;
using System.Data;
using Microsoft.Data.SqlClient;

namespace DogClubAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DogsController : ControllerBase
    {
        private readonly DbHelper _db;
        public DogsController(DbHelper db) => _db = db;

        [HttpGet]
        public IActionResult GetAll()
        {
            var dt = _db.ExecuteQuery("SELECT * FROM KURS_Sobaka");

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

        [HttpPost]
        public IActionResult Add([FromBody] DogDto dog)
        {
            string query = @"INSERT INTO KURS_Sobaka (Klichka, Kod_porody, Kod_vladelca, Data_rozhdeniya, Pol, Nomer_rodoslovnoy) 
                             VALUES (@klichka, @poroda, @vladelec, @date, @pol, @rod)";

            SqlParameter[] parameters = {
                new SqlParameter("@klichka", dog.Klichka),
                new SqlParameter("@poroda", dog.Kod_porody),
                new SqlParameter("@vladelec", dog.Kod_vladelca),
                new SqlParameter("@date", dog.Data_rozhdeniya),
                new SqlParameter("@pol", dog.Pol),
                new SqlParameter("@rod", dog.Nomer_rodoslovnoy)
            };

            _db.ExecuteNonQuery(query, parameters);
            return Ok(new { message = "Собака добавлена" });
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var param = new SqlParameter("@id", id);
            _db.ExecuteNonQuery("DELETE FROM KURS_Sobaka WHERE Kod_sobaki = @id", new[] { param });
            return Ok(new { message = "Удалено" });
        }
    }

    public class DogDto
    {
        public string Klichka { get; set; } = string.Empty;
        public int Kod_porody { get; set; }
        public int Kod_vladelca { get; set; }
        public DateTime Data_rozhdeniya { get; set; }
        public string Pol { get; set; } = string.Empty;
        public string Nomer_rodoslovnoy { get; set; } = string.Empty;
    }
}