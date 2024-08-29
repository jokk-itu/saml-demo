using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace samlwebapp.Pages
{
    [Authorize(AuthenticationSchemes = "saml2")]
    public class ClaimsModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
