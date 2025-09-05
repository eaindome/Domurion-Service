using Microsoft.AspNetCore.Mvc;
using Fido2NetLib;
using Fido2NetLib.Objects;
using Domurion.Services;
using System.Text;
using Domurion.Services.Interfaces;

namespace Domurion.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class Fido2Controller(Fido2Service fido2Service, IUserService userService, IFidoCredentialService fidoCredentialService) : ControllerBase
	{
		private readonly Fido2Service _fido2Service = fido2Service;
        private readonly IUserService _userService = userService;
	    private readonly IFidoCredentialService _fidoCredentialService = fidoCredentialService;

        [HttpPost("register/begin")]
		public IActionResult RegisterBegin([FromBody] string username)
		{
			// Look up user by username
			var userEntity = _userService.GetByUsername(username);
			if (userEntity == null)
				return NotFound(new { error = "User not found." });

			var user = new Fido2User
			{
				Id = userEntity.Id.ToByteArray(),
				Name = userEntity.Username,
				DisplayName = userEntity.Username
			};

			// Fetch existing credentials for user from FidoCredentialService
			var fidoCredentials = _fidoCredentialService.GetCredentialsByUserId(userEntity.Id);
			var existingKeys = fidoCredentials
				.Select(c => new PublicKeyCredentialDescriptor(c.CredentialId))
				.ToList();

			var options = _fido2Service.RequestNewCredential(user, existingKeys);
			return Ok(options);
		}

		[HttpPost("register/complete")]
		public async Task<IActionResult> RegisterComplete([FromBody] AuthenticatorAttestationRawResponse attestationResponse, [FromQuery] string username)
		{
			// TODO: Fetch existing credentials for user from DB
			var existingKeys = new List<PublicKeyCredentialDescriptor>();
			var result = await _fido2Service.MakeNewCredentialAsync(attestationResponse, username, existingKeys);
			// TODO: Store credential info in DB
			return Ok(result);
		}

		[HttpPost("login/begin")]
		public IActionResult LoginBegin([FromBody] string username)
		{
			// TODO: Fetch allowed credentials for user from DB
			var allowedKeys = new List<PublicKeyCredentialDescriptor>();
			var options = _fido2Service.GetAssertionOptions(allowedKeys);
			return Ok(options);
		}

		[HttpPost("login/complete")]
		public async Task<IActionResult> LoginComplete([FromBody] AuthenticatorAssertionRawResponse assertionResponse, [FromQuery] string username)
		{
			// TODO: Fetch allowed credentials for user from DB
			var allowedKeys = new List<PublicKeyCredentialDescriptor>();
			var result = await _fido2Service.MakeAssertionAsync(assertionResponse, username, allowedKeys);
            // TODO: Validate and sign in user
			return Ok(result);
		}
	}
}
