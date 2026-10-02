namespace AeroTech.Ancillary.Domain._Shared.Resources
{
    public static class ExceptionMessages
    {
        public const string CallerHasNoCustomerContext = "The caller has no customer context.";
        public const string CountryCodeIsNotRecognised = "Country code {0} is not recognised.";
        public const string CallerPrincipalTypeIsNotRecognised = "Caller principal type {0} is not recognised.";
        public const string CallerContextTypeIsNotRecognised = "Caller context type {0} is not recognised.";
        public const string AncillaryProductNotFound = "Product not found.";
        public const string AncillaryProductReferenceAlreadyExists = "Product reference already exists for this airline.";
        public const string AncillaryProductIsNotDraft = "Product is not a Draft and cannot be changed.";
        public const string AncillaryProductStatusChangeNotAllowed = "Product status change not allowed.";
        public const string AncillaryProductIsInvalid = "Product is invalid: {0}.";
        public const string AncillaryProductDraftAlreadyExists = "A Draft of this product already exists.";
        public const string AncillaryProductVersionCannotBeRevised = "This version cannot be revised.";
        public const string AncillaryProductSubCodeIsNotActive = "The product's sub code is not an Active sub code of the airline.";
        public const string ServiceSubCodeNotFound = "Sub code not found.";
        public const string ServiceSubCodeAlreadyRegistered = "This code is already registered for the airline.";
        public const string ServiceSubCodeIsInvalid = "Sub code is invalid: {0}.";
        public const string ServiceSubCodeStatusChangeNotAllowed = "Sub code status change not allowed.";
        public const string IndustrySubCodeNotInReference = "Industry sub code is not in the industry reference.";
        public const string AncillaryPriceRuleNotFound = "Price rule not found.";
        public const string AncillaryPriceRuleIsNotDraft = "Price rule is not a Draft and cannot be changed.";
        public const string AncillaryPriceRuleStatusChangeNotAllowed = "Price rule status change not allowed.";
        public const string AncillaryPriceRulePriorityAlreadyTaken = "Another Active rule has this priority.";
        public const string AncillaryPriceRuleProductDoesNotExist = "The rule's product does not exist.";
        public const string AncillaryPriceRuleIsInvalid = "Price rule is invalid: {0}.";
        public const string AncillaryQuoteRequestIsInconsistent = "Quote request is inconsistent: {0}.";
        public const string AncillaryQuoteSelectedProductNotFound = "Selected product not found.";
        public const string AncillaryQuoteSelectionDoesNotFitScope = "Selection does not fit the product's scope.";
        public const string AncillaryQuoteReferenceNotInRequest = "A reference is not in the request: {0}.";
        public const string AncillaryQuoteOccurrenceNotApplicable = "Selected occurrence is not applicable.";
        public const string AncillaryQuoteQuantityNotAllowed = "Quantity not allowed.";
        public const string AncillaryQuoteOccurrenceSelectedTwice = "Occurrence selected twice.";
        public const string AncillaryQuoteSelectionNoLongerCurrent = "The selection is no longer current (product version or price rule changed).";
    }
}
