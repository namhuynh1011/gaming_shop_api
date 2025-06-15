using gaming_shop_api.Models;
using gaming_shop_api.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace gaming_shop_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductAPIController : ControllerBase
    {
        private readonly IProductRepository _productRepo;
        private readonly ICategoryRepository _categoryRepo;
        private readonly IBrandRepository _brandRepo;
        private readonly IProductImageRepository _imageRepo;

        public ProductAPIController(
            IProductRepository productRepo,
            ICategoryRepository categoryRepo,
            IBrandRepository brandRepo,
            IProductImageRepository imageRepo)
        {
            _productRepo = productRepo;
            _categoryRepo = categoryRepo;
            _brandRepo = brandRepo;
            _imageRepo = imageRepo;
        }

        // GET: api/ProductAPI
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetAll()
        {
            var products = await _productRepo.GetAllAsync();
            return Ok(products);
        }

        // GET: api/ProductAPI/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> Get(int id)
        {
            var product = await _productRepo.GetByIdAsync(id);
            if (product == null)
                return NotFound();
            return Ok(product);
        }

        // POST: api/ProductAPI
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Product>> Create([FromBody] Product product)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Optionally: check category & brand exist
            var category = await _categoryRepo.GetByIdAsync(product.CategoryId);
            var brand = await _brandRepo.GetByIdAsync(product.BrandId);
            if (category == null || brand == null)
                return BadRequest("Category or Brand not found");

            var created = await _productRepo.AddAsync(product);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }

        // PUT: api/ProductAPI/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Product>> Update(int id, [FromBody] Product product)
        {
            if (id != product.Id)
                return BadRequest("Mismatched Product ID");

            var exist = await _productRepo.GetByIdAsync(id);
            if (exist == null)
                return NotFound();

            // Optionally: check category & brand exist
            var category = await _categoryRepo.GetByIdAsync(product.CategoryId);
            var brand = await _brandRepo.GetByIdAsync(product.BrandId);
            if (category == null || brand == null)
                return BadRequest("Category or Brand not found");

            await _productRepo.UpdateAsync(product);
            return NoContent();
        }

        // DELETE: api/ProductAPI/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Delete(int id)
        {
            var exist = await _productRepo.GetByIdAsync(id);
            if (exist == null)
                return NotFound();

            await _productRepo.DeleteAsync(id);
            return NoContent();
        }

        // POST: api/ProductAPI/{productId}/images
        [HttpPost("{productId}/images")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> UploadImages(int productId, [FromForm] List<IFormFile> files)
        {
            var product = await _productRepo.GetByIdAsync(productId);
            if (product == null)
                return NotFound("Product not found");

            if (files == null || files.Count == 0)
                return BadRequest("No file uploaded");

            var imageList = new List<ProductImage>();

            foreach (var file in files)
            {
                var fileName = $"{Guid.NewGuid()}_{file.FileName}";
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "products", fileName);

                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var imageUrl = $"/images/products/{fileName}";
                var img = new ProductImage
                {
                    ImageUrl = imageUrl,
                    ProductId = productId
                };
                await _imageRepo.AddAsync(img);
                imageList.Add(img);
            }

            return Ok(imageList);
        }

        // GET: api/ProductAPI/{productId}/images
        [HttpGet("{productId}/images")]
        public async Task<ActionResult<IEnumerable<ProductImage>>> GetImages(int productId)
        {
            var images = await _imageRepo.GetByProductIdAsync(productId);
            return Ok(images);
        }

        // DELETE: api/ProductAPI/images/{imageId}
        [HttpDelete("images/{imageId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteImage(int imageId)
        {
            var img = await _imageRepo.GetByIdAsync(imageId);
            if (img == null)
                return NotFound();

            // Optionally: Delete file from disk
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", img.ImageUrl.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);

            await _imageRepo.DeleteAsync(imageId);
            return NoContent();
        }
    }
}
