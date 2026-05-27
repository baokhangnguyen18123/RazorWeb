using System.Security.Claims;
using App.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace App.Security.Requirements;

public class AppAuthorizationHandler : IAuthorizationHandler
{
    private readonly ILogger<AppAuthorizationHandler> _logger;
    private readonly UserManager<AppUser> _userManager; 
    public AppAuthorizationHandler(ILogger<AppAuthorizationHandler> logger, UserManager<AppUser> userManager)
    {
        _logger = logger;
        _userManager = userManager; 
    } 
    public Task HandleAsync(AuthorizationHandlerContext context)
    {
        var requirements = context.PendingRequirements.ToList();
        _logger.LogInformation("Xử lý yêu cầu ủy quyền cho tài nguyên: {ResourceName}", context.Resource?.GetType().Name);
        foreach ( var r in requirements){
            if(r is GenZRequirements){
                // đoạn code kiểm tra user
                if(IsGenZ(context.User,r as GenZRequirements)){
                
                    context.Succeed(r);
                }


            }
            if (r is ArticleUpdateRequirement)
            {
                bool canUpdate = CanUpdateArticle(context.User,context.Resource, (ArticleUpdateRequirement)r);
            }
            // if (r is OtherRequirements){
            //     // đoạn code kiểm tra user
            //     context.Succeed(r);
            // }
        }
        return Task.CompletedTask;
    }

    private bool CanUpdateArticle(ClaimsPrincipal user, object? resource, ArticleUpdateRequirement r)
    {
        if (user.IsInRole("Admin"))
        {
            _logger.LogInformation("Admin cập nhật ...");
            return true;
        }
        var article = resource as Article;
        var dateCreated = article.Created;
        var dateCanUpdate = new DateTime(r.Year, r.Month, r.Day);
        if (dateCreated < dateCanUpdate)
        {
            _logger.LogInformation("Quá ngày để cập nhật bài viết");
            return false;
        }
        return true;
        
    }

    private bool IsGenZ(ClaimsPrincipal user, GenZRequirements requirement )
    {
        var appUseTask = _userManager.GetUserAsync(user);
        Task.WaitAll(appUseTask);
        var appUser = appUseTask.Result;
        if (appUser.BirthDate == null)
        {
            _logger.LogInformation("Người dùng không có ngày sinh. {UserName}", appUser.UserName);
            return false;
        }
        int yearOB =appUser.BirthDate.Value.Year;
        var success = (yearOB >= requirement.FromYear && yearOB <= requirement.ToYear);
        if (success)
        {
            _logger.LogInformation("Người dùng {UserName} thuộc thế hệ Gen Z.", appUser.UserName);
        }
        else
        {
            _logger.LogInformation("Người dùng {UserName} không thuộc thế hệ Gen Z.", appUser.UserName);
        }
        return success;
    }
}
