using BusinessLayer.Enums;
using BusinessLayer.Models;
using Microsoft.AspNetCore.Http;
using File = DatabaseLayer.Models.KDO.File;

namespace BusinessLayer.Interfaces.ContractInterfaces
{
    public interface IFileService
    {
        int? Create(IFormFileCollection files, FolderEnum folder, int entityId, string nestedFolder = null);
        IEnumerable<FileDTO> GetAll();
        IEnumerable<FileDTO> Find(Func<File, bool> predicate);
        FileDTO? GetById(int id, bool? useArchiveData = null);
        void Update(FileDTO item);
        void Delete(int id);
        void DeleteByPath(string absolutePath);
        IEnumerable<FileDTO> GetAttachedFiles(int entityId, FolderEnum folder, bool? useArchiveData = null);
        void AttachFileToEntity(int fileId, int entityId, FolderEnum folder);

        IEnumerable<FileDTO> GetByBuildingCode(int contractId, string buildingCode, string keyFolder, bool? useArchiveData = null);
    }
}
