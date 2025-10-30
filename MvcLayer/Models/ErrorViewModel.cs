using System.ComponentModel.DataAnnotations;

namespace MVC_layer.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        [Display(Name = "Error")]
        public string Error { get; set; }

        [Display(Name = "Description")]
        public string ErrorDescription { get; set; }

        // Diagnostics (shown in Development)
        public bool IsDevelopment { get; set; }
        public int? StatusCode { get; set; }
        public string? Path { get; set; }
        public string? Method { get; set; }
        public string? QueryString { get; set; }
        public string? UserName { get; set; }
        public string? ExceptionType { get; set; }
        public string? StackTrace { get; set; }
    }
}