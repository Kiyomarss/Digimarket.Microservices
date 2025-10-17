using Catalog.Components;
using Catalog.Components.Contracts;
using BuildingBlocks.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.Api.Controllers
{
    public class CatalogItemController : BaseController
    {
        private readonly ICatalogItemUpdaterService _catalogItemUpdaterService;
        private readonly ICatalogItemGetterService _catalogItemGetterService;

        public CatalogItemController(
            ICatalogItemUpdaterService catalogItemUpdaterService,
            ICatalogItemGetterService catalogItemGetterService)
        {
            _catalogItemUpdaterService = catalogItemUpdaterService;
            _catalogItemGetterService = catalogItemGetterService;
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
            var catalogItem = await _catalogItemGetterService.GetCatalogItemByIds(ids);
        
            return Ok(new { CatalogItems = catalogItem });
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> RemoveItem(Guid id)
        {
            await _catalogItemUpdaterService.RemoveItem(id);
            return Ok();
        }
    }
}