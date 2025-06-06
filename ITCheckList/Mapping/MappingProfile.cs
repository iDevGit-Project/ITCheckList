using AutoMapper;
using ITCheckList.Models.VModels;
using ITCheckList.Models;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<TBL_CheckItem, CheckItemViewModels>();
    }
}

