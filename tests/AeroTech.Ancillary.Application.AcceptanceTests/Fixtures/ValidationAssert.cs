using FluentValidation;
using Xunit;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;

public static class ValidationAssert
{
    public static void Accepts<T>(IValidator<T> validator, T request) => validator.ValidateAndThrow(request);

    public static void Rejects<T>(IValidator<T> validator, T request)
        => Assert.Throws<ValidationException>(() => validator.ValidateAndThrow(request));
}
