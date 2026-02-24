using Microsoft.AspNetCore.Mvc;
using System.Data.OleDb;
using System.Collections.Generic;
using System.Configuration;

namespace ApiAccess.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DonneesController : ControllerBase
    {
        // -----------------------------
        // 1️⃣ Méthode existante : liste des noms
        // -----------------------------
        [HttpGet]
        public IEnumerable<string> Get()
        {
            var liste = new List<string>();

            string connectionString =
                @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=\\Desktop-riddror\D\Developpements\VsCode\ApiAccess\ApiAccess\Base\API_DB_01.accdb;";

            using var conn = new OleDbConnection(connectionString);
            conn.Open();

            string sql = "SELECT Nom FROM tblNoms";

            using var cmd = new OleDbCommand(sql, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                liste.Add(reader.GetString(0));
            }

            return liste;
        }
        
        // -----------------------------
        // 2️⃣ Nouvelle méthode : FiltreNiveau 1
        // -----------------------------
        [HttpGet("filtre/{niveau1}")]        
        public IEnumerable<string> GetNiveau_1(string nom)
        {
            var liste = new List<string>();
            string nomlocal = "";
            nomlocal=nom;
            int idNivo1 = 0;
            idNivo1 = ObtenirId("tblNiveau1",nom);
            return liste;
        }

        private int ObtenirId(string table, string nom)
        {
            return 0;
        }

        // -----------------------------
        // 2️⃣ Nouvelle méthode : détails d’un nom
        // -----------------------------
        [HttpGet("details/{nom}")]
        public IActionResult GetDetails(string nom)
        {
            string connectionString =
                @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=\\Desktop-riddror\D\Developpements\VsCode\ApiAccess\ApiAccess\Base\API_DB_01.accdb;";

            using var conn = new OleDbConnection(connectionString);
            conn.Open();

            string sql = "SELECT Nom, Adresse, Telephone, Age FROM tblNoms WHERE Nom = @nom";

            using var cmd = new OleDbCommand(sql, conn);
            cmd.Parameters.AddWithValue("@nom", nom);

            using var reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                var result = new
                {
                    Nom = reader["Nom"].ToString(),
                    Adresse = reader["Adresse"].ToString(),
                    Telephone = reader["Telephone"].ToString(),
                    Age = reader["Age"].ToString()
                };

                return Ok(result);
            }

            return NotFound($"Aucun enregistrement trouvé pour : {nom}"); 
        }        
    }
}