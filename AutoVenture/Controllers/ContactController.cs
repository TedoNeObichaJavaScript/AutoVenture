using Microsoft.AspNetCore.Mvc;
using AutoVenture.Models;

namespace AutoVenture.Controllers
{
    public class ContactController : Controller
    {
        public IActionResult Index()
        {
            return View(); // Returns the Contact/Index.cshtml view
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Submit(ContactViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Your form submission logic here (e.g., send email, save to DB)
                ViewBag.Message = "Thank you for contacting us!";

                return RedirectToAction("ThankYou"); // Redirect to a ThankYou page
            }

            // If there are validation errors, return to the contact form view
            return View("Index", model);
        }

        public IActionResult ThankYou()
        {
            return View(); // This is the Thank You page that the user will be redirected to after successful submission
        }
    }
}
