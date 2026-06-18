using ApiAccess.Models;
namespace ApiAccess.Controllers
{
    public class AjoutRequest
    {
        public required Personne Personne { get; set; }
        public required string Valeur { get; set; }
        public required int AjouPourNiveau { get; set; }
    }
}