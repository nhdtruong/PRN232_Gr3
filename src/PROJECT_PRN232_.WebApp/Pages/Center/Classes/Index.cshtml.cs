using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PROJECT_PRN232_.Application.DTOs;
using PROJECT_PRN232_.Application.Services;
using PROJECT_PRN232_.Infrastructure.Data;
using PROJECT_PRN232_.Domain;

namespace PROJECT_PRN232_.WebApp.Pages.Center.Classes
{
    [Authorize(Roles = "Center")]
    public class IndexModel : PageModel
    {
        private readonly IClassService _classService;
        private readonly AppDbContext _context;

        public IndexModel(IClassService classService, AppDbContext context)
        {
            _classService = classService;
            _context = context;
        }

        public IEnumerable<ClassResponseDto> Classes { get; set; } = new List<ClassResponseDto>();
        public List<Room> Rooms { get; set; } = new();
        public List<Slot> Slots { get; set; } = new();

        [BindProperty]
        public ClassCreateDto CreateDto { get; set; } = new();

        [BindProperty]
        public ClassUpdateDto UpdateDto { get; set; } = new();

        // Room binding properties
        [BindProperty]
        public Room RoomInput { get; set; } = new();

        // Slot binding properties
        [BindProperty]
        public Slot SlotInput { get; set; } = new();

        public async Task OnGetAsync()
        {
            Classes = await _classService.GetAllClassesAsync();
            Rooms = await _context.Rooms.OrderBy(r => r.RoomName).ToListAsync();
            Slots = await _context.Slots.OrderBy(s => s.StartTime).ToListAsync();
        }

        public async Task<JsonResult> OnGetClassLessonsAsync(int classId)
        {
            var classObj = await _context.Classes.FindAsync(classId);
            var lessons = await _context.Lessons
                .Where(l => l.ClassId == classId)
                .OrderBy(l => l.LessonDate)
                .Select(l => new 
                {
                    l.Id,
                    l.RoomId,
                    l.SlotId,
                    LessonDate = l.LessonDate.ToString("yyyy-MM-dd"),
                    DayOfWeek = l.LessonDate.DayOfWeek
                })
                .ToListAsync();

            // Lấy thông tin phòng học đầu tiên của các buổi học hoặc lấy ngẫu nhiên một phòng
            var firstRoomId = lessons.FirstOrDefault(l => l.RoomId.HasValue)?.RoomId;

            return new JsonResult(new
            {
                classId = classId,
                className = classObj?.ClassName ?? "",
                maxCapacity = classObj?.MaxCapacity ?? 30,
                roomId = firstRoomId,
                totalLessons = lessons.Count, // Tổng số buổi thực tế từ bảng Lessons
                lessons = lessons
            });
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            ModelState.Clear();
            if (!TryValidateModel(CreateDto, nameof(CreateDto)))
            {
                await OnGetAsync();
                return Page();
            }

            await _classService.CreateClassAsync(CreateDto);
            TempData["SuccessMessage"] = "Tạo lớp học thành công!";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateAsync()
        {
            ModelState.Clear();
            if (!TryValidateModel(UpdateDto, nameof(UpdateDto)))
            {
                await OnGetAsync();
                return Page();
            }

            var success = await _classService.UpdateClassAsync(UpdateDto);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, "Không tìm thấy lớp học để sửa.");
                await OnGetAsync();
                return Page();
            }

            TempData["SuccessMessage"] = "Cập nhật thông tin lớp thành công!";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var success = await _classService.DeleteClassAsync(id);
            if (success)
            {
                TempData["SuccessMessage"] = "Xóa lớp học thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy lớp học để xóa.";
            }
            return RedirectToPage();
        }

        // Room Actions
        public async Task<IActionResult> OnPostCreateRoomAsync()
        {
            ModelState.Clear();
            if (string.IsNullOrWhiteSpace(RoomInput.RoomName))
            {
                TempData["ErrorMessage"] = "Tên phòng học không được để trống!";
                return RedirectToPage();
            }

            _context.Rooms.Add(RoomInput);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Thêm phòng học mới thành công!";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteRoomAsync(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room != null)
            {
                _context.Rooms.Remove(room);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa phòng học thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy phòng học để xóa!";
            }
            return RedirectToPage();
        }

        // Slot Actions
        public async Task<IActionResult> OnPostCreateSlotAsync()
        {
            ModelState.Clear();
            if (string.IsNullOrWhiteSpace(SlotInput.SlotName))
            {
                TempData["ErrorMessage"] = "Tên ca học không được để trống!";
                return RedirectToPage();
            }

            _context.Slots.Add(SlotInput);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Thêm ca học mới thành công!";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteSlotAsync(int id)
        {
            var slot = await _context.Slots.FindAsync(id);
            if (slot != null)
            {
                _context.Slots.Remove(slot);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa ca học thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy ca học để xóa!";
            }
            return RedirectToPage();
        }
    }
}
