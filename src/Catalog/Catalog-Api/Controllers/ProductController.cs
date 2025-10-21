using Catalog.Components;
using BuildingBlocks.Controllers;
using Catalog.Components.DTO;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.Api.Controllers
{
    public class ProductController : BaseController
    {
        private readonly IProductUpdaterService _productUpdaterService;
        private readonly IProductGetterService _productGetterService;

        public ProductController(
            IProductUpdaterService productUpdaterService,
            IProductGetterService productGetterService)
        {
            _productUpdaterService = productUpdaterService;
            _productGetterService = productGetterService;
        }
        
        [HttpPost]
        public async Task<IActionResult> Create(CreateProductDto dto)
        {
            await _productUpdaterService.AddProduct(dto);

            return Ok();
        }
        
        //TODO: با وجود اینکه این اکشن فقط داده‌ها را "دریافت" می‌کند، از متد HTTP POST استفاده شده است،
        // زیرا لیست شناسه‌ها (ids) در بدنه درخواست ارسال می‌شود و ممکن است تعداد زیادی مقدار داشته باشد.
        // در HTTP GET نمی‌توان بدنه (body) داشت و ارسال آرایه بزرگ از طریق query string می‌تواند باعث
        // بروز خطا یا محدودیت طول URL شود.
        // از آنجا که این متد صرفاً در ارتباط با کلاینت داخلی (مثلاً فرانت‌اند پروژه) استفاده می‌شود،
        // استفاده از POST کاملاً منطقی و ایمن است.
        [HttpPost("by-ids")]
        public async Task<IActionResult> GetCatalogItemByIds([FromBody] List<Guid> ids)
        {
            var catalogItem = await _productGetterService.GetProductByIds(ids);
        
            return Ok(new { CatalogItems = catalogItem });
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> RemoveItem(Guid id)
        {
            await _productUpdaterService.RemoveItem(id);
            return Ok();
        }
    }
}