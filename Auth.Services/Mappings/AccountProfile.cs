using Auth.Application.DTOs;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Auth.Domain.Entities;
//using Auth.Infrastructure.Models;
namespace Auth.Application.Mappings
{
    public class AccountProfile : Profile
    {
        public AccountProfile()
        {
            CreateMap<Account, AccountDto>()
                .ForCtorParam("Id", opt => opt.MapFrom(src => src.Id))
                .ForCtorParam("Email", opt => opt.MapFrom(src => src.Email))
                .ForCtorParam("Name", opt => opt.MapFrom(src => src.Name))
                .ForCtorParam("Role", opt => opt.MapFrom(src => src.Role))
                .ForCtorParam("Status", opt => opt.MapFrom(src => src.Status))
                .ForCtorParam("Phone", opt => opt.MapFrom(src => src.Phone))
                .ForCtorParam("Address", opt => opt.MapFrom(src => src.Address))    
                .ForCtorParam("CreatedAt", opt => opt.MapFrom(src => src.CreatedAt));
        }
    }
}
