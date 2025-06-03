using TransportExpenditureTracker.ViewModels;

namespace TransportExpenditureTracker.Services.Interfaces
{
    public interface IPartyService
    { 
      Task<List<PartyViewModel>> GetAllPartiesAsync();
      Task<PartyViewModel> GetPartyByIdAsync(int id);
      Task AddPartyAsync(PartyViewModel partyViewModel);
      Task UpdatePartyAsync(PartyViewModel partyViewModel);
      Task DeletePartyAsync(int id);
      Task<List<PartyViewModel>> SearchPartiesAsync(string searchTerm);
      Task<bool> PartyExistsAsync(int id);
    }
}
