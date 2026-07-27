using LeaveManagementSystem.Domain.Dtos.Employee;
using LeaveManagementSystem.Domain.Entities;

namespace LeaveManagementSystem.Domain.AutoMapperProfile
{
    public class EmployeeMappingProfile : Profile
    {
        public EmployeeMappingProfile()
        {
            CreateMap<Employee, EmployeeResponseDto>();

            CreateMap<CreateEmployeeRequestDto, Employee>();

            CreateMap<UpdateEmployeeRequestDto, Employee>();
        }
    }
}
