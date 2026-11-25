using FreshGo.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace FreshGo.Pages.Account;
[AllowAnonymous]
public class LoginModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signIn;
    private readonly UserManager<ApplicationUser> _users;
    public LoginModel(SignInManager<ApplicationUser> signIn, UserManager<ApplicationUser> users) { _signIn = signIn; _users = users; }

    [BindProperty] public LoginInput Input { get; set; } = new();
    [BindProperty(SupportsGet = true)] public string? ReturnUrl { get; set; }

    public class LoginInput
    {
        [Required, EmailAddress] public string Email { get; set; } = "";
        [Required, DataType(DataType.Password)] public string Password { get; set; } = "";
        public bool RememberMe { get; set; }
    }

    public void OnGet() { }
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        var user = await _users.FindByEmailAsync(Input.Email);
        if (user == null || !user.IsActive)
        {
            ModelState.AddModelError(string.Empty, "Tài khoản không tồn tại hoặc đã bị khóa.");
            return Page();
        }
        var result = await _signIn.PasswordSignInAsync(user.UserName!, Input.Password, Input.RememberMe, lockoutOnFailure: true);
        if (result.Succeeded)
        {
            if (!string.IsNullOrWhiteSpace(ReturnUrl) && Url.IsLocalUrl(ReturnUrl)) return LocalRedirect(ReturnUrl);
            return RedirectToPage("/Dashboard");
        }
        ModelState.AddModelError(string.Empty, result.IsLockedOut ? "Tài khoản tạm khóa do đăng nhập sai nhiều lần." : "Email hoặc mật khẩu không chính xác.");
        return Page();
    }
}
