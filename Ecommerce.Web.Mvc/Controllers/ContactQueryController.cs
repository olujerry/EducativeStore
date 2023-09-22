using AutoMapper;
using Ecommerce.Application.Dto;
using Ecommerce.Application.Handlers.ContactQueries.Commands;
using Ecommerce.Application.Handlers.ContactQueries.Queries;
using Ecommerce.Domain.Identity.Permissions;
using Ecommerce.Web.Mvc.Helpers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace Ecommerce.Web.Mvc.Controllers;

[Authorize]
public class ContactQueryController : Controller
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;
    public ContactQueryController(IMediator mediator, IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }

    [Authorize(Permissions.Permissions_ContactQuery_View)]
    public IActionResult Index()
    {
        return View();
    }

    [Authorize(Permissions.Permissions_Customer_View)]
    public async Task<IActionResult> RenderView()
    {
        var paging = new PageRequest().GetPageResponse(Request);
        var result = await _mediator.Send(new GetContactQueriesWithPagingQuery() { page = paging.PageIndex, length = paging.Length, searchValue = paging.SearchValue, sortColumn = paging.SortColumnName, sortOrder = paging.SortOrder });

        var jsonData = new { data = result.Items, draw = paging.Draw, recordsFiltered = result.TotalCount, recordsTotal = result.TotalCount };
        return Json(jsonData);
    }

    [AllowAnonymous]
    public IActionResult Send()
    {
        return View();
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Send(ContactQueryDto dto)
    {
        if (ModelState.IsValid)
        {
            var command = _mapper.Map<CreateContactQueryCommand>(dto);
            var response = await _mediator.Send(command);
            if (response.Succeeded)
            {
                TempData["notification"] = "<script>swal(`" + "Success!" + "`, `" + "Your Message Send Successfully! `,`" + "success" + "`)" + "</script>";
                return Redirect("/");
            }
            ModelState.AddModelError(string.Empty, response.Message);
        }

        return View(dto);
    }

}
