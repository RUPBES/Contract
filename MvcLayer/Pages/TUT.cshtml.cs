using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MvcLayer.Pages
{
    [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
    public class TUTModel : PageModel
    {
        private readonly ILogger<TUTModel> _logger;

        public IFormFileCollection FileUploads { get; set; }

        public TUTModel(ILogger<TUTModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync([FromForm] IFormCollection formData)
        {
            //foreach (var fl in FileUploads)
            //{
            //    if (fl.Length > 0)
            //    {
            //        //using (var stream = System.IO.File.Create(filePath))
            //        using (var stream = new MemoryStream())
            //        {
            //            await fl.CopyToAsync(stream);
            //        }
            //    }
            //}
            var file = formData.Files["file"];
            if (file != null && file.Length > 0)
            {
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                }
            }
            return Page();
        }
    }
}