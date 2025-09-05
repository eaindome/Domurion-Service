using Fido2NetLib;
using Fido2NetLib.Objects;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Linq;

namespace Domurion.Services
{
    using Domurion.Services.Interfaces;

    public class Fido2Service(Fido2 fido2, IMemoryCache cache, IUserService userService, IFidoCredentialService fidoCredentialService)
    {
		private readonly Fido2 _fido2 = fido2;
		private readonly IMemoryCache _cache = cache;
        private readonly IUserService _userService = userService;
        private readonly IFidoCredentialService _fidoCredentialService = fidoCredentialService;


		public CredentialCreateOptions RequestNewCredential(Fido2User user, List<PublicKeyCredentialDescriptor> existingKeys)
        {
            var param = new RequestNewCredentialParams
            {
                User = user,
                ExcludeCredentials = existingKeys,
                AuthenticatorSelection = new AuthenticatorSelection()
            };
            var options = _fido2.RequestNewCredential(param);
            _cache.Set($"fido2.attestation.{user.Name}", options, TimeSpan.FromMinutes(5));
            return options;
        }

		public async Task<RegisteredPublicKeyCredential> MakeNewCredentialAsync(AuthenticatorAttestationRawResponse attestationResponse, string username, List<PublicKeyCredentialDescriptor> existingKeys)
		{
			var options = _cache.Get<CredentialCreateOptions>($"fido2.attestation.{username}") ?? throw new InvalidOperationException("Attestation options not found.");
			var param = new MakeNewCredentialParams
			{
				AttestationResponse = attestationResponse,
				OriginalOptions = options,
				IsCredentialIdUniqueToUserCallback = (credentialId, cancellationToken) => Task.FromResult(true)
			};
			var result = await _fido2.MakeNewCredentialAsync(param);
			return result;
		}

		public AssertionOptions GetAssertionOptions(List<PublicKeyCredentialDescriptor> allowedKeys)
		{
			var options = _fido2.GetAssertionOptions(allowedKeys, UserVerificationRequirement.Discouraged);
			_cache.Set($"fido2.assertion", options, TimeSpan.FromMinutes(5));
			return options;
		}

		public async Task<dynamic> MakeAssertionAsync(
            AuthenticatorAssertionRawResponse assertionResponse,
            string username,
            List<PublicKeyCredentialDescriptor> allowedKeys)
        {
            var options = _cache.Get<AssertionOptions>($"fido2.assertion") ?? throw new InvalidOperationException("Assertion options not found.");

            // 1. Lookup user and their credential by credentialId
            var userEntity = _userService.GetByUsername(username) ?? throw new InvalidOperationException("User not found.");

            // The credentialId comes from the assertion response
            var credentialIdBytes = Convert.FromBase64String(assertionResponse.Id);
            // Lookup the credential in your DB using IFidoCredentialService
            var fidoCredentials = _fidoCredentialService.GetCredentialsByUserId(userEntity.Id);
            var fidoCredential = fidoCredentials.FirstOrDefault(c => c.CredentialId.SequenceEqual(credentialIdBytes)) ?? throw new InvalidOperationException("Credential not found.");

            // 2. Prepare the parameters for verification
            var param = new MakeAssertionParams
            {
                AssertionResponse = assertionResponse,
                OriginalOptions = options,
                StoredPublicKey = fidoCredential.PublicKey,
                StoredSignatureCounter = fidoCredential.SignatureCounter,
                IsUserHandleOwnerOfCredentialIdCallback = (args, cancellationToken) =>
                {
                    // args.UserHandle is the user handle from the assertion
                    return Task.FromResult(args.UserHandle.SequenceEqual(userEntity.Id.ToByteArray()));
                }
            };

            // 3. Call Fido2 to verify the assertion
            var result = await _fido2.MakeAssertionAsync(param);

            return result;
        }
	}
}
