using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using EdenRequest.Api.Data;
using EdenRequest.Api.DTO;
using EdenRequest.Api.DTOs;
using EdenRequest.Api.Repositories;

namespace EdenRequest.Api.Services
{
    public interface IExtraDirtyReportService
    {
        Task<ExtraDirtyReport> CreateReportAsync(CreateExtraDirtyReportDto dto);
        Task<ExtraDirtyReportDto?> GetReportByIdAsync(int id);
        Task<PagedResponse<ExtraDirtyReportDto>> GetAllReportsAsync(AllExtraDirtyQueryDto filters);
    }
    public class ExtraDirtyReportService : IExtraDirtyReportService
    {
        private readonly IExtraDirtyReportRepository _repository;
        private readonly Cloudinary _cloudinary;
        public ExtraDirtyReportService(
            IExtraDirtyReportRepository repository,
            IConfiguration configuration)
        {
            _repository = repository;

            var account = new Account(
                configuration["Cloudinary:CloudName"],
                configuration["Cloudinary:ApiKey"],
                configuration["Cloudinary:ApiSecret"]
            );
            _cloudinary = new Cloudinary(account);
        }

        public async Task<ExtraDirtyReport> CreateReportAsync(CreateExtraDirtyReportDto dto)
        {
            var mediaFiles = new List<MediaFile>();

            foreach (var file in dto.Files)
            {
                if (file.Length == 0) continue;

                using var stream = file.OpenReadStream();
                bool isVideo = file.ContentType.StartsWith("video/");

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
                    Url = uploadResult.SecureUrl.ToString(),
                    PublicId = uploadResult.PublicId,
                    MediaType = isVideo ? "video" : "image"
                });
            }

            var report = new ExtraDirtyReport
            {
                RoomNumber = dto.RoomNumber,
                ReportedById = dto.ReportedById,
                Notes = dto.Notes,
                MediaFiles = mediaFiles
            };

            return await _repository.AddReportAsync(report);
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
