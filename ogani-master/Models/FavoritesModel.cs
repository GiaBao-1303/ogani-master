using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ogani_master.Models
{
    [Table("Favorites")]
    public class FavoritesModel : BaseModel
    {
        [Key]
        
        public int Id { get; set; }
    
        public int UserID { get; set; }

        public int ProductId { get; set; }

        public Product? Product { get; set; }

    }
}
