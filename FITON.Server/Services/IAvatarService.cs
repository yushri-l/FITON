using FITON.Server.Models;

namespace FITON.Server.Services
{
    public interface IAvatarService
    {
        Task<Avatar?> GenerateAvatarFromMeasurementsAsync(int userId, Measurement measurements);
        Task<bool> DeleteAvatarAsync(int userId);
        Task<Avatar?> RegenerateAvatarAsync(int userId);
        Task<Avatar?> GetUserAvatarAsync(int userId);
    }
}