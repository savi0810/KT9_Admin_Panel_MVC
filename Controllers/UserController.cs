using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KT9_Admin_Panel_MVC.Data;
using KT9_Admin_Panel_MVC.Models;

namespace KT9_Admin_Panel_MVC.Controllers;

[Authorize]
public class UsersController(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    ApplicationDbContext context) : Controller
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly RoleManager<IdentityRole> _roleManager = roleManager;
    private readonly ApplicationDbContext _context = context;

    public async Task<IActionResult> Index()
    {
        var users = await _userManager.Users.ToListAsync();
        var userRoles = new Dictionary<string, IList<string>>();
        foreach (var user in users)
            userRoles[user.Id] = await _userManager.GetRolesAsync(user);
        ViewBag.UserRoles = userRoles;
        return View(users);
    }

    [Authorize(Policy = "AdminOnly")]
    public IActionResult Create() => View();

    [HttpPost, Authorize(Policy = "AdminOnly"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
        var result = await _userManager.CreateAsync(user, model.Password);
        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, "User");
            return RedirectToAction(nameof(Index));
        }
        foreach (var error in result.Errors)
            ModelState.AddModelError("", error.Description);
        return View(model);
    }

    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Edit(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();
        var roles = await _roleManager.Roles.ToListAsync();
        var userRoles = await _userManager.GetRolesAsync(user);
        var model = new EditUserViewModel
        {
            Id = user.Id,
            Email = user.Email,
            Roles = roles.Select(r => new RoleCheckbox
            {
                RoleId = r.Id,
                RoleName = r.Name,
                IsSelected = userRoles.Contains(r.Name)
            }).ToList()
        };
        return View(model);
    }

    [HttpPost, Authorize(Policy = "AdminOnly"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, EditUserViewModel model)
    {
        if (id != model.Id || !ModelState.IsValid) return View(model);
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        user.Email = user.UserName = model.Email;
        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            foreach (var error in updateResult.Errors)
                ModelState.AddModelError("", error.Description);
            return View(model);
        }

        var currentRoles = await _userManager.GetRolesAsync(user);
        var selectedRoles = model.Roles.Where(r => r.IsSelected).Select(r => r.RoleName).ToList();

        foreach (var role in currentRoles.Except(selectedRoles))
            await _userManager.RemoveFromRoleAsync(user, role);
        foreach (var role in selectedRoles.Except(currentRoles))
            await _userManager.AddToRoleAsync(user, role);

        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Delete(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        return user == null ? NotFound() : View(user);
    }

    [HttpPost, ActionName("Delete"), Authorize(Policy = "AdminOnly"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user != null) await _userManager.DeleteAsync(user);
        return RedirectToAction(nameof(Index));
    }
}