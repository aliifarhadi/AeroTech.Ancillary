using AeroTech.Ancillary.Application.ServiceSubCodeAggregate.Projection;
using AeroTech.Ancillary.Domain.ServiceSubCodeAggregate;
using AeroTech.Ancillary.Domain.ServiceSubCodeAggregate.Contracts;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Repository;
using AeroTech.Framework.Core.ServiceContracts;

namespace AeroTech.Ancillary.Application.ServiceSubCodeAggregate.Commands.RegisterServiceSubCode
{
    public sealed class RegisterServiceSubCodeService : IRegisterServiceSubCodeService
    {
        private readonly IServiceSubCodeRepository _subCodes;
        private readonly IServiceSubCodeQueryDbSynchronizer _synchronizer;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IIdGenerator _idGenerator;
        private readonly IClock _clock;

        public RegisterServiceSubCodeService(
            IServiceSubCodeRepository subCodes,
            IServiceSubCodeQueryDbSynchronizer synchronizer,
            IUnitOfWork unitOfWork,
            IIdGenerator idGenerator,
            IClock clock)
        {
            _subCodes = subCodes;
            _synchronizer = synchronizer;
            _unitOfWork = unitOfWork;
            _idGenerator = idGenerator;
            _clock = clock;
        }

        public async Task<ServiceSubCodeResult> RegisterAsync(IRegisterServiceSubCodeCommand command, CancellationToken cancellationToken = default)
        {
            var subCode = ServiceSubCode.Register(
                _idGenerator.NewId(),
                command.OwnerAirlineId,
                command.Code,
                command.Rfic,
                command.GroupCode,
                command.SubGroupCode,
                command.Description1Code,
                command.Description2Code,
                command.CommercialName,
                _clock.GetDateTime());

            if (await _subCodes.FindAsync(subCode.OwnerAirlineId, subCode.Code, cancellationToken) is not null)
                throw ExceptionFactory.ServiceSubCodeAlreadyRegistered();

            await _subCodes.AddAsync(subCode, cancellationToken);
            await _synchronizer.ProjectAsync(subCode.ToReadModelSnapshot(), cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new ServiceSubCodeResult(subCode.Id, subCode.OwnerAirlineId, subCode.Code, subCode.Source, subCode.Status);
        }
    }
}
