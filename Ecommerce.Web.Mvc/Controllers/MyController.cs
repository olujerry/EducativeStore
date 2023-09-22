using AspNetCore.ReportingServices.ReportProcessing.ReportObjectModel;
using AutoMapper;
using Ecommerce.Application.Dto;
using Ecommerce.Application.Handlers.CustomerAccount.Commands;
using Ecommerce.Application.Handlers.Customers.Commands;
using Ecommerce.Application.Handlers.Customers.Queries;
using Ecommerce.Application.Handlers.NewFolder.Queries;
using Ecommerce.Application.Handlers.Orders.Queries;
using Ecommerce.Application.Handlers.OrderStatuses.Queries;
using Ecommerce.Application.Handlers.ProductReviews.Commands;
using Ecommerce.Application.Handlers.RenderItems.Queries;
using Ecommerce.Application.Identity;
using Ecommerce.Application.Interfaces;
using Ecommerce.Web.Mvc.Extension;
using Ecommerce.Web.Mvc.Helpers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json;

namespace Ecommerce.Web.Mvc.Controllers;

[Authorize]
public class MyController : Controller
{
    #region Basic Config
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;
    private readonly IKeyAccessor _keyAccessor;
    private readonly IAccountService _accountService;
    private readonly IUserService _userService;
    private readonly ICurrentUser _currentUser;
    private readonly IProfileService _profileService;
    private readonly IEmailService _emailService;
    public MyController(IMediator mediator,
        IMapper mapper,
        IKeyAccessor keyAccessor,
        IAccountService accountService,
        IProfileService profileService,
        IUserService userService,
        ICurrentUser currentUser,
        IEmailService emailService)
    {
        _mediator = mediator;
        _mapper = mapper;
        _keyAccessor = keyAccessor;
        _accountService = accountService;
        _profileService = profileService;
        _userService = userService;
        _currentUser = currentUser;
        _emailService = emailService;
    }
    #endregion

    public async Task<IActionResult> Index()
    {
        var customerInfo = await _mediator.Send(new GetCustomerInfoByLoginUserQuery());
        var currentUser = await _userService.GetCurrentUsersAsync();
        ViewBag.CustomerInfo = customerInfo;
        ViewBag.CurrentUser = currentUser;
        return View();
    }

    #region Order
    public async Task<IActionResult> Orders(int id)
    {
        var getOrders = await _mediator.Send(new GetCurrentCustomerOrdersQuery());
        return View(getOrders);
    }
    #endregion

    #region Reviews
    public async Task<IActionResult> Reviews()
    {
        var customerInfo = await _mediator.Send(new GetCustomerInfoByLoginUserQuery());
        var customerReviews = new List<ProductReviewDto>();
        if (customerInfo != null)
        {
            customerReviews = await _mediator.Send(new GetCustomerReviewsByCustomerId { CustomerId = customerInfo.Id });

        };
        ViewBag.CustomerReviews = customerReviews;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> CreateReview(CreateProductReviewByCustomerCommand data)
    {
        if (ModelState.IsValid)
        {
            var response = await _mediator.Send(data);
            if (response.Succeeded) return Json(new JsonResponse { Success = true, Data = response });
        }
        return Json(new JsonResponse { Success = false, Error = ModelState.ToSerializedDictionary() });
    }
    #endregion

    #region Wishlist
    public IActionResult WishList() => View();

    [HttpGet]
    public async Task<IActionResult> GetWishListItems(string ids)
    {
        if (ids.IsNullOrEmpty()) return Json(String.Empty);
        long[] productIds = JsonSerializer.Deserialize<long[]>(ids)!;
        var wishListItems = await _mediator.Send(new GetWishListItemByProductIdsQuery { ProductIds = productIds });
        return Json(wishListItems);
    }
    #endregion

    #region Address
    [HttpGet]
    public async Task<IActionResult> Address()
    {
        var customerInfo = await _mediator.Send(new GetCustomerInfoByLoginUserQuery());
        return View(customerInfo);
    }

    [HttpPost]
    public async Task<IActionResult> Address(CustomerDto customer)
    {
        var response = await _mediator.Send(new UpdateCustomerAddressCommand { Customer = customer });
        if (response.Succeeded) return View(customer);
        else ModelState.AddModelError(string.Empty, response.Message);
        return View(customer);
    }
    #endregion

    #region Tracking Order
    [HttpGet]
    public async Task<IActionResult> TrackOrder(string invoiceNo)
    {
        if (invoiceNo == null) { return RedirectToAction("Orders"); }
        var customer = await _mediator.Send(new GetCustomerInfoByLoginUserQuery());
        if (customer == null) { return RedirectToAction("Orders"); }
        var order = await _mediator.Send(new GetOrderByInvoiceNoAndCustomerIdQuery { InvoiceNo = invoiceNo, CustomerId = customer.Id });
        if (order == null) { return RedirectToAction("Orders"); }

        var orderStatus = await _mediator.Send(new GetOrderStatusByIdQuery { OrderId = order.Id });
        ViewBag.CurrentOrderStatus = orderStatus.OrderByDescending(o => o.Id).Select(o => o.OrderStatusValue.StatusValue).FirstOrDefault();
        ViewBag.InvoiceNo = invoiceNo;
        ViewBag.OrderStatus = orderStatus;
        return View();
    }

    #endregion

    #region Account Info
    public async Task<IActionResult> AccountInfo()
    {
        var customerInfo = await _mediator.Send(new GetCustomerInfoByLoginUserQuery());
        return View(customerInfo);
    }

    [HttpPost]
    public async Task<IActionResult> AccountInfo(CustomerDto customer)
    {
        var response = await _mediator.Send(new UpdateCustomerAccountInfoCommand { Customer = customer });
        if (response.Succeeded) return View(customer);
        else ModelState.AddModelError(string.Empty, response.Message);
        return View(customer);
    }
    #endregion

    #region Customer Login
    [HttpGet]
    [Route("customerlogin")]
    [Route("My/Login")]
    [AllowAnonymous]
    public ActionResult Login(string? returnUrl)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginUserDto());
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginUserDto loginUserDto, string? returnUrl)
    {
        var conAdv = _keyAccessor?["AdvancedConfiguration"] != null ? JsonSerializer.Deserialize<AdvancedConfigurationDto>(_keyAccessor["AdvancedConfiguration"]) : new AdvancedConfigurationDto();
        if (!ModelState.IsValid) return View(loginUserDto);
        var rs = await _accountService.SignInAsync(loginUserDto);
        if (rs.Succeeded)
        {
            var user = await _userService.GetUserByUserNameAsync(loginUserDto.UserName);
            //TempData["notification"] = "<script>swal(`" + "Welcome Back!" + "`, `" + "Hello " + user?.Data?.FullName?? loginUserDto.UserName + ", Welcome back!" + "`,`" + "success" + "`)" + "</script>";
            TempData["notification"] = $"<script>swal('Welcome Back!', 'Hello {user?.Data?.FullName ?? loginUserDto.UserName}, Welcome back!', 'success')</script>";
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
            //return Redirect("/");
        }
        else if (rs.Data.RequiresTwoFactor)
        {
            var sendCode = true;
            return RedirectToAction("LoginTwoStep", "Account", new { loginUserDto.UserName, loginUserDto.RememberMe, returnUrl, sendCode });
        }
        else if (rs.Data.IsLockedOut) ModelState.AddModelError(string.Empty, "Account locked");
        else if (conAdv.EnableEmailConfirmation && !rs.Data.IsEmailConfirmed) ModelState.AddModelError(string.Empty, "Email are Not Confirmed!");
        else ModelState.AddModelError(string.Empty, "Invalid Login Attempt");

        return View(loginUserDto);
    }
    #endregion

    #region Customer Register
    [AllowAnonymous]
    public ActionResult Register() => View();

    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> Register(CustomerRegisterDto customerRegister)
    {
        if (!ModelState.IsValid) return View(customerRegister);
        SecurityConfigurationDto conSec = _keyAccessor?["SecurityConfiguration"] != null ? JsonSerializer.Deserialize<SecurityConfigurationDto>(_keyAccessor["SecurityConfiguration"])! : new SecurityConfigurationDto();
        AdvancedConfigurationDto conAdv = _keyAccessor?["AdvancedConfiguration"] != null ? JsonSerializer.Deserialize<AdvancedConfigurationDto>(_keyAccessor["AdvancedConfiguration"])! : new AdvancedConfigurationDto();

        if (customerRegister?.Password.Length < conSec?.PasswordRequiredLength)
        {
            ModelState.AddModelError(customerRegister.Password, $"The Password must be at least {conSec?.PasswordRequiredLength} characters long.");
        }

        if (ModelState.IsValid)
        {
            var response = await _mediator.Send(new RegisterCustomerCommand { CustomerRegister = customerRegister });
            if (response.Succeeded)
            {
                if (conAdv.EnableEmailConfirmation)
                {
                    try
                    {
                        var url = $"{Request.Scheme}://{Request.Host.Value}/emailconfirmation";
                        var url2 = $"emailconfirmation";
                        await _emailService.SendEmailConfirmationAsync(customerRegister.UserName, "Email Confirmation", url);
                        TempData["notification"] = $"<script>swal('Success!', 'A confirmation email send to your email.', 'success')</script>";
                        return Redirect("/my/login");
                    }
                    catch
                    {
                        TempData["notification"] = "<script>swal(`" + "Error Occurred!" + "`, `" + "Can't send email confirmation. please contact support." + "`,`" + "error" + "`)" + "</script>";
                        return Redirect("/my/login");
                    }
                }
                else
                {
                    return RedirectToAction("Login", "My", new { succeeded = response.Succeeded, message = response.Message });
                }

            }
            ModelState.AddModelError(string.Empty, response.Message);
        }

        return View(customerRegister);
    }
    #endregion

    #region Update Password
    public IActionResult Password()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Password(EditPasswordDto editPassword)
    {
        if (ModelState.IsValid)
        {
            var response = await _profileService.UpdatePasswordAsync(editPassword);
            if (response.Succeeded)
            {
                await _accountService.SignOutAsync();
                TempData["notification"] = "<script>swal(`" + "Your Password Changed!" + "`, `" + "Please login to continue." + "`,`" + "success" + "`)" + "</script>";
                return Redirect("/my/login");
            }
            ModelState.AddModelError("", response.Message);
        }

        return View(editPassword);
    }
    #endregion

}
