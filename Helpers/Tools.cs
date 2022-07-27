using System.Text.RegularExpressions;

namespace ApiVrEdu.Helpers;

public static class Tools
{
    public static T LoopToUpdateObject<T, TDto>(T entity, TDto dto, string[] except)
    {
        if (dto is null) return entity;
        foreach (var prop in dto.GetType().GetProperties())
        {
            if (except.Select(v => v.ToLower()).Contains(prop.Name.ToLower())) continue;
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

    public static async Task<T> LoopToUpdateFile<T, TDto>(T entity, TDto dto, IWebHostEnvironment env, string[] except)
    {
        if (dto is null) return entity;

        foreach (var prop in dto.GetType().GetProperties())
        {
            if (except.Select(v => v.ToLower()).Contains(prop.Name.ToLower())) continue;

            var val = prop.GetValue(dto, null);
            if (val is not IFormFile file) continue;

            var type = entity?.GetType();
            var pr = type?.GetProperty(prop.Name);
            if (pr?.GetValue(entity, null) is string oldPath) FileManager.DeleteFile(oldPath, env);
            var path = await FileManager.CreateFile(file, $"update_{file.Name}", env, new[] { Locations.Texture });
            pr?.SetValue(entity, path);
        }

        return entity;
    }

    public static void LoopToDeleteFiles<T>(T entity, IWebHostEnvironment env)
    {
        foreach (var prop in entity?.GetType().GetProperties()!)
        {
            var val = prop.GetValue(entity, null);
            if (val is not string file) continue;
            var regex = new Regex(@"^(.+)\/([^\/]+)$");
            if (!regex.IsMatch(file)) continue;

            FileManager.DeleteFile(file, env);
        }
    }

    public static class Locations
    {
        public const string Texture = "textures";
        public static readonly string Element = "elements";
        public static readonly string[] Equipment = { "3d", "equipments" };
        public static readonly string User = "users";
    }
}