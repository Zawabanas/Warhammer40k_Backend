using AutoMapper;
using WarHammer40K.Entities;
using WarHammer40K.DTOs;

namespace WarHammer40K.Profile
{
    public class MappingProfile : AutoMapper.Profile
    {
        public MappingProfile()
        {
            CreateMap<Faction, FactionDTO>().ReverseMap();
            CreateMap<Character, CharacterDTO>().ReverseMap();
            CreateMap<Unit, UnitDTO>().ReverseMap();
        }
    }
}
