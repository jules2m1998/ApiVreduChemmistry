using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace ApiVrEdu.Exceptions;

public static class ExceptionCatcher
{
    public static string CatchUniqueViolation(DbUpdateException ex, UniqueViolation violation)
    {
        if (ex.InnerException is not PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } npgex)
            return ex.Message;
        var constraintName = npgex.ConstraintName;
        if (constraintName != null && constraintName.ToLower().Contains(violation.Name))
            return violation.Msg;
        return "Cette element existe !";
    }
}