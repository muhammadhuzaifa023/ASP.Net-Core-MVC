using ASP.Net_Core_MVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Drawing;
using System.Net;
using System.Runtime.Intrinsics.X86;

namespace ASP.Net_Core_MVC.Controllers
{
    public class AccountController : Controller

    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        //private readonly RoleManager<ApplicationUser> _roleManager;
        public AccountController(UserManager<ApplicationUser> userManager,
                                      SignInManager<ApplicationUser> signInManager
                                      /*RoleManager<ApplicationUser> roleManager*/)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            //_roleManager = roleManager;
        }
        //public IActionResult Index()
        //{
        //    return View();
        //}

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Copy data from RegisterViewModel to ApplicationUser
                var user = new ApplicationUser  // Here IdentityUser Generic Class 
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                // If user is successfully created, sign-in the user using
                // SignInManager and redirect to index action of HomeController
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    return RedirectToAction("index", "Home");
                }
                // If there are any errors, add them to the ModelState object
                // which will be displayed by the validation summary tag helper
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                ModelState.AddModelError(string.Empty, "Invalid");

            }
            return View(model);
        }
        [HttpGet]
        [AllowAnonymous]  //The [AllowAnonymous] attribute is used in ASP.NET Core to specify that an action or controller should be accessible without requiring authentication.
                          //Here are some scenarios where you might use [AllowAnonymous]
        public IActionResult Login(string? ReturnUrl = null)

        {
            ViewData["ReturnUrl"] = ReturnUrl;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string? ReturnUrl)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    //// Handle successful login
                    //return RedirectToAction(nameof(HomeController.Index), "Home");
                    // Handle successful login
                    // Check if the ReturnUrl is not null and is a local URL
                    if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
                    {
                        return Redirect(ReturnUrl);
                    }
                    else
                    {
                        // Redirect to default page
                        return RedirectToAction("Index", "Home");
                    }
                }
                if (result.RequiresTwoFactor)
                {
                    // Handle two-factor authentication case
                }
                if (result.IsLockedOut)
                {
                    // Handle lockout scenario
                }
                else
                {
                    // Handle failure
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return View(model);
                }
            }
            // If we got this far, something failed, redisplay form
            return View(model);
        }

        // For Remote Validation 
        //The Remote Validation in ASP.NET Core is a feature that allows us to perform server-side validation asynchronously from the client side. Remote Validation allows a controller action method to be called using a client-side script. This is useful when calling a server-side method without a full-page post back.

        //So, this is useful when we need to validate input filed against server-side data or logic that cannot be replicated or performed on the client side.For instance, checking if a username or email address is already in use typically requires a database lookup, which can only be done on the server.




                [AllowAnonymous]
        [HttpPost]
        [HttpGet]
        public async Task<IActionResult> IsEmailAvailable(string Email)
        {
            //Check If the Email Id is Already in the Database
            var user = await _userManager.FindByEmailAsync(Email);
            if (user == null)
            {
                return Json(true);
            }
            else
            {
                return Json($"Email {Email} is already in use.");
            }
        }
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }
    }

      
}
