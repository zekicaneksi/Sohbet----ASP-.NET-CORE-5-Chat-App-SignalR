using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Sohbet.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Http;

namespace Sohbet.Controllers
{
    public class HomeController : Controller
    {
        public QueryManager queryManager;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            queryManager = new QueryManager(configuration);
        }

        [Authorize]
        public IActionResult Index()
        {
            return View("Index", HttpContext.User.Claims);
        }

        public IActionResult Login(string ReturnUrl = "/")
        {
            
            UserModel userModel = new UserModel();
            userModel.ReturnUrl = ReturnUrl;
            return View("LoginPage", userModel);
        }

        [HttpPost]
        public IActionResult RegisterUser(string Nick, string Password)
        {
            try
            {
                UserModel userModel = new UserModel();
                userModel.Nick = Nick;
                userModel.Password = Password;

                QueryManager.AddUserReturnMessage returnMessage = queryManager.AddUser(userModel);

                if (returnMessage == QueryManager.AddUserReturnMessage.Success)
                {
                    return Json("success");
                }
                else if (returnMessage == QueryManager.AddUserReturnMessage.InvalidInput)
                {
                    return Json("Invalid Input");
                }
                else if (returnMessage == QueryManager.AddUserReturnMessage.NickInUse)
                {
                    return Json("Nick In Use");
                }
                else
                {
                    return Json("Unknown Error");
                }
            }
            catch (Exception)
            {
                return Redirect("/");
            }
            
        }

        public async Task<IActionResult> LoginUser(string Nick, string Password)
        {
            try
            {
                UserModel userModel = new UserModel();
                userModel.Nick = Nick;
                userModel.Password = Password;

                if (ModelState.IsValid)
                {
                    QueryManager.ValidateUserReturnMessage returnMessage = queryManager.ValidateUser(userModel);
                    if (returnMessage == QueryManager.ValidateUserReturnMessage.NotFound)
                    {
                        return Json("invalid credential(s)");
                    }
                    else if (returnMessage == QueryManager.ValidateUserReturnMessage.UnknownError)
                    {
                        return Json("Unknown Error");
                    }
                    else
                    {
                        var claims = new List<Claim>
                    {
                        new Claim("Nick", userModel.Nick),
                    };
                        var claimsIdentity = new ClaimsIdentity(
                            claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        var authProperties = new AuthenticationProperties
                        {
                            AllowRefresh = true,
                            ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(10),
                            IsPersistent = true,
                            IssuedUtc = DateTime.Now,
                            RedirectUri = "/"
                        };

                        await HttpContext.SignInAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme,
                            new ClaimsPrincipal(claimsIdentity),
                            authProperties);

                        return Json("success");
                    }


                }
                return Json("Unknown Error");
            }
            catch (Exception)
            {
                return Json("Unknown Error");
            }
        }

        public async Task<IActionResult> LogOutUser()
        {  
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return LocalRedirect("/");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public IActionResult SetLanguage(string culture, string returnUrl)
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );

            return LocalRedirect(returnUrl);
        }
    }
}
