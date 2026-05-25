using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Grpc.Core;
using Grpc.Core.Interceptors;
using BuildingBlocks.Exceptions.Application;
using Microsoft.AspNetCore.Http;

public sealed class GrpcExceptionInterceptor : Interceptor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public GrpcExceptionInterceptor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request, ServerCallContext context, UnaryServerMethod<TRequest, TResponse> continuation)
    {
        // 1. استخراج توکن از Metadata
        var authHeader = context.RequestHeaders.FirstOrDefault(h => h.Key == "authorization")?.Value;

        if (!string.IsNullOrEmpty(authHeader))
        {
            // 2. دیکود کردن توکن و ساخت ClaimsPrincipal
            // نکته: برای جلوگیری از سربارِ Validate دوباره، می‌توانید فقط Claimها را بخوانید
            var handler = new JwtSecurityTokenHandler();
            if (handler.ReadToken(authHeader.Replace("Bearer ", "")) is JwtSecurityToken jwtToken)
            {
                var claimsIdentity = new ClaimsIdentity(jwtToken.Claims, "grpc-auth");
                var user = new ClaimsPrincipal(claimsIdentity);

                // 3. تزریق به HttpContext (ترفند کلیدی برای اینکه CurrentUserService کار کند)
                if (_httpContextAccessor.HttpContext != null)
                {
                    _httpContextAccessor.HttpContext.User = user;
                }
            }
        }

        try
        {
            return await continuation(request, context);
        }
        catch (Exception ex)
        {
            // مدیریت خطاهای شما
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }
}