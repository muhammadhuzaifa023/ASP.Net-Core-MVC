using ASP.Net_Core_MVC.Infrastructure.Generic;
using ASP.Net_Core_MVC.Infrastructure.IGeneric;
using ASP.Net_Core_MVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Encodings.Web;

namespace ASP.Net_Core_MVC.Controllers
{
    public class AccountController : Controller

    {
        //userManager will hold the UserManager instance
        private readonly UserManager<ApplicationUser> _userManager;

        //signInManager will hold the SignInManager instance
        private readonly SignInManager<ApplicationUser> _signInManager;
        //ISenderEmail will hold the EmailSender instance
        private readonly ISenderEmail _emailSender;
        public AccountController(UserManager<ApplicationUser> userManager,
                                      SignInManager<ApplicationUser> signInManager, ISenderEmail emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
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
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Copy data from RegisterViewModel to ApplicationUser
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName
                };
                // Store user data in AspNetUsers database table
                var result = await _userManager.CreateAsync(user, model.Password);
                // If user is successfully created, sign-in the user using
                // SignInManager and redirect to index action of HomeController
                if (result.Succeeded)
                {
                    //Then send the Confirmation Email to the User
                    await SendConfirmationEmail(model.Email, user);
                    // If the user is signed in and in the Admin role, then it is
                    // the Admin user that is creating a new user. 
                    // So, redirect the Admin user to ListUsers action of Administration Controller
                    if (_signInManager.IsSignedIn(User) && User.IsInRole("Admin"))
                    {
                        return RedirectToAction("ListUsers", "Administration");
                    }
                    //If it is not Admin user, then redirect the user to RegistrationSuccessful View
                    return View("RegistrationSuccessful");
                }
                // If there are any errors, add them to the ModelState object
                // which will be displayed by the validation summary tag helper
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }
        //[HttpGet]
        //[AllowAnonymous]  //The [AllowAnonymous] attribute is used in ASP.NET Core to specify that an action or controller should be accessible without requiring authentication.
        //Here are some scenarios where you might use [AllowAnonymous]

        private async Task SendAccountLockedEmail(string? email)
        {
            //Send the Confirmation Email to the User Email Id
            await _emailSender.SendEmailAsync(email, "Account Locked", "Your Account is Locked Due to Multiple Invalid Attempts.", false);
        }




        [HttpGet]
        public IActionResult Login(string? ReturnUrl = null)
        {
            ViewData["ReturnUrl"] = ReturnUrl;
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model, string? ReturnUrl)
        {
            if (ModelState.IsValid)
            {
                //First Fetch the User Details by Email Id
                var user = await _userManager.FindByEmailAsync(model.Email);
                //Then Check if User Exists, EmailConfirmed and Password Is Valid
                //CheckPasswordAsync: Returns a flag indicating whether the given password is valid for the specified user.
                if (user != null && !user.EmailConfirmed &&
                            (await _userManager.CheckPasswordAsync(user, model.Password)))
                {
                    ModelState.AddModelError(string.Empty, "Email not confirmed yet");
                    return View(model);
                }
                // The last boolean parameter lockoutOnFailure indicates if the account should be locked on failed login attempt. 
                // On every failed login attempt AccessFailedCount column value in AspNetUsers table is incremented by 1. 
                // When the AccessFailedCount reaches the configured MaxFailedAccessAttempts which in our case is 5,
                // the account will be locked and LockoutEnd column is populated.
                // After the account is lockedout, even if we provide the correct username and password,
                // PasswordSignInAsync() method returns Lockedout result and
                // the login will not be allowed for the duration the account is locked.
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);
                if (result.Succeeded)
                {
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
                    //It's important to inform users when their account is locked.
                    //This can be done through the UI or by sending an email notification.
                    await SendAccountLockedEmail(model.Email);
                    return View("AccountLocked");
                }
                else
                {
                    // Handle failure
                    // Get the number of attempts left
                    var attemptsLeft = _userManager.Options.Lockout.MaxFailedAccessAttempts - await _userManager.GetAccessFailedCountAsync(user);
                    ModelState.AddModelError(string.Empty, $"Invalid Login Attempt. Remaining Attempts : {attemptsLeft}");
                    return View(model);
                }
            }
            // If we got this far, something failed, redisplay form
            return View(model);
        }


        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction("Login");
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        //  -------------------------------------------   Email Confirmation  -------------------------------------------------//

        private async Task SendConfirmationEmail(string? email, ApplicationUser? user)
        {
            //Generate the Token
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            //Build the Email Confirmation Link which must include the Callback URL
            var ConfirmationLink = Url.Action("ConfirmEmail", "Account",
            new { UserId = user.Id, Token = token }, protocol: HttpContext.Request.Scheme);
            //Send the Confirmation Email to the User Email Id
            await _emailSender.SendEmailAsync(email, "Confirm Your Email", $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(ConfirmationLink)}'>clicking here</a>.", true);
        }



        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string UserId, string Token)
        {
            if (UserId == null || Token == null)
            {
                ViewBag.Message = "The link is Invalid or Expired";
            }
            //Find the User By Id
            var user = await _userManager.FindByIdAsync(UserId);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"The User ID {UserId} is Invalid";
                return View("NotFound");
            }
            //Call the ConfirmEmailAsync Method which will mark the Email as Confirmed
            var result = await _userManager.ConfirmEmailAsync(user, Token);
            if (result.Succeeded)
            {
                ViewBag.Message = "Thank you for confirming your email";
                return View();
            }
            ViewBag.Message = "Email cannot be confirmed";
            return View();
        }


        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResendConfirmationEmail(bool IsResend = true)
        {
            if (IsResend)
            {
                ViewBag.Message = "Resend Confirmation Email";
            }
            else
            {
                ViewBag.Message = "Send Confirmation Email";
            }
            return View();
        }


        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendConfirmationEmail(string Email)
        {
            var user = await _userManager.FindByEmailAsync(Email);
            if (user == null || await _userManager.IsEmailConfirmedAsync(user))
            {
                // Handle the situation when the user does not exist or Email already confirmed.
                // For security, don't reveal that the user does not exist or Email is already confirmed
                return View("ConfirmationEmailSent");
            }
            //Then send the Confirmation Email to the User
            await SendConfirmationEmail(Email, user);
            return View("ConfirmationEmailSent");
        }


        // --------------------------------------------    Forgot Password  ----------------------------------------------------//
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }



        private async Task SendForgotPasswordEmail(string? email, ApplicationUser? user)
        {
            // Generate the reset password token
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            // Build the password reset link which must include the Callback URL
            // Build the password reset link
            var passwordResetLink = Url.Action("ResetPassword", "Account",
                    new { Email = email, Token = token }, protocol: HttpContext.Request.Scheme);
            //Send the Confirmation Email to the User Email Id
            await _emailSender.SendEmailAsync(email, "Reset Your Password", $"Please reset your password by <a href='{HtmlEncoder.Default.Encode(passwordResetLink)}'>clicking here</a>.", true);
        }


        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Find the user by email
                var user = await _userManager.FindByEmailAsync(model.Email);
                // If the user is found AND Email is confirmed
                if (user != null && await _userManager.IsEmailConfirmedAsync(user))
                {
                    await SendForgotPasswordEmail(user.Email, user);
                    // Send the user to Forgot Password Confirmation view
                    return RedirectToAction("ForgotPasswordConfirmation", "Account");
                }
                // To avoid account enumeration and brute force attacks, don't
                // reveal that the user does not exist or is not confirmed
                return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }
            return View(model);
        }

        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string Token, string Email)
        {
            // If password reset token or email is null, most likely the
            // user tried to tamper the password reset link
            if (Token == null || Email == null)
            {
                ViewBag.ErrorTitle = "Invalid Password Reset Token";
                ViewBag.ErrorMessage = "The Link is Expired or Invalid";
                return View("Error");
            }
            else
            {
                ResetPasswordViewModel model = new ResetPasswordViewModel();
                model.Token = Token;
                model.Email = Email;
                return View(model);
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Find the user by email
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    // reset the user password
                    var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
                    if (result.Succeeded)
                    {
                        // Upon successful password reset and if the account is lockedout,
                        // set the account lockout end date to current UTC date time, 
                        // so the user can login with the new password
                        if (await _userManager.IsLockedOutAsync(user))
                        {
                            await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow);
                        }
                        //Once the Password is Reset, remove the token from the database if you are storing the token
                        await _userManager.RemoveAuthenticationTokenAsync(user, "ResetPassword", "ResetPasswordToken");
                        return RedirectToAction("ResetPasswordConfirmation", "Account");
                    }
                    // Display validation errors. For example, password reset token already
                    // used to change the password or password complexity rules not met
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View(model);
                }
                // To avoid account enumeration and brute force attacks, don't
                // reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            // Display validation errors if model state is not valid
            return View(model);
        }


        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }





        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }


        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                //fetch the User Details
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    //If User does not exists, redirect to the Login Page
                    return RedirectToAction("Login", "Account");
                }
                // ChangePasswordAsync Method changes the user password
                var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
                // The new password did not meet the complexity rules or the current password is incorrect.
                // Add these errors to the ModelState and rerender ChangePassword view
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View();
                }
                // Upon successfully changing the password refresh sign-in cookie
                await _signInManager.RefreshSignInAsync(user);
                //Then redirect the user to the ChangePasswordConfirmation view
                return RedirectToAction("ChangePasswordConfirmation", "Account");
            }
            return View(model);
        }

        [Authorize]
        [HttpGet]
        public IActionResult ChangePasswordConfirmation()
        {
            return View();
        }



        [Authorize]
        [HttpGet]
        public async Task<IActionResult> ManageTwoFactorAuthentication()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }
            //First, we need to check whether the User Phone and Email is confirmed or not
            if (!user.PhoneNumberConfirmed && !user.EmailConfirmed)
            {
                ViewBag.ErrorTitle = "You cannot Enable/Disable Two Factor Authentication";
                ViewBag.ErrorMessage = "Your Phone Number and Email Not Yet Confirmed";
                return View("Error");
            }
            string Message;
            if (user.TwoFactorEnabled)
            {
                Message = "Disable 2FA";
            }
            else
            {
                Message = "Enable 2FA";
            }
            //Generate the Two Factor Authentication Token
            var TwoFactorToken = await _userManager.GenerateTwoFactorTokenAsync(user, TokenOptions.DefaultPhoneProvider);
            //Send the Token to user Mobile Number and Email Id
            //Sending SMS
            //var result = await smsSender.SendSmsAsync(user.PhoneNumber, $"Your Token to {Message} is {TwoFactorToken}");
            //Sending Email
            await _emailSender.SendEmailAsync(user.Email, Message, $"Your Token to {Message} is {TwoFactorToken}", false);
            return View(); // View for the user to enter the token
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ManageTwoFactorAuthentication(string Token)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }
            var result = await _userManager.VerifyTwoFactorTokenAsync(user, TokenOptions.DefaultPhoneProvider, Token);
            if (result)
            {
                // Token is valid
                if (user.TwoFactorEnabled)
                {
                    user.TwoFactorEnabled = false;
                    ViewBag.Message = "You have Sucessfully Disabled Two Factor Authentication";
                }
                else
                {
                    user.TwoFactorEnabled = true;
                    ViewBag.Message = "You have Sucessfully Enabled Two Factor Authentication";
                }
                await _userManager.UpdateAsync(user);
                // Redirect to success page 
                return View("TwoFactorAuthenticationSuccessful");
            }
            else
            {
                // Handle invalid token
                ViewBag.ErrorTitle = "Unable to Enable/Disable Two Factor Authentication";
                ViewBag.ErrorMessage = "Either the Token is Expired or you entered some wrong information";
                return View("Error");
            }
        }





        [HttpGet]
        [AllowAnonymous]
        public IActionResult VerifyTwoFactorToken(string Email, string ReturnUrl, bool RememberMe, string TwoFactorAuthenticationToken)
        {
            VerifyTwoFactorTokenViewModel model = new VerifyTwoFactorTokenViewModel()
            {
                RememberMe = RememberMe,
                Email = Email,
                ReturnUrl = ReturnUrl,
                Token = TwoFactorAuthenticationToken
            };
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyTwoFactorToken(VerifyTwoFactorTokenViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid Login Attempt.");
                return View(model);
            }
            // Validate the 2FA token
            var result = await _userManager.VerifyTwoFactorTokenAsync(user, "Email", model.TwoFactorCode);
            if (result)
            {
                // Sign in the user and redirect
                await _signInManager.SignInAsync(user, isPersistent: model.RememberMe);
                // Check if the ReturnUrl is not null and is a local URL
                if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                {
                    return Redirect(model.ReturnUrl);
                }
                else
                {
                    // Redirect to default page
                    return RedirectToAction("Index", "Home");
                }
            }
            ModelState.AddModelError(string.Empty, "Invalid verification code.");
            return View(model);
        }


    }

    



}
