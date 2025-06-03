using Microsoft.AspNetCore.Mvc;
using TransportExpenditureTracker.Services.Interfaces;
using TransportExpenditureTracker.ViewModels;

namespace TransportExpenditureTracker.Controllers
{
    public class PartiesController : Controller
    {
        private readonly IPartyService _partyService;

        public PartiesController(IPartyService partyService)
        {
            _partyService = partyService;
        }

        // GET: Parties
        public async Task<IActionResult> Index(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                var allParties = await _partyService.GetAllPartiesAsync();
                return View(allParties);
            }
            else
            {
                var filteredParties = await _partyService.SearchPartiesAsync(searchTerm);
                return View(filteredParties);
            }
        }

        // GET: Parties/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var party = await _partyService.GetPartyByIdAsync(id.Value);
            if (party == null)
                return NotFound();

            return View(party);
        }

        public IActionResult GetAddPartyPartial()
        {
            return PartialView("_AddPartyPartial", new PartyViewModel());
        }

        // GET: Parties/Create
        public IActionResult Create()
        {
            return View(new PartyViewModel());
        }

        // POST: Parties/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PartyViewModel partyViewModel)
        {
            if (!ModelState.IsValid)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    return Json(new { success = false, errors });
                }
                return View(partyViewModel);
            }

            await _partyService.AddPartyAsync(partyViewModel);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, message = "Party created successfully." });
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Parties/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var party = await _partyService.GetPartyByIdAsync(id.Value);
            if (party == null)
                return NotFound();

            return View(party);
        }

        // POST: Parties/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PartyViewModel partyViewModel)
        {
            if (id != partyViewModel.PartyId)
                return NotFound();

            if (!ModelState.IsValid)
                return View(partyViewModel);

            await _partyService.UpdatePartyAsync(partyViewModel);
            return RedirectToAction(nameof(Index));
        }

        // GET: Parties/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var party = await _partyService.GetPartyByIdAsync(id.Value);
            if (party == null)
                return NotFound();

            return View(party);
        }

        // POST: Parties/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _partyService.DeletePartyAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // Optional: Search endpoint
        public async Task<IActionResult> Search(string searchTerm)
        {
            var results = await _partyService.SearchPartiesAsync(searchTerm);
            return View("Index", results);
        }
    }
}
