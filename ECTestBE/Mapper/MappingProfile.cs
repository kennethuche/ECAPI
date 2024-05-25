using AutoMapper;
using ECTest.Service.Models;
using ECTestBE.ViewModel;

namespace ECTestBE.Mapper
{
    public class MappingProfile : Profile
    {

        public MappingProfile()
        {

            CreateMap<StudentVm, Student>()
            .ForMember(dest => dest.Courses, opt => opt.MapFrom(src => src.Courses));

            CreateMap<CourseVm, Course>();

        }
    }
}

