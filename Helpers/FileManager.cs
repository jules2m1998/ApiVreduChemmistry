namespace ApiVrEdu.Helpers;

public static class FileManager
{
    private static readonly string MyPath = Path.Combine(new[]
    {
        "wwwroot",
        "images"
    });

    private static readonly string MyPathOther = Path.Combine("wwwroot");

    private static readonly string[] ImgExt =
    {
        "png",
        "jpg",
        "jpeg"
    };

    public static async Task<string?> CreateFile(IFormFile image, string name, IWebHostEnvironment env,
        string[] outFile, IEnumerable<string>? imgExt = null, bool isImg = true)
    {
        var fileExtension = Path.GetExtension(image.FileName);
        var removedExt = fileExtension.Replace(".", "");
        var isContain = imgExt?.Contains(removedExt) ?? ImgExt.Contains(removedExt);

        if (!isContain)
            throw new Exception(
                $"Le format {removedExt} n'est pas supporte pour une photo de profile veillez selectonner un autre fichier");

        var uuidPath = name + "_" + Guid.NewGuid() + fileExtension;
        var fileOut = Path.Combine(outFile);
        var directory = Path.Combine(new[]
        {
            env.ContentRootPath,
            isImg ? MyPath : MyPathOther,
            fileOut
        });
        Directory.CreateDirectory(directory);
        var filePath = Path.Combine(new[]
        {
            directory,
            uuidPath
        });
        await using (Stream fileStream = new FileStream(filePath, FileMode.Create))
        {
            await image.CopyToAsync(fileStream);
        }

        return Path.Combine(new[]
        {
            isImg ? "images" : string.Empty,
            fileOut,
            uuidPath
        });
    }

    public static void DeleteFile(string filename, IWebHostEnvironment env)
    {
        var filePath = Path.Combine(new[]
        {
            env.ContentRootPath,
            "wwwroot",
            filename
        });

        if (File.Exists(filePath)) File.Delete(filePath);
    }
}