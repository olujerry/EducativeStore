﻿using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Identity.Constants;
using Ecommerce.Domain.Identity.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Ecommerce.Infrastructure.Sql.Repositories;

public sealed class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    private List<Claim> _permissions;
    public CurrentUser(IHttpContextAccessor httpContextAccessor,
                        UserManager<ApplicationUser> userManager,
                        RoleManager<IdentityRole> roleManager)
    {
        _httpContextAccessor = httpContextAccessor;
        _userManager = userManager;
        _roleManager = roleManager;
        UserId = httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        UserName = httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value;
        UserFullName = httpContextAccessor.HttpContext?.User?.FindFirst("FullName")?.Value;
        Roles = httpContextAccessor.HttpContext?.User?.FindAll(ClaimTypes.Role).Select(x => x.Value.ToString()).ToList();
    }
    public string UserId { get; }
    public string UserName { get; }
    public string? UserFullName { get; }
    public IReadOnlyList<string> Roles { get; }
    public async Task<IList<Claim>> Permissions() => await GetPermissions();


    private async Task<IList<Claim>> GetPermissions()
    {
        if (_permissions != null) return _permissions;
        var allUsers = await _userManager.Users.ToListAsync();
        var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext?.User);
        if (user == null)
            return null;
        var userPermissions = await _userManager.GetClaimsAsync(user);
        _permissions = userPermissions.Where(x => x.Type == CustomClaimTypes.Permission).ToList();

        var roleNames = await _userManager.GetRolesAsync(user);
        foreach (var roleName in roleNames)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            var roleClaims = await _roleManager.GetClaimsAsync(role);
            foreach (var roleClaim in roleClaims.Where(x => x.Type == CustomClaimTypes.Permission))
            {
                if (_permissions.Any(x => x.Value == roleClaim.Value) == false)
                {
                    _permissions.Add(roleClaim);
                }
            }
        }
        return _permissions;
    }
}
