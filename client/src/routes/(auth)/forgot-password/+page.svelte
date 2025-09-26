<script lang="ts">
	import { goto } from '$app/navigation';
	import navLogo from '$lib/assets/logo.png';
	import { requestPasswordReset } from '$lib/api/auth';
	import { toast } from '$lib/stores/toast';

	let email = '';
	let isLoading = false;
	let isSubmitted = false;
	let error = '';

	async function handleRequestReset(event: SubmitEvent) {
		event.preventDefault();
		
		if (!email.trim()) {
			error = 'Please enter your email address';
			return;
		}

		// Basic email validation
		const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
		if (!emailRegex.test(email.trim())) {
			error = 'Please enter a valid email address';
			return;
		}

		isLoading = true;
		error = '';

		try {
			const response = await requestPasswordReset(email.trim());
			if (response.success) {
				isSubmitted = true;
				toast.show(response.message || 'If the email exists, a password reset link will be sent.', 'success');
			} else {
				error = response.message || 'Failed to request password reset';
				toast.show(error, 'error');
			}
		} catch (err) {
			console.log(`Error: ${err}`);
			error = 'Something went wrong. Please try again.';
			toast.show(error, 'error');
		} finally {
			isLoading = false;
		}
	}

	function handleBackToLogin() {
		goto('/login');
	}
</script>

<svelte:head>
	<title>Forgot Password - Vault</title>
</svelte:head>

<div class="flex min-h-screen flex-col justify-center bg-gray-50 py-12 sm:px-6 lg:px-8">
	<!-- Header -->
	<div class="sm:mx-auto sm:w-full sm:max-w-md">
		<div class="flex justify-center">
			<!-- Logo and Brand -->
			<div class="flex items-center">
				<div class="flex items-center h-16">
					<img src={navLogo} alt="Domurion Logo" class="max-h-64 max-w-64 rounded-lg mr-8 mt-4" />
				</div>
			</div>
		</div>
		<h2 class="mt-3 text-center text-2xl font-semibold text-gray-900">Reset Your Password</h2>
		<p class="mt-2 text-center text-sm text-gray-600">
			Enter your email address and we'll send you a link to reset your password.
		</p>
	</div>

	<!-- Form or Success Message -->
	<div class="mt-5 sm:mx-auto sm:w-full sm:max-w-md">
		<div class="rounded-2xl border border-gray-100 bg-white px-6 py-8 shadow-sm sm:px-10">
			{#if isSubmitted}
				<!-- Success State -->
				<div class="text-center space-y-4">
					<div class="mx-auto flex h-12 w-12 items-center justify-center rounded-full bg-green-100">
						<svg class="h-6 w-6 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
							<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 8l7.89 5.26a2 2 0 002.22 0L21 8M5 19h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z"/>
						</svg>
					</div>
					<div class="space-y-2">
						<h3 class="text-lg font-medium text-gray-900">Check Your Email</h3>
						<p class="text-sm text-gray-600">
							If an account with that email exists, we've sent you a password reset link.
						</p>
					</div>
					
					<!-- Instructions -->
					<div class="rounded-lg border border-blue-200 bg-blue-50 p-3 text-left">
						<div class="flex">
							<svg class="h-5 w-5 text-blue-400 flex-shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
								<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"/>
							</svg>
							<div class="ml-3">
								<p class="text-sm text-blue-800">
									<strong>Next steps:</strong><br>
									1. Check your email inbox for a message from Domurion<br>
									2. Click the "Reset My Password" button in the email<br>
									3. Create a new secure password<br><br>
									The reset link will expire in 1 hour for security.
								</p>
							</div>
						</div>
					</div>

					<!-- Actions -->
					<div class="space-y-3">
						<button
							type="button"
							on:click={handleBackToLogin}
							class="flex w-full items-center justify-center rounded-xl bg-indigo-600 px-8 py-3.5 text-sm font-semibold text-white shadow-lg transition-all duration-200 hover:bg-indigo-700 hover:shadow-xl focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2"
						>
							<svg class="mr-2 h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
								<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 17l-5-5m0 0l5-5m-5 5h12"/>
							</svg>
							Back to Login
						</button>
						<button
							type="button"
							on:click={() => { isSubmitted = false; email = ''; error = ''; }}
							class="w-full text-sm text-gray-600 hover:text-gray-500 transition-colors"
						>
							Try a different email
						</button>
					</div>
				</div>
			{:else}
				<!-- Request Form -->
				<form class="space-y-6" on:submit={handleRequestReset}>
					<!-- Error Message -->
					{#if error}
						<div class="rounded-xl border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-600">
							{error}
						</div>
					{/if}

					<!-- Email Input -->
					<div>
						<label for="email" class="block text-sm font-semibold text-gray-800">Email Address</label>
						<div class="mt-2">
							<input
								id="email"
								name="email"
								type="email"
								autocomplete="email"
								required
								bind:value={email}
								disabled={isLoading}
								class="block w-full rounded-xl border-2 border-gray-200 bg-white px-4 py-3 text-gray-900 transition-all duration-150 placeholder:text-gray-400 focus:border-indigo-500 focus:outline-none focus:ring-4 focus:ring-indigo-100 disabled:cursor-not-allowed disabled:opacity-50 disabled:bg-gray-50"
								placeholder="Enter your email address"
							/>
						</div>
						<p class="mt-2 text-xs text-gray-500">We'll send a password reset link to this email address.</p>
					</div>

					<!-- Submit Button -->
					<div>
						<button
							type="submit"
							disabled={isLoading || !email.trim()}
							class="flex w-full items-center justify-center rounded-xl bg-indigo-600 px-8 py-3.5 text-sm font-semibold text-white shadow-lg transition-all duration-200 hover:bg-indigo-700 hover:shadow-xl focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50 disabled:hover:bg-indigo-600"
						>
							{#if isLoading}
								<svg class="mr-3 -ml-1 h-5 w-5 animate-spin" fill="none" viewBox="0 0 24 24">
									<circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
									<path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
								</svg>
								Sending Reset Link...
							{:else}
								<svg class="mr-2 h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
									<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 8l7.89 5.26a2 2 0 002.22 0L21 8M5 19h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z"/>
								</svg>
								Send Reset Link
							{/if}
						</button>
					</div>

					<!-- Back to Login -->
					
                    <div class="text-center">
                        <button
                            type="button"
                            on:click={handleBackToLogin}
                            class="inline-flex items-center text-sm font-medium text-gray-600 transition-colors hover:text-indigo-600 hover:bg-gray-50 px-3 py-2 rounded-lg"
                        >
                            <svg class="mr-1 h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 19l-7-7m0 0l7-7m-7 7h18"/>
                            </svg>
                            Back to Login
                        </button>
                    </div>
				</form>
			{/if}

			<!-- Security Note -->
			{#if !isSubmitted}
				<div class="mt-6 rounded-lg border border-amber-200 bg-amber-50 p-3">
					<div class="flex">
						<svg class="h-5 w-5 text-amber-400 flex-shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
							<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z"/>
						</svg>
						<div class="ml-3">
							<p class="text-sm text-amber-800">
								<strong>Security note:</strong> For your protection, we won't reveal whether an email address exists in our system. You'll receive an email only if the address is registered.
							</p>
						</div>
					</div>
				</div>
			{/if}
		</div>
	</div>
</div>

<style>
	/* Additional custom styles if needed */
	input:focus {
		box-shadow: 0 0 0 3px rgba(99, 102, 241, 0.1);
	}
</style>
