using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace TransportExpenditureTracker.Helper;

public static class ControllerHelpers
{
    public static Dictionary<string, string[]> GetModelStateErrors(ModelStateDictionary modelState)
    {
        return modelState
            .Where(kv => kv.Value != null && kv.Value.Errors.Count > 0)
            .ToDictionary(
                kv => char.ToLowerInvariant(kv.Key[0]) + kv.Key[1..],
                kv => kv.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
            );
    }
}

