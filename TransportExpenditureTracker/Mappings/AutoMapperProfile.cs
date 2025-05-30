using AutoMapper;
using TransportExpenditureTracker.Models;
using TransportExpenditureTracker.ViewModels;

namespace TransportExpenditureTracker.Mappings
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Party, PartyViewModel>().ReverseMap();

            CreateMap<Invoice, InvoiceViewModel>().ReverseMap();

            CreateMap<Item, ItemViewModel>().ReverseMap();
        }
    }
}
