using Domurion.Services.Interfaces;
using Domurion.Models;
using Domurion.Data;

namespace Domurion.Services
{
    public class PreferencesService(DataContext context) : IPreferencesService
    {
        private readonly DataContext _context = context;

        public UserPreferences GetPreferences(Guid userId)
        {
            var prefs = _context.UserPreferences.FirstOrDefault(p => p.UserId == userId);
            if (prefs == null)
            {
                prefs = new UserPreferences { UserId = userId };
                _context.UserPreferences.Add(prefs);
                _context.SaveChanges();
            }
            return prefs;
        }

        public UserPreferences UpdatePreferences(Guid userId, UserPreferences updated)
        {
            var prefs = _context.UserPreferences.FirstOrDefault(p => p.UserId == userId);
            if (prefs == null)
            {
                updated.UserId = userId;
                _context.UserPreferences.Add(updated);
                _context.SaveChanges();
                return updated;
            }
            prefs.PasswordLength = updated.PasswordLength;
            prefs.UseUppercase = updated.UseUppercase;
            prefs.UseLowercase = updated.UseLowercase;
            prefs.UseNumbers = updated.UseNumbers;
            prefs.UseSymbols = updated.UseSymbols;
            prefs.AutoSaveEntries = updated.AutoSaveEntries;
            prefs.ShowPasswordStrength = updated.ShowPasswordStrength;
            prefs.SessionTimeoutMinutes = updated.SessionTimeoutMinutes;
            prefs.AutoLockEnabled = updated.AutoLockEnabled;
            prefs.LoginNotificationsEnabled = updated.LoginNotificationsEnabled;
            _context.SaveChanges();
            return prefs;
        }
    }
}
