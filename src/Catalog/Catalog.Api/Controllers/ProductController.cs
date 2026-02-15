using BuildingBlocks.Controllers;
using Catalog.Application.DTO;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.Api.Controllers
{
    [Route("/Product")]
    public class ProductController : BaseController
    {
        //TODO: با وجود اینکه این اکشن فقط داده‌ها را "دریافت" می‌کند، از متد HTTP POST استفاده شده است،
        // زیرا لیست شناسه‌ها (ids) در بدنه درخواست ارسال می‌شود و ممکن است تعداد زیادی مقدار داشته باشد.
        // در HTTP GET نمی‌توان بدنه (body) داشت و ارسال آرایه بزرگ از طریق query string می‌تواند باعث
        // بروز خطا یا محدودیت طول URL شود.
        // از آنجا که این متد صرفاً در ارتباط با کلاینت داخلی (مثلاً فرانت‌اند پروژه) استفاده می‌شود،
        // استفاده از POST کاملاً منطقی و ایمن است.
        [HttpPost("by-ids")]
        public async Task<IActionResult> GetCatalogItemByIds([FromBody] List<Guid> ids)
        {
        
            return Ok();
        }
    }
}