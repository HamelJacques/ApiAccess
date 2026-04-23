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
        [HttpPost("{niveau}")]
        public IActionResult AjouterValeur(int niveau, [FromBody] string valeur){
            Console.WriteLine($"Dans AjouterValeur, NIVEAU ({niveau}) VALEUR ({valeur})");
            //return Ok("Ajout réussi");
            bool succes = false;
            bool nompresent = false;

            try
            {
                if (string.IsNullOrWhiteSpace(valeur))
                    return BadRequest("La valeur ne peut pas être vide.");

                //Console.WriteLine($"Dans AjouterValeur, NIVEAU ({niveau}), valeur = {valeur}");

                // Exemple : insertion dans la BD
                //Vérifier si la valeur est présente dans la table du niveau
                nompresent = IsNomPresent(niveau, valeur);
                if(!nompresent){
                    // Ajouter à la table
                    succes= !nompresent;
                    return Ok("Ajout en développement");
                }
                

                if (succes){
                    Console.WriteLine($"Échec de l'ajout dans la base de données.");
                    return StatusCode(500, "Échec de l'ajout dans la base de données.");
                }
                

                
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur serveur : {ex.Message}");
            }
            
            return Ok(); // ✔️ obligatoire
        }





        // -----------------------------
        // Retourne une liste de noms selon le niveau demandé
        // -----------------------------
        [HttpGet("{niveau}")]
        public IEnumerable<string> GetNiveau(int niveau)
        {
            Console.WriteLine($"Dans GetNiveau({niveau})");

            var liste = new List<string>();

            string connectionString =
                @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=\\Desktop-riddror\D\Developpements\VsCode\ApiAccess\ApiAccess\Base\API_DB_01.accdb;";

            using var conn = new OleDbConnection(connectionString);
            conn.Open();

            // 🔥 Construire la requête selon le niveau demandé
            string sql = niveau switch
            {
                0 => "SELECT Nom FROM tblNoms ORDER BY Nom",
                1 => "SELECT Nom FROM tblNiveau1 ORDER BY Nom",
                2 => "SELECT Nom FROM tblNiveau2 ORDER BY Nom",
                _ => throw new ArgumentException("Niveau invalide")
            };

            using var cmd = new OleDbCommand(sql, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                liste.Add(reader.GetString(0));
            }

            Console.WriteLine("Résultat : " + string.Join(", ", liste));

            return liste;
        }


        // -----------------------------
        // Retourne la liste des noms usagers
        // -----------------------------
        [HttpGet]
        public IEnumerable<string> GetNivo0()
        {
            Console.Write("Dans GetNivo0" + Environment.NewLine);
            var liste = new List<string>();

            string connectionString =
                @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=\\Desktop-riddror\D\Developpements\VsCode\ApiAccess\ApiAccess\Base\API_DB_01.accdb;";

            using var conn = new OleDbConnection(connectionString);
            Console.Write(connectionString + Environment.NewLine);

            conn.Open();

            string sql = "SELECT Nom FROM tblNoms order by Nom";

            using var cmd = new OleDbCommand(sql, conn);
            Console.Write("Après new OleDbCommand" + Environment.NewLine);
            using var reader = cmd.ExecuteReader();
            Console.Write("Après ExecuteReader" + Environment.NewLine);

            while (reader.Read())
            {
                liste.Add(reader.GetString(0));
            }
            Console.Write("Après lecture" + Environment.NewLine);
            Console.Write(string.Join(", ", liste));
            

            return liste;
        }
/*
        // -----------------------------
        // 1️⃣ Méthode existante : Retourne une liste des noms
        // -----------------------------
        [HttpGet]
        public IEnumerable<string> Get()
        {
            Console.Write("Dans Get les noms" + Environment.NewLine);
            var liste = new List<string>();

            string connectionString =
                @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=\\Desktop-riddror\D\Developpements\VsCode\ApiAccess\ApiAccess\Base\API_DB_01.accdb;";

            using var conn = new OleDbConnection(connectionString);
            Console.Write(connectionString + Environment.NewLine);

            conn.Open();

            string sql = "SELECT Nom FROM tblNoms order by Nom";

            using var cmd = new OleDbCommand(sql, conn);
            Console.Write("Après new OleDbCommand" + Environment.NewLine);
            using var reader = cmd.ExecuteReader();
            Console.Write("Après ExecuteReader" + Environment.NewLine);

            while (reader.Read())
            {
                liste.Add(reader.GetString(0));
            }

            return liste;
        }
        */
        // -----------------------------
        // 2️⃣ Nouvelle méthode : FiltreNiveau 1
        // -----------------------------
        [HttpGet("filtre/nom")]        
        public IEnumerable<string> GetNiveau_1(string nom)
        {
            var liste = new List<string>();
            Console.Write("Dans GetNiveau_1" + Environment.NewLine);
            
            string connectionString =
                @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=\\Desktop-riddror\D\Developpements\VsCode\ApiAccess\ApiAccess\Base\API_DB_01.accdb;";
            Console.Write(connectionString + Environment.NewLine);
            
            string nomlocal = "";
            nomlocal=nom;
            int idNivo1 = 0;
            Console.Write(connectionString + Environment.NewLine);
            idNivo1 = ObtenirId("tblNoms", nom, connectionString);
            Console.Write("ObtenirId = " + idNivo1.ToString()  + Environment.NewLine);

            // Obtenir la liste de tous les éléments de niveau 1 pour le nom sélectionné
            liste.Add(idNivo1.ToString());
            return liste;
        }

        [HttpPost("filtre/donnees")]
        public bool ajouterDonnee(string nom)
        {
            Console.Write("Dans GetNiveau_1" + Environment.NewLine);
            return false;
        }
#region méthodes privées
        /// <summary>
        /// Retourne l'Id unique d'un enregistrement selon une table
        /// </summary>
        /// <param name="table"></param>
        /// <param name="nom"></param>
        /// <param name="connectionstring"></param>
        /// <returns></returns>
        private int ObtenirId(string table, string nom, string connectionstring)
        {
            Console.Write("Dans ObtenirId" + Environment.NewLine);
            Console.Write("table = " + table  + Environment.NewLine);
            Console.Write("nom = " + nom  + Environment.NewLine);
            Console.Write("connexionstring = " + connectionstring  + Environment.NewLine);
            using var conn = new OleDbConnection(connectionstring);
            conn.Open();

            string sql = "SELECT [Id] FROM " + table + " WHERE Nom = ?";             //" + nom + "'"
            Console.Write(sql);
            using var cmd = new OleDbCommand(sql, conn);

            cmd.Parameters.AddWithValue("?", nom);

            object? result = cmd.ExecuteScalar();

            if (result != null && int.TryParse(result.ToString(), out int id))
            {
                return id;
            }

            return -1; // ou -1 selon ta logique
        }

        private bool IsNomPresent(int nivo, string valeur){
            Console.Write("Dans IsNomPresent" + Environment.NewLine);
            // 🔥 Construire la requête selon le niveau demandé
            string sql = nivo switch
            {
                0 => "SELECT COUNT(Nom) FROM tblNoms WHERE Nom = '" + valeur + "'",
                1 => "SELECT Nom FROM tblNiveau1 ORDER BY Nom",
                2 => "SELECT Nom FROM tblNiveau2 ORDER BY Nom",
                _ => throw new ArgumentException("Niveau invalide")
            };
            Console.Write(sql + Environment.NewLine);

            string connectionString =
                @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=\\Desktop-riddror\D\Developpements\VsCode\ApiAccess\ApiAccess\Base\API_DB_01.accdb;";

            using var conn = new OleDbConnection(connectionString);
            conn.Open();
            using var cmd = new OleDbCommand(sql, conn);
            //using var reader = cmd.ExecuteReader();

            var result = cmd.ExecuteScalar();
            int count = Convert.ToInt32(result);

            //string sql = "SELECT Nom FROM tblNoms order by Nom";
            Console.Write("Après ExecuteReader" + Environment.NewLine);
            Console.Write("COUNT =" + count + Environment.NewLine);

            return count >= 1;
        }

        private bool AjoutNomDansTable(int nivo, string valeur){
            Console.Write("Dans ObtenirId" + Environment.NewLine);
            //Console.Write("table = " + table  + Environment.NewLine);
            //Console.Write("nom = " + nom  + Environment.NewLine);
            return false;
        }


#endregion
        // -----------------------------
        // 2️⃣ Nouvelle méthode : détails d’un nom
        // -----------------------------
        [HttpGet("details/{nom}")]
        public IActionResult GetDetails(string nom)
        {
            string connectionString =
                @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=\\Desktop-riddror\D\Developpements\VsCode\ApiAccess\ApiAccess\Base\API_DB_01.accdb;";

            //int idNom = ObtenirId("tblNiveau1",nom, connectionString);

            using var conn = new OleDbConnection(connectionString);
            string sql = "SELECT Nom, Adresse, Telephone, Age FROM tblNoms WHERE Nom = @nom";
            conn.Open();

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