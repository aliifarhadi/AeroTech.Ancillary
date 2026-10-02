namespace AeroTech.Ancillary.Application.ServiceSubCodeAggregate.Commands.RegisterServiceSubCode
{
    public interface IRegisterServiceSubCodeCommand
    {
        int OwnerAirlineId { get; }

        string Code { get; }

        string? Rfic { get; }

        string? GroupCode { get; }

        string? SubGroupCode { get; }

        string? Description1Code { get; }

        string? Description2Code { get; }

        string? CommercialName { get; }
    }
}
