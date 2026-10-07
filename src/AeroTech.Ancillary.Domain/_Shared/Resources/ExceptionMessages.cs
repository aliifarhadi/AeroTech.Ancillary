namespace AeroTech.Ancillary.Domain._Shared.Resources
{
    public static class ExceptionMessages
    {
        public const string CallerHasNoCustomerContext = "The caller has no customer context.";
        public const string CountryCodeIsNotRecognised = "Country code {0} is not recognised.";
        public const string CallerPrincipalTypeIsNotRecognised = "Caller principal type {0} is not recognised.";
        public const string CallerContextTypeIsNotRecognised = "Caller context type {0} is not recognised.";
        public const string SupplierNotFound = "The supplier was not found.";
        public const string SupplierIsInvalid = "Supplier field {0} is invalid.";
        public const string SupplierStatusChangeNotAllowed = "The supplier status does not allow this change.";
        public const string SupplierFulfillmentProviderNotRegistered = "Supplier fulfillment provider {0} is not registered.";
        public const string ServiceDefinitionNotFound = "The service definition was not found.";
        public const string ServiceDefinitionIsInvalid = "Service definition field {0} is invalid.";
        public const string ServiceDefinitionStatusChangeNotAllowed = "The service definition status does not allow this change.";
        public const string ServiceDefinitionRefAlreadyActive = "An active service definition already exists for reference {0}.";
        public const string ServiceDefinitionSupplierNotFound = "The referenced supplier was not found.";
        public const string ServiceDefinitionSupplierNotActive = "The referenced supplier is not active.";
        public const string IndustryServiceSubCodeNotFound = "Industry service sub code {0} is not in the reference dataset.";
        public const string IndustryServiceSubCodeSemanticsConflict = "Field {0} contradicts the industry reference entry for sub code {1}.";
        public const string ProvisionNotFound = "The provision was not found.";
        public const string ProvisionIsInvalid = "Provision field {0} is invalid.";
        public const string ProvisionStatusChangeNotAllowed = "The provision status does not allow this change.";
        public const string ProvisionServiceDefinitionNotFound = "The referenced service definition was not found.";
        public const string ProvisionFeeApplicationUnitNotSupported = "Fee application unit {0} is not supported for activation.";
        public const string ProvisionSequenceAlreadyActive = "An active provision already exists at sequence {0} of the service definition.";
        public const string AncillaryHoldNotFound = "The ancillary hold was not found.";
        public const string AncillaryHoldRequestIsInvalid = "Ancillary hold field {0} is invalid.";
        public const string AncillaryHoldIdempotencyKeyReused = "The idempotency key was already used with a different request.";
        public const string AncillaryHoldServiceDefinitionNotFound = "The service definition was not found for the hold unit.";
        public const string AncillaryHoldProvisionMismatch = "The provision does not belong to the service definition.";
        public const string AncillaryHoldOrderServiceAlreadyReserved = "Order service {0} already has a live reservation.";
        public const string AncillaryHoldHasExpired = "The ancillary hold has expired.";
        public const string AncillaryHoldIsInMixedState = "The ancillary hold units are in a mixed state.";
        public const string AncillaryHoldQuantityNotAllowed = "The requested quantity is outside the provision quantity rule.";
        public const string AncillaryHoldCoverageMismatch = "The hold unit coverage does not match the provision coverage.";
    }
}
