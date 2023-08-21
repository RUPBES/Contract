using AutoMapper;
using BusinessLayer.Enums;
using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Reflection;
using File = DatabaseLayer.Models.File;

namespace BusinessLayer.Services
{
    internal class FileService : IFileService
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
                    _logger.WriteLog(LogLevel.Information, $"create file ID={fileNew.Id}", typeof(FileService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);

                    id = fileNew.Id;
                }
            }
            else
            {
                _logger.WriteLog(LogLevel.Warning, $"not create file, object or IFormFileCollection or name of folder is null", typeof(FileService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
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
                            _logger.WriteLog(LogLevel.Information, $"file has been removed from folder ##{file.FilePath}##, ID={id}", typeof(FileService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
                        }

                        _database.Files.Delete(file.Id);
                        _database.Save();
                        _logger.WriteLog(LogLevel.Information, $"file has been removed from database, ID={id}", typeof(FileService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);

                    }
                    catch (Exception e)
                    {
                        _logger.WriteLog(LogLevel.Error, e.Message, typeof(FileService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
                    }
                }
                else
                {
                    _logger.WriteLog(LogLevel.Warning, $"file didn't find", typeof(FileService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
                }
            }
            else
            {
                _logger.WriteLog(LogLevel.Warning, $"not delete file, ID is not more than zero", typeof(FileService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
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
                _logger.WriteLog(LogLevel.Information, $"update file, ID={item.Id}", typeof(FileService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
            }
            else
            {
                _logger.WriteLog(LogLevel.Warning, $"not update file, object is null", typeof(FileService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
            }
        }

        public IEnumerable<FileDTO> GetFilesOfEntity(int entityId, FolderEnum folder)
        {
            List<File> result = new List<File>();
            
            switch (folder)
            {
                case FolderEnum.Acts:
                    var filesAct = _database.ActFiles.Find(x => x.ActId == entityId);
                    foreach (var file in filesAct)
                    {
                        result.AddRange(_database.Files.Find(x => x.Id == file.FileId));
                    }
                    return _mapper.Map<IEnumerable<FileDTO>>(result);

                case FolderEnum.Amendment:

                    var filesAmend = _database.AmendmentFiles.Find(x => x.AmendmentId == entityId);

                    foreach (var file in filesAmend)
                    {
                        result.AddRange(_database.Files.Find(x => x.Id == file.FileId));
                    }

                    return _mapper.Map<IEnumerable<FileDTO>>(result);

                case FolderEnum.CommissionActs:
                    var filesComm = _database.CommissionActFiles.Find(x => x.СommissionActId == entityId);
                    foreach (var file in filesComm)
                    {
                        result.AddRange(_database.Files.Find(x => x.Id == file.FileId));
                    }
                    return _mapper.Map<IEnumerable<FileDTO>>(result);

                case FolderEnum.Correspondences:
                    var filesCorres = _database.CorrespondenceFiles.Find(x => x.CorrespondenceId == entityId);
                    foreach (var file in filesCorres)
                    {
                        result.AddRange(_database.Files.Find(x => x.Id == file.FileId));
                    }
                    return _mapper.Map<IEnumerable<FileDTO>>(result);

                case FolderEnum.EstimateDocumentations:
                    
                    var filesEstimate = _database.EstimateDocFiles.Find(x => x.EstimateDocId == entityId);
                    foreach (var file in filesEstimate)
                    {
                        result.AddRange(_database.Files.Find(x => x.Id == file.FileId));
                    }
                    return _mapper.Map<IEnumerable<FileDTO>>(result);
                case FolderEnum.Form3C:

                    var filesForm = _database.FormFiles.Find(x => x.FormId == entityId);
                    foreach (var file in filesForm)
                    {
                        result.AddRange(_database.Files.Find(x => x.Id == file.FileId));
                    }
                    return _mapper.Map<IEnumerable<FileDTO>>(result);

            }

            return _mapper.Map<IEnumerable<FileDTO>>(result);
        }

        public void AttachFileToEntity(int fileId, int entityId, FolderEnum folder)
        {
            if (fileId > 0 && entityId > 0)
            {
                switch (folder)
                {
                    case FolderEnum.Acts:
                        _database.ActFiles.Create(new ActFile { FileId = fileId, ActId = entityId });
                        _database.Save();
                        _logger.WriteLog(LogLevel.Information, $"attach file to act", typeof(FileService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
                        break;


                    case FolderEnum.Amendment:
                        _database.AmendmentFiles.Create(new AmendmentFile { FileId = fileId, AmendmentId = entityId });
                        _database.Save();
                        _logger.WriteLog(LogLevel.Information, $"attach file to amendment", typeof(FileService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
                        break;

                    case FolderEnum.CommissionActs:
                        _database.CommissionActFiles.Create(new CommissionActFile { FileId = fileId, СommissionActId = entityId });
                        _database.Save();
                        _logger.WriteLog(LogLevel.Information, $"attach file to commission of act", typeof(FileService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
                        break;

                    case FolderEnum.Correspondences:
                        _database.CorrespondenceFiles.Create(new CorrespondenceFile { FileId = fileId, CorrespondenceId = entityId });
                        _database.Save();
                        _logger.WriteLog(LogLevel.Information, $"attach file to correspondence", typeof(FileService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
                        break;

                    case FolderEnum.EstimateDocumentations:
                        _database.EstimateDocFiles.Create(new EstimateDocFile { FileId = fileId, EstimateDocId = entityId });
                        _database.Save();
                        _logger.WriteLog(LogLevel.Information, $"attach file to estimate document", typeof(FileService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
                        break;
                    case FolderEnum.Form3C:
                        _database.FormFiles.Create(new FormFile { FileId = fileId, FormId = entityId });
                        _database.Save();
                        _logger.WriteLog(LogLevel.Information, $"attach file to form c-3a", typeof(FileService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
                        break;

                    case FolderEnum.Other:
                        break;                   
                }
            }
            else
            {
                _logger.WriteLog(LogLevel.Warning, $"not create file, file id or entity id is less than zero or name of folder is null", typeof(FileService).Name, MethodBase.GetCurrentMethod().Name, _http?.HttpContext?.User?.Identity?.Name);
            }
        }
    }
}