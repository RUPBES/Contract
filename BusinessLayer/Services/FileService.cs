using AutoMapper;
using BusinessLayer.Enums;
using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Reflection;
using File = DatabaseLayer.Models.File;

namespace BusinessLayer.Services
{
    internal class FileService: IFileService
    {
        private IMapper _mapper;
        private readonly IContractUoW _database;
        private readonly ILoggerContract _logger;
        private readonly IHttpContextAccessor _http;
        private readonly IHostingEnvironment _env;

        public FileService(IContractUoW database, IMapper mapper, ILoggerContract logger, IHttpContextAccessor http, IHostingEnvironment env)
        {
            _database = database;
            _mapper = mapper;
            _logger = logger;
            _http = http;
            _env = env;
        }

        public int? Create(IFormFileCollection files, FolderEnum folder)
        {
            int id = default;
            if (files != null /*&& item != null*/)
            {
                foreach (var file in files)
                {
                    string fileName = file.FileName;
                    string folderPath = @$"\StaticFiles\{folder}\";
                    string fullPath = _env.WebRootPath + folderPath + fileName;

                    int i = 1;
                    int positionDot = file.FileName.LastIndexOf('.');

                    if (!Directory.Exists(_env.WebRootPath + "\\StaticFiles\\" + folder))
                    {
                        DirectoryInfo directory = new DirectoryInfo($@"{_env.WebRootPath}\StaticFiles\{folder}");
                        directory.Create();
                    }

                    while (System.IO.File.Exists(fullPath))
                    {
                        fileName = file.FileName.Insert(positionDot, "[" + i + "]");
                        fullPath = @$"{_env.WebRootPath}\StaticFiles\{folder}\{fileName}";
                        i++;
                    }

                    folderPath += fileName;

                    using (var fileStream = new FileStream(fullPath, FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }

                    File fileNew = new File
                    {
                        FileName = fileName,
                        FilePath = folderPath,
                        FileType = file.ContentType,
                        DateUploud = DateTime.Now,                        
                    };

                    _database.Files.Create(fileNew);
                    _database.Save();
                    _logger.WriteLog(LogLevel.Information, $"create file, ID={fileNew.Id}", typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);

                    id = fileNew.Id;
                }
            }
            else
            {
                _logger.WriteLog(LogLevel.Warning, $"not create file, object or IFormFileCollection or name of folder is null", typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
            }


            return id;            
        }

        public void Delete(int id)
        {
            if (id > 0)
            {
                var file = _database.Files.GetById(id);

                if (file is not null)
                {
                    try
                    {
                        file.FilePath = _env.WebRootPath + file.FilePath;

                        if (System.IO.File.Exists(file.FilePath))
                        {
                            System.IO.File.Delete(file.FilePath);
                            _logger.WriteLog(LogLevel.Information, $"file has been removed from folder ##{file.FilePath}##, ID={id}", typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
                        }

                        _database.Files.Delete(file.Id);
                        _database.Save();
                        _logger.WriteLog(LogLevel.Information, $"file has been removed from database, ID={id}", typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);

                    }
                    catch (Exception e)
                    {
                        _logger.WriteLog(LogLevel.Error, e.Message, typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
                    }
                }
                else
                {
                    _logger.WriteLog(LogLevel.Warning, $"file didn't find", typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
                }
            }
            else
            {
                _logger.WriteLog(LogLevel.Warning, $"not delete file, ID is not more than zero", typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
            }
        }

        public IEnumerable<FileDTO> GetAll()
        {
            return _mapper.Map<IEnumerable<FileDTO>>(_database.Files.GetAll());
        }

        public IEnumerable<FileDTO> Find(Func<File, bool> predicate)
        {
            return _mapper.Map<IEnumerable<FileDTO>>(_database.Files.Find(predicate));
        }

        public FileDTO? GetById(int id)
        {
            var file = _database.Files.GetById(id);

            if (file is not null)
            {
                return _mapper.Map<FileDTO>(file);
            }
            else
            {
                return null;
            }
        }

        public void Update(FileDTO item)
        {
            if (item is not null)
            {
                _database.Files.Update(_mapper.Map<File>(item));
                _database.Save();
                _logger.WriteLog(LogLevel.Information, $"update file, ID={item.Id}", typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
            }
            else
            {
                _logger.WriteLog(LogLevel.Warning, $"not update file, object is null", typeof(OrganizationService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
            }
        }

        public IEnumerable<FileDTO> GetFilesByAmendmentId(int amendmentId)
        {
            List<File> result = new List<File>();
            var files = _database.AmendmentFiles.Find(x=>x.AmendmentId == amendmentId);

            foreach ( var file in files)
            {
                result.AddRange(_database.Files.Find(x => x.Id == file.FileId));
            }

            return _mapper.Map<IEnumerable<FileDTO>>(result);
        }
    }
}