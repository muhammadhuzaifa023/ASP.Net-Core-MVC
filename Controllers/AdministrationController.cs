using ASP.Net_Core_MVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ASP.Net_Core_MVC.Controllers
{
    //[Authorize(Roles = "Admin")]
    //[Authorize(Roles = "Moderator")]
    public class AdministrationController : Controller
    {
        //public IActionResult Index()
        //{
        //    return View();
        //}
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        public AdministrationController(RoleManager<ApplicationRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }
        [HttpGet]
        public async Task<IActionResult> ListRoles()
        {
            List<ApplicationRole> roles = await _roleManager.Roles.ToListAsync();
            return View(roles);
        }
        [HttpGet]
        public IActionResult CreateRole()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole(CreateRoleViewModel roleModel)
        {
            if (ModelState.IsValid)
            {
                // Check if the role already exists
                bool roleExists = await _roleManager.RoleExistsAsync(roleModel?.RoleName);
                if (roleExists)
                {
                    ModelState.AddModelError("", "Role Already Exists");
                }
                else
                {
                    // Create the role
                    // We just need to specify a unique role name to create a new role
                    ApplicationRole identityRole = new ApplicationRole
                    {
                        Name = roleModel?.RoleName,
                        Description = roleModel?.Description
                    };
                    // Saves the role in the underlying AspNetRoles table
                    IdentityResult result = await _roleManager.CreateAsync(identityRole);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Index", "Home");
                    }
                    foreach (IdentityError error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }
            return View(roleModel);
        }
        //[HttpGet]
        //public async Task<IActionResult> ListRoles()
        //{
        //    List<IdentityRole> roles = await _roleManager.Roles.ToListAsync();
        //    return View(roles);
        //}

        [HttpGet]
        public async Task<IActionResult> EditRole(string roleId)
        {
            //First Get the role information from the database
            ApplicationRole role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                // Handle the scenario when the role is not found
                return View("Error");
            }
            //Populate the EditRoleViewModel from the data retrived from the database
            var model = new EditRoleViewModel
            {
                Id = role.Id,
                RoleName = role.Name,
                Description = role.Description
                // You can add other properties here if needed
            };
            //Initialize the Users and Claims Property to avoid Null Reference Exception while Add the user name
            model.Users = new List<string>();
            model.Claims = new List<string>();
            // Gets a list of claims associated with the specified role.
            var roleClaims = await _roleManager.GetClaimsAsync(role);
            model.Claims = roleClaims.Select(c => c.Value).ToList();
            // Retrieve all the Users
            foreach (var user in _userManager.Users.ToList())
            {
                // If the user is in this role, add the username to
                // Users property of EditRoleViewModel. 
                // This model object is then passed to the view for display
                if (await _userManager.IsInRoleAsync(user, role.Name))
                {
                    model.Users.Add(user.UserName);
                }
            }
            return View(model);
        }
        [HttpPost]
        [Authorize(Policy = "EditRolePolicy")]
        public async Task<IActionResult> EditRole(EditRoleViewModel model)
        {
            if (ModelState.IsValid)
            {
                var role = await _roleManager.FindByIdAsync(model.Id);
                if (role == null)
                {
                    // Handle the scenario when the role is not found
                    ViewBag.ErrorMessage = $"Role with Id = {model.Id} cannot be found";
                    return View("NotFound");
                }
                else
                {
                    role.Name = model.RoleName;
                    role.Description = model.Description;
                    // Update other properties if needed
                    var result = await _roleManager.UpdateAsync(role);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("ListRoles"); // Redirect to the roles list
                    }
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View(model);
                }
            }
            return View(model);

        }


        [HttpPost]
        [Authorize(Policy = "DeleteRolePolicy")]
        public async Task<IActionResult> DeleteRole(string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                // Role not found, handle accordingly
                ViewBag.ErrorMessage = $"Role with Id = {roleId} cannot be found";
                return View("NotFound");
            }
            else
            {
                // Wrap the code in a try/catch block
                try
                {
                    var result = await _roleManager.DeleteAsync(role);
                    if (result.Succeeded)
                    {
                        // Role deletion successful
                        return RedirectToAction("ListRoles"); // Redirect to the roles list page
                    }
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    // If we reach here, something went wrong, return to the view
                    return View("ListRoles", await _roleManager.Roles.ToListAsync());
                }
                // If the exception is DbUpdateException, we know we are not able to
                // delete the role as there are users in the role being deleted
                catch (DbUpdateException ex)
                {
                    // Log the exception to a file. 
                    ViewBag.Error = ex.Message;
                    // Pass the ErrorTitle and ErrorMessage that you want to show to the user using ViewBag.
                    // The Error view retrieves this data from the ViewBag and displays to the user.
                    ViewBag.ErrorTitle = $"{role.Name} Role is in Use";
                    ViewBag.ErrorMessage = $"{role.Name} Role cannot be deleted as there are users in this role. If you want to delete this role, please remove the users from the role and then try to delete";
                    return View("Error");
                    throw;
                }
            }
        }
        [HttpGet]
        public async Task<IActionResult> EditUsersInRole(string roleId)
        {
            ViewBag.roleId = roleId;
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role with Id = {roleId} cannot be found";
                return View("NotFound");
            }
            ViewBag.RollName = role.Name;
            var model = new List<UserRoleViewModel>();
            foreach (var user in _userManager.Users.ToList())
            {
                var userRoleViewModel = new UserRoleViewModel
                {
                    UserId = user.Id,
                    UserName = user.UserName
                };
                if (await _userManager.IsInRoleAsync(user, role.Name))
                {
                    userRoleViewModel.IsSelected = true;
                }
                else
                {
                    userRoleViewModel.IsSelected = false;
                }
                model.Add(userRoleViewModel);
            }
            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> EditUsersInRole(List<UserRoleViewModel> model, string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role with Id = {roleId} cannot be found";
                return View("NotFound");
            }
            for (int i = 0; i < model.Count; i++)
            {
                var user = await _userManager.FindByIdAsync(model[i].UserId);
                IdentityResult? result;
                if (model[i].IsSelected && !(await _userManager.IsInRoleAsync(user, role.Name)))
                {
                    //If IsSelected is true and User is not already in this role, then add the user
                    result = await _userManager.AddToRoleAsync(user, role.Name);
                }
                else if (!model[i].IsSelected && await _userManager.IsInRoleAsync(user, role.Name))
                {
                    //If IsSelected is false and User is already in this role, then remove the user
                    result = await _userManager.RemoveFromRoleAsync(user, role.Name);
                }
                else
                {
                    //Don't do anything simply continue the loop
                    continue;
                }
                //If you add or remove any user, please check the Succeeded of the IdentityResult
                if (result.Succeeded)
                {
                    if (i < (model.Count - 1))
                        continue;
                    else
                        return RedirectToAction("EditRole", new { roleId = roleId });
                }
            }
            return RedirectToAction("EditRole", new { roleId = roleId });
        }
        //-------------------------------------------------------------------------------------------------------------------------------------------//
        [HttpGet]
        public IActionResult ListUsers()
        {
            var users = _userManager.Users;
            return View(users);
        }

        [HttpGet]
        public async Task<IActionResult> EditUser(string UserId)
        {
            //First Fetch the User Details by UserId
            var user = await _userManager.FindByIdAsync(UserId);
            //Check if User Exists in the Database
            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id = {UserId} cannot be found";
                return View("NotFound");
            }
            // GetClaimsAsync retunrs the list of user Claims
            var userClaims = await _userManager.GetClaimsAsync(user);
            // GetRolesAsync returns the list of user Roles
            var userRoles = await _userManager.GetRolesAsync(user);
            //Store all the information in the EditUserViewModel instance
            var model = new EditUserViewModel
            {
                Id = user.Id,
                Email = user.Email,
                UserName = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Claims = userClaims.Select(c => c.Value).ToList(),
                Roles = userRoles
            };
            //Pass the Model to the View
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> EditUser(EditUserViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id = {model.Id} cannot be found";
                return View("NotFound");
            }
            else
            {
                //Populate the user instance with the data from EditUserViewModel
                user.Email = model.Email;
                user.UserName = model.UserName;
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                //UpdateAsync Method will update the user data in the AspNetUsers Identity table
                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    //Once user data updated redirect to the ListUsers view
                    return RedirectToAction("ListUsers");
                }
                else
                {
                    //In case any error, stay in the same view and show the model validation error
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
                return View(model);
            }
        }
        [HttpPost]
        public async Task<IActionResult> DeleteUser(string UserId)
        {
            //First Fetch the User you want to Delete
            var user = await _userManager.FindByIdAsync(UserId);
            if (user == null)
            {
                // Handle the case where the user wasn't found
                ViewBag.ErrorMessage = $"User with Id = {UserId} cannot be found";
                return View("NotFound");
            }
            else
            {
                //Delete the User Using DeleteAsync Method of UserManager Service
                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    // Handle a successful delete
                    return RedirectToAction("ListUsers");
                }
                else
                {
                    // Handle failure
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
                return View("ListUsers");
            }
        }



        [HttpGet]
        public async Task<IActionResult> ManageUserRoles(string UserId)
        {
            //First Fetch the User Information from the Identity database by user Id
            var user = await _userManager.FindByIdAsync(UserId);
            if (user == null)
            {
                //handle if User Not Found in the Database
                ViewBag.ErrorMessage = $"User with Id = {UserId} cannot be found";
                return View("NotFound");
            }
            //Store the UserId in the ViewBag which is required while updating the Data
            //Store the UserName in the ViewBag which is used for displaying purpose
            ViewBag.UserId = UserId;
            ViewBag.UserName = user.UserName;
            //Create a List to Hold all the Roles Information
            var model = new List<UserRolesViewModel>();
            //Loop Through Each role and populate the model 
            foreach (var role in await _roleManager.Roles.ToListAsync())
            {
                var userRolesViewModel = new UserRolesViewModel
                {
                    RoleId = role.Id,
                    RoleName = role.Name,
                    Description = role.Description
                };
                //Check if the Role is already assigned to this user
                if (await _userManager.IsInRoleAsync(user, role.Name))
                {
                    userRolesViewModel.IsSelected = true;
                }
                else
                {
                    userRolesViewModel.IsSelected = false;
                }
                //Add the userRolesViewModel to the model
                model.Add(userRolesViewModel);
            }
            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> ManageUserRoles(List<UserRolesViewModel> model, string UserId)
        {
            var user = await _userManager.FindByIdAsync(UserId);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id = {UserId} cannot be found";
                return View("NotFound");
            }
            //fetch the list of roles the specified user belongs to
            var roles = await _userManager.GetRolesAsync(user);
            //Then remove all the assigned roles for this user
            var result = await _userManager.RemoveFromRolesAsync(user, roles);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot remove user existing roles");
                return View(model);
            }
            List<string> RolesToBeAssigned = model.Where(x => x.IsSelected).Select(y => y.RoleName).ToList();
            //If At least 1 Role is assigned, Any Method will return true
            if (RolesToBeAssigned.Any())
            {
                //add a user to multiple roles simultaneously
                result = await _userManager.AddToRolesAsync(user, RolesToBeAssigned);
                if (!result.Succeeded)
                {
                    ModelState.AddModelError("", "Cannot Add Selected Roles to User");
                    return View(model);
                }
            }
            return RedirectToAction("EditUser", new { UserId = UserId });
        }


        // ----------------------------------------------------    User Claimn  -----------------------------------------------------------------------//


        [HttpGet]
        public async Task<IActionResult> ManageUserClaims(string UserId)
        {
            //First, fetch the User Details Based on the UserId
            var user = await _userManager.FindByIdAsync(UserId);
            if (user == null)
            {
                //handle if the User is not Exists in the database
                ViewBag.ErrorMessage = $"User with Id = {UserId} cannot be found";
                return View("NotFound");
            }
            //Storing the UserName in the ViewBag for Display Purpose
            ViewBag.UserName = user.UserName;
            //Create UserClaimsViewModel Instance
            var model = new UserClaimsViewModel
            {
                UserId = UserId
            };
            // UserManager service GetClaimsAsync method gets all the current claims of the user
            var existingUserClaims = await _userManager.GetClaimsAsync(user);
            // Loop through each claim we have in our application
            // Call the GetAllClaims Static Method ClaimsStore Class
            foreach (Claim claim in ClaimsStore.GetAllClaims())
            {
                //Create an Instance of UserClaim class
                UserClaim userClaim = new UserClaim
                {
                    ClaimType = claim.Type
                };
                // If the user has the claim, set IsSelected property to true, so the checkbox
                // next to the claim is checked on the UI
                if (existingUserClaims.Any(c => c.Type == claim.Type))
                {
                    userClaim.IsSelected = true;
                }
                //By default the IsSelected is False, no need to set as false
                //Add the userClaim to UserClaimsViewModel Instance 
                model.Cliams.Add(userClaim);
            }
            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> ManageUserClaims(UserClaimsViewModel model)
        {
            //First fetch the User Details
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id = {model.UserId} cannot be found";
                return View("NotFound");
            }
            // Get all the user existing claims and delete them
            var claims = await _userManager.GetClaimsAsync(user);
            var result = await _userManager.RemoveClaimsAsync(user, claims);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot remove user existing claims");
                return View(model);
            }
            // Add all the claims that are selected on the UI
            var AllSelectedClaims = model.Cliams.Where(c => c.IsSelected)
                        .Select(c => new Claim(c.ClaimType, c.ClaimType))
                        .ToList();
            //If At least 1 Claim is assigned, Any Method will return true
            if (AllSelectedClaims.Any())
            {
                //add a user to multiple claims simultaneously
                result = await _userManager.AddClaimsAsync(user, AllSelectedClaims);
                if (!result.Succeeded)
                {
                    ModelState.AddModelError("", "Cannot add selected claims to user");
                    return View(model);
                }
            }
            return RedirectToAction("EditUser", new { UserId = model.UserId });
        }


        [HttpGet]
        public async Task<IActionResult> ManageRoleClaims(string RoleId)
        {
            //First, fetch the Role Details Based on the RoleId
            var role = await _roleManager.FindByIdAsync(RoleId);
            if (role == null)
            {
                //handle if the role is not Exists in the database
                ViewBag.ErrorMessage = $"Role with Id = {RoleId} cannot be found";
                return View("NotFound");
            }
            //Storing the Role Name in the ViewBag for Display Purpose
            ViewBag.RoleName = role.Name;
            //Create RoleClaimsViewModel Instance
            var model = new RoleClaimsViewModel
            {
                RoleId = RoleId
            };
            // RoleManager service GetClaimsAsync method gets all the current claims of the role
            var existingRoleClaims = await _roleManager.GetClaimsAsync(role);
            // Loop through each claim we have in our application
            // Call the GetAllClaims Static Method ClaimsStore Class
            foreach (Claim claim in ClaimsStore.GetAllClaims())
            {
                //Create an Instance of RoleClaim class
                RoleClaim roleClaim = new RoleClaim
                {
                    ClaimType = claim.Type
                };
                // If the Role has the claim, set IsSelected property to true, so the checkbox
                // next to the claim is checked on the UI
                if (existingRoleClaims.Any(c => c.Type == claim.Type))
                {
                    roleClaim.IsSelected = true;
                }
                //By default, the IsSelected is False, no need to set as false
                //Add the roleClaim to RoleClaimsViewModel Instance 
                model.Claims.Add(roleClaim);
            }
            return View(model);
        }





        [HttpPost]
        public async Task<IActionResult> ManageRoleClaims(RoleClaimsViewModel model)
        {
            //First fetch the Role Details
            var role = await _roleManager.FindByIdAsync(model.RoleId);
            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role with Id = {model.RoleId} cannot be found";
                return View("NotFound");
            }
            // Get all the existing claims of the role
            var claims = await _roleManager.GetClaimsAsync(role);
            for (int i = 0; i < model.Claims.Count; i++)
            {
                Claim claim = new Claim(model.Claims[i].ClaimType, model.Claims[i].ClaimType);
                IdentityResult? result;
                if (model.Claims[i].IsSelected && !(claims.Any(c => c.Type == claim.Type)))
                {
                    //If IsSelected is true and User is not already in this role, then add the user
                    //result = await _userManager.AddToRoleAsync(user, role.Name);
                    result = await _roleManager.AddClaimAsync(role, claim);
                }
                else if (!model.Claims[i].IsSelected && claims.Any(c => c.Type == claim.Type))
                {
                    //If IsSelected is false and User is already in this role, then remove the user
                    result = await _roleManager.RemoveClaimAsync(role, claim);
                }
                else
                {
                    //Don't do anything simply continue the loop
                    continue;
                }
                //If you add or remove any user, please check the Succeeded of the IdentityResult
                if (result.Succeeded)
                {
                    if (i < (model.Claims.Count - 1))
                        continue;
                    else
                        return RedirectToAction("EditRole", new { roleId = model.RoleId });
                }
                else
                {
                    ModelState.AddModelError("", "Cannot add or removed selected claims to role");
                    return View(model);
                }
            }
            return RedirectToAction("EditRole", new { roleId = model.RoleId });
        }








    }

}
