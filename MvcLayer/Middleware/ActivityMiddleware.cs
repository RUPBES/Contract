using BusinessLayer.Interfaces.Core;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;

namespace MvcLayer.Middleware
{
    public class ActivityMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _cache;
        private readonly ILogger<ActivityMiddleware> _logger;

        public ActivityMiddleware(
            RequestDelegate next,
            IMemoryCache cache,
            ILogger<ActivityMiddleware> logger)
        {
            _next = next;
            _cache = cache;
            _logger = logger;
        }

        // Scoped-сервисы (IAdminService) принимаем здесь
        public async Task InvokeAsync(HttpContext ctx, IAdminService admin)
        {
            if (IsPageRequest(ctx) && ctx.User.Identity?.IsAuthenticated == true)
            {
                var userId = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userName = ctx.User.FindFirstValue(ClaimTypes.Email);

                if (!string.IsNullOrEmpty(userId))
                {
                    var now = DateTime.UtcNow;
                    var today = DateOnly.FromDateTime(now);
                    var key = $"visit:{userId}:{today:yyyyMMdd}";

                    if (!_cache.TryGetValue(key, out _))
                    {
                        try
                        {
                            await admin.SetLastVisitAsync(userId, userName, today, now);
                            _cache.Set(key, true, TimeSpan.FromMinutes(5));
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Не удалось записать визит пользователя {UserId}", userId);
                        }
                    }
                }
            }

            await _next(ctx);
        }

        private static bool IsPageRequest(HttpContext ctx)
        {
            var path = ctx.Request.Path;

            return HttpMethods.IsGet(ctx.Request.Method)
                && !Path.HasExtension(path)
                && !path.StartsWithSegments("/api")
                && !path.StartsWithSegments("/health")
                && !path.StartsWithSegments("/admin")
                && !path.StartsWithSegments("/Home")
                && !path.StartsWithSegments("/release-notes");
        }
    }
}
