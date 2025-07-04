using Ecom_ProductApi.Models.DTos;
using Ecom_ProductApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ecom_ProductApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController(IProductService service) : ControllerBase
    {

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] ProductDto product, CancellationToken token = default)
        {
            if(product == null)
            {
                return BadRequest();
            }

           return Ok(await service.InsertProductWithImagesAsync(product, token));
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken token = default)
        {
            var products = await service.GetDetailedProductsAsync(token);
            if (products == null || !products.Any())
            {
                return NotFound();
            }
            return Ok(products);
        }

        [HttpGet("{productId:guid}")]
        public async Task<IActionResult> GetById(Guid productId, CancellationToken token = default)
        {
            if (productId == Guid.Empty)
            {
                return BadRequest("Invalid product ID.");
            }
            var product = await service.GetProductByIdAsync(productId, token);
            if (product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }
    }
}
