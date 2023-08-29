using System.Reflection;
using System.Resources;

namespace Api;

public class ResourcesHelper
{
    public static async Task<byte[]> GetResourceBytes(string fileName)
    {
        var type = typeof(ResourcesHelper);
        var assembly = type.GetTypeInfo().Assembly;
        var fullFileName = $"{type.Namespace}.{fileName}";
        var stream = assembly.GetManifestResourceStream(fullFileName) ??
                     throw new MissingManifestResourceException(fullFileName);
        var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        return memoryStream.ToArray();
    }
}