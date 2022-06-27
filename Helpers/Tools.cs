namespace ApiVrEdu.Helpers;

public static class Tools
{
    public static T LoopToUpdateObject<T, TDto>(T entity, TDto dto, string[] except)
    {
        if (dto is null) return entity;
        foreach (var prop in dto.GetType().GetProperties())
        {
            if (except.Contains(prop.Name.ToLower())) continue;
            var val = prop.GetValue(dto, null);
            if (val is null or "") continue;
            var type = entity?.GetType();
            var pr = type?.GetProperty(prop.Name);
            pr?.SetValue(entity, val);

            if (dto.GetType().GetProperties().Length <= 1 ||
                !dto.GetType().GetProperties().Last().Equals(prop)) continue;
        }

        return entity;
    }
}