using gaming_shop_api.Models;

namespace gaming_shop_api.Repositories
{
    public interface IProductImageRepository
    {
        Task<IEnumerable<ProductImage>> GetByProductIdAsync(int productId);
        Task<ProductImage?> GetByIdAsync(int id);
        Task<ProductImage> AddAsync(ProductImage img);
        Task<bool> DeleteAsync(int id);
    }
}
