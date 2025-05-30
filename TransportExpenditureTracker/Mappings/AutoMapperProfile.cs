using AutoMapper;
using TransportExpenditureTracker.Models;
using TransportExpenditureTracker.ViewModels;

namespace TransportExpenditureTracker.Mappings
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // Party
            CreateMap<Party, PartyViewModel>().ReverseMap();

            // Invoice 
            CreateMap<Invoice, InvoiceViewModel>().ReverseMap();
        }
    }
}
