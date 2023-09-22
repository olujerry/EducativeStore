using Ecommerce.Application.Dto;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Identity.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Web.Mvc.Controllers;

[Authorize]
public class GalleryController : Controller
{
    private readonly IMediaService _mediaService;
    public GalleryController(IMediaService mediaService)
    {
        _mediaService = mediaService;
    }
    [Authorize(Permissions.Permissions_Gallery_Manage)]
    public IActionResult Index()
    {
        return View();
    }
    [Route("getpagedmedia")]
    [Route("[controller]/[action]")]
    [HttpGet]
    public async Task<ActionResult> GetMedia(int pageIndex, int pageSize)
    {
        //System.Threading.Thread.Sleep(1000);
        var query = await _mediaService.GetPagedAsync(pageIndex, pageSize);
        return Json(query);
    }

    [Route("getmediabyid/{id}")]
    [Route("[controller]/[action]/{id}")]
    [HttpGet]
    public async Task<ActionResult> GetMediaById(string id)
    {
        var query = await _mediaService.GetByIdAsync(id);
        return Json(query);
    }


    [HttpPost]
    [Route("uploadmedia")]
    [Route("[controller]/[action]/{id}")]
    public async Task<IActionResult> UploadMedia(ICollection<IFormFile> files)
    {
        if (files.Count != 0)
        {
            foreach (var file in files)
            {
                FileUploadDto fileUpload = new()
                {
                    File = file
                };
                await _mediaService.FileUploadAsync(fileUpload);
            }
            return Json("{success}");
        }
        return Json("{No media detected!}");
    }

    [HttpPost]
    [Route("updatemedia")]
    [Route("[controller]/[action]/{id}")]
    public async Task<ActionResult> UpdateMediaInfo(GalleryDto gallery)
    {
        var res = await _mediaService.UpdateAsync(gallery);
        return Json("{success:true}");
    }

    [Route("deletemedia")]
    [Route("[controller]/[action]")]
    [HttpPost]
    public async Task<ActionResult> DeleteMedia(string id)
    {
        try
        {
            var res = await _mediaService.RemoveAsync(id);
            return Json("success");
        }
        catch
        {
            return Json("failed");
        }

    }
}
