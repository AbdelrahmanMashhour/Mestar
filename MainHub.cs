using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using RepositoryPatternWithUOW.Core.Interfaces;
using RepositoryPatternWithUOW.Core.Models;
using RepositoryPatternWithUOW.EF;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Mestar
{
    [Authorize(Roles ="Student")]
    public class MainHub(IUnitOfWork unitOfWork,AppDbContext Appcontext):Hub
    {
        public override async Task OnConnectedAsync()
        {
           // await Appcontext.Set<UserConnection>().ExecuteDeleteAsync();
            var jwt =new JwtSecurityTokenHandler().ReadJwtToken( Context.GetHttpContext().Request.Cookies["accessToken"]);
            int id=int.Parse( jwt.Payload[ClaimTypes.NameIdentifier].ToString()!);
           // var user=await unitOfWork.StudentRepository.FindAsync(x=>x.UserId == id);   
            if (await unitOfWork.StudentRepository.IsExist(x => x.UserId == id && x.UserConnection == null))
            {
                await unitOfWork.UserConnection.AddAsync(new() { ConnectionId = Context.ConnectionId, StudentId = id ,RequestedToVideo=false});
                await unitOfWork.SaveChangesAsync();
            }
            else
            {
                Context.Abort();
                return;
            }
            await base.OnConnectedAsync();

        }
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await unitOfWork.UserConnection.ExecuteDeleteAsync(x => x.ConnectionId == Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }
    }
}
