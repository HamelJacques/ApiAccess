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
        // 1️⃣ Déclaration du champ global
        private readonly string _connectionString;
        // 👉 CONSTRUCTEUR AJOUTÉ ICI
        public DonneesController(IConfiguration  config)
        {
            _connectionString = config.GetConnectionString("MaBaseAccess")!;
        }
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
            //bool nompresent = false;

            try
            {
                if (string.IsNullOrWhiteSpace(valeur))
                    return BadRequest("La valeur ne peut pas être vide.");

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

           
            using var conn = new OleDbConnection(_connectionString);
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

            using var conn = new OleDbConnection(_connectionString);
            Console.Write(_connectionString + Environment.NewLine);

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
            
            int leniveau = 0;
            leniveau = ObtenirId("tblNoms",unNom);
            Console.WriteLine("  L'id' est " + leniveau);

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
            
            string nomlocal = "";
            nomlocal=nom;
            int idNivo1 = 0;
            Console.Write(_connectionString + Environment.NewLine);
            idNivo1 = ObtenirId("tblNoms", nom);
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
        /// <returns>Le dernier Id</returns>
        private int ObtenirDernierId(string table, string nom){
            Console.Write("Dans ObtenirDernierId" + Environment.NewLine);
            string sql = "SELECT MAX([Id]) FROM " + table;

            Console.Write("sql = " + sql + Environment.NewLine);

            using var conn = new OleDbConnection(_connectionString);
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
        private int ObtenirId(string table, string nom)
        {
            Console.Write("Dans ObtenirId" + Environment.NewLine);
            Console.Write("table = " + table+ "; nom = " + nom  + Environment.NewLine);
            //Console.Write("nom = " + nom  + Environment.NewLine);
            //Console.Write("connexionstring = " + _connectionString  + Environment.NewLine);
            using var conn = new OleDbConnection(_connectionString);
            conn.Open();

            string sql = "SELECT [Id] FROM " + table + " WHERE Nom = ?";   
            
            // Affichage pour debug
            Console.WriteLine("SQL (debug) = SELECT [Id] FROM " + table + " WHERE Nom = '" + nom + "'");
              //" + nom + "'"
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
                1 => "SELECT COUNT(Nom) FROM tblNiveau_1 WHERE Nom = '" + valeur + "'",
                2 => "SELECT Nom FROM tblNiveau_2 ORDER BY Nom",
                _ => throw new ArgumentException("Niveau invalide")
            };
            Console.Write(sql + Environment.NewLine);
            
            try
            {
                using var conn = new OleDbConnection(_connectionString);
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
            catch(Exception ex)
            {
                Console.Write(ex.ToString() + Environment.NewLine);
                throw;
            }
            
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
                1 => "jctNiveau_0_Niveau_1",
                2 => "jctNiveau_1_Niveau_2",
                3 => "jctNiveau_2_Niveau_3",
                _ => throw new ArgumentException("Niveau invalide")
            };
            return table;
        }

        #region Les INSERTS
        private bool AjoutValeurDansTable(int niveau, string lavaleur, Personne lapersonne){
            Console.WriteLine("=== Dans AjoutValeurDansTable ===");
            string latableRef;
            string leLien;

            Console.WriteLine("niveau =" + niveau);            

            if(niveau == 0){
                if( IsNomPresent(niveau, lavaleur))
                {
                    return false;
                }
                return AjouterDansTablerUsagers(lavaleur);
            }

            // les niveaux 1, 2 et 3
            latableRef = ObtenirNomTableParNiveau(niveau);
            leLien = ObtenirNomTableLienParNiveau(niveau);

            Console.WriteLine("L'Ajout sera dans la table = " + latableRef);
            Console.WriteLine("La table de lisaison sera  = " + leLien);
            Console.WriteLine("Niveau_0  = " + lapersonne.Niveau0 + "; Niveau_1 = " + lapersonne .Niveau1+ "; Niveau_2 = " + lapersonne .Niveau2+ "; Niveau_3 = " + lapersonne .Niveau3);
            OleDbConnection conn = new OleDbConnection(_connectionString);
                                
            try
            {
                if(!IsNomPresent(niveau, lavaleur)){
                    Console.WriteLine("=== IsNomPresent est faux");
                    // obtenir le dernier id du lien ajouter 1
                    int IdRef = ObtenirDernierId(latableRef, "") + 1;
                    Console.WriteLine("=== Id pour la nouvelle valeur " + lavaleur + " est " + IdRef);
                    // Id == 0, pas dans la table et en plus la table est vide.  
                    conn.Open();
                    using var transaction = conn.BeginTransaction(); 
                    try
                    {
                        // On ajoute lavaleur à la table
                        string sqlInsertB = "INSERT INTO " + latableRef + " (idTable, Nom) VALUES (?, ?)";
                        Console.WriteLine(sqlInsertB);
                        Console.WriteLine("La connexion est  = " + conn.State.ToString());
                        
                        Console.WriteLine("La connexion est  = " + conn.State.ToString());
                        //conn.Close();
                        //bool ajoutROk = 
                        AjoutValeurDansTable(latableRef, IdRef, lavaleur, conn, transaction);

                        // puis on ajoute les 2 id dans latable de liaison
                        AjoutIdsDansTableLiaison(leLien, lapersonne, IdRef, niveau, conn, transaction); // méthode à créer 
                        transaction.Commit();
                        conn.Close();
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine("ERREUR LORS DE L'AJOUT D'UNE NOUVELLE VALEUR DE RÉFÉRENCE : " + Environment.NewLine + ex.Message);
                        transaction.Rollback();
                    }
                }
                else{ //Le nom existe j'ai besoin de son Id
                    Console.WriteLine("=== Dans le else de !IsNomPresent de AjoutValeurDansTable()");
                    Console.WriteLine("=== La valeur est présente, on ajoutera simplement les liens dans la tble de liaison");
                    int idNom = ObtenirId(latableRef,lavaleur);

                    //Console.WriteLine("=== Id pour la nouvelle valeur " + lavaleur + " est " + idNom);
                    // obtenir le dernier id du lien
                }                

                return true;
            }
            catch (Exception ex)
            {                
                throw;
            }
        }

        private void AjoutIdsDansTableLiaison(string leLien,  Personne lapersonne, int idRef, int niveau, OleDbConnection conn, OleDbTransaction transaction)
        {
            Console.WriteLine("Dans AjoutIdsDansTableLiaison");
            // on écrira le Idref (IdNom) et le Personne.idpersonne
           
            try
            {
                Console.WriteLine("La table : " + leLien  + ", Id Niveau 0 :" + lapersonne.Niveau0 + ", IdRef :" + idRef);
                // créer le insert ici
                string sql = $"INSERT INTO {leLien} (IdNiveau0, IdNiveau1) VALUES (?, ?)";
                Console.WriteLine("SQL = " + sql);
                //using var conn = new OleDbConnection(_connectionString);
                //conn.Open();
                using var cmd = new OleDbCommand(sql, conn, transaction);
                cmd.Parameters.AddWithValue("@p1", lapersonne.Niveau0);
                cmd.Parameters.AddWithValue("@p2", idRef);
                int rows = cmd.ExecuteNonQuery();
                Console.WriteLine("Rows affected = " + rows);
                //conn.Close();
            }
            catch(Exception ex){
                Console.WriteLine("Erreur SQL : " + ex.Message);
                throw;
            }
            //throw new NotImplementedException();
        }

        private void AjoutValeurDansTable(string latable, int leID, string lavaleur, OleDbConnection conn, OleDbTransaction transaction){
            Console.WriteLine("Dans AjoutValeurDansTable avec " + leID + " , " + lavaleur);
            
            try{
               
                string sql = $"INSERT INTO {latable} (Id, Nom) VALUES (?, ?)";
                
                Console.WriteLine("SQL = " + sql);
                //using var cmd = new OleDbCommand(sql, conn, transaction);
                
                //conn.Open();
                Console.WriteLine("La connexion est  = " + conn.State.ToString());
                using var cmd = new OleDbCommand(sql, conn, transaction);
                cmd.Parameters.AddWithValue("@p1", leID);
                cmd.Parameters.AddWithValue("@p2", lavaleur);

                int rows = cmd.ExecuteNonQuery();
                Console.WriteLine("Rows affected = " + rows);
                //conn.Close();
                Console.WriteLine("La connexion est  = " + conn.State.ToString());
                //return true;
            }
            catch(Exception ex){
                Console.WriteLine("Erreur SQL : " + ex.Message);
                throw;                
            }
            //return false;
    }

        private bool AjoutValeurDansTable(string latable, string lavaleur){
            Console.WriteLine("Dans AjoutValeurDansTable avec " + lavaleur);
            if(latable == "tblNoms"){
                return AjouterDansTablerUsagers(lavaleur);
            }           

            try{
                string sql = $"INSERT INTO {latable} (Nom) VALUES (?)";
                
                Console.WriteLine("SQL = " + sql);
                using var conn = new OleDbConnection(_connectionString);
                
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
                //int dernierid = ObtenirDernierId(latable, lavaleur);
                string sql = $"INSERT INTO {latable} (Nom) VALUES (?)";
                
                Console.WriteLine("SQL = " + sql);
                using var conn = new OleDbConnection(_connectionString);
                
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
            //int idNom = ObtenirId("tblNiveau1",nom, connectionString);

            using var conn = new OleDbConnection(_connectionString);
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