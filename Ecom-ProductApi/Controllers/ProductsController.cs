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
    }
}
