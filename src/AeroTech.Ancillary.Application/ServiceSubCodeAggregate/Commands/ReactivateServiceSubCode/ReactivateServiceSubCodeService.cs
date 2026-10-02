using AeroTech.Ancillary.Application.ServiceSubCodeAggregate.Commands.RegisterServiceSubCode;
using AeroTech.Ancillary.Application.ServiceSubCodeAggregate.Projection;
using AeroTech.Ancillary.Domain.ServiceSubCodeAggregate.Contracts;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Repository;

namespace AeroTech.Ancillary.Application.ServiceSubCodeAggregate.Commands.ReactivateServiceSubCode
{
    public sealed class ReactivateServiceSubCodeService : IReactivateServiceSubCodeService
    {
        private readonly IServiceSubCodeRepository _subCodes;
        private readonly IServiceSubCodeQueryDbSynchronizer _synchronizer;
        private readonly IUnitOfWork _unitOfWork;

        public ReactivateServiceSubCodeService(
            IServiceSubCodeRepository subCodes,
            IServiceSubCodeQueryDbSynchronizer synchronizer,
            IUnitOfWork unitOfWork)
        {
            _subCodes = subCodes;
            _synchronizer = synchronizer;
            _unitOfWork = unitOfWork;
        }

        public async Task<ServiceSubCodeResult> ReactivateAsync(IReactivateServiceSubCodeCommand command, CancellationToken cancellationToken = default)
        {
            var subCode = await _subCodes.GetAsync(command.ServiceSubCodeId, cancellationToken)
                          ?? throw ExceptionFactory.ServiceSubCodeNotFound();

            subCode.Reactivate();

            await _synchronizer.ProjectAsync(subCode.ToReadModelSnapshot(), cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new ServiceSubCodeResult(subCode.Id, subCode.OwnerAirlineId, subCode.Code, subCode.Source, subCode.Status);
        }
    }
}
