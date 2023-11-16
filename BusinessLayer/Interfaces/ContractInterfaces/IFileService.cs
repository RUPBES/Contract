using BusinessLayer.Enums;
using BusinessLayer.Models;
using Microsoft.AspNetCore.Http;
using File = DatabaseLayer.Models.File;

namespace BusinessLayer.Interfaces.ContractInterfaces
{
    public interface IFileService
    {
        int? Create(IFormFileCollection files, FolderEnum folder, int entityId);
        IEnumerable<FileDTO> GetAll();
        IEnumerable<FileDTO> Find(Func<File, bool> predicate);
        FileDTO? GetById(int id);
        void Update(FileDTO item);
        void Delete(int id);
        IEnumerable<FileDTO> GetFilesOfEntity(int amendmentId, FolderEnum folder);
        void AttachFileToEntity(int fileId, int entityId, FolderEnum folder);
    }
}
