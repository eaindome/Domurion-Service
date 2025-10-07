<script lang="ts">
	import { goto } from '$app/navigation';
	import { onMount } from 'svelte';
	import { page } from '$app/stores';
	import navLogo from '$lib/assets/logo.png';
	import { resetPassword } from '$lib/api/auth';
	import { toast } from '$lib/stores/toast';

	let token = '';
	let newPassword = '';
	let confirmPassword = '';
	let isLoading = false;
	let isSuccess = false;
	let error = '';
	let tokenError = '';

	// Password strength validation
	let passwordStrength = {
		length: false,
		lowercase: false,
		uppercase: false,
		number: false,
		special: false
	};

	// Get token from URL parameters
	onMount(() => {
		const urlToken = $page.url.searchParams.get('token');
		if (urlToken) {
			token = urlToken;
		} else {
			tokenError = 'Invalid reset link. Please request a new password reset.';
		}
	});

	// Update password strength indicators
	$: {
		passwordStrength = {
			length: newPassword.length >= 8,
			lowercase: /[a-z]/.test(newPassword),
			uppercase: /[A-Z]/.test(newPassword),
			number: /\d/.test(newPassword),
			special: /[!@#$%^&*(),.?":{}|<>]/.test(newPassword)
		};
	}

	$: isPasswordValid = Object.values(passwordStrength).every(Boolean);
	$: passwordsMatch = newPassword && confirmPassword && newPassword === confirmPassword;

	function getStrengthColor() {
		const validCount = Object.values(passwordStrength).filter(Boolean).length;
		if (validCount < 2) return 'text-red-600';
		if (validCount < 4) return 'text-yellow-600';
		if (validCount < 5) return 'text-blue-600';
		return 'text-green-600';
	}

	function getStrengthText() {
		const validCount = Object.values(passwordStrength).filter(Boolean).length;
		if (validCount < 2) return 'Weak';
		if (validCount < 4) return 'Fair';
		if (validCount < 5) return 'Good';
		return 'Strong';
	}

	async function handleResetPassword(event: SubmitEvent) {
		event.preventDefault();
		
		if (!token) {
			error = 'Invalid reset token';
			return;
		}

		if (!isPasswordValid) {
			error = 'Password does not meet strength requirements';
			return;
		}

		if (!passwordsMatch) {
			error = 'Passwords do not match';
			return;
		}

		isLoading = true;
		error = '';

		try {
			const response = await resetPassword(token, newPassword);
			if (response.success) {
				isSuccess = true;
				toast.show(response.message || 'Password has been reset successfully.', 'success');
				// Clear sensitive data
				newPassword = '';
				confirmPassword = '';
			} else {
				error = response.message || 'Failed to reset password';
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

	function handleGoToLogin() {
		goto('/login');
	}

	function handleRequestNewReset() {
		goto('/forgot-password');
	}
</script>

<svelte:head>
	<title>Reset Password - Vault</title>
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
		<h2 class="mt-3 text-center text-2xl font-semibold text-gray-900">
			{isSuccess ? 'Password Reset Complete' : 'Create New Password'}
		</h2>
		<p class="mt-2 text-center text-sm text-gray-600">
			{isSuccess 
				? 'Your password has been successfully updated.' 
				: 'Choose a strong password to secure your account.'}
		</p>
	</div>

	<!-- Form or Success/Error State -->
	<div class="mt-5 sm:mx-auto sm:w-full sm:max-w-md">
		<div class="rounded-2xl border border-gray-100 bg-white px-6 py-8 shadow-sm sm:px-10">
			{#if tokenError}
				<!-- Token Error State -->
				<div class="text-center space-y-4">
					<div class="mx-auto flex h-12 w-12 items-center justify-center rounded-full bg-red-100">
						<svg class="h-6 w-6 text-red-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
							<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"/>
						</svg>
					</div>
					<div class="space-y-2">
						<h3 class="text-lg font-medium text-gray-900">Invalid Reset Link</h3>
						<p class="text-sm text-gray-600">
							This password reset link is invalid or has expired.
						</p>
					</div>
					<div class="space-y-3">
						<button
							type="button"
							on:click={handleRequestNewReset}
							class="flex w-full items-center justify-center rounded-xl bg-indigo-600 px-8 py-3.5 text-sm font-semibold text-white shadow-lg transition-all duration-200 hover:bg-indigo-700 hover:shadow-xl focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2"
						>
							Request New Reset Link
						</button>
						<button
							type="button"
							on:click={handleGoToLogin}
							class="w-full text-sm text-gray-600 hover:text-gray-500 transition-colors"
						>
							Back to Login
						</button>
					</div>
				</div>
			{:else if isSuccess}
				<!-- Success State -->
				<div class="text-center space-y-4">
					<div class="mx-auto flex h-12 w-12 items-center justify-center rounded-full bg-green-100">
						<svg class="h-6 w-6 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
							<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7"/>
						</svg>
					</div>
					<div class="space-y-2">
						<h3 class="text-lg font-medium text-gray-900">Password Successfully Reset</h3>
						<p class="text-sm text-gray-600">
							Your password has been updated. You can now log in with your new password.
						</p>
					</div>
					<div class="space-y-3">
						<button
							type="button"
							on:click={handleGoToLogin}
							class="flex w-full items-center justify-center rounded-xl bg-indigo-600 px-8 py-3.5 text-sm font-semibold text-white shadow-lg transition-all duration-200 hover:bg-indigo-700 hover:shadow-xl focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2"
						>
							<svg class="mr-2 h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
								<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 16l-4-4m0 0l4-4m-4 4h14"/>
							</svg>
							Go to Login
						</button>
					</div>
				</div>
			{:else}
				<!-- Reset Form -->
				<form class="space-y-6" on:submit={handleResetPassword}>
					<!-- Error Message -->
					{#if error}
						<div class="rounded-xl border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-600">
							{error}
						</div>
					{/if}

					<!-- New Password Input -->
					<div>
						<label for="newPassword" class="block text-sm font-semibold text-gray-800">New Password</label>
						<div class="mt-2">
							<input
								id="newPassword"
								name="newPassword"
								type="password"
								autocomplete="new-password"
								required
								bind:value={newPassword}
								disabled={isLoading}
								class="block w-full rounded-xl border-2 border-gray-200 bg-white px-4 py-3 text-gray-900 transition-all duration-150 placeholder:text-gray-400 focus:border-indigo-500 focus:outline-none focus:ring-4 focus:ring-indigo-100 disabled:cursor-not-allowed disabled:opacity-50 disabled:bg-gray-50"
								placeholder="Enter your new password"
							/>
						</div>
						
						<!-- Password Strength Indicator -->
						{#if newPassword}
							<div class="mt-3 space-y-2">
								<div class="flex items-center justify-between">
									<span class="text-xs text-gray-600">Password strength:</span>
									<span class="text-xs font-medium {getStrengthColor()}">{getStrengthText()}</span>
								</div>
								
								<!-- Requirements List -->
								<div class="grid grid-cols-1 gap-1 text-xs">
									<div class="flex items-center space-x-2">
										<svg class="h-3 w-3 {passwordStrength.length ? 'text-green-500' : 'text-gray-300'}" fill="currentColor" viewBox="0 0 20 20">
											<path fill-rule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clip-rule="evenodd"/>
										</svg>
										<span class="{passwordStrength.length ? 'text-green-700' : 'text-gray-500'}">At least 8 characters</span>
									</div>
									<div class="flex items-center space-x-2">
										<svg class="h-3 w-3 {passwordStrength.lowercase ? 'text-green-500' : 'text-gray-300'}" fill="currentColor" viewBox="0 0 20 20">
											<path fill-rule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clip-rule="evenodd"/>
										</svg>
										<span class="{passwordStrength.lowercase ? 'text-green-700' : 'text-gray-500'}">One lowercase letter</span>
									</div>
									<div class="flex items-center space-x-2">
										<svg class="h-3 w-3 {passwordStrength.uppercase ? 'text-green-500' : 'text-gray-300'}" fill="currentColor" viewBox="0 0 20 20">
											<path fill-rule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clip-rule="evenodd"/>
										</svg>
										<span class="{passwordStrength.uppercase ? 'text-green-700' : 'text-gray-500'}">One uppercase letter</span>
									</div>
									<div class="flex items-center space-x-2">
										<svg class="h-3 w-3 {passwordStrength.number ? 'text-green-500' : 'text-gray-300'}" fill="currentColor" viewBox="0 0 20 20">
											<path fill-rule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clip-rule="evenodd"/>
										</svg>
										<span class="{passwordStrength.number ? 'text-green-700' : 'text-gray-500'}">One number</span>
									</div>
									<div class="flex items-center space-x-2">
										<svg class="h-3 w-3 {passwordStrength.special ? 'text-green-500' : 'text-gray-300'}" fill="currentColor" viewBox="0 0 20 20">
											<path fill-rule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clip-rule="evenodd"/>
										</svg>
										<span class="{passwordStrength.special ? 'text-green-700' : 'text-gray-500'}">One special character</span>
									</div>
								</div>
							</div>
						{/if}
					</div>

					<!-- Confirm Password Input -->
					<div>
						<label for="confirmPassword" class="block text-sm font-semibold text-gray-800">Confirm New Password</label>
						<div class="mt-2">
							<input
								id="confirmPassword"
								name="confirmPassword"
								type="password"
								autocomplete="new-password"
								required
								bind:value={confirmPassword}
								disabled={isLoading}
								class="block w-full rounded-xl border-2 border-gray-200 bg-white px-4 py-3 text-gray-900 transition-all duration-150 placeholder:text-gray-400 focus:border-indigo-500 focus:outline-none focus:ring-4 focus:ring-indigo-100 disabled:cursor-not-allowed disabled:opacity-50 disabled:bg-gray-50"
								placeholder="Confirm your new password"
							/>
						</div>
						
						<!-- Password Match Indicator -->
						{#if confirmPassword}
							<div class="mt-2 flex items-center space-x-2">
								{#if passwordsMatch}
									<svg class="h-4 w-4 text-green-500" fill="currentColor" viewBox="0 0 20 20">
										<path fill-rule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clip-rule="evenodd"/>
									</svg>
									<span class="text-sm text-green-700">Passwords match</span>
								{:else}
									<svg class="h-4 w-4 text-red-500" fill="currentColor" viewBox="0 0 20 20">
										<path fill-rule="evenodd" d="M4.293 4.293a1 1 0 011.414 0L10 8.586l4.293-4.293a1 1 0 111.414 1.414L11.414 10l4.293 4.293a1 1 0 01-1.414 1.414L10 11.414l-4.293 4.293a1 1 0 01-1.414-1.414L8.586 10 4.293 5.707a1 1 0 010-1.414z" clip-rule="evenodd"/>
									</svg>
									<span class="text-sm text-red-700">Passwords do not match</span>
								{/if}
							</div>
						{/if}
					</div>

					<!-- Submit Button -->
					<div>
						<button
							type="submit"
							disabled={isLoading || !isPasswordValid || !passwordsMatch || !newPassword || !confirmPassword}
							class="flex w-full items-center justify-center rounded-xl bg-indigo-600 px-8 py-3.5 text-sm font-semibold text-white shadow-lg transition-all duration-200 hover:bg-indigo-700 hover:shadow-xl focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50 disabled:hover:bg-indigo-600"
						>
							{#if isLoading}
								<svg class="mr-3 -ml-1 h-5 w-5 animate-spin" fill="none" viewBox="0 0 24 24">
									<circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
									<path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
								</svg>
								Resetting Password...
							{:else}
								<svg class="mr-2 h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
									<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z"/>
								</svg>
								Reset Password
							{/if}
						</button>
					</div>

					<!-- Back to Login -->
					<div class="text-center">
                        <button
                            type="button"
                            on:click={handleGoToLogin}
                            class="inline-flex w-full items-center justify-center rounded-xl border-2 border-gray-200 bg-white px-4 py-3 text-sm font-medium text-gray-700 transition-all duration-200 hover:bg-gray-50 hover:border-gray-300 focus:ring-2 focus:ring-gray-200 focus:ring-offset-2"
                        >
                            <svg class="mr-2 h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 19l-7-7m0 0l7-7m-7 7h18"/>
                            </svg>
                            Back to Login
                        </button>
                    </div>
				</form>
			{/if}

			<!-- Security Note -->
			{#if !tokenError && !isSuccess}
				<div class="mt-6 rounded-lg border border-blue-200 bg-blue-50 p-3">
					<div class="flex">
						<svg class="h-5 w-5 text-blue-400 flex-shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
							<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"/>
						</svg>
						<div class="ml-3">
							<p class="text-sm text-blue-800">
								<strong>Password Requirements:</strong> Your new password cannot be the same as any of your previous 5 passwords for security reasons.
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