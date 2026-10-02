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

        public static BusinessException AncillaryProductNotFound(params object?[] args) =>
            new(16101, ExceptionMessages.AncillaryProductNotFound, args) { HttpStatus = 404 };

        public static BusinessException AncillaryProductReferenceAlreadyExists(params object?[] args) =>
            new(16102, ExceptionMessages.AncillaryProductReferenceAlreadyExists, args) { HttpStatus = 409 };

        public static BusinessException AncillaryProductIsNotDraft(params object?[] args) =>
            new(16103, ExceptionMessages.AncillaryProductIsNotDraft, args) { HttpStatus = 409 };

        public static BusinessException AncillaryProductStatusChangeNotAllowed(params object?[] args) =>
            new(16104, ExceptionMessages.AncillaryProductStatusChangeNotAllowed, args) { HttpStatus = 409 };

        public static BusinessException AncillaryProductCombinationIsNotSold(params object?[] args) =>
            new(16105, ExceptionMessages.AncillaryProductCombinationIsNotSold, args) { HttpStatus = 422 };

        public static BusinessException AncillaryProductIsInvalid(params object?[] args) =>
            new(16106, ExceptionMessages.AncillaryProductIsInvalid, args) { HttpStatus = 422 };

        public static BusinessException AncillaryProductDraftAlreadyExists(params object?[] args) =>
            new(16107, ExceptionMessages.AncillaryProductDraftAlreadyExists, args) { HttpStatus = 409 };

        public static BusinessException AncillaryProductVersionCannotBeRevised(params object?[] args) =>
            new(16108, ExceptionMessages.AncillaryProductVersionCannotBeRevised, args) { HttpStatus = 409 };

        public static BusinessException AncillaryProductSubCodeIsNotActive(params object?[] args) =>
            new(16109, ExceptionMessages.AncillaryProductSubCodeIsNotActive, args) { HttpStatus = 422 };

        public static BusinessException ServiceSubCodeNotFound(params object?[] args) =>
            new(16110, ExceptionMessages.ServiceSubCodeNotFound, args) { HttpStatus = 404 };

        public static BusinessException ServiceSubCodeAlreadyRegistered(params object?[] args) =>
            new(16111, ExceptionMessages.ServiceSubCodeAlreadyRegistered, args) { HttpStatus = 409 };

        public static BusinessException ServiceSubCodeIsInvalid(params object?[] args) =>
            new(16112, ExceptionMessages.ServiceSubCodeIsInvalid, args) { HttpStatus = 422 };

        public static BusinessException ServiceSubCodeStatusChangeNotAllowed(params object?[] args) =>
            new(16113, ExceptionMessages.ServiceSubCodeStatusChangeNotAllowed, args) { HttpStatus = 409 };

        public static BusinessException IndustrySubCodeNotInReference(params object?[] args) =>
            new(16114, ExceptionMessages.IndustrySubCodeNotInReference, args) { HttpStatus = 422 };

        public static BusinessException AncillaryPriceRuleNotFound(params object?[] args) =>
            new(16201, ExceptionMessages.AncillaryPriceRuleNotFound, args) { HttpStatus = 404 };

        public static BusinessException AncillaryPriceRuleIsNotDraft(params object?[] args) =>
            new(16202, ExceptionMessages.AncillaryPriceRuleIsNotDraft, args) { HttpStatus = 409 };

        public static BusinessException AncillaryPriceRuleStatusChangeNotAllowed(params object?[] args) =>
            new(16203, ExceptionMessages.AncillaryPriceRuleStatusChangeNotAllowed, args) { HttpStatus = 409 };

        public static BusinessException AncillaryPriceRulePriorityAlreadyTaken(params object?[] args) =>
            new(16204, ExceptionMessages.AncillaryPriceRulePriorityAlreadyTaken, args) { HttpStatus = 409 };

        public static BusinessException AncillaryPriceRuleProductDoesNotExist(params object?[] args) =>
            new(16205, ExceptionMessages.AncillaryPriceRuleProductDoesNotExist, args) { HttpStatus = 422 };

        public static BusinessException AncillaryPriceRuleIsInvalid(params object?[] args) =>
            new(16206, ExceptionMessages.AncillaryPriceRuleIsInvalid, args) { HttpStatus = 422 };

        public static BusinessException AncillaryQuoteRequestIsInconsistent(params object?[] args) =>
            new(16301, ExceptionMessages.AncillaryQuoteRequestIsInconsistent, args) { HttpStatus = 422 };

        public static BusinessException AncillaryQuoteSelectedProductNotFound(params object?[] args) =>
            new(16302, ExceptionMessages.AncillaryQuoteSelectedProductNotFound, args) { HttpStatus = 422 };

        public static BusinessException AncillaryQuoteSelectionDoesNotFitScope(params object?[] args) =>
            new(16303, ExceptionMessages.AncillaryQuoteSelectionDoesNotFitScope, args) { HttpStatus = 422 };

        public static BusinessException AncillaryQuoteReferenceNotInRequest(params object?[] args) =>
            new(16304, ExceptionMessages.AncillaryQuoteReferenceNotInRequest, args) { HttpStatus = 422 };

        public static BusinessException AncillaryQuoteOccurrenceNotApplicable(params object?[] args) =>
            new(16305, ExceptionMessages.AncillaryQuoteOccurrenceNotApplicable, args) { HttpStatus = 422 };

        public static BusinessException AncillaryQuoteQuantityNotAllowed(params object?[] args) =>
            new(16306, ExceptionMessages.AncillaryQuoteQuantityNotAllowed, args) { HttpStatus = 422 };

        public static BusinessException AncillaryQuoteOccurrenceSelectedTwice(params object?[] args) =>
            new(16307, ExceptionMessages.AncillaryQuoteOccurrenceSelectedTwice, args) { HttpStatus = 422 };

        public static BusinessException AncillaryQuoteSelectionNoLongerCurrent(params object?[] args) =>
            new(16309, ExceptionMessages.AncillaryQuoteSelectionNoLongerCurrent, args) { HttpStatus = 409 };
    }
}
