using AeroTech.Framework.Core.Domain.Exceptions;
using Xunit;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;

public static class BusinessAssert
{
    public static async Task<BusinessException> ThrowsAsync(int code, int httpStatus, Func<Task> request)
    {
        var exception = await Assert.ThrowsAsync<BusinessException>(request);

        Assert.Equal(code, exception.Code);
        Assert.Equal(httpStatus, exception.HttpStatus);

        return exception;
    }
}
