using System;
using AutoMapper;
using BiddingService.DTOs;

namespace BiddingService.Models;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<Bid, BidDto>();
    }
}
