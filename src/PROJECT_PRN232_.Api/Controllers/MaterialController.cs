using System;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PROJECT_PRN232_.Application.DTOs;
using PROJECT_PRN232_.Application.Services;

namespace PROJECT_PRN232_.Controllers
{
    [ApiController]
    [Authorize]
    public class MaterialController : ControllerBase
    {
        private readonly IMaterialService _materialService;
        private readonly IWebHostEnvironment _env;

        public MaterialController(IMaterialService materialService, IWebHostEnvironment env)
        {
            _materialService = materialService;
            _env = env;
        }

        /// <summary>
        /// Lấy danh sách học liệu của 1 buổi học
        /// GET /api/center/lessons/{lessonId}/materials
        /// </summary>
        [HttpGet("api/center/lessons/{lessonId}/materials")]
        public async Task<IActionResult> GetByLesson(int lessonId)
        {
            var list = await _materialService.GetByLessonIdAsync(lessonId);
            return Ok(list);
        }

        /// <summary>
        /// Upload tài liệu mới vào buổi học (lưu thông tin)
        /// POST /api/center/lessons/{lessonId}/materials
        /// </summary>
        [HttpPost("api/center/lessons/{lessonId}/materials")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Upload(int lessonId, [FromBody] MaterialCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var teacherUserId = GetUserId();
            var result = await _materialService.CreateAsync(lessonId, dto, teacherUserId);

            if (result == null)
            {
                return BadRequest(new { message = "Không thể tải lên tài liệu. Vui lòng kiểm tra quyền sở hữu buổi học." });
            }

            return Created($"/api/center/lessons/{lessonId}/materials", result);
        }

        /// <summary>
        /// API UPLOAD FILE VẬT LÝ LÊN SERVER
        /// POST /api/center/materials/upload-file
        /// Hỗ trợ upload tài liệu (PDF, Word, Excel), ảnh (JPG, PNG) giới hạn tối đa 20MB.
        /// </summary>
        [HttpPost("api/center/materials/upload-file")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> UploadPhysicalFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "Không có file nào được chọn để tải lên." });

            // Giới hạn dung lượng: tối đa 20MB cho tài liệu/ảnh
            const long maxFileSize = 20 * 1024 * 1024; 
            if (file.Length > maxFileSize)
                return BadRequest(new { message = "Dung lượng file vượt quá giới hạn cho phép (Tối đa 20MB)." });

            // Chỉ cho phép upload các định dạng tài liệu và ảnh phổ biến
            var extension = Path.GetExtension(file.FileName).ToLower();
            var allowedExtensions = new[] { ".pdf", ".docx", ".doc", ".xlsx", ".xls", ".png", ".jpg", ".jpeg", ".txt", ".pptx", ".ppt" };
            
            // Nếu cố tình tải lên file video nặng (.mp4, .mkv), hệ thống sẽ chặn và hướng dẫn dùng link ngoài
            if (extension == ".mp4" || extension == ".avi" || extension == ".mkv" || extension == ".mov")
            {
                return BadRequest(new { message = "Video quá nặng! Vui lòng không upload trực tiếp. Hãy tải video lên Drive/Youtube rồi dán liên kết vào ô URL." });
            }

            if (Array.IndexOf(allowedExtensions, extension) < 0)
                return BadRequest(new { message = "Định dạng file không được hỗ trợ. Chỉ chấp nhận các file tài liệu và hình ảnh." });

            try
            {
                // Tạo thư mục wwwroot/uploads/materials nếu chưa tồn tại
                var uploadDir = Path.Combine(_env.WebRootPath, "uploads", "materials");
                if (!Directory.Exists(uploadDir))
                {
                    Directory.CreateDirectory(uploadDir);
                }

                // Tạo tên file ngẫu nhiên để tránh ghi đè
                var uniqueFileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadDir, uniqueFileName);

                // Lưu file vào thư mục vật lý
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Trả về URL tương đối để client truy cập
                var fileUrl = $"/uploads/materials/{uniqueFileName}";
                return Ok(new { url = fileUrl, originalName = file.FileName });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Lỗi hệ thống khi lưu file: {ex.Message}" });
            }
        }

        /// <summary>
        /// Xóa tài liệu học tập
        /// DELETE /api/center/materials/{materialId}
        /// </summary>
        [HttpDelete("api/center/materials/{materialId}")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Delete(int materialId)
        {
            var teacherUserId = GetUserId();
            var success = await _materialService.DeleteAsync(materialId, teacherUserId);

            if (!success)
            {
                return BadRequest(new { message = "Không thể xóa tài liệu. Tài liệu không tồn tại hoặc bạn không có quyền." });
            }

            return NoContent();
        }

        // Helper lấy User ID từ Claims
        private int GetUserId()
        {
            var idString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(idString, out int userId) ? userId : 0;
        }
    }
}
