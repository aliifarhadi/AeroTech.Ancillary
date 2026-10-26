using AeroTech.Framework.Core.Domain.Exceptions;

namespace AeroTech.Ancillary.Query.Shopping.Common
{
    public static class SelectionBatch
    {
        public static async Task<T> ItemAsync<T>(string member, int index, Func<Task<T>> action)
        {
            try
            {
                return await action();
            }
            catch (BusinessException exception)
            {
                throw new BusinessException(exception.Code, $"{member}[{index}]: {exception.Message}") { HttpStatus = exception.HttpStatus };
            }
        }
    }
}
