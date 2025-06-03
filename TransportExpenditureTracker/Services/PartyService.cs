using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TransportExpenditureTracker.Data;
using TransportExpenditureTracker.Models;
using TransportExpenditureTracker.Services.Interfaces;
using TransportExpenditureTracker.ViewModels;

namespace TransportExpenditureTracker.Services
{
    public class PartyService : IPartyService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public PartyService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<PartyViewModel>> GetAllPartiesAsync()
        {
            var parties = await _context.Parties.ToListAsync();
            return _mapper.Map<List<PartyViewModel>>(parties);
        }

        public async Task<PartyViewModel> GetPartyByIdAsync(int id)
        {
            var party = await _context.Parties.FindAsync(id);
            if (party == null)
                return null;

            return _mapper.Map<PartyViewModel>(party);

        }

        public async Task AddPartyAsync(PartyViewModel partyViewModel)
        {
            var party = _mapper.Map<Party>(partyViewModel);
            _context.Parties.Add(party);
            await _context.SaveChangesAsync();
        }

        public async Task UpdatePartyAsync(PartyViewModel partyViewModel)
        {
            var party = await _context.Parties.FindAsync(partyViewModel.PartyId);
            if (party == null) throw new KeyNotFoundException("Party not found.");

            _mapper.Map(partyViewModel, party);
            _context.Parties.Update(party);
            await _context.SaveChangesAsync();
        }

        public async Task DeletePartyAsync(int id)
        {
            var party = await _context.Parties.FindAsync(id);
            if (party == null) throw new KeyNotFoundException("Party not found.");

            _context.Parties.Remove(party);
            await _context.SaveChangesAsync();
        }

        public async Task<List<PartyViewModel>> SearchPartiesAsync(string searchTerm)
        {
            var parties = await _context.Parties
                .Where(p => p.PartyName.Contains(searchTerm) ||
                            p.Location.Contains(searchTerm) ||
                            p.VatNo.Contains(searchTerm))
                .ToListAsync();

            return _mapper.Map<List<PartyViewModel>>(parties);
        }

        public async Task<bool> PartyExistsAsync(int id)
        {
            return await _context.Parties.AnyAsync(p => p.PartyId == id);
        }
    }
}
