using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using EdenRequest.Api.Data;
using EdenRequest.Api.DTO;
using EdenRequest.Api.DTOs;
using EdenRequest.Api.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EdenRequest.Api.Services
{
    public interface IExtraDirtyReportService
    {
        Task<int> CreateReportMetadataAsync(CreateReportMetadataDto dto);
        Task AttachMediaToReportAsync(int reportId, List<IFormFile> files);
        Task<ExtraDirtyReportDto?> GetReportByIdAsync(int id);
        Task<PagedResponse<ExtraDirtyReportDto>> GetAllReportsAsync(AllExtraDirtyQueryDto filters);
    }

    public class ExtraDirtyReportService : IExtraDirtyReportService
    {
        private readonly IExtraDirtyReportRepository _repository;
        private readonly AppDbContext _context; 
        private readonly Cloudinary _cloudinary;

        public ExtraDirtyReportService(
            IExtraDirtyReportRepository repository,
            AppDbContext context, 
            IConfiguration configuration)
        {
            _repository = repository;
            _context = context;

            var account = new Account(
                configuration["Cloudinary:CloudName"],
                configuration["Cloudinary:ApiKey"],
                configuration["Cloudinary:ApiSecret"]
            );
            _cloudinary = new Cloudinary(account);
        }

        public async Task<int> CreateReportMetadataAsync(CreateReportMetadataDto dto)
        {
            var validEmployeeId = dto.ReportedById;
            var employeeExists = await _context.Employees.AnyAsync(e => e.Id == validEmployeeId);

            if (!employeeExists)
            {
                validEmployeeId = 1; 
            }

            var report = new ExtraDirtyReport
            {
                RoomNumber = dto.RoomNumber,
                ReportedById = validEmployeeId,
                Notes = dto.Notes,
                CreatedAt = DateTime.UtcNow,
                MediaFiles = new List<MediaFile>()
            };

            var createdReport = await _repository.AddReportAsync(report);
            return createdReport.Id;
        }

        public async Task AttachMediaToReportAsync(int reportId, List<IFormFile> files)
        {
            var report = await _repository.GetByIdAsync(reportId);
            if (report == null)
            {
                throw new KeyNotFoundException($"Extra dirty report with ID {reportId} was not found.");
            }

            var mediaFiles = new List<MediaFile>();

            foreach (var file in files)
            {
                if (file.Length == 0) continue;

                using var stream = file.OpenReadStream();

                bool isVideo = file.ContentType.StartsWith("video/") ||
                               file.FileName.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase) ||
                               file.FileName.EndsWith(".mov", StringComparison.OrdinalIgnoreCase);

                RawUploadResult uploadResult;

                if (isVideo)
                {
                    var uploadParams = new VideoUploadParams
                    {
                        File = new FileDescription(file.FileName, stream),
                        Folder = "extra-dirty-reports/videos"
                    };
                    uploadResult = await _cloudinary.UploadAsync(uploadParams);
                }
                else
                {
                    var uploadParams = new ImageUploadParams
                    {
                        File = new FileDescription(file.FileName, stream),
                        Folder = "extra-dirty-reports/images"
                    };
                    uploadResult = await _cloudinary.UploadAsync(uploadParams);
                }

                if (uploadResult.Error != null)
                {
                    throw new Exception($"Cloudinary upload failed: {uploadResult.Error.Message}");
                }

                mediaFiles.Add(new MediaFile
                {
                    ExtraDirtyReportId = reportId,
                    Url = uploadResult.SecureUrl.ToString(),
                    PublicId = uploadResult.PublicId,
                    MediaType = isVideo ? "video" : "image"
                });
            }

            if (mediaFiles.Count > 0)
            {
                await _repository.AddMediaFilesAsync(mediaFiles);
            }
        }


        public async Task<ExtraDirtyReportDto?> GetReportByIdAsync(int id)
        {
            var report = await _repository.GetByIdAsync(id);

            if (report == null) return null;

            return new ExtraDirtyReportDto
            {
                Id = report.Id,
                RoomNumber = report.RoomNumber,
                ReportedById = report.ReportedById,
                ReportedByEmployee = report.ReportedBy != null ? report.ReportedBy.Name : "Unknown",
                Notes = report.Notes,
                CreatedAt = report.CreatedAt,
                MediaFiles = report.MediaFiles.Select(m => new MediaFileDto
                {
                    Id = m.Id,
                    Url = m.Url,
                    PublicId = m.PublicId,
                    MediaType = m.MediaType
                }).ToList()
            };
        }

        public async Task<PagedResponse<ExtraDirtyReportDto>> GetAllReportsAsync(AllExtraDirtyQueryDto filters)
        {
            var pagedResult = await _repository.GetPagedReportsAsync(filters);

            var mappedData = pagedResult.Data.Select(report => new ExtraDirtyReportDto
            {
                Id = report.Id,
                RoomNumber = report.RoomNumber,
                ReportedById = report.ReportedById,
                ReportedByEmployee = report.ReportedBy != null ? report.ReportedBy.Name : "Unknown",
                Notes = report.Notes,
                CreatedAt = report.CreatedAt,
                MediaFiles = report.MediaFiles.Select(m => new MediaFileDto
                {
                    Id = m.Id,
                    Url = m.Url,
                    PublicId = m.PublicId,
                    MediaType = m.MediaType
                }).ToList()
            }).ToList();

            return new PagedResponse<ExtraDirtyReportDto>
            {
                Data = mappedData,
                TotalCount = pagedResult.TotalCount
            };
        }
    }
}
