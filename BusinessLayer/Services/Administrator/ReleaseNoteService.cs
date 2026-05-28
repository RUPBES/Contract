using AutoMapper;
using BusinessLayer.Interfaces.ContractInterfaces;
using BusinessLayer.Interfaces.Core;
using BusinessLayer.Models.Settings.Note;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models.EXTRA;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
//using File = DatabaseLayer.Models.KDO.File;

namespace BusinessLayer.Services.Administrator
{
    public class ReleaseNoteService : IReleaseNoteService
    {

        private readonly IContractUoW _db;
        private readonly IHostingEnvironment _env;
        //private readonly IContractsLogger _logger;
        private readonly IFileService _fileService;
        private IMapper _mapper;

        public ReleaseNoteService(
            IContractUoW db,
            IHostingEnvironment env,
           /* IContractsLogger logger,*/ IMapper mapper, IFileService fileService)
        {
            _db = db;
            _env = env;
            _mapper = mapper;
            _fileService = fileService;
        }

        // ── Клиентские методы ──────────────────────────────────────────────────

        public async Task<IEnumerable<ReleaseNoteListItemDto>> GetPublishedAsync(string userId)
        {
            var note = await _db.ReleaseNotes.GetPublishedAsync(userId);

            if (note is null) return null;

            return _mapper.Map<IEnumerable<ReleaseNoteListItemDto>>(note);
        }

        public async Task<ReleaseNoteDetail?> GetDetailAsync(int id, string userId)
        {
            var note = await _db.ReleaseNotes.GetDetailAsync(id, userId);

            if (note is null) return null;

            return note;
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await _mapper.Map<Task<int>>(_db.ReleaseNotes.GetUnreadCountAsync(userId));
        }

        public async Task MarkAsReadAsync(int releaseNoteId, string userId)
        {
            await _db.ReleaseNotes.MarkAsReadAsync(releaseNoteId, userId);
        }

        public async Task MarkAllAsReadAsync(string userId)
        {
            await _db.ReleaseNotes.MarkAllAsReadAsync(userId);

        }

        // ── Административные методы ───────────────────────────────────────────

        public async Task<IEnumerable<ReleaseNote>> GetAllForAdminAsync()
        {
            return await _db.ReleaseNotes.GetAllForAdminAsync();
        }

        public async Task<ReleaseNote?> GetByIdAsync(int id)
        {
            return await _db.ReleaseNotes.GetByIdAsync(id);
        }

        public async Task<ReleaseNote> CreateAsync(CreateReleaseNoteDto dto, string createdByUserId)
        {
            var note = await _db.ReleaseNotes.CreateAsync(_mapper.Map<CreateReleaseNote>(dto), createdByUserId);

            SaveNoteFiles(note.Id, dto.Images, dto.Annotations);

            return note;
        }

        public async Task<bool> UpdateAsync(int id, UpdateReleaseNoteDto dto)
        {
            var resultUpdates = await _db.ReleaseNotes.UpdateAsync(id, _mapper.Map<UpdateReleaseNote>(dto));
            if (resultUpdates && (dto.DeleteImageIds.Count != 0))
            {
                var toDelete = _db.ReleaseNoteFiles
                    .Find(rnf => dto.DeleteImageIds.Contains(rnf.Id))
                    .ToList();

                foreach (var rnf in toDelete)
                {
                    _fileService.Delete(rnf.File.Id);
                }
            }

            if (resultUpdates && (dto.NewImages.Count != 0))
                SaveNoteFiles(id, dto.NewImages, dto.NewAnnotations);

            return resultUpdates;
        }

        public async Task<bool> PublishAsync(int id)
        {
            return await _db.ReleaseNotes.PublishAsync(id);
        }

        public async Task<bool> ArchiveAsync(int id)
        {
            return await _db.ReleaseNotes.ArchiveAsync(id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var result = await _db.ReleaseNotes.DeleteAsync(id);
            if (result)
            {
                var files = _fileService.GetAttachedFiles(id, Enums.Folder.ReleaseNote);
                foreach (var file in files)
                {
                    _fileService.Delete(file.Id);
                }
            }

            return result;
        }

        private void SaveNoteFiles(
            int noteId,
            List<IFormFile> uploads,
            List<string?> annotations)
        {
            if (uploads is null || uploads.Count == 0) return;

            var uploadDir = Path.Combine(_env.WebRootPath, "StaticFiles", "ReleaseNotes", noteId.ToString());
            Directory.CreateDirectory(uploadDir);

            var maxOrder = _db.ReleaseNoteFiles
                .Find(f => f.ReleaseNoteId == noteId)
                .Select(f => (int?)f.SortOrder)
                .Max() ?? -1;

            for (var i = 0; i < uploads.Count; i++)
            {
                var upload = uploads[i];

                //if (!AllowedMimeTypes.Contains(upload.ContentType))
                //{
                //    _logger.WriteLog(LogLevel.Warning,
                //        $"Skipped file {upload.FileName}: unsupported mime type {upload.ContentType}");
                //    continue;
                //}

                var ext = Path.GetExtension(upload.FileName);
                var storedName = $"{Guid.NewGuid():N}{ext}";
                var relativePath = $"/StaticFiles/ReleaseNotes/{noteId}/{storedName}";
                var fullPath = Path.Combine(_env.WebRootPath, relativePath.TrimStart('/'));

                using var stream = new FileStream(fullPath, FileMode.Create);
                upload.CopyTo(stream);

                // Сохраняем в таблицу File (общая для всего проекта)
                var file = new DatabaseLayer.Models.KDO.File
                {
                    FileName = upload.FileName,
                    FilePath = relativePath,
                    FileType = upload.ContentType,
                    DateUploud = DateTime.UtcNow
                };

                _db.Files.Create(file);
                _db.Save(); // нужен file.Id для связки

                // Создаём связку ReleaseNoteFile
                _db.ReleaseNoteFiles.Create(new ReleaseNoteFile
                {
                    ReleaseNoteId = noteId,
                    FileId = file.Id,
                    Annotation = i < annotations.Count ? annotations[i] : null,
                    SortOrder = ++maxOrder
                });

                _db.Save();

            }
        }

        // ── Вспомогательные методы ────────────────────────────────────────────

        //private async Task SaveFilesAsync(
        //    int noteId,
        //    List<IFormFile> uploads,
        //    List<string?> annotations)
        //{
        //    if (uploads is null || uploads.Count == 0) return;

        //    var uploadDir = Path.Combine(
        //        _env.WebRootPath, "uploads", "release-notes", noteId.ToString());
        //    Directory.CreateDirectory(uploadDir);

        //    var maxOrder = await _db.ReleaseNoteFiles
        //        .Where(f => f.ReleaseNoteId == noteId)
        //        .Select(f => (int?)f.SortOrder)
        //        .MaxAsync() ?? -1;

        //    for (var i = 0; i < uploads.Count; i++)
        //    {
        //        var upload = uploads[i];

        //        if (!AllowedMimeTypes.Contains(upload.ContentType))
        //        {
        //            _logger.WriteLog( LogLevel.Warning,
        //                $"Skipped file { upload.FileName}: unsupported mime type {upload.ContentType}");
        //            continue;
        //        }

        //        var ext = Path.GetExtension(upload.FileName);
        //        var storedName = $"{Guid.NewGuid():N}{ext}";
        //        var relativePath = $"/uploads/release-notes/{noteId}/{storedName}";
        //        var fullPath = Path.Combine(_env.WebRootPath, relativePath.TrimStart('/'));

        //        await using var stream = new FileStream(fullPath, FileMode.Create);
        //        await upload.CopyToAsync(stream);

        //        // Сохраняем в таблицу File (общая для всего проекта)
        //        var file = new DatabaseLayer.Models.KDO.File
        //        {
        //            FileName = upload.FileName,
        //            FilePath = relativePath,
        //            FileType = upload.ContentType,
        //            DateUploud = DateTime.UtcNow
        //        };

        //        _db.Files.Add(file);
        //        await _db.SaveChangesAsync(); // нужен file.Id для связки

        //        // Создаём связку ReleaseNoteFile
        //        _db.ReleaseNoteFiles.Add(new ReleaseNoteFile
        //        {
        //            ReleaseNoteId = noteId,
        //            FileId = file.Id,
        //            Annotation = i < annotations.Count ? annotations[i] : null,
        //            SortOrder = ++maxOrder
        //        });

        //        await _db.SaveChangesAsync();
        //    }
        //}
    }
}