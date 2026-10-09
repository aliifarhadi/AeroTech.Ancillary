using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.Domain._Shared.Resources;

namespace AeroTech.Ancillary.Application._Shared.Authorization
{
    public sealed class InventoryCallerScope : IInventoryCallerScope
    {
        private readonly ICallerContext _callerContext;
        private readonly IOperatorAirlineResolver _operatorAirline;

        public InventoryCallerScope(ICallerContext callerContext, IOperatorAirlineResolver operatorAirline)
        {
            _callerContext = callerContext;
            _operatorAirline = operatorAirline;
        }

        public async Task<int> RequireOwnerAirlineIdAsync(CancellationToken cancellationToken = default)
        {
            if (_callerContext.ContextType != CallerContextType.Airline)
                throw ExceptionFactory.InventoryOwnerNotAuthorized();

            var homeAirlineId = await _operatorAirline.FindHomeAirlineIdAsync(cancellationToken);

            return homeAirlineId is > 0 and <= int.MaxValue
                ? (int)homeAirlineId.Value
                : throw ExceptionFactory.InventoryOwnerNotAuthorized();
        }

        public async Task EnsureOwnerAsync(int ownerAirlineId, CancellationToken cancellationToken = default)
        {
            if (await RequireOwnerAirlineIdAsync(cancellationToken) != ownerAirlineId)
                throw ExceptionFactory.InventoryOwnerNotAuthorized();
        }

        public long RequireActorId()
            => _callerContext.ActorId > 0 ? _callerContext.ActorId : throw ExceptionFactory.InventoryOwnerNotAuthorized();
    }
}
