using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ForestGuard.Pages
{
    public class ContactModel : PageModel
    {
        public bool hasData = false;
        public string fname = "";
        public void OnGet()
        {
        }

        public void OnPost()
        {
            hasData = true;
            fname = Request.Form["firstname"];

        }
    }
}
