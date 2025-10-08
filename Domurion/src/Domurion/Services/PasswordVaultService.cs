using Domurion.Models;
using Domurion.Helpers;
using Domurion.Data;
using Domurion.Services.Interfaces;
using System.Linq;

namespace Domurion.Services
{
    public class PasswordVaultService(DataContext context) : IPasswordVaultService
    {
        private readonly DataContext _context = context;

        #region Credntial Management
        public Credential AddCredential(Guid userId, string site, string? siteUrl, string email, string password, string? notes = null, string? ipAddress = null)
        {
            if (string.IsNullOrWhiteSpace(site) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Site, email, and password are required.");

            if (!Helper.IsStrongPassword(password))
                throw new ArgumentException("Password does not meet strength requirements.");

            var encryptedPassword = CryptoHelper.EncryptPassword(password);
            var hmacKey = Environment.GetEnvironmentVariable("HMAC_KEY");
            if (string.IsNullOrWhiteSpace(hmacKey))
                throw new InvalidOperationException("HMAC_KEY environment variable is not set.");
            var integrityHash = Helper.ComputeHmac(encryptedPassword, hmacKey);
            var credential = new Credential
            {
                UserId = userId,
                Site = site,
                SiteUrl = siteUrl ?? string.Empty,
                Email = email,
                EncryptedPassword = encryptedPassword,
                IntegrityHash = integrityHash,
                Notes = notes
            };
            _context.Credentials.Add(credential);
            _context.SaveChanges();
            // Audit log
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            AuditLogger.Log(_context, userId, user?.Email ?? string.Empty, credential.Id, "AddCredential", ipAddress, site);
            return credential;
        }

        public IEnumerable<Credential> GetCredentials(Guid userId)
        {
            return [.. _context.Credentials.Where(c => c.UserId == userId)];
        }

        public Credential? GetById(Guid credentialId)
        {
            return _context.Credentials.FirstOrDefault(c => c.Id == credentialId);
        }

        public string RetrievePassword(Guid credentialId, Guid userId, string? ipAddress = null)
        {
            var hmacKey = Environment.GetEnvironmentVariable("HMAC_KEY");
            if (string.IsNullOrWhiteSpace(hmacKey))
                throw new InvalidOperationException("HMAC_KEY environment variable is not set.");
            var expectedHash = Helper.ComputeHmac(
                _context.Credentials
                    .Where(c => c.Id == credentialId && c.UserId == userId)
                    .Select(c => c.EncryptedPassword)
                    .FirstOrDefault() ?? string.Empty,
                hmacKey);
            var credential = _context.Credentials
                .FirstOrDefault(c => c.Id == credentialId && c.UserId == userId)
                ?? throw new KeyNotFoundException("Credential not found.");
            if (credential.IntegrityHash != expectedHash)
                throw new InvalidOperationException("Data integrity check failed.");
            // Audit log
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            AuditLogger.Log(_context, userId, user?.Email ?? string.Empty, credentialId, "RetrievePassword", ipAddress, credential.Site);
            return CryptoHelper.DecryptPassword(credential.EncryptedPassword);
        }

        public Credential UpdateCredential(Guid credentialId, Guid userId, string? site, string? siteUrl, string? email, string? password, string? notes = null, string? ipAddress = null)
        {
            var credential = _context.Credentials.FirstOrDefault(c => c.Id == credentialId && c.UserId == userId)
                ?? throw new KeyNotFoundException("Credential not found.");

            if (site != null)
                credential.Site = site;
            if (siteUrl != null)
                credential.SiteUrl = siteUrl;
            if (email != null)
                credential.Email = email;
            if (password != null)
            {
                if (!Helper.IsStrongPassword(password))
                    throw new ArgumentException("Password does not meet strength requirements.");
                var encryptedPassword = CryptoHelper.EncryptPassword(password);
                credential.EncryptedPassword = encryptedPassword;
                var hmacKey = Environment.GetEnvironmentVariable("HMAC_KEY");
                if (string.IsNullOrWhiteSpace(hmacKey))
                    throw new InvalidOperationException("HMAC_KEY environment variable is not set.");
                credential.IntegrityHash = Helper.ComputeHmac(encryptedPassword, hmacKey);
            }
            if (notes != null)
                credential.Notes = notes;

            _context.SaveChanges();
            // Audit log
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            AuditLogger.Log(_context, userId, user?.Username ?? string.Empty, credentialId, "UpdateCredential", ipAddress, credential.Site);
            return credential;
        }

        public void DeleteCredential(Guid credentialId, Guid userId, string? ipAddress = null)
        {
            var credential = _context.Credentials.FirstOrDefault(c => c.Id == credentialId && c.UserId == userId)
                ?? throw new KeyNotFoundException("Credential not found.");
            _context.Credentials.Remove(credential);
            _context.SaveChanges();
            // Audit log
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            AuditLogger.Log(_context, userId, user?.Username ?? string.Empty, credentialId, "DeleteCredential", ipAddress, credential.Site);
        }
        #endregion

        #region Credential Sharing
        // Share a credential with another user by username
        public Credential ShareCredential(Guid credentialId, Guid fromUserId, string toUsername, string? ipAddress = null)
        {
            var fromUser = _context.Users.FirstOrDefault(u => u.Id == fromUserId)
                ?? throw new KeyNotFoundException("Sender user not found.");
            var toUser = _context.Users.FirstOrDefault(u => u.Username == toUsername)
                ?? throw new KeyNotFoundException("Recipient user not found.");
            if (fromUser.Id == toUser.Id)
                throw new ArgumentException("Cannot share credential with yourself.");

            var credential = _context.Credentials.FirstOrDefault(c => c.Id == credentialId && c.UserId == fromUserId)
                ?? throw new KeyNotFoundException("Credential not found.");

            // Duplicate the credential for the recipient
            var newCredential = new Credential
            {
                UserId = toUser.Id,
                Site = credential.Site,
                Username = credential.Username,
                EncryptedPassword = credential.EncryptedPassword,
                IntegrityHash = credential.IntegrityHash
            };
            _context.Credentials.Add(newCredential);
            _context.SaveChanges();

            // Audit log for both users
            AuditLogger.Log(_context, fromUser.Id, fromUser.Username, credential.Id, "ShareCredential", ipAddress, credential.Site);
            AuditLogger.Log(_context, toUser.Id, toUser.Username, newCredential.Id, "ReceiveSharedCredential", ipAddress, credential.Site);

            return newCredential;
        }

        // Create a share invitation
        public SharedCredentialInvitation CreateShareInvitation(Guid credentialId, Guid fromUserId, string toIdentifier, DataContext context)
        {
            // Try username first, then email
            var toUser = (context.Users.FirstOrDefault(u => u.Username == toIdentifier)
                    ?? context.Users.FirstOrDefault(u => u.Email == toIdentifier)) ?? throw new ArgumentException("Recipient not found by username or email.");
            var invitation = new SharedCredentialInvitation
            {
                CredentialId = credentialId,
                FromUserId = fromUserId,
                ToUserId = toUser.Id,
                ToEmail = toUser.Email,
                CreatedAt = DateTime.UtcNow,
                Accepted = false,
                Rejected = false
            };
            context.SharedCredentialInvitations.Add(invitation);
            context.SaveChanges();
            // (Optional) Send notification/email here
            return invitation;
        }

        // Accept a share invitation
        public Credential AcceptShareInvitation(Guid invitationId, Guid recipientUserId, DataContext context)
        {
            var invitation = context.SharedCredentialInvitations.FirstOrDefault(i => i.Id == invitationId && i.ToUserId == recipientUserId);
            if (invitation == null || invitation.Accepted || invitation.Rejected)
                throw new InvalidOperationException("Invalid or already processed invitation.");

            var original = context.Credentials.FirstOrDefault(c => c.Id == invitation.CredentialId);
            if (original == null)
                throw new KeyNotFoundException("Original credential not found.");

            // Duplicate credential for recipient
            var newCredential = new Credential
            {
                UserId = recipientUserId,
                Site = original.Site,
                Username = original.Username,
                EncryptedPassword = original.EncryptedPassword,
                IntegrityHash = original.IntegrityHash,
                Notes = original.Notes,
                IsShared = true,
                SharedFromUserId = invitation.FromUserId,
                SharedAt = DateTime.UtcNow
            };
            context.Credentials.Add(newCredential);

            invitation.Accepted = true;
            invitation.RespondedAt = DateTime.UtcNow;
            context.SaveChanges();
            return newCredential;
        }

        // Reject a share invitation
        public void RejectShareInvitation(Guid invitationId, Guid recipientUserId, DataContext context)
        {
            var invitation = context.SharedCredentialInvitations.FirstOrDefault(i => i.Id == invitationId && i.ToUserId == recipientUserId);
            if (invitation == null || invitation.Accepted || invitation.Rejected)
                throw new InvalidOperationException("Invalid or already processed invitation.");

            invitation.Rejected = true;
            invitation.RespondedAt = DateTime.UtcNow;
            context.SaveChanges();
        }

        // List shared credentials (pending and accepted)
        public IEnumerable<object> ListSharedCredentials(Guid userId, DataContext context)
        {
            // Accepted: credentials duplicated for user
            var accepted = context.Credentials
                .Where(c => c.UserId == userId && c.IsShared)
                .Select(c => new
                {
                    Id = (Guid?)c.Id,
                    Site = c.Site,
                    Username = c.Username,
                    Notes = c.Notes,
                    SharedFromUserId = c.SharedFromUserId,
                    SharedAt = c.SharedAt,
                    Status = "Accepted",
                    InvitationId = (Guid?)null, // For consistency
                    CreatedAt = (DateTime?)null // For consistency
                });

            // Pending: invitations not yet accepted/rejected
            var pending = context.SharedCredentialInvitations
                .Where(i => i.ToUserId == userId && !i.Accepted && !i.Rejected)
                .Join(context.Credentials, i => i.CredentialId, c => c.Id, (i, c) => new
                {
                    Id = (Guid?)null, // For consistency
                    Site = c.Site,
                    Username = c.Username,
                    Notes = c.Notes,
                    SharedFromUserId = (Guid?)i.FromUserId,
                    SharedAt = (DateTime?)null,
                    Status = "Pending",
                    InvitationId = (Guid?)i.Id,
                    CreatedAt = (DateTime?)i.CreatedAt
                });

            return accepted.Concat(pending).ToList();
        }
        #endregion

        #region Bulk Operations
        public int DeleteAllUserVaultItems(string userId)
        {
            if (!Guid.TryParse(userId, out var userGuid))
                throw new ArgumentException("Invalid user ID format.");

            var credentials = _context.Credentials.Where(c => c.UserId == userGuid).ToList();
            var deletedCount = credentials.Count;

            if (deletedCount > 0)
            {
                _context.Credentials.RemoveRange(credentials);

                // Also remove any shared credential invitations
                var invitations = _context.SharedCredentialInvitations
                    .Where(i => i.FromUserId == userGuid || i.ToUserId == userGuid)
                    .ToList();
                _context.SharedCredentialInvitations.RemoveRange(invitations);

                _context.SaveChanges();

                // Audit log
                var user = _context.Users.FirstOrDefault(u => u.Id == userGuid);
                AuditLogger.Log(_context, userGuid, user?.Email ?? string.Empty, null, "DeleteAllVaultData", null, $"Deleted {deletedCount} credentials");
            }

            return deletedCount;
        }
        #endregion
    }
}