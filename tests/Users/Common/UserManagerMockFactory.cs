using AutoServiceUsers.DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace AutoServiceUsers.Tests.Common;

internal static class UserManagerMockFactory
{
    public static Mock<UserManager<ApplicationUser>> Create()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        return new Mock<UserManager<ApplicationUser>>(
            store.Object,
            null!, null!, null!, null!, null!, null!, null!, null!);
    }
}
