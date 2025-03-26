using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FootballAPI.Core.Entities;
using FootballAPI.DTOs;

namespace FootballAPI.Mapping
{
       public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Card Statistics mapping for Player
            CreateMap<Player, CardStatisticsDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.CardCount, opt => opt.Ignore()) // Set manually in controller
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => "Player"));
                
            // Card Statistics mapping for Manager
            CreateMap<Manager, CardStatisticsDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.CardCount, opt => opt.Ignore()) // Set manually in controller
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => "Manager"));
                
            // Minutes Played Statistics mapping for Player
            CreateMap<Player, MinutesPlayedStatisticsDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.MinutesPlayed, opt => opt.MapFrom(src => src.MinutesPlayed));
        }
    }
}