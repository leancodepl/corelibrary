using LeanCode.DomainModels.Ids;

namespace LeanCode.DomainModels.EF;

internal static class TypedIdExtensions
{
    extension<TId>(IHasEmptyId<TId>)
        where TId : struct, IHasEmptyId<TId>
    {
        public static int? GetRawLength()
        {
            if (TId.Empty is not IConstSizeTypedId)
            {
                return null;
            }

            return (int)typeof(TId).GetProperty(nameof(IConstSizeTypedId.RawLength))!.GetValue(null, null)!;
        }

        public static int? GetMaxLength()
        {
            if (TId.Empty is not IMaxLengthTypedId)
            {
                return null;
            }

            return (int)typeof(TId).GetProperty(nameof(IMaxLengthTypedId.MaxLength))!.GetValue(null, null)!;
        }
    }
}
