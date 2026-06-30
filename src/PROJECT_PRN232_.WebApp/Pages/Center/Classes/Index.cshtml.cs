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

        [BindProperty]
        public int CreateRoomId { get; set; }

        [BindProperty]
        public int UpdateRoomId { get; set; }

        public async Task OnGetAsync()
        {
            Classes = await _classService.GetAllClassesAsync();
            Rooms = await _context.Rooms.OrderBy(r => r.RoomName).ToListAsync();
            Slots = await _context.Slots.OrderBy(s => s.StartTime).ToListAsync();
        }

        /// <summary>
        /// AJAX endpoint: trả về thông tin lớp + lịch học UNIQUE (deduplicated by DayOfWeek + SlotId)
        /// để hiển thị vào Edit Modal — chỉ hiện các mẫu lịch, không hiện toàn bộ 24 buổi
        /// </summary>
        public async Task<JsonResult> OnGetClassLessonsAsync(int classId)
        {
            var classObj = await _context.Classes.FindAsync(classId);
            if (classObj == null)
            {
                return new JsonResult(new { error = "Không tìm thấy lớp học" });
            }

            // Lấy tất cả lessons của lớp, sắp xếp theo ngày
            var allLessons = await _context.Lessons
                .Where(l => l.ClassId == classId)
                .OrderBy(l => l.LessonDate)
                .Select(l => new
                {
                    l.Id,
                    l.RoomId,
                    l.SlotId,
                    LessonDate = l.LessonDate,
                    DayOfWeek = (int)l.LessonDate.DayOfWeek
                })
                .ToListAsync();

            // Lấy roomId đầu tiên từ lessons
            var firstRoomId = allLessons.FirstOrDefault(l => l.RoomId.HasValue)?.RoomId;

            // Deduplicate: chỉ giữ các mẫu lịch UNIQUE (DayOfWeek + SlotId)
            // để hiển thị vào phần "Lịch học" trong Edit Modal
            var uniqueSchedules = allLessons
                .GroupBy(l => new { l.DayOfWeek, l.SlotId })
                .Select(g => g.First())
                .Select(l => new
                {
                    l.RoomId,
                    l.SlotId,
                    l.DayOfWeek
                })
                .ToList();

            return new JsonResult(new
            {
                classId = classId,
                className = classObj.ClassName,
                maxCapacity = classObj.MaxCapacity,
                totalLessons = classObj.TotalLessons,  // lấy từ Class, không đếm Lessons
                roomId = firstRoomId,
                subject = classObj.Subject,
                // Trả về lịch UNIQUE để hiển thị vào form
                lessons = uniqueSchedules
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

            var createdClass = await _classService.CreateClassAsync(CreateDto);

            // Parse Lịch học gửi từ Form
            var days = Request.Form["CreateDayOfWeek[]"];
            var slots = Request.Form["CreateSlotId[]"];

            if (createdClass != null && days.Count > 0 && slots.Count > 0)
            {
                // Tự động sinh danh sách buổi học (Lessons) dựa trên Tổng số buổi học
                int totalLessonsNeeded = CreateDto.TotalLessons > 0 ? CreateDto.TotalLessons : 24;
                var startDate = System.DateTime.Today;
                var lessonsCreated = 0;
                var currentCheckingDate = startDate;

                // Chuẩn hóa danh sách thứ trong tuần
                var targetDaysMap = new Dictionary<string, DayOfWeek>
                {
                    { "Thứ 2", DayOfWeek.Monday },
                    { "Thứ 3", DayOfWeek.Tuesday },
                    { "Thứ 4", DayOfWeek.Wednesday },
                    { "Thứ 5", DayOfWeek.Thursday },
                    { "Thứ 6", DayOfWeek.Friday },
                    { "Thứ 7", DayOfWeek.Saturday },
                    { "Chủ nhật", DayOfWeek.Sunday }
                };

                var scheduleList = new List<(DayOfWeek day, int slotId)>();
                for (int i = 0; i < days.Count; i++)
                {
                    if (targetDaysMap.TryGetValue(days[i]!, out DayOfWeek targetDay)
                        && int.TryParse(slots[i], out int slotId))
                    {
                        scheduleList.Add((targetDay, slotId));
                    }
                }

                if (scheduleList.Count > 0)
                {
                    while (lessonsCreated < totalLessonsNeeded)
                    {
                        foreach (var schedule in scheduleList)
                        {
                            if (lessonsCreated >= totalLessonsNeeded) break;
                            if (schedule.day == currentCheckingDate.DayOfWeek)
                            {
                                var lesson = new Lesson
                                {
                                    ClassId = createdClass.Id,
                                    Title = $"Buổi {lessonsCreated + 1}",
                                    Description = $"Bài học buổi thứ {lessonsCreated + 1} của lớp {createdClass.ClassName}",
                                    LessonDate = currentCheckingDate,
                                    RoomId = CreateRoomId > 0 ? CreateRoomId : (int?)null,
                                    SlotId = schedule.slotId,
                                    IsPublished = true
                                };
                                _context.Lessons.Add(lesson);
                                lessonsCreated++;
                            }
                        }
                        currentCheckingDate = currentCheckingDate.AddDays(1);

                        // Safety guard: không chạy vô hạn
                        if (currentCheckingDate > startDate.AddYears(5)) break;
                    }
                    await _context.SaveChangesAsync();
                }
            }

            TempData["SuccessMessage"] = "Tạo lớp học và xếp lịch học thành công!";
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

            // Xóa các buổi học cũ để cập nhật lịch học mới
            var existingLessons = _context.Lessons.Where(l => l.ClassId == UpdateDto.Id);
            _context.Lessons.RemoveRange(existingLessons);
            await _context.SaveChangesAsync();

            // Lấy lịch học mới gửi lên từ Form sửa
            var days = Request.Form["UpdateDayOfWeek[]"];
            var slots = Request.Form["UpdateSlotId[]"];

            if (days.Count > 0 && slots.Count > 0)
            {
                int totalLessonsNeeded = UpdateDto.TotalLessons > 0 ? UpdateDto.TotalLessons : 24;
                var startDate = System.DateTime.Today;
                var lessonsCreated = 0;
                var currentCheckingDate = startDate;

                var targetDaysMap = new Dictionary<string, DayOfWeek>
                {
                    { "Thứ 2", DayOfWeek.Monday },
                    { "Thứ 3", DayOfWeek.Tuesday },
                    { "Thứ 4", DayOfWeek.Wednesday },
                    { "Thứ 5", DayOfWeek.Thursday },
                    { "Thứ 6", DayOfWeek.Friday },
                    { "Thứ 7", DayOfWeek.Saturday },
                    { "Chủ nhật", DayOfWeek.Sunday }
                };

                var scheduleList = new List<(DayOfWeek day, int slotId)>();
                for (int i = 0; i < days.Count; i++)
                {
                    if (targetDaysMap.TryGetValue(days[i]!, out DayOfWeek targetDay)
                        && int.TryParse(slots[i], out int slotId))
                    {
                        scheduleList.Add((targetDay, slotId));
                    }
                }

                if (scheduleList.Count > 0)
                {
                    while (lessonsCreated < totalLessonsNeeded)
                    {
                        foreach (var schedule in scheduleList)
                        {
                            if (lessonsCreated >= totalLessonsNeeded) break;
                            if (schedule.day == currentCheckingDate.DayOfWeek)
                            {
                                var lesson = new Lesson
                                {
                                    ClassId = UpdateDto.Id,
                                    Title = $"Buổi {lessonsCreated + 1}",
                                    Description = $"Bài học buổi thứ {lessonsCreated + 1} của lớp {UpdateDto.ClassName}",
                                    LessonDate = currentCheckingDate,
                                    RoomId = UpdateRoomId > 0 ? UpdateRoomId : (int?)null,
                                    SlotId = schedule.slotId,
                                    IsPublished = true
                                };
                                _context.Lessons.Add(lesson);
                                lessonsCreated++;
                            }
                        }
                        currentCheckingDate = currentCheckingDate.AddDays(1);

                        // Safety guard
                        if (currentCheckingDate > startDate.AddYears(5)) break;
                    }
                    await _context.SaveChangesAsync();
                }
            }

            TempData["SuccessMessage"] = "Cập nhật thông tin lớp học và lịch học thành công!";
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
