using Microsoft.AspNetCore.Mvc;
using blood.Models;
using Microsoft.EntityFrameworkCore;

namespace blood.Controllers
{
    public class DonorsController : Controller
    {
        private readonly BloodBankDbContext _context;
        public DonorsController(BloodBankDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Donors.Include(d => d.Donations).ToListAsync());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Donor donor, int? VolumeMl, string? CampName, DateOnly? DonationDate)
        {
            if (string.IsNullOrWhiteSpace(donor.FullName)) ModelState.AddModelError("FullName", "FullName is required.");
            if (string.IsNullOrWhiteSpace(donor.BloodGroup)) ModelState.AddModelError("BloodGroup", "BloodGroup is required.");

            if (ModelState.IsValid)
            {
                if (VolumeMl.HasValue || !string.IsNullOrWhiteSpace(CampName))
                {
                    var donation = new Donation
                    {
                        VolumeMl = VolumeMl ?? 0,
                        CampName = CampName ?? "Unknown",
                        DonationDate = DonationDate ?? DateOnly.FromDateTime(DateTime.Today)
                    };
                    donor.Donations.Add(donation);
                    
                    if (donor.LastDonationDate == null || donation.DonationDate > donor.LastDonationDate)
                    {
                        donor.LastDonationDate = donation.DonationDate;
                    }
                }

                _context.Add(donor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(donor);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var donor = await _context.Donors.Include(d => d.Donations).FirstOrDefaultAsync(m => m.DonorId == id);
            if (donor == null) return NotFound();
            return View(donor);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, Donor donor, int? VolumeMl, string? CampName, DateOnly? DonationDate)
        {
            if (id != donor.DonorId) return NotFound();
            if (string.IsNullOrWhiteSpace(donor.FullName)) ModelState.AddModelError("FullName", "FullName is required.");
            if (string.IsNullOrWhiteSpace(donor.BloodGroup)) ModelState.AddModelError("BloodGroup", "BloodGroup is required.");

            if (ModelState.IsValid)
            {
                var existingDonor = await _context.Donors.Include(d => d.Donations).FirstOrDefaultAsync(d => d.DonorId == id);
                if (existingDonor != null)
                {
                    existingDonor.FullName = donor.FullName;
                    existingDonor.BloodGroup = donor.BloodGroup;
                    existingDonor.ContactNo = donor.ContactNo;
                    existingDonor.City = donor.City;
                    existingDonor.LastDonationDate = donor.LastDonationDate;

                    if (VolumeMl.HasValue && VolumeMl > 0)
                    {
                        var donation = new Donation
                        {
                            VolumeMl = VolumeMl.Value,
                            CampName = CampName ?? "Unknown",
                            DonationDate = DonationDate ?? DateOnly.FromDateTime(DateTime.Today)
                        };
                        existingDonor.Donations.Add(donation);
                        
                        if (existingDonor.LastDonationDate == null || donation.DonationDate > existingDonor.LastDonationDate)
                        {
                            existingDonor.LastDonationDate = donation.DonationDate;
                        }
                    }

                    _context.Update(existingDonor);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            var reloadedDonor = await _context.Donors.Include(d => d.Donations).FirstOrDefaultAsync(d => d.DonorId == id);
            return View(reloadedDonor ?? donor);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var donor = await _context.Donors.FirstOrDefaultAsync(m => m.DonorId == id);
            if (donor == null) return NotFound();
            return View(donor);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var donor = await _context.Donors.FindAsync(id);
            if (donor != null)
            {
                _context.Donors.Remove(donor);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
