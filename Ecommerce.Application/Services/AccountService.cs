using AutoMapper;
using Ecommerce.Application.Common;
using Ecommerce.Application.Dto;
using Ecommerce.Application.Identity;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Common;
using Ecommerce.Domain.Identity.Constants;
using Ecommerce.Domain.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.Services;

public class AccountService : IAccountService
{
    private readonly IApplicationUserManager _userManager;
    private readonly IApplicationSignInManager _signInManager;
    private readonly IApplicationRoleManager _roleManager;
    private readonly IMapper _mapper;
    private readonly IKeyAccessor _keyAccessor;
    private readonly IDataContext _db;
    private readonly ICurrentUser _currentUser;


    public AccountService(IApplicationUserManager userManager,
                            IApplicationSignInManager signInManager,
                            IApplicationRoleManager roleManager,
                            IMapper mapper,
                            IKeyAccessor keyAccessor, IDataContext db, ICurrentUser currentUser)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _mapper = mapper;
        _keyAccessor = keyAccessor;
        _db = db;
        _currentUser = currentUser;
    }
    public async Task<Response<UserIdentityDto>> RegisterUserAsync(RegisterUserDto registerUserDto)
    {
        var user = _mapper.Map<ApplicationUser>(registerUserDto);
        var rs = await _userManager.RegisterUserAsync(user);
        return rs.Succeeded
            ? Response<UserIdentityDto>.Success(new UserIdentityDto { Id = user.Id }, rs.ToString())
            : Response<UserIdentityDto>.Fail(rs.ToString());
    }

    public async Task<Response<UserIdentityDto>> SignInAsync(LoginUserDto loginUserDto)
    {
        var user = await _userManager.GetUserByNameAsync(loginUserDto.UserName);
        if (user == null) return Response<UserIdentityDto>.Fail(new UserIdentityDto { RequiresTwoFactor = false }, "Username does not exists");
        if (user.IsActive == false)
        {
            //return Response<UserIdentityDto>.Fail("Sorry you can't signin.");
            return Response<UserIdentityDto>.Fail(new UserIdentityDto { RequiresTwoFactor = false }, "Sorry you can't signin");
        }

        var conAdv = _keyAccessor?["AdvancedConfiguration"] != null ? JsonSerializer.Deserialize<AdvancedConfigurationDto>(_keyAccessor["AdvancedConfiguration"]) : new AdvancedConfigurationDto();
        var conSec = _keyAccessor?["SecurityConfiguration"] != null ? JsonSerializer.Deserialize<SecurityConfigurationDto>(_keyAccessor["SecurityConfiguration"]) : new SecurityConfigurationDto();
        var isTwoFactorEnabled = conAdv?.EnableTwoFactorAuthentication;
        var userRoles = _userManager.GetRolesAsync(user);
        var isSuperAdmin = userRoles.Result.Contains(DefaultApplicationRoles.SuperAdmin.ToString());

        if (isTwoFactorEnabled == false || isSuperAdmin == true)
        {
            user.TwoFactorEnabled = false;
        }

        bool isEmailConfirmed = false;
        if (conAdv.EnableEmailConfirmation)
        {
            isEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user);
            if (!isEmailConfirmed) return Response<UserIdentityDto>.Fail(new UserIdentityDto { Id = user.Id, IsEmailConfirmed = false }, "Email are not Confirmed!");
        }

        var rs = await _signInManager.PasswordSignInAsync(user, loginUserDto.Password, loginUserDto.RememberMe, conSec.IsUserLockoutEnabled);

        if (rs.Succeeded)
        {
            var profilePic = await _db.Galleries.Where(c => c.Id == user.ProfilePicture).FirstOrDefaultAsync();

            var fullName = new Claim("FullName", user.FullName.ToString());
            if (profilePic?.Name != null)
            {
                await _userManager.AddClaimAsync(user, new Claim("ProfilePic", profilePic.Name.ToString()));
            }
            await _userManager.AddClaimAsync(user, fullName);
            return Response<UserIdentityDto>.Success(new UserIdentityDto { Id = user.Id }, rs.ToString());
        }
        return Response<UserIdentityDto>.Fail(new UserIdentityDto { Id = user.Id, RequiresTwoFactor = rs.Data.RequiresTwoFactor, IsLockedOut = rs.Data.IsLockedOut, IsEmailConfirmed = isEmailConfirmed }, rs.ToString());
    }

    public async Task<Response<UserIdentityDto>> TwoFactorSignInAsync(TwoStepDto twoStepDto)
    {
        var conSec = _keyAccessor?["SecurityConfiguration"] != null ? JsonSerializer.Deserialize<SecurityConfigurationDto>(_keyAccessor["SecurityConfiguration"]) : new SecurityConfigurationDto();
        var user = await _userManager.GetUserByNameAsync(twoStepDto.UserName);
        var rs = await _signInManager.TwoFactorSignInAsync(user, "Email", twoStepDto.TwoFactorCode, twoStepDto.RememberMe, conSec.IsUserLockoutEnabled);
        return rs.Succeeded
            ? Response<UserIdentityDto>.Success(rs.ToString())
            : Response<UserIdentityDto>.Fail(rs.ToString());
    }

    public async Task<Response<UserIdentityDto>> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
    {
        var user = await _userManager.FindByEmailAsync(resetPasswordDto.Email);
        if (user == null) return Response<UserIdentityDto>.Fail(new UserIdentityDto { RequiresTwoFactor = false }, "User does not exists");

        var rs = await _signInManager.ResetPasswordAsync(user, resetPasswordDto.Token, resetPasswordDto.Password);
        return rs.Succeeded
            ? Response<UserIdentityDto>.Success(rs.ToString())
            : Response<UserIdentityDto>.Fail(rs.ToString());
    }
    public async Task<Response<UserIdentityDto>> ConfirmEmailAsync(string email, string token)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null) return Response<UserIdentityDto>.Fail(new UserIdentityDto { RequiresTwoFactor = false }, "User does not exists");
        var rs = await _userManager.ConfirmEmailAsync(user, token);
        return rs.Succeeded
            ? Response<UserIdentityDto>.Success(rs.ToString())
            : Response<UserIdentityDto>.Fail(rs.ToString());
    }


    public async Task SignOutAsync()
    {
        // Remove all claims from the user
        var user = await _userManager.GetUserByIdAsync(_currentUser.UserId);
        var claims = await _userManager.GetClaimsAsync(user);
        await _userManager.RemoveClaimsAsync(user, claims.ToList());

        await _signInManager.SignOutAsync();
    }
}
