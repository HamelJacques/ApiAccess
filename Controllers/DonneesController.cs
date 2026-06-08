using Microsoft.AspNetCore.Mvc;
using System.Data.OleDb;
using System.Collections.Generic;
using System.Configuration;
using System.Text.Json;
using ApiAccess.Models;


namespace ApiAccess.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DonneesController : ControllerBase
    {
        [HttpPost("ajout")]
        public IActionResult AjouterValeur([FromBody] AjoutRequest req)
        {
            var personne = req.Personne;
            var valeur = req.Valeur;
            var ajoutpourniveau = req.AjouPourNiveau;
            

            Console.WriteLine("=== PERSONNE REÇUE ===");
            Console.WriteLine(JsonSerializer.Serialize(personne));
            Console.WriteLine("======================");

            Console.WriteLine("Valeur reçue : " + valeur);
            Console.WriteLine("AjouPourNiveau : " + ajoutpourniveau);

            // traitement...
            bool ajoutOk =  this.AjoutValeurDansTable(ajoutpourniveau,valeur, personne);

            return Ok("En développement !!!");
            //return Ok("Ajout réussi");
        }

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
                //Vérifier si la valeur est présente dans la table du niveau
                //nompresent = IsNomPresent(niveau, valeur);

                // sélection du niveau d'ajout
                /*
                if(niveau == 0){
                    if!(IsNomPresent(niveau, valeur)){
                        //je l'ajoute à la table nom
                        succes = AjoutValeurDansTable("tblNoms",valeur);
                    }
                    else{
                        return StatusCode(10, valeur +  " existe deja dans dans la base de données.");
                    }
                }
                else{ // Niveau 1, 2, ou 3
                //private bool AjoutValeurDansTable(string latable, string lavaleur){
                succes = AjoutValeurDansTable("tblNiveau" + Niveau.ToString(),valeur);

                }

*/
                
                //Console.WriteLine($"Success = " + succes.ToString());
                
                
                if(IsNomPresent(niveau, valeur)){
                    Console.WriteLine(valeur+ " existe deja dans la base de données.");
                    return StatusCode(10, valeur +  " existe deja dans dans la base de données.");
                }
                else// on peut ajouter  appeler la methode qui fera le insert
                {                    
                    if(niveau == 0){
                       succes = AjoutValeurDansTable("tblNoms",valeur);
                    }
                    else{
                        // Vérifier si 
                        // ca prend une transaction pour lier les tables
                        Console.WriteLine($"Dans le else.");
                        succes = false; // en attendant la fin du développement
                    }                    
                }

                if (!succes){
                    // 

                    Console.WriteLine($"Échec de l'ajout dans la base de données.");
                    return StatusCode(500, "Échec de l'ajout dans la base de données.");
                }                

                return Ok(); // ✔️ obligatoire
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur serveur : {ex.Message}");
            }
            
            
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
                1 => "SELECT Nom FROM tblNiveau_1 ORDER BY Nom",
                2 => "SELECT Nom FROM tblNiveau_2 ORDER BY Nom",
                _ => throw new ArgumentException("Niveau invalide")
            };
            Console.WriteLine($"SQL = {sql}");

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

// -----------------------------
// Retourne l'ID associé à un nom
// -----------------------------
        [HttpGet("id-par-nom/{unNom}/{niveau}")]
        public int GetIdSpecifique(string unNom, int niveau){
            Console.WriteLine("Dans GetIdSpecifique pour " + unNom + " Niveau " + niveau);
            // ici, je veux lire la bd et obtenir l'ID de unNom
            string connectionString =
                @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=\\Desktop-riddror\D\Developpements\VsCode\ApiAccess\ApiAccess\Base\API_DB_01.accdb;";

            int leniveau = 0;
            leniveau = ObtenirId("tblNoms",unNom,connectionString);
            Console.WriteLine("L'id' est " + leniveau);

            return leniveau;
        }
        
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
        ///<summary>
        /// Retourne le dernier Id de la table mentionn/e
        /// </summary>
        /// <param name="table"></param>
        /// <param name="connectionstring"></param>
        /// <returns>Le dernier Id</returns>
        private int ObtenirDernierId(string table, string nom, string connectionstring){
            Console.Write("Dans ObtenirDernierId" + Environment.NewLine);
            string sql = "SELECT MAX([Id]) FROM " + table;

            Console.Write("sql = " + sql + Environment.NewLine);

            using var conn = new OleDbConnection(connectionstring);
            conn.Open();
            using var cmd = new OleDbCommand(sql, conn);
            object? result = cmd.ExecuteScalar();
            if (result != null && int.TryParse(result.ToString(), out int id))
            {
                Console.Write("id = " + id + Environment.NewLine);
                return id;
            }
            Console.Write("id = 0" + Environment.NewLine);
            return 0;
        }
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
                1 => "SELECT Nom FROM tblNiveau_1 ORDER BY Nom",
                2 => "SELECT Nom FROM tblNiveau_2 ORDER BY Nom",
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

            return count == 1;
        }
        private string ObtenirNomTableParNiveau(int niveau){
            Console.WriteLine("=== Dans ObtenirNomTableParNiveau === POUR LE NIVEAU " + niveau);
            string table = niveau switch
            {
                0 => "tblNoms",
                1 => "tblNiveau_1",
                2 => "tblNiveau_2",
                3 => "tblNiveau_3",
                _ => throw new ArgumentException("Niveau invalide")
            };
            
            return table;
        }
        private string ObtenirNomTableLienParNiveau(int niveau){
            string table = niveau switch
            {
                1 => "jctNoms_Niveau_1",
                2 => "jctNoms_Niveau_1",
                3 => "jctNoms_Niveau_1",
                _ => throw new ArgumentException("Niveau invalide")
            };
            return table;
        }

        #region Les INSERTS
        private bool AjoutValeurDansTable(int niveau, string lavaleur, Personne lapersonne){
            Console.WriteLine("=== Dans AjoutValeurDansTable ===");
            string latable;
            string leLien;
            string connectionString =
                @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=\\Desktop-riddror\D\Developpements\VsCode\ApiAccess\ApiAccess\Base\API_DB_01.accdb;";

            Console.WriteLine("niveau =" + niveau);

            if(niveau == 0){
                if( IsNomPresent(niveau, lavaleur))
                {
                    return false;
                }
                return AjouterDansTablerUsagers(lavaleur);
            }

            // les niveaux 1, 2 et 3
            latable = ObtenirNomTableParNiveau(niveau);
            leLien = ObtenirNomTableLienParNiveau(niveau);

            
            Console.WriteLine("latable =" + latable);
            Console.WriteLine("leLien =" + leLien);

            OleDbConnection conn = new OleDbConnection(connectionString);
            conn.Open();
            using var transaction = conn.BeginTransaction();

            try
            {
                if(!IsNomPresent(niveau, lavaleur)){
                    // obtenir le dernier id du lien ajouter 1
                    int idTable = ObtenirDernierId(latable, "", connectionString);
                }
                else{
                    // obtenir le dernier id du lien
                }
/*
                int parentId;

                // INSERT parent
                using (var cmd1 = new OleDbCommand("INSERT INTO " + latable + " (Nom) VALUES (?)", conn, transaction))
                {
                   cmd1.Parameters.AddWithValue("@p1", leLien = ObtenirNomTableLienParNiveau(niveau));
                    //cmd1.ExecuteNonQuery();

                    cmd1.CommandText = "SELECT @@IDENTITY";
                    parentId = Convert.ToInt32(cmd1.ExecuteScalar());
                }

                // INSERT enfant
                using (var cmd2 = new OleDbCommand("INSERT INTO tblEnfant (ParentID, Valeur) VALUES (?, ?)", conn, transaction))
                {
                    cmd2.Parameters.AddWithValue("@p1", parentId);
                    cmd2.Parameters.AddWithValue("@p2", lavaleur);
                    //cmd2.ExecuteNonQuery();
                }

                transaction.Commit();
                */
                return true;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Console.WriteLine("Erreur transaction ajout : " + ex.Message);
                return false;
            }
        }

        private bool AjoutValeurDansTable(string latable, string lavaleur){
            Console.WriteLine("Dans AjoutValeurDansTable avec " + lavaleur);
            if(latable == "tblNoms"){
                return AjouterDansTablerUsagers(lavaleur);
            }           

            try{
                string connectionString =
                @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=\\Desktop-riddror\D\Developpements\VsCode\ApiAccess\ApiAccess\Base\API_DB_01.accdb;";

                // Ouvrir une transaction
            
                string sql = $"INSERT INTO {latable} (Nom) VALUES (?)";
                
                Console.WriteLine("SQL = " + sql);
                using var conn = new OleDbConnection(connectionString);
                
                conn.Open();
                using var cmd = new OleDbCommand(sql, conn);
                cmd.Parameters.AddWithValue("@p1", lavaleur);

                int rows = cmd.ExecuteNonQuery();
                Console.WriteLine("Rows affected = " + rows);
            }
            catch(Exception ex){
                Console.WriteLine("Erreur SQL : " + ex.Message);
                return false;
            }

        return true;
    }
    private bool AjouterDansTablerUsagers(string lavaleur){
        Console.WriteLine("Dans AjouterDansTablerUsagers avec " + lavaleur);
        bool retour;
        string latable = "tblNoms";
        retour = true;
        try{
                string connectionString =
                @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=\\Desktop-riddror\D\Developpements\VsCode\ApiAccess\ApiAccess\Base\API_DB_01.accdb;";
                //int dernierid = ObtenirDernierId(latable, lavaleur, connectionString);
                string sql = $"INSERT INTO {latable} (Nom) VALUES (?)";
                
                Console.WriteLine("SQL = " + sql);
                using var conn = new OleDbConnection(connectionString);
                
                conn.Open();
                using var cmd = new OleDbCommand(sql, conn);
                cmd.Parameters.AddWithValue("@p1", lavaleur);

                int rows = cmd.ExecuteNonQuery();
                Console.WriteLine("Rows affected = " + rows);
            }
            catch(Exception ex){
                Console.WriteLine("Erreur SQL : " + ex.Message);
                return false;
            }
        return retour;
    }
        
        
        #endregion


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