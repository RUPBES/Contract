using BusinessLayer.Interfaces.CommonInterfaces;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace BusinessLayer.Services
{
    public class StreamFileUploadLocalService : IStreamFileUploadService
    {

        private readonly IHostingEnvironment _env;
        public StreamFileUploadLocalService(IHostingEnvironment env)
        {
                _env = env;
        }
        public async Task<bool> UploadFile(MultipartReader reader, MultipartSection? section)
        {
            

            
            while (section != null)
            {
                var hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(
                    section.ContentDisposition, out var contentDisposition
                );

                if (hasContentDispositionHeader)
                {
                    if (contentDisposition.DispositionType.Equals("form-data") &&
                    (!string.IsNullOrEmpty(contentDisposition.FileName.Value) ||
                    !string.IsNullOrEmpty(contentDisposition.FileNameStar.Value)))
                    {
                        string filePath = Path.GetFullPath(Path.Combine(_env.WebRootPath + "\\StaticFiles\\Contracts"));
                        // byte[] fileArray;
                        using (var memoryStream = new FileStream(Path.Combine(filePath, contentDisposition.FileName.Value), FileMode.Create))
                        {
                            await section.Body.CopyToAsync(memoryStream);
                            await memoryStream.FlushAsync();
                            memoryStream.Close();
                            //fileArray = memoryStream.ToArray();
                        }
                        //using (var fileStream = System.IO.File.Create(Path.Combine(filePath, contentDisposition.FileName.Value)))
                        //{
                        //    await fileStream.WriteAsync(fileArray);
                        //}
                    }
                }
                section = await reader.ReadNextSectionAsync();
                
        }
            return true;
        }
    }
}
