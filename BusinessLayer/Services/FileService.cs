using AutoMapper;
using BusinessLayer.Enums;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Interfaces.Shared;
using BusinessLayer.Models.KDO;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models.EXTRA;
using DatabaseLayer.Models.KDO;
using DatabaseLayer.Models.PRO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Reflection;
using File = DatabaseLayer.Models.KDO.File;

namespace BusinessLayer.Services
{
    internal class FileService : IFileService
    {
        private IMapper _mapper;
        private readonly IContractUoW _database;
        private readonly IContractArchiveUoW _databaseArch;
        private readonly IContractsLogger _logger;
        private readonly IHostingEnvironment _env;

        public FileService(IContractUoW database, IMapper mapper, IContractsLogger logger, IHostingEnvironment env, IContractArchiveUoW databaseArch)
        {
            _database = database;
            _mapper = mapper;
            _logger = logger;
            _env = env;
            _databaseArch = databaseArch;
        }

        public int? Create(IFormFileCollection files, Folder folder, int entityId, string nestedFolder = null)
        {
            int id = default;
            if (files != null)
            {
                foreach (var file in files)
                {
                    string fileName = file.FileName;
                    string folderNested = nestedFolder is null ? $@"{folder}" : $@"{folder}\{nestedFolder}";
                    string folderPath = @$"\StaticFiles\{folderNested}\";
                    string fullPath = _env.WebRootPath + folderPath + fileName;

                    int i = 1;
                    int positionDot = file.FileName.LastIndexOf('.');

                    //if (!Directory.Exists(_env.WebRootPath + "\\StaticFiles\\" + folderNested))
                    //{
                    //    DirectoryInfo directory = new DirectoryInfo($@"{_env.WebRootPath}\StaticFiles\{folderNested}");
                    //    directory.Create();
                    //}

                    if (!Directory.Exists($@"{_env.WebRootPath}\StaticFiles\{folderNested}"))
                    {
                        Directory.CreateDirectory($@"{_env.WebRootPath}\StaticFiles\{folderNested}");
                    }

                    while (System.IO.File.Exists(fullPath))
                    {
                        fileName = file.FileName.Insert(positionDot, "[" + i + "]");
                        fullPath = @$"{_env.WebRootPath}\StaticFiles\{folderNested}\{fileName}";
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

                    _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"create file ID={fileNew.Id}",
                            nameSpace: typeof(FileService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);

                    id = fileNew.Id;

                    if (folder != Folder.Other && entityId != 0)
                    {
                        AttachFileToEntity(fileNew.Id, entityId, folder);
                    }
                }
            }
            else
            {
                _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not create file, object or IFormFileCollection or name of folder is null",
                            nameSpace: typeof(FileService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
            }

            return id;
        }

        public int? CreateAsync(List<IFormFile> files, Folder folder, int entityId, string nestedFolder = null)
        {
            int id = default;
            if (files != null)
            {
                foreach (var file in files)
                {
                    string fileName = file.FileName;
                    string folderNested = nestedFolder is null ? $@"{folder}" : $@"{folder}\{nestedFolder}";
                    string folderPath = @$"\StaticFiles\{folderNested}\";
                    string fullPath = _env.WebRootPath + folderPath + fileName;

                    int i = 1;
                    int positionDot = file.FileName.LastIndexOf('.');

                    //if (!Directory.Exists(_env.WebRootPath + "\\StaticFiles\\" + folderNested))
                    //{
                    //    DirectoryInfo directory = new DirectoryInfo($@"{_env.WebRootPath}\StaticFiles\{folderNested}");
                    //    directory.Create();
                    //}

                    if (!Directory.Exists($@"{_env.WebRootPath}\StaticFiles\{folderNested}"))
                    {
                        Directory.CreateDirectory($@"{_env.WebRootPath}\StaticFiles\{folderNested}");
                    }

                    while (System.IO.File.Exists(fullPath))
                    {
                        fileName = file.FileName.Insert(positionDot, "[" + i + "]");
                        fullPath = @$"{_env.WebRootPath}\StaticFiles\{folderNested}\{fileName}";
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

                    _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"create file ID={fileNew.Id}",
                            nameSpace: typeof(FileService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);

                    id = fileNew.Id;

                    if (folder != Folder.Other && entityId != 0)
                    {
                        AttachFileToEntity(fileNew.Id, entityId, folder);
                    }
                }
            }
            else
            {
                _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not create file, object or IFormFileCollection or name of folder is null",
                            nameSpace: typeof(FileService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
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
                            System.IO.File.Delete(file.FilePath); /// удаляем с диска

                            _logger.WriteLog(
                               logLevel: LogLevel.Information,
                               message: $"file has been removed from folder {file.FilePath}, ID={id}",
                               nameSpace: typeof(FileService).Name,
                               methodName: MethodBase.GetCurrentMethod().Name);
                        }

                        _database.Files.Delete(file.Id); /// удаляем из БД
                        _database.Save();

                        _logger.WriteLog(
                               logLevel: LogLevel.Information,
                               message: $"file has been removed from database, ID={id}",
                               nameSpace: typeof(FileService).Name,
                               methodName: MethodBase.GetCurrentMethod().Name);
                    }
                    catch (Exception e)
                    {
                        _logger.WriteLog(
                               logLevel: LogLevel.Error,
                               message: e.Message,
                               nameSpace: typeof(FileService).Name,
                               methodName: MethodBase.GetCurrentMethod().Name);
                    }
                }
                else
                {
                    _logger.WriteLog(
                               logLevel: LogLevel.Warning,
                               message: $"file didn't find",
                               nameSpace: typeof(FileService).Name,
                               methodName: MethodBase.GetCurrentMethod().Name);
                }
            }
            else
            {
                _logger.WriteLog(
                               logLevel: LogLevel.Warning,
                               message: $"not delete file, ID is not more than zero",
                               nameSpace: typeof(FileService).Name,
                               methodName: MethodBase.GetCurrentMethod().Name);
            }
        }

        public void DeleteByPath(string absolutePath)
        {
            if (System.IO.File.Exists(absolutePath))
            {
                System.IO.File.Delete(absolutePath);
            }
        }

        public void DeleteFromFolderByContractId(int contractId, IEnumerable<File> files)
        {
            if (contractId is 0 || !files.Any())
            {
                _logger.WriteLog(logLevel: LogLevel.Warning,
                                   message: $"not delete file, ID is not more than zero",
                                   nameSpace: typeof(FileService).Name,
                                   methodName: MethodBase.GetCurrentMethod().Name);
                return;
            }

            try
            {
                foreach (var file in files)
                {
                    file.FilePath = _env.WebRootPath + file.FilePath;

                    if (System.IO.File.Exists(file.FilePath))
                    {
                        System.IO.File.Delete(file.FilePath);
                    }
                }
                _logger.WriteLog(logLevel: LogLevel.Information,
                                  message: $"files has been removed from folder",
                                  nameSpace: typeof(FileService).Name,
                                  methodName: MethodBase.GetCurrentMethod().Name);

            }
            catch (Exception e)
            {
                _logger.WriteLog(logLevel: LogLevel.Error,
                                   message: e.Message,
                                   nameSpace: typeof(FileService).Name,
                                   methodName: MethodBase.GetCurrentMethod().Name);
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

        public FileDTO? GetById(int id, bool? useArchiveData)
        {
            var file = (useArchiveData == true) ?
                _databaseArch.Files.GetById(id) :
                _database.Files.GetById(id);

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

                _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"update file, ID={item.Id}",
                            nameSpace: typeof(FileService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
            }
            else
            {
                _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not update file, object is null",
                            nameSpace: typeof(FileService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
            }
        }

        public IEnumerable<FileDTO> GetAttachedFiles(int entityId, Folder folder, bool? useArchiveData)
        {
            List<File> result = new List<File>();

            switch (folder)
            {
                case Folder.Acts:
                    var filesAct = (useArchiveData == true) ?
                        _databaseArch.ActFiles.Find(x => x.ActId == entityId) :
                        _database.ActFiles.Find(x => x.ActId == entityId);

                    foreach (var file in filesAct)
                    {
                        result.AddRange((useArchiveData == true) ?
                            _databaseArch.Files.Find(x => x.Id == file.FileId) :
                            _database.Files.Find(x => x.Id == file.FileId));
                    }

                    return _mapper.Map<IEnumerable<FileDTO>>(result);

                case Folder.Amendment:

                    var filesAmend = (useArchiveData == true) ?
                        _databaseArch.AmendmentFiles.Find(x => x.AmendmentId == entityId) :
                        _database.AmendmentFiles.Find(x => x.AmendmentId == entityId);

                    foreach (var file in filesAmend)
                    {
                        result.AddRange((useArchiveData == true) ?
                            _databaseArch.Files.Find(x => x.Id == file.FileId) :
                            _database.Files.Find(x => x.Id == file.FileId));
                    }

                    return _mapper.Map<IEnumerable<FileDTO>>(result);

                case Folder.AdditionalTerms:

                    var filesAdd = 
                        //(useArchiveData == true) ?
                        //_databaseArch.Add.Find(x => x.AmendmentId == entityId) :
                        _database.AdditionalTermFiles.Find(x => x.AdditionalTermId == entityId);

                    foreach (var file in filesAdd)
                    {
                        result.AddRange((useArchiveData == true) ?
                            _databaseArch.Files.Find(x => x.Id == file.FileId) :
                            _database.Files.Find(x => x.Id == file.FileId));
                    }

                    return _mapper.Map<IEnumerable<FileDTO>>(result);

                case Folder.CommissionActs:

                    var filesComm = (useArchiveData == true) ?
                        _databaseArch.CommissionActFiles.Find(x => x.СommissionActId == entityId) :
                        _database.CommissionActFiles.Find(x => x.СommissionActId == entityId);

                    foreach (var file in filesComm)
                    {
                        result.AddRange((useArchiveData == true) ?
                            _databaseArch.Files.Find(x => x.Id == file.FileId) :
                            _database.Files.Find(x => x.Id == file.FileId));
                    }

                    return _mapper.Map<IEnumerable<FileDTO>>(result);

                case Folder.Correspondences:

                    var filesCorres = (useArchiveData == true) ?
                        _databaseArch.CorrespondenceFiles.Find(x => x.CorrespondenceId == entityId) :
                        _database.CorrespondenceFiles.Find(x => x.CorrespondenceId == entityId);

                    foreach (var file in filesCorres)
                    {
                        result.AddRange((useArchiveData == true) ?
                           _databaseArch.Files.Find(x => x.Id == file.FileId) :
                           _database.Files.Find(x => x.Id == file.FileId));
                    }
                    return _mapper.Map<IEnumerable<FileDTO>>(result);

                case Folder.Estimate:

                    var filesEstimate = (useArchiveData == true) ?
                        _databaseArch.EstimateFiles.Find(x => x.EstimateId == entityId) :
                        _database.EstimateFiles.Find(x => x.EstimateId == entityId);

                    foreach (var file in filesEstimate)
                    {
                        result.AddRange((useArchiveData == true) ?
                           _databaseArch.Files.Find(x => x.Id == file.FileId) :
                           _database.Files.Find(x => x.Id == file.FileId));
                    }

                    return _mapper.Map<IEnumerable<FileDTO>>(result);

                case Folder.EstimateDocumentations:

                    var filesEstimateDoc = (useArchiveData == true) ?
                        _databaseArch.EstimateDocFiles.Find(x => x.EstimateDocId == entityId) :
                        _database.EstimateDocFiles.Find(x => x.EstimateDocId == entityId);

                    foreach (var file in filesEstimateDoc)
                    {
                        result.AddRange((useArchiveData == true) ?
                           _databaseArch.Files.Find(x => x.Id == file.FileId) :
                           _database.Files.Find(x => x.Id == file.FileId));
                    }

                    return _mapper.Map<IEnumerable<FileDTO>>(result);

                case Folder.Form3C:

                    var filesForm = (useArchiveData == true) ?
                        _databaseArch.FormFiles.Find(x => x.FormId == entityId) :
                        _database.FormFiles.Find(x => x.FormId == entityId);

                    foreach (var file in filesForm)
                    {
                        result.AddRange((useArchiveData == true) ?
                           _databaseArch.Files.Find(x => x.Id == file.FileId) :
                           _database.Files.Find(x => x.Id == file.FileId));
                    }

                    return _mapper.Map<IEnumerable<FileDTO>>(result);

                case Folder.Contracts:

                    var filesContract = (useArchiveData == true) ?
                        _databaseArch.ContractFiles.Find(x => x.ContractId == entityId) :
                        _database.ContractFiles.Find(x => x.ContractId == entityId);

                    foreach (var file in filesContract)
                    {
                        result.AddRange((useArchiveData == true) ?
                           _databaseArch.Files.Find(x => x.Id == file.FileId) :
                           _database.Files.Find(x => x.Id == file.FileId));
                    }

                    return _mapper.Map<IEnumerable<FileDTO>>(result);

                case Folder.PrepaymentTake:

                    var filesPrepTakeId = (useArchiveData == true) ?
                        _databaseArch.PrepaymentTakes.GetById(entityId).FileId :
                        _database.PrepaymentTakes.GetById(entityId).FileId;


                    result.AddRange(
                        (useArchiveData == true) ?
                            _databaseArch.Files.Find(x => x.Id == filesPrepTakeId) :
                            _database.Files.Find(x => x.Id == filesPrepTakeId)
                    );

                    return _mapper.Map<IEnumerable<FileDTO>>(result);

                case Folder.SelectionProcedures:

                    var filesProcedure = (useArchiveData == true) ?
                        _databaseArch.SlctnProcedureFiles.Find(x => x.SlctnProcedureId == entityId) :
                        _database.SlctnProcedureFiles.Find(x => x.SlctnProcedureId == entityId);

                    foreach (var file in filesProcedure)
                    {
                        result.AddRange((useArchiveData == true) ?
                           _databaseArch.Files.Find(x => x.Id == file.FileId) :
                           _database.Files.Find(x => x.Id == file.FileId));
                    }
                    return _mapper.Map<IEnumerable<FileDTO>>(result);

                case Folder.ReleaseNote:

                    var releaseNote = _database.ReleaseNoteFiles.Find(x => x.ReleaseNoteId == entityId);
                    List<FileDTO> resultRe = new List<FileDTO>();

                    foreach (var file in releaseNote)
                    {
                        resultRe.AddRange(_database.Files.Find(x => x.Id == file.FileId).Select(x=> new FileDTO
                        {
                            Id = file.File.Id,
                            FileName = file.File.FileName,
                            FileType = file.File.FileType,
                            FilePath = file.File.FilePath,
                            Annotation = file.Annotation,
                            SortOrder = file.SortOrder,
                        }));
                    }
                    return resultRe;
            }

            return _mapper.Map<IEnumerable<FileDTO>>(result);
        }

        public void AttachFileToEntity(int fileId, int entityId, Folder folder)
        {
            if (fileId > 0 && entityId > 0)
            {
                switch (folder)
                {
                    case Folder.Acts:
                        _database.ActFiles.Create(new ActFile { FileId = fileId, ActId = entityId });
                        _database.Save();

                        _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"attach file to act",
                            nameSpace: typeof(FileService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
                        break;

                    case Folder.Amendment:
                        _database.AmendmentFiles.Create(new AmendmentFile { FileId = fileId, AmendmentId = entityId });
                        _database.Save();

                        _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"attach file to amendment",
                            nameSpace: typeof(FileService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
                        break;

                    case Folder.AdditionalTerms:
                        _database.AdditionalTermFiles.Create(new AdditionalTermFile { FileId = fileId, AdditionalTermId = entityId });
                        _database.Save();

                        _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"attach file to additional term",
                            nameSpace: typeof(FileService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
                        break;
                    case Folder.CommissionActs:
                        _database.CommissionActFiles.Create(new CommissionActFile { FileId = fileId, СommissionActId = entityId });
                        _database.Save();

                        _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"attach file to commission of act",
                            nameSpace: typeof(FileService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
                        break;

                    case Folder.Correspondences:
                        _database.CorrespondenceFiles.Create(new CorrespondenceFile { FileId = fileId, CorrespondenceId = entityId });
                        _database.Save();

                        _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"attach file to correspondence",
                            nameSpace: typeof(FileService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
                        break;

                    case Folder.EstimateDocumentations:
                        _database.EstimateDocFiles.Create(new EstimateDocFile { FileId = fileId, EstimateDocId = entityId });
                        _database.Save();

                        _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"attach file to estimate document",
                            nameSpace: typeof(FileService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
                        break;

                    case Folder.Form3C:
                        _database.FormFiles.Create(new FormFile { FileId = fileId, FormId = entityId });
                        _database.Save();

                        _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"attach file to form c-3a",
                            nameSpace: typeof(FileService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
                        break;

                    case Folder.Contracts:
                        _database.ContractFiles.Create(new ContractFile { FileId = fileId, ContractId = entityId });
                        _database.Save();

                        _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"attach file to contract",
                            nameSpace: typeof(FileService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
                        break;

                    case Folder.Estimate:
                        _database.EstimateFiles.Create(new EstimateFile { FileId = fileId, EstimateId = entityId });
                        _database.Save();

                        _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"attach file to estimate",
                            nameSpace: typeof(FileService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
                        break;

                    case Folder.SelectionProcedures:
                        _database.SlctnProcedureFiles.Create(new SlctnProcedureFile { FileId = fileId, SlctnProcedureId = entityId });
                        _database.Save();

                        _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"attach file to selection of procedure",
                            nameSpace: typeof(FileService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
                        break;

                    case Folder.ReleaseNote:
                        _database.ReleaseNoteFiles.Create(new ReleaseNoteFile { FileId = fileId, ReleaseNoteId = entityId });
                        _database.Save();

                        _logger.WriteLog(
                            logLevel: LogLevel.Information,
                            message: $"attach file to release note",
                            nameSpace: typeof(FileService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
                        break;

                    case Folder.Other:
                        break;
                }
            }
            else
            {
                _logger.WriteLog(
                            logLevel: LogLevel.Warning,
                            message: $"not create file, file id or entity id is less than zero or name of folder is null",
                            nameSpace: typeof(FileService).Name,
                            methodName: MethodBase.GetCurrentMethod().Name);
            }
        }

        public IEnumerable<FileDTO> GetByBuildingCode(int contractId, string buildingCode, string keyFolder, bool? useArchiveData)
        {
            return (useArchiveData == true) ?
                _mapper.Map<IEnumerable<FileDTO>>(_databaseArch.Files.Find(x => x.FilePath.Contains($@"\{contractId}\{buildingCode}\") && x.FilePath.Contains($@"\{keyFolder}\"))) :
                _mapper.Map<IEnumerable<FileDTO>>(_database.Files.Find(x => x.FilePath.Contains($@"\{contractId}\{buildingCode}\") && x.FilePath.Contains($@"\{keyFolder}\")));
        }
    }
}