using System.Threading.Tasks;

namespace FITON.Server.Services
{
 public interface IImageGenerator
 {
 /// <summary>
 /// Generate an image from a text prompt and return a data URL (e.g. "data:image/png;base64,...").
 /// </summary>
 Task<string> GenerateImageAsync(string prompt);
 }
}