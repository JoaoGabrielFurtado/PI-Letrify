using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace pi_projetolivros.Servicos;

public class CloudinaryService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryService(IConfiguration configuration)
    {
        var account = new Account(
            configuration["Cloudinary:CloudName"],
            configuration["Cloudinary:ApiKey"],
            configuration["Cloudinary:ApiSecret"]
        );
        _cloudinary = new Cloudinary(account);
        _cloudinary.Api.Secure = true;
    }

    public async Task<string> UploadFotoPerfilAsync(IFormFile foto)
        => await UploadAsync(foto, "fotos-perfil");

    public async Task<string> UploadFotoGrupoAsync(IFormFile foto)
        => await UploadAsync(foto, "fotos-grupos");

    public async Task DeletarAsync(string? url)
    {
        if (string.IsNullOrEmpty(url)) return;

        // Ignora URLs relativas antigas (ex: /fotos/arquivo.jpg)
        // que foram salvas antes da migração para o Cloudinary
        if (!Uri.IsWellFormedUriString(url, UriKind.Absolute)) return;

        // Verifica se é uma URL do Cloudinary de fato
        if (!url.Contains("cloudinary.com")) return;

        var uri = new Uri(url);
        var segmentos = uri.AbsolutePath.Split('/');
        var uploadIdx = Array.IndexOf(segmentos, "upload");

        if (uploadIdx < 0 || uploadIdx + 2 >= segmentos.Length) return;

        var publicId = string.Join("/",
            segmentos[(uploadIdx + 2)..])
            .Replace(Path.GetExtension(url), "");

        var deleteParams = new DeletionParams(publicId);
        await _cloudinary.DestroyAsync(deleteParams);
    }

    private async Task<string> UploadAsync(IFormFile foto, string pasta)
    {
        using var stream = foto.OpenReadStream();

        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(foto.FileName, stream),
            Folder = pasta,
            Transformation = new Transformation().Quality("auto").FetchFormat("auto")
        };

        var resultado = await _cloudinary.UploadAsync(uploadParams);
        return resultado.SecureUrl.ToString();
    }
}