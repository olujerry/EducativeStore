using Ecommerce.Application.Dto;
using Ecommerce.Application.Handlers.Categories.Queries;
using Ecommerce.Application.Handlers.Colors.Queries;
using Ecommerce.Application.Handlers.Products.Commands;
using Ecommerce.Application.Handlers.Products.Queries;
using Ecommerce.Application.Handlers.Sizes.Queries;
using Ecommerce.Domain.Constants;
using Ecommerce.Domain.Identity.Permissions;
using Ecommerce.Web.Mvc.Helpers;
using Ecommerce.Web.Mvc.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Reflection;

namespace Ecommerce.Web.Mvc.Controllers;

[Authorize]
public class ProductController : Controller
{
    private readonly IMediator _mediator;
    public ProductController(IMediator mediator)
    {
        _mediator = mediator;
    }
    [Authorize(Permissions.Permissions_Product_View)]
    public IActionResult Index()
    {
        return View();
    }
    [Authorize(Permissions.Permissions_Product_View)]
    public async Task<IActionResult> RenderView()
    {
        var paging = new PageRequest().GetPageResponse(Request);
        var result = await _mediator.Send(new GetProductsWithPagingQuery { page = paging.PageIndex, length = paging.Length, searchValue = paging.SearchValue, sortColumn = paging.SortColumnName, sortOrder = paging.SortOrder });

        var jsonData = new { data = result.Items, draw = paging.Draw, recordsFiltered = result.TotalCount, recordsTotal = result.TotalCount };
        return Json(jsonData);
    }

    [Authorize(Permissions.Permissions_Product_Create)]
    public async Task<IActionResult> Create()
    {
        ViewData["CategoryId"] = new SelectList(await _mediator.Send(new GetCategoriesQuery()), "Id", "Name");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Permissions.Permissions_Product_Create)]
    public async Task<IActionResult> Create(CreateProductCommand command)
    {
        ViewData["CategoryId"] = new SelectList(await _mediator.Send(new GetCategoriesQuery()), "Id", "Name");
        if (ModelState.IsValid)
        {
            var response = await _mediator.Send(command);
            if (response.Succeeded) return RedirectToAction(nameof(Index));
            ModelState.AddModelError(string.Empty, response.Message);
        }
        //return Json(new JsonResponse { Success = false, Error = ModelState.Keys.SelectMany(k => ModelState[k].Errors).Select(m => m.ErrorMessage).ToArray() });

        return View(command);
    }
    [Authorize(Permissions.Permissions_Product_Edit)]
    public async Task<IActionResult> Details(int id)
    {
        var response = await _mediator.Send(new GetProductWithVariablesByIdQuery { Id = id });

        var productVariantTheme = typeof(ProductVariantTheme).GetFields(BindingFlags.Static | BindingFlags.Public)
            .Select(o => new { Text = o.GetValue(null).ToString(), Value = o.Name }).ToList();

        if (response == null) return NotFound();

        ViewData["CategoryId"] = new SelectList(await _mediator.Send(new GetCategoriesQuery()), "Id", "Name", response.CategoryId);             
        ViewData["SizeId"] = new SelectList(await _mediator.Send(new GetSizesQuery()), "Id", "Name");
        ViewData["ColorId"] = new SelectList(await _mediator.Send(new GetColorsQuery()), "Id", "Name");
        ViewData["VariableTheme"] = new SelectList(productVariantTheme, "Text", "Text", response.VariableTheme);
        return View(response);
    }

    public async Task<IActionResult> ProductVariablesById(int id)
    {
        var response = await _mediator.Send(new GetProductVariablesByIdQuery { Id = id });
        return Json(response);
    }

    [HttpPost]
    [Authorize(Permissions.Permissions_Product_Edit)]
    public async Task<IActionResult> UpdateProduct(ProductForEditDto data)
    {
        if (data != null)
        {
            var response = await _mediator.Send(new UpdateProductWithVariablesCommand { ProductForEditDto = data });
            if (response.Succeeded) return Json(response.Data);
        }
        return Json(data);
    }
    [AllowAnonymous]
    public async Task<IActionResult> GetSize()
    {
        var res = await _mediator.Send(new GetSizesQuery());
        return Json(res.Select(o => new DropdownSelectVM { Value = o.Id, Text = o.Name }));
    }
    [AllowAnonymous]
    public async Task<IActionResult> GetColor()
    {
        var res = await _mediator.Send(new GetColorsQuery());
        return Json(res.Select(o => new DropdownSelectVM { Value = o.Id, Text = o.Name }));
    }
    [AllowAnonymous]
    [Route("[controller]/[action]/{searchValue}")]
    [HttpGet]
    public async Task<IActionResult> ProductSearch(string searchValue)
    {
        var response = await _mediator.Send(new GetProductsBySearchQuery { SearchValue = searchValue, MaxResult = 8 });
        return Json(response);
    }

    [HttpPost]
    [Authorize(Permissions.Permissions_Product_Delete)]
    public async Task<IActionResult> Delete(int id)
    {
        var response = await _mediator.Send(new DeleteProductCommand { Id = id });
        return Json(response);
    }
}
