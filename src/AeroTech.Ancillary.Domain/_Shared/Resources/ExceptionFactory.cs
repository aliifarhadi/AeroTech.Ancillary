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

        public static BusinessException ConcurrentChangeDetected(params object?[] args) =>
            new(16005, ExceptionMessages.ConcurrentChangeDetected, args) { HttpStatus = 409 };

        public static BusinessException SupplierNotFound(params object?[] args) =>
            new(16101, ExceptionMessages.SupplierNotFound, args) { HttpStatus = 404 };

        public static BusinessException SupplierIsInvalid(params object?[] args) =>
            new(16102, ExceptionMessages.SupplierIsInvalid, args) { HttpStatus = 422 };

        public static BusinessException SupplierStatusChangeNotAllowed(params object?[] args) =>
            new(16103, ExceptionMessages.SupplierStatusChangeNotAllowed, args) { HttpStatus = 409 };

        public static BusinessException SupplierFulfillmentProviderNotRegistered(params object?[] args) =>
            new(16104, ExceptionMessages.SupplierFulfillmentProviderNotRegistered, args) { HttpStatus = 422 };

        public static BusinessException ServiceDefinitionNotFound(params object?[] args) =>
            new(16201, ExceptionMessages.ServiceDefinitionNotFound, args) { HttpStatus = 404 };

        public static BusinessException ServiceDefinitionIsInvalid(params object?[] args) =>
            new(16202, ExceptionMessages.ServiceDefinitionIsInvalid, args) { HttpStatus = 422 };

        public static BusinessException ServiceDefinitionStatusChangeNotAllowed(params object?[] args) =>
            new(16203, ExceptionMessages.ServiceDefinitionStatusChangeNotAllowed, args) { HttpStatus = 409 };

        public static BusinessException ServiceDefinitionRefAlreadyActive(params object?[] args) =>
            new(16204, ExceptionMessages.ServiceDefinitionRefAlreadyActive, args) { HttpStatus = 409 };

        public static BusinessException ServiceDefinitionSupplierNotFound(params object?[] args) =>
            new(16205, ExceptionMessages.ServiceDefinitionSupplierNotFound, args) { HttpStatus = 422 };

        public static BusinessException ServiceDefinitionSupplierNotActive(params object?[] args) =>
            new(16206, ExceptionMessages.ServiceDefinitionSupplierNotActive, args) { HttpStatus = 422 };

        public static BusinessException IndustryServiceSubCodeNotFound(params object?[] args) =>
            new(16207, ExceptionMessages.IndustryServiceSubCodeNotFound, args) { HttpStatus = 422 };

        public static BusinessException IndustryServiceSubCodeSemanticsConflict(params object?[] args) =>
            new(16208, ExceptionMessages.IndustryServiceSubCodeSemanticsConflict, args) { HttpStatus = 422 };

        public static BusinessException ServiceDefinitionPricingUnitConflict(params object?[] args) =>
            new(16209, ExceptionMessages.ServiceDefinitionPricingUnitConflict, args) { HttpStatus = 409 };

        public static BusinessException ServiceDefinitionPricingUnitNotAssigned(params object?[] args) =>
            new(16210, ExceptionMessages.ServiceDefinitionPricingUnitNotAssigned, args) { HttpStatus = 409 };

        public static BusinessException ServiceDefinitionPricingUnitAlreadyAssigned(params object?[] args) =>
            new(16211, ExceptionMessages.ServiceDefinitionPricingUnitAlreadyAssigned, args) { HttpStatus = 409 };

        public static BusinessException ServiceDefinitionServiceDateBasisConflict(params object?[] args) =>
            new(16212, ExceptionMessages.ServiceDefinitionServiceDateBasisConflict, args) { HttpStatus = 409 };

        public static BusinessException ServiceDefinitionServiceDateBasisNotAssigned(params object?[] args) =>
            new(16213, ExceptionMessages.ServiceDefinitionServiceDateBasisNotAssigned, args) { HttpStatus = 409 };

        public static BusinessException ServiceDefinitionServiceDateBasisAlreadyAssigned(params object?[] args) =>
            new(16214, ExceptionMessages.ServiceDefinitionServiceDateBasisAlreadyAssigned, args) { HttpStatus = 409 };

        public static BusinessException ProvisionNotFound(params object?[] args) =>
            new(16301, ExceptionMessages.ProvisionNotFound, args) { HttpStatus = 404 };

        public static BusinessException ProvisionIsInvalid(params object?[] args) =>
            new(16302, ExceptionMessages.ProvisionIsInvalid, args) { HttpStatus = 422 };

        public static BusinessException ProvisionStatusChangeNotAllowed(params object?[] args) =>
            new(16303, ExceptionMessages.ProvisionStatusChangeNotAllowed, args) { HttpStatus = 409 };

        public static BusinessException ProvisionServiceDefinitionNotFound(params object?[] args) =>
            new(16304, ExceptionMessages.ProvisionServiceDefinitionNotFound, args) { HttpStatus = 422 };

        public static BusinessException FeeApplicationUnitNotSupported(params object?[] args) =>
            new(16305, ExceptionMessages.FeeApplicationUnitNotSupported, args) { HttpStatus = 422 };

        public static BusinessException ProvisionSequenceAlreadyActive(params object?[] args) =>
            new(16306, ExceptionMessages.ProvisionSequenceAlreadyActive, args) { HttpStatus = 409 };

        public static BusinessException ProvisionServiceDefinitionNotActive(params object?[] args) =>
            new(16307, ExceptionMessages.ProvisionServiceDefinitionNotActive, args) { HttpStatus = 409 };

        public static BusinessException ProvisionConditionNotFound(params object?[] args) =>
            new(16308, ExceptionMessages.ProvisionConditionNotFound, args) { HttpStatus = 404 };

        public static BusinessException ProvisionConditionAlreadyExists(params object?[] args) =>
            new(16309, ExceptionMessages.ProvisionConditionAlreadyExists, args) { HttpStatus = 409 };

        public static BusinessException ProvisionActivePricingRequired(params object?[] args) =>
            new(16310, ExceptionMessages.ProvisionActivePricingRequired, args) { HttpStatus = 409 };

        public static BusinessException ProvisionActivePricingNotAllowed(params object?[] args) =>
            new(16311, ExceptionMessages.ProvisionActivePricingNotAllowed, args) { HttpStatus = 409 };

        public static BusinessException ProvisionQuantityUnitIncompatible(params object?[] args) =>
            new(16312, ExceptionMessages.ProvisionQuantityUnitIncompatible, args) { HttpStatus = 409 };

        public static BusinessException ProvisionRuleContextNotSupported(params object?[] args) =>
            new(16313, ExceptionMessages.ProvisionRuleContextNotSupported, args) { HttpStatus = 409 };

        public static BusinessException ProvisionAdvancePurchaseUnitNotSupported(params object?[] args) =>
            new(16314, ExceptionMessages.ProvisionAdvancePurchaseUnitNotSupported, args) { HttpStatus = 422 };

        public static BusinessException ProvisionRuleUnreachable(params object?[] args) =>
            new(16315, ExceptionMessages.ProvisionRuleUnreachable, args) { HttpStatus = 409 };

        public static BusinessException AncillaryHoldNotFound(params object?[] args) =>
            new(16401, ExceptionMessages.AncillaryHoldNotFound, args) { HttpStatus = 404 };

        public static BusinessException AncillaryHoldRequestIsInvalid(params object?[] args) =>
            new(16402, ExceptionMessages.AncillaryHoldRequestIsInvalid, args) { HttpStatus = 422 };

        public static BusinessException AncillaryHoldIdempotencyKeyReused(params object?[] args) =>
            new(16403, ExceptionMessages.AncillaryHoldIdempotencyKeyReused, args) { HttpStatus = 409 };

        public static BusinessException AncillaryHoldServiceDefinitionNotFound(params object?[] args) =>
            new(16404, ExceptionMessages.AncillaryHoldServiceDefinitionNotFound, args) { HttpStatus = 422 };

        public static BusinessException AncillaryHoldProvisionMismatch(params object?[] args) =>
            new(16405, ExceptionMessages.AncillaryHoldProvisionMismatch, args) { HttpStatus = 422 };

        public static BusinessException AncillaryHoldOrderServiceAlreadyReserved(params object?[] args) =>
            new(16406, ExceptionMessages.AncillaryHoldOrderServiceAlreadyReserved, args) { HttpStatus = 409 };

        public static BusinessException AncillaryHoldHasExpired(params object?[] args) =>
            new(16407, ExceptionMessages.AncillaryHoldHasExpired, args) { HttpStatus = 409 };

        public static BusinessException AncillaryHoldIsInMixedState(params object?[] args) =>
            new(16408, ExceptionMessages.AncillaryHoldIsInMixedState, args) { HttpStatus = 409 };

        public static BusinessException AncillaryHoldQuantityNotAllowed(params object?[] args) =>
            new(16409, ExceptionMessages.AncillaryHoldQuantityNotAllowed, args) { HttpStatus = 422 };

        public static BusinessException AncillaryHoldCoverageMismatch(params object?[] args) =>
            new(16410, ExceptionMessages.AncillaryHoldCoverageMismatch, args) { HttpStatus = 422 };

        public static BusinessException PricingNotFound(params object?[] args) =>
            new(16501, ExceptionMessages.PricingNotFound, args) { HttpStatus = 404 };

        public static BusinessException PricingIsInvalid(params object?[] args) =>
            new(16502, ExceptionMessages.PricingIsInvalid, args) { HttpStatus = 422 };

        public static BusinessException PricingStatusChangeNotAllowed(params object?[] args) =>
            new(16503, ExceptionMessages.PricingStatusChangeNotAllowed, args) { HttpStatus = 409 };

        public static BusinessException PricingProvisionNotFound(params object?[] args) =>
            new(16504, ExceptionMessages.PricingProvisionNotFound, args) { HttpStatus = 422 };

        public static BusinessException PricingProvisionNotPriceable(params object?[] args) =>
            new(16505, ExceptionMessages.PricingProvisionNotPriceable, args) { HttpStatus = 409 };

        public static BusinessException PricingUnitMismatch(params object?[] args) =>
            new(16506, ExceptionMessages.PricingUnitMismatch, args) { HttpStatus = 409 };

        public static BusinessException PricingAlreadyActive(params object?[] args) =>
            new(16507, ExceptionMessages.PricingAlreadyActive, args) { HttpStatus = 409 };

        public static BusinessException PricingSelectorConflict(params object?[] args) =>
            new(16508, ExceptionMessages.PricingSelectorConflict, args) { HttpStatus = 409 };

        public static BusinessException PricingRequiredByActiveProvision(params object?[] args) =>
            new(16509, ExceptionMessages.PricingRequiredByActiveProvision, args) { HttpStatus = 409 };

        public static BusinessException PricingSwitchExpectationFailed(params object?[] args) =>
            new(16510, ExceptionMessages.PricingSwitchExpectationFailed, args) { HttpStatus = 409 };
    }
}
