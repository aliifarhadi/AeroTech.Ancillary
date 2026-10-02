using AeroTech.Framework.Core.Domain.Exceptions;
using Xunit;

namespace AeroTech.Ancillary.Domain.ConformanceTests.Fixtures;

public static class BusinessAssert
{
    public static void Throws(int code, int httpStatus, Action action)
    {
        var exception = Assert.Throws<BusinessException>(action);

        Assert.Equal(code, exception.Code);
        Assert.Equal(httpStatus, exception.HttpStatus);
    }
}
