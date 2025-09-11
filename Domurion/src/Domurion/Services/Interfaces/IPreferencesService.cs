using Domurion.Models;

namespace Domurion.Services.Interfaces
{
    public interface IPreferencesService
    {
        UserPreferences GetPreferences(Guid userId);
        UserPreferences UpdatePreferences(Guid userId, UserPreferences updated);
    }
}
