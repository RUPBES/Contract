using BusinessLayer.Interfaces.CommonInterfaces;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using MVC_layer.Models;

namespace MvcLayer.Controllers
{
    [Route("Error")]
    public class ErrorController : Controller
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IHttpHelper _httpHelper;

        public ErrorController(IWebHostEnvironment environment, IHttpHelper httpHelper)
        {
            _environment = environment;
            _httpHelper = httpHelper;
        }

        private static string GetRussianTitle(int statusCode)
        {
            return statusCode switch
            {
                StatusCodes.Status400BadRequest => "Неверный запрос",
                StatusCodes.Status401Unauthorized => "Не авторизован",
                StatusCodes.Status403Forbidden => "Доступ запрещён",
                StatusCodes.Status404NotFound => "Не найдено",
                StatusCodes.Status409Conflict => "Конфликт",
                418 => "Я — чайник",
                StatusCodes.Status422UnprocessableEntity => "Ошибка проверки данных",
                StatusCodes.Status429TooManyRequests => "Слишком много запросов",
                StatusCodes.Status500InternalServerError => "Внутренняя ошибка сервера",
                StatusCodes.Status501NotImplemented => "Не реализовано",
                StatusCodes.Status502BadGateway => "Ошибка шлюза",
                StatusCodes.Status503ServiceUnavailable => "Сервис недоступен",
                StatusCodes.Status504GatewayTimeout => "Таймаут шлюза",
                _ => $"Ошибка {statusCode}"
            };
        }

        private static string GetRussianDescription(int statusCode)
        {
            return statusCode switch
            {
                StatusCodes.Status400BadRequest => "Сервер не смог обработать запрос из-за некорректных данных.",
                StatusCodes.Status401Unauthorized => "Пожалуйста, авторизуйтесь для доступа к ресурсу.",
                StatusCodes.Status403Forbidden => "У вас нет прав для выполнения этой операции.",
                StatusCodes.Status404NotFound => "Запрошенный ресурс не найден или был удалён.",
                StatusCodes.Status409Conflict => "Конфликт состояния ресурса. Повторите попытку позже или обновите данные.",
                418 => "Сервер отказывается заваривать кофе в чайнике.",
                StatusCodes.Status422UnprocessableEntity => "Данные не прошли проверку. Исправьте ошибки и попробуйте снова.",
                StatusCodes.Status429TooManyRequests => "Слишком много запросов. Попробуйте позже.",
                StatusCodes.Status500InternalServerError => "Произошла непредвиденная ошибка на сервере. Попробуйте позже.",
                StatusCodes.Status501NotImplemented => "Функциональность ещё не реализована.",
                StatusCodes.Status502BadGateway => "Промежуточный сервер вернул некорректный ответ.",
                StatusCodes.Status503ServiceUnavailable => "Сервис временно недоступен. Ведутся технические работы или перегрузка.",
                StatusCodes.Status504GatewayTimeout => "Время ожидания ответа от сервера истекло.",
                _ => "Произошла ошибка при обработке запроса."
            };
        }

        [HttpGet]
        public IActionResult Error()
        {
            var feature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            bool isDev = _environment.EnvironmentName == "Development";
            var description = isDev
                ? feature?.Error?.Message ?? feature?.Error?.ToString()
                : GetRussianDescription(StatusCodes.Status500InternalServerError);

            var model = new ErrorViewModel
            {
                RequestId = HttpContext.TraceIdentifier,
                Error = GetRussianTitle(StatusCodes.Status500InternalServerError),
                ErrorDescription = description ?? string.Empty,
                IsDevelopment = isDev,
                StatusCode = StatusCodes.Status500InternalServerError,
                Path = feature?.Path ?? HttpContext.Request.Path,
                Method = HttpContext.Request.Method,
                QueryString = HttpContext.Request.QueryString.HasValue ? HttpContext.Request.QueryString.Value : string.Empty,
                UserName = _httpHelper.GetUserName(), // User?.Identity?.IsAuthenticated == true ? User.Identity!.Name : null,
                ExceptionType = feature?.Error?.GetType().FullName,
                StackTrace = feature?.Error?.ToString()
            };
            Response.StatusCode = StatusCodes.Status500InternalServerError;
            return View("~/Views/Shared/Error.cshtml", model);
        }

        [HttpGet("{code:int}")]
        public IActionResult ErrorByCode(int code)
        {
            var statusFeature = HttpContext.Features.Get<Microsoft.AspNetCore.Diagnostics.IStatusCodeReExecuteFeature>();
            var forwardedMessage = HttpContext.Items.ContainsKey("StatusMessage") ? HttpContext.Items["StatusMessage"] as string : null;
            var model = new ErrorViewModel
            {
                RequestId = HttpContext.TraceIdentifier,
                Error = GetRussianTitle(code),
                ErrorDescription = !string.IsNullOrWhiteSpace(forwardedMessage)
                                    ? forwardedMessage
                                    : GetRussianDescription(code),
                IsDevelopment = _environment.EnvironmentName == "Development",
                StatusCode = code,
                Path = statusFeature?.OriginalPath ?? HttpContext.Request.Path,
                Method = HttpContext.Request.Method,
                QueryString = statusFeature?.OriginalQueryString ?? HttpContext.Request.QueryString.Value,
                UserName = _httpHelper.GetUserName(),
                //User?.Identity?.IsAuthenticated == true ? User.Identity!.Name : null
            };
            Response.StatusCode = code;
            return View("~/Views/Shared/Error.cshtml", model);
        }
    }
}
