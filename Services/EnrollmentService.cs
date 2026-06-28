using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PROJECT_PRN232_.Data.Entities;
using PROJECT_PRN232_.Repositories;

namespace PROJECT_PRN232_.Services
{
    public class EnrollmentService : IEnrollmentService
    {
        private readonly IClassStudentRepository _classStudentRepository;
        private readonly IClassRepository _classRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly INotificationService _notificationService;

        public EnrollmentService(
            IClassStudentRepository classStudentRepository,
            IClassRepository classRepository,
            IStudentRepository studentRepository,
            INotificationService notificationService)
        {
            _classStudentRepository = classStudentRepository;
            _classRepository = classRepository;
            _studentRepository = studentRepository;
            _notificationService = notificationService;
        }

        public async Task<IEnumerable<Student>> GetStudentsInClassAsync(int classId)
        {
            var enrollments = await _classStudentRepository.GetStudentsByClassIdAsync(classId);
            var students = new List<Student>();
            foreach (var e in enrollments)
            {
                if (e.Student != null)
                {
                    students.Add(e.Student);
                }
            }
            return students;
        }

        public async Task<bool> EnrollStudentAsync(int classId, int studentId)
        {
            // 1. Check if class exists
            var classEntity = await _classRepository.GetClassByIdAsync(classId);
            if (classEntity == null)
            {
                throw new ArgumentException($"Lớp học ID {classId} không tồn tại.");
            }

            // 2. Check if class is Closed
            if (string.Equals(classEntity.Status, "Closed", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Không thể thêm học viên vào lớp học đã đóng.");
            }

            // 3. Check if student exists
            var student = await _studentRepository.GetByIdAsync(studentId);
            if (student == null)
            {
                throw new ArgumentException($"Học sinh ID {studentId} không tồn tại.");
            }

            // 4. Check for duplicate enrollment
            var existing = await _classStudentRepository.GetEnrollmentAsync(classId, studentId);
            if (existing != null)
            {
                throw new InvalidOperationException("Học sinh này đã đăng ký học ở lớp này rồi.");
            }

            // 5. Create enrollment
            var enrollment = new ClassStudent
            {
                ClassId = classId,
                StudentId = studentId,
                EnrolledAt = DateTime.Now
            };

            await _classStudentRepository.AddAsync(enrollment);

            // Thông báo real-time cho phụ huynh: con được nhập học vào lớp
            await _notificationService.NotifyStudentEnrolledAsync(
                student.ParentId,
                student.FullName,
                classEntity.Id,
                classEntity.ClassName);

            return true;
        }

        public async Task<bool> RemoveStudentFromClassAsync(int classId, int studentId)
        {
            var student = await _studentRepository.GetByIdAsync(studentId);
            var classEntity = await _classRepository.GetClassByIdAsync(classId);

            var success = await _classStudentRepository.RemoveAsync(classId, studentId);

            if (success && student != null && classEntity != null)
            {
                await _notificationService.NotifyStudentRemovedAsync(
                    student.ParentId,
                    student.FullName,
                    classEntity.Id,
                    classEntity.ClassName);
            }

            return success;
        }

        public async Task<bool> TransferStudentClassAsync(int studentId, int fromClassId, int toClassId)
        {
            // 1. Verify target class is active
            var targetClass = await _classRepository.GetClassByIdAsync(toClassId);
            if (targetClass == null)
            {
                throw new ArgumentException($"Lớp học đích ID {toClassId} không tồn tại.");
            }

            if (string.Equals(targetClass.Status, "Closed", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Không thể chuyển học sinh vào lớp học đã đóng.");
            }

            // 1.5. Verify source class and student exist
            var sourceClass = await _classRepository.GetClassByIdAsync(fromClassId);
            if (sourceClass == null)
            {
                throw new ArgumentException($"Lớp học nguồn ID {fromClassId} không tồn tại.");
            }

            var student = await _studentRepository.GetByIdAsync(studentId);
            if (student == null)
            {
                throw new ArgumentException($"Học sinh ID {studentId} không tồn tại.");
            }

            // 2. Verify target class is not duplicate
            var targetEnrollment = await _classStudentRepository.GetEnrollmentAsync(toClassId, studentId);
            if (targetEnrollment != null)
            {
                throw new InvalidOperationException("Học sinh đã có tên trong lớp học đích.");
            }

            // 3. Verify current enrollment exists
            var currentEnrollment = await _classStudentRepository.GetEnrollmentAsync(fromClassId, studentId);
            if (currentEnrollment == null)
            {
                throw new InvalidOperationException("Học sinh không có trong lớp học nguồn.");
            }

            // 4. Remove current and add to new
            await _classStudentRepository.RemoveAsync(fromClassId, studentId);
            var newEnrollment = new ClassStudent
            {
                ClassId = toClassId,
                StudentId = studentId,
                EnrolledAt = DateTime.Now
            };
            await _classStudentRepository.AddAsync(newEnrollment);

            // Gửi thông báo chuyển lớp thành công cho Phụ huynh
            await _notificationService.NotifyStudentTransferredAsync(
                student.ParentId,
                student.FullName,
                sourceClass.Id,
                sourceClass.ClassName,
                targetClass.Id,
                targetClass.ClassName);

            return true;
        }

        public async Task<IEnumerable<Class>> GetClassesForStudentAsync(int studentId)
        {
            var enrollments = await _classStudentRepository.GetEnrollmentsByStudentIdAsync(studentId);
            var classes = new List<Class>();
            foreach (var e in enrollments)
            {
                if (e.Class != null)
                {
                    classes.Add(e.Class);
                }
            }
            return classes;
        }
    }
}
