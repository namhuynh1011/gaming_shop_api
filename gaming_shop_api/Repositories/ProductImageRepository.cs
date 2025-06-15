using gaming_shop_api.Models;
using Microsoft.EntityFrameworkCore;

namespace gaming_shop_api.Repositories
{
    public class ProductImageRepository : IProductImageRepository
    {
        private readonly ApplicationDbContext _context;
        public ProductImageRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ProductImage>> GetByProductIdAsync(int productId)
        {
            return await _context.ProductImages
                .Where(img => img.ProductId == productId)
                .ToListAsync();
        }

        public async Task<ProductImage?> GetByIdAsync(int id)
        {
            return await _context.ProductImages.FindAsync(id);
        }

        public async Task<ProductImage> AddAsync(ProductImage img)
        {
            _context.ProductImages.Add(img);
            await _context.SaveChangesAsync();
            return img;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var img = await _context.ProductImages.FindAsync(id);
            if (img == null) return false;
            _context.ProductImages.Remove(img);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
