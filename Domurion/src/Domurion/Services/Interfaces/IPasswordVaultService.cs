using Domurion.Models;
using Domurion.Data;
using Microsoft.AspNetCore.Mvc;

namespace Domurion.Services.Interfaces
{
    public interface IPasswordVaultService
    {
        Credential AddCredential(Guid userId, string site, string? siteUrl, string email, string password, string? notes = null, string? ipAddress = null);
        IEnumerable<Credential> GetCredentials(Guid userId);
        string RetrievePassword(Guid credentialId, Guid userId, string? ipAddress = null);
        Credential UpdateCredential(Guid credentialId, Guid userId, string? site, string? siteUrl, string? email, string? password, string? notes = null, string? ipAddress = null);
        void DeleteCredential(Guid credentialId, Guid userId, string? ipAddress = null);
        Credential? GetById(Guid credentialId);

        // Share invitation
        SharedCredentialInvitation CreateShareInvitation(Guid credentialId, Guid fromUserId, string toIdentifier, DataContext context);
        Credential AcceptShareInvitation(Guid invitationId, Guid recipientUserId, DataContext context);

        void RejectShareInvitation(Guid invitationId, Guid recipientUserId, DataContext context);
        IEnumerable<object> ListSharedCredentials(Guid userId, DataContext context);

        // Password sharing
        Credential ShareCredential(Guid credentialId, Guid fromUserId, string toUsername, string? ipAddress = null);
    }
}