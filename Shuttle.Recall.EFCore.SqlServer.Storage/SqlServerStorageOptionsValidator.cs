using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;

namespace Shuttle.Recall.EFCore.SqlServer.Storage;

public class SqlServerStorageOptionsValidator : IValidateOptions<SqlServerStorageOptions>
{
    public ValidateOptionsResult Validate(string? name, SqlServerStorageOptions options)
    {
        Guard.AgainstNull(options);

        if (string.IsNullOrWhiteSpace(options.ConnectionStringName))
        {
            return ValidateOptionsResult.Fail(Resources.ConnectionStringOptionException);
        }

        return ValidateOptionsResult.Success;
    }
}