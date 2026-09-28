using AeroTech.Framework.Core.Domain.Exceptions;

namespace AeroTech.Ancillary.Domain._Shared.Resources
{
    public static class ExceptionFactory
    {
        public static BusinessException CallerHasNoCustomerContext(params object?[] args) =>
            new(16001, ExceptionMessages.CallerHasNoCustomerContext, args) { HttpStatus = 403 };

        public static BusinessException CountryCodeIsNotRecognised(params object?[] args) =>
            new(16002, ExceptionMessages.CountryCodeIsNotRecognised, args) { HttpStatus = 422 };

        public static BusinessException CallerPrincipalTypeIsNotRecognised(params object?[] args) =>
            new(16003, ExceptionMessages.CallerPrincipalTypeIsNotRecognised, args) { HttpStatus = 403 };

        public static BusinessException CallerContextTypeIsNotRecognised(params object?[] args) =>
            new(16004, ExceptionMessages.CallerContextTypeIsNotRecognised, args) { HttpStatus = 403 };
    }
}
