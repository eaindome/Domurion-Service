<script lang="ts">
	// eslint-disable-next-line svelte/no-navigation-without-resolve, svelte/no-useless-mustaches
	import { goto } from '$app/navigation';
	import navLogo from '$lib/assets/logo.png';

	let email = '';
	let password = '';
	let confirmPassword = '';
	let isLoading = false;
	let error = '';
	let passwordStrength = 0;

	// Password validation
	$: passwordMatch = !!password && !!confirmPassword && password === confirmPassword;
	$: passwordTooShort = !!password && password.length < 8;

	// Calculate password strength
	$: {
		let strength = 0;
		if (password.length >= 8) strength += 25;
		if (/[A-Z]/.test(password)) strength += 25;
		if (/[a-z]/.test(password)) strength += 25;
		if (/[0-9]/.test(password) && /[^A-Za-z0-9]/.test(password)) strength += 25;
		passwordStrength = strength;
	}

	function getPasswordStrengthColor() {
		if (passwordStrength <= 25) return 'bg-red-500';
		if (passwordStrength <= 50) return 'bg-yellow-500';
		if (passwordStrength <= 75) return 'bg-blue-500';
		return 'bg-green-500';
	}

	function getPasswordStrengthText() {
		if (passwordStrength <= 25) return 'Weak';
		if (passwordStrength <= 50) return 'Fair';
		if (passwordStrength <= 75) return 'Good';
		return 'Strong';
	}

	async function handleRegister(event: SubmitEvent) {
		event.preventDefault();

		// Client-side validation
		if (password !== confirmPassword) {
			error = 'Passwords do not match';
			return;
		}

		if (password.length < 8) {
			error = 'Password must be at least 8 characters long';
			return;
		}

		isLoading = true;
		error = '';

		try {
			// TODO: Replace with your actual API call
			const response = await fetch('/api/auth/register', {
				method: 'POST',
				headers: {
					'Content-Type': 'application/json'
				},
				body: JSON.stringify({ email, password })
			});

			if (response.ok) {
				// Registration successful - redirect to login or dashboard
				// eslint-disable-next-line svelte/no-navigation-without-resolve
				goto('/login?message=registration-success');
			} else {
				const data = await response.json();
				error = data.message || 'Registration failed. Please try again.';
			}
		} catch (err) {
			console.log(`Error: ${err}`);
			error = 'Something went wrong. Please try again.';
		} finally {
			isLoading = false;
		}
	}
</script>

<svelte:head>
	<title>Create Account - Vault</title>
</svelte:head>

<div class="flex min-h-screen flex-col justify-center bg-gray-50 py-12 sm:px-6 lg:px-8">
	<!-- Header -->
	<div class="sm:mx-auto sm:w-full sm:max-w-md">
		<div class="flex justify-center">
			<!-- Logo and Brand -->
			<div class="flex items-center">
				<div class="flex items-center justify-center h-16">
					<img src={navLogo} alt="Domurion Logo" class="max-h-64 max-w-64 rounded-lg mr-8 mt-4" />
				</div>
			</div>
		</div>
		<h2 class="mt-6 text-center text-3xl font-semibold text-gray-900">Create your account</h2>
		<p class="mt-2 text-center text-sm text-gray-600">
			Already have an account? <a
				href={'/login'}
				class="font-medium text-indigo-600 transition-colors hover:text-indigo-500">Sign in</a
			>
		</p>
	</div>

	<!-- Registration Form -->
	<div class="mt-8 sm:mx-auto sm:w-full sm:max-w-md">
		<div class="rounded-2xl border border-gray-100 bg-white px-6 py-8 shadow-sm sm:px-10">
			<form class="space-y-6" on:submit={handleRegister}>
				<!-- Error Message -->
				{#if error}
					<div class="rounded-xl border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-600">
						{error}
					</div>
				{/if}

				<!-- Email Field -->
				<div>
					<label for="email" class="mb-2 block text-sm font-medium text-gray-700">
						Email address
					</label>
					<div class="relative">
						<input
							id="email"
							name="email"
							type="email"
							autocomplete="email"
							required
							bind:value={email}
							class="w-full rounded-xl border border-gray-200 px-4 py-3 text-gray-900 placeholder-gray-400 transition-all duration-200 focus:border-transparent focus:ring-2 focus:ring-indigo-500"
							placeholder="Enter your email"
						/>
						<div class="pointer-events-none absolute inset-y-0 right-0 flex items-center pr-3">
							<svg
								class="h-5 w-5 text-gray-400"
								fill="none"
								stroke="currentColor"
								viewBox="0 0 24 24"
							>
								<path
									stroke-linecap="round"
									stroke-linejoin="round"
									stroke-width="2"
									d="M16 12a4 4 0 10-8 0 4 4 0 008 0zm0 0v1.5a2.5 2.5 0 005 0V12a9 9 0 10-9 9m4.5-1.206a8.959 8.959 0 01-4.5 1.207"
								/>
							</svg>
						</div>
					</div>
				</div>

				<!-- Password Field -->
				<div>
					<label for="password" class="mb-2 block text-sm font-medium text-gray-700">
						Password
					</label>
					<div class="relative">
						<input
							id="password"
							name="password"
							type="password"
							autocomplete="new-password"
							required
							bind:value={password}
							class="w-full rounded-xl border border-gray-200 px-4 py-3 text-gray-900 placeholder-gray-400 transition-all duration-200 focus:border-transparent focus:ring-2 focus:ring-indigo-500"
							placeholder="Create a strong password"
						/>
						<div class="pointer-events-none absolute inset-y-0 right-0 flex items-center pr-3">
							<svg
								class="h-5 w-5 text-gray-400"
								fill="none"
								stroke="currentColor"
								viewBox="0 0 24 24"
							>
								<path
									stroke-linecap="round"
									stroke-linejoin="round"
									stroke-width="2"
									d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z"
								/>
							</svg>
						</div>
					</div>

					<!-- Password Strength Indicator -->
					{#if password}
						<div class="mt-2">
							<div class="mb-1 flex items-center justify-between text-xs text-gray-600">
								<span>Password strength</span>
								<span class="font-medium">{getPasswordStrengthText()}</span>
							</div>
							<div class="h-2 w-full rounded-full bg-gray-200">
								<div
									class="h-2 rounded-full transition-all duration-300 {getPasswordStrengthColor()}"
									style="width: {passwordStrength}%"
								></div>
							</div>
						</div>
					{/if}
				</div>

				<!-- Confirm Password Field -->
				<div>
					<label for="confirmPassword" class="mb-2 block text-sm font-medium text-gray-700">
						Confirm Password
					</label>
					<div class="relative">
						<input
							id="confirmPassword"
							name="confirmPassword"
							type="password"
							autocomplete="new-password"
							required
							bind:value={confirmPassword}
							class="w-full rounded-xl border border-gray-200 px-4 py-3 text-gray-900 placeholder-gray-400 transition-all duration-200 focus:border-transparent focus:ring-2 focus:ring-indigo-500"
							placeholder="Confirm your password"
						/>
						<div class="pointer-events-none absolute inset-y-0 right-0 flex items-center pr-3">
							{#if confirmPassword}
								{#if passwordMatch}
									<svg
										class="h-5 w-5 text-green-500"
										fill="none"
										stroke="currentColor"
										viewBox="0 0 24 24"
									>
										<path
											stroke-linecap="round"
											stroke-linejoin="round"
											stroke-width="2"
											d="M5 13l4 4L19 7"
										/>
									</svg>
								{:else}
									<svg
										class="h-5 w-5 text-red-500"
										fill="none"
										stroke="currentColor"
										viewBox="0 0 24 24"
									>
										<path
											stroke-linecap="round"
											stroke-linejoin="round"
											stroke-width="2"
											d="M6 18L18 6M6 6l12 12"
										/>
									</svg>
								{/if}
							{:else}
								<svg
									class="h-5 w-5 text-gray-400"
									fill="none"
									stroke="currentColor"
									viewBox="0 0 24 24"
								>
									<path
										stroke-linecap="round"
										stroke-linejoin="round"
										stroke-width="2"
										d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z"
									/>
								</svg>
							{/if}
						</div>
					</div>

					<!-- Password Match Indicator -->
					{#if confirmPassword && !passwordMatch}
						<p class="mt-1 text-sm text-red-600">Passwords do not match</p>
					{/if}
				</div>

				<!-- Password Requirements -->
				<div class="rounded-xl bg-gray-50 p-4">
					<h4 class="mb-2 text-sm font-medium text-gray-900">Password requirements:</h4>
					<ul class="space-y-1 text-xs text-gray-600">
						<li class="flex items-center">
							<svg
								class="mr-2 h-3 w-3 {password.length >= 8 ? 'text-green-500' : 'text-gray-400'}"
								fill="currentColor"
								viewBox="0 0 20 20"
							>
								<path
									fill-rule="evenodd"
									d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z"
									clip-rule="evenodd"
								/>
							</svg>
							At least 8 characters
						</li>
						<li class="flex items-center">
							<svg
								class="mr-2 h-3 w-3 {/[A-Z]/.test(password) ? 'text-green-500' : 'text-gray-400'}"
								fill="currentColor"
								viewBox="0 0 20 20"
							>
								<path
									fill-rule="evenodd"
									d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z"
									clip-rule="evenodd"
								/>
							</svg>
							One uppercase letter
						</li>
						<li class="flex items-center">
							<svg
								class="mr-2 h-3 w-3 {/[a-z]/.test(password) ? 'text-green-500' : 'text-gray-400'}"
								fill="currentColor"
								viewBox="0 0 20 20"
							>
								<path
									fill-rule="evenodd"
									d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z"
									clip-rule="evenodd"
								/>
							</svg>
							One lowercase letter
						</li>
						<li class="flex items-center">
							<svg
								class="mr-2 h-3 w-3 {/[0-9]/.test(password) && /[^A-Za-z0-9]/.test(password)
									? 'text-green-500'
									: 'text-gray-400'}"
								fill="currentColor"
								viewBox="0 0 20 20"
							>
								<path
									fill-rule="evenodd"
									d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z"
									clip-rule="evenodd"
								/>
							</svg>
							One number and one special character
						</li>
					</ul>
				</div>

				<!-- Terms and Conditions -->
				<div class="flex items-start">
					<div class="flex h-5 items-center">
						<input
							id="terms"
							name="terms"
							type="checkbox"
							required
							class="h-4 w-4 rounded border-gray-300 text-indigo-600 focus:ring-indigo-500"
						/>
					</div>
					<div class="ml-3 text-sm">
						<label for="terms" class="text-gray-700">
							I agree to the <a
								href={'/terms'}
								class="font-medium text-indigo-600 hover:text-indigo-500">Terms of Service</a
							>
							and
							<a href={'/privacy'} class="font-medium text-indigo-600 hover:text-indigo-500"
								>Privacy Policy</a
							>
						</label>
					</div>
				</div>

				<!-- Submit Button -->
				<div>
					<button
						type="submit"
						disabled={isLoading || !passwordMatch || passwordTooShort}
						class="group relative flex w-full justify-center rounded-xl border border-transparent bg-indigo-600 px-4 py-3 text-sm font-medium text-white transition-all duration-200 hover:bg-indigo-700 focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2 focus:outline-none disabled:cursor-not-allowed disabled:opacity-50"
					>
						{#if isLoading}
							<svg
								class="mr-3 -ml-1 h-5 w-5 animate-spin text-white"
								xmlns="http://www.w3.org/2000/svg"
								fill="none"
								viewBox="0 0 24 24"
							>
								<circle
									class="opacity-25"
									cx="12"
									cy="12"
									r="10"
									stroke="currentColor"
									stroke-width="4"
								></circle>
								<path
									class="opacity-75"
									fill="currentColor"
									d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
								></path>
							</svg>
							Creating account...
						{:else}
							Create account
						{/if}
					</button>
				</div>
			</form>

			<!-- Social Registration (Optional) -->
			<div class="mt-6">
				<div class="relative">
					<div class="absolute inset-0 flex items-center">
						<div class="w-full border-t border-gray-200"></div>
					</div>
					<div class="relative flex justify-center text-sm">
						<span class="bg-white px-2 text-gray-500">Or continue with</span>
					</div>
				</div>

				<div class="mt-6">
					<button
						type="button"
						class="inline-flex w-full justify-center rounded-xl border border-gray-200 bg-white px-4 py-3 text-sm font-medium text-gray-700 transition-all duration-200 hover:bg-gray-50"
					>
						<svg class="mr-2 h-5 w-5" viewBox="0 0 24 24">
							<path
								fill="#4285F4"
								d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z"
							/>
							<path
								fill="#34A853"
								d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z"
							/>
							<path
								fill="#FBBC05"
								d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.93l2.85-2.22.81-.62z"
							/>
							<path
								fill="#EA4335"
								d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84c.87-2.6 3.3-4.53 6.16-4.53z"
							/>
						</svg>
						Continue with Google
					</button>
				</div>
			</div>
		</div>
	</div>
</div>

<style>
	/* Additional custom styles if needed */
	input:focus {
		box-shadow: 0 0 0 3px rgba(99, 102, 241, 0.1);
	}
</style>
