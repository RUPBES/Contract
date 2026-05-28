using DatabaseLayer.Data;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models.EXTRA;
using DatabaseLayer.Models.KDO;
using Microsoft.EntityFrameworkCore;


namespace DatabaseLayer.RepositoriesEF.Notes
{
    internal class ReleaseNoteRepository : IReleaseNoteRepository
    {

        private readonly ContractsContext _context;
        public ReleaseNoteRepository(ContractsContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ReleaseNoteListItem>> GetPublishedAsync(string userId)
        {
            var note =  await _context.ReleaseNotes
                .Where(rn => rn.Status == ReleaseNoteStatus.Published)
                .OrderByDescending(rn => rn.PublishedAt)
                .Select(rn => new ReleaseNoteListItem
                {
                    Id = rn.Id,
                    Title = rn.Title,
                    Version = rn.Version,
                    PublishedAt = rn.PublishedAt!.Value,
                    ImagesCount = rn.ReleaseNoteFiles.Count,
                    IsRead = rn.UserReleaseNotes
                        .Any(urn => urn.UserId == userId && urn.IsRead)
                })
                .AsNoTracking()
                .ToListAsync();

            return note;
        }

        public async Task<ReleaseNoteDetail?> GetDetailAsync(int id, string userId)
        {
            var note = await _context.ReleaseNotes
                .Where(rn => rn.Id == id && rn.Status == ReleaseNoteStatus.Published)
                .Include(rn => rn.ReleaseNoteFiles.OrderBy(f => f.SortOrder))
                    .ThenInclude(rnf => rnf.File)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (note is null) return null;

            var isRead = await _context.UserReleaseNotes
                .AnyAsync(urn => urn.UserId == userId && urn.ReleaseNoteId == id && urn.IsRead);

            return new ReleaseNoteDetail
            {
                Id = note.Id,
                Title = note.Title,
                Content = note.Content,
                Version = note.Version,
                PublishedAt = note.PublishedAt!.Value,
                IsRead = isRead,
                CreatedByUserId = note.CreatedByUserId,
                Images = note.ReleaseNoteFiles
                    .Select(rnf => new ReleaseNoteFile
                    {
                        Id = rnf.Id,
                        FileId = rnf.File.Id, //_context?.Files?.Where(x=>x.Id == rnf.File.Id)?.FirstOrDefault()?.Id ?? 0,
                        ReleaseNoteId = rnf.ReleaseNoteId,
                        Annotation = rnf.Annotation,
                        SortOrder = rnf.SortOrder
                    })
                    .ToList()
            };
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            var readIds = await _context.UserReleaseNotes
                .Where(urn => urn.UserId == userId && urn.IsRead)
                .Select(urn => urn.ReleaseNoteId)
                .ToListAsync();

            return await _context.ReleaseNotes
                .CountAsync(rn => rn.Status == ReleaseNoteStatus.Published
                                  && !readIds.Contains(rn.Id));
        }

        public async Task MarkAsReadAsync(int releaseNoteId, string userId)
        {
            var existing = await _context.UserReleaseNotes
                .FirstOrDefaultAsync(urn => urn.UserId == userId && urn.ReleaseNoteId == releaseNoteId);

            if (existing is null)
            {
                _context.UserReleaseNotes.Add(new UserReleaseNote
                {
                    UserId = userId,
                    ReleaseNoteId = releaseNoteId,
                    IsRead = true,
                    ReadAt = DateTime.UtcNow
                });
            }
            else if (!existing.IsRead)
            {
                existing.IsRead = true;
                existing.ReadAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        public async Task MarkAllAsReadAsync(string userId)
        {
            var publishedIds = await _context.ReleaseNotes
                .Where(rn => rn.Status == ReleaseNoteStatus.Published)
                .Select(rn => rn.Id)
                .ToListAsync();

            var existing = await _context.UserReleaseNotes
                .Where(urn => urn.UserId == userId && publishedIds.Contains(urn.ReleaseNoteId))
                .ToListAsync();

            var existingIds = existing.Select(e => e.ReleaseNoteId).ToHashSet();
            var now = DateTime.UtcNow;

            foreach (var urn in existing.Where(e => !e.IsRead))
            {
                urn.IsRead = true;
                urn.ReadAt = now;
            }

            var newEntries = publishedIds
                .Where(id => !existingIds.Contains(id))
                .Select(id => new UserReleaseNote
                {
                    UserId = userId,
                    ReleaseNoteId = id,
                    IsRead = true,
                    ReadAt = now
                });

            await _context.UserReleaseNotes.AddRangeAsync(newEntries);
            await _context.SaveChangesAsync();
        }

        // ── Административные методы ───────────────────────────────────────────

        public async Task<IEnumerable<ReleaseNote>> GetAllForAdminAsync()
        {
            return await _context.ReleaseNotes
                .OrderByDescending(rn => rn.CreatedAt)
                .ToListAsync();
        }

        public async Task<ReleaseNote?> GetByIdAsync(int id)
        {
            return await _context.ReleaseNotes
                .Include(rn => rn.ReleaseNoteFiles.OrderBy(f => f.SortOrder))
                    .ThenInclude(rnf => rnf.File)
                .FirstOrDefaultAsync(rn => rn.Id == id);
        }

        public async Task<ReleaseNote> CreateAsync(CreateReleaseNote dto, string createdByUserId)
        {
            var note = new ReleaseNote
            {
                Title = dto.Title,
                Content = dto.Content,
                Version = dto.Version,
                CreatedByUserId = createdByUserId,
                Status = ReleaseNoteStatus.Draft
            };

            _context.ReleaseNotes.Add(note);
            await _context.SaveChangesAsync();

            //await SaveFilesAsync(note.Id, dto.Images, dto.Annotations);

            return note;
        }

        public async Task<bool> UpdateAsync(int id, UpdateReleaseNote dto)
        {
            var note = await _context.ReleaseNotes
                .Include(rn => rn.ReleaseNoteFiles)
                    .ThenInclude(rnf => rnf.File)
                .FirstOrDefaultAsync(rn => rn.Id == id);

            if (note is null) return false;

            note.Title = dto.Title;
            note.Content = dto.Content;
            note.Version = dto.Version;
            note.UpdatedAt = DateTime.UtcNow;

            //// Удаляем помеченные файлы
            //if (dto.DeleteImageIds.Count != 0)
            //{
            //    var toDelete = note.ReleaseNoteFiles
            //        .Where(rnf => dto.DeleteImageIds.Contains(rnf.Id))
            //        .ToList();

            //    foreach (var rnf in toDelete)
            //    {
            //        DeleteFileFromDisk(rnf.File.FilePath);
            //        _context.ReleaseNoteFiles.Remove(rnf);
            //        // Удаляем и саму запись File, так как она принадлежит только этому уведомлению
            //        _context.Files.Remove(rnf.File);
            //    }
            //}

            await _context.SaveChangesAsync();

            //await SaveFilesAsync(note.Id, dto.NewImages, dto.NewAnnotations);

            return true;
        }

        public async Task<bool> PublishAsync(int id)
        {
            var note = await _context.ReleaseNotes.FindAsync(id);
            if (note is null || note.Status == ReleaseNoteStatus.Archived) return false;

            note.Status = ReleaseNoteStatus.Published;
            note.PublishedAt ??= DateTime.UtcNow;
            note.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ArchiveAsync(int id)
        {
            var note = await _context.ReleaseNotes.FindAsync(id);
            if (note is null) return false;

            note.Status = ReleaseNoteStatus.Archived;
            note.ArchivedAt = DateTime.UtcNow;
            note.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var note = await _context.ReleaseNotes
                .Include(rn => rn.ReleaseNoteFiles)
                    .ThenInclude(rnf => rnf.File)
                .FirstOrDefaultAsync(rn => rn.Id == id);

            if (note is null) return false;

            foreach (var rnf in note.ReleaseNoteFiles)
            {
                //DeleteFileFromDisk(rnf.File.FilePath);
                _context.Files.Remove(rnf.File);
            }

            _context.ReleaseNotes.Remove(note);
            await _context.SaveChangesAsync();
            return true;
        }

        //// ── Вспомогательные методы ────────────────────────────────────────────

        //private async Task SaveFilesAsync(
        //    int noteId,
        //    List<File> uploads,
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
        //            _logger.WriteLog(LogLevel.Warning,
        //                $"Skipped file {upload.FileName}: unsupported mime type {upload.ContentType}");
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

        //private void DeleteFileFromDisk(string relativePath)
        //{
        //    try
        //    {
        //        var fullPath = Path.Combine(
        //            _env.WebRootPath, relativePath.TrimStart('/'));

        //        if (File.Exists(fullPath))
        //            File.Delete(fullPath);
        //    }
        //    catch (Exception ex)
        //    {
        //        //_logger.WriteLog(LogLevel.Warning,
        //        //        $"{ex},   Failed to delete file: {relativePath}");
        //    }
        //}
    }
}
