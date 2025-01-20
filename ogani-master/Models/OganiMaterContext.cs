using Microsoft.EntityFrameworkCore;

namespace ogani_master.Models
{
    public class OganiMaterContext : DbContext
    {
        public OganiMaterContext(DbContextOptions options) : base(options) { }
        public DbSet<AdminUser> AdminUsers { get; set; }
        
        public DbSet<Banner> Banners { get; set; }
        public DbSet<Feature> Features { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Menu> Menus { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Setting> Settings { get; set; }
        public DbSet<UserBehaviorLog> UserBehaviorLogs { get; set; }
        public DbSet<User> users { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<ContactAd> Contacts { get; set; }

        public DbSet<FavoritesModel> Favorites { get; set; }

        public DbSet<Blog> Blogs { get; set; }


    }
}
