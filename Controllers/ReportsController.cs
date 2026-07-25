using Microsoft.AspNetCore.Mvc;
using blood.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace blood.Controllers
{
    public class ReportsController : Controller
    {
        private readonly BloodBankDbContext _context;
        public ReportsController(BloodBankDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> FilterByBloodGroup(string bloodGroup)
        {
            var query = _context.Donors.AsQueryable();
            if(!string.IsNullOrEmpty(bloodGroup)) {
                query = query.Where(d => d.BloodGroup == bloodGroup);
            }
            ViewBag.BloodGroup = bloodGroup;
            return View(await query.ToListAsync());
        }

        public async Task<IActionResult> SortedByDate()
        {
            var donors = await _context.Donors
                .OrderByDescending(d => d.LastDonationDate)
                .ToListAsync();
            return View(donors);
        }

        public async Task<IActionResult> DonorDonationsCount()
        {
            var data = await _context.Donors
                .Select(d => new DonorDonationCountViewModel {
                    FullName = d.FullName,
                    BloodGroup = d.BloodGroup,
                    DonationCount = d.Donations.Count()
                })
                .ToListAsync();
            return View(data);
        }

        public async Task<IActionResult> TotalBloodVolume()
        {
            var totalVolume = await _context.Donations.SumAsync(d => (int?)d.VolumeMl) ?? 0;
            return View(totalVolume);
        }
    }

    public class DonorDonationCountViewModel
    {
        public string? FullName { get; set; }
        public string? BloodGroup { get; set; }
        public int DonationCount { get; set; }
    }
}
