<script lang="ts">
	// eslint-disable-next-line svelte/no-navigation-without-resolve, svelte/no-useless-mustaches
	import { goto } from '$app/navigation';
	import navLogo from '$lib/assets/logo.png';
	import { login, signInWithGoogle } from '$lib/api/auth';
	import { toast } from '$lib/stores/toast';


	let email = '';
	let password = '';
	let isLoading = false;
	let error = '';
	let showPassword = false;

	async function handleLogin(event: SubmitEvent) {
		event.preventDefault();
		isLoading = true;
		error = '';

		try {
			const response = await login(email, password);
			if (response.success) {
				toast.show('Login successful', 'success');
				await new Promise(res => setTimeout(res, 2000));
				goto('/dashboard');
			} else {
				error = response.message || 'Login failed';
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
	<title>Sign In - Vault</title>
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
		<h2 class="mt-3 text-center text-2xl font-semibold text-gray-900">Sign in to your account</h2>
		
	</div>

	<!-- Login Form -->
	<div class="mt-5 sm:mx-auto sm:w-full sm:max-w-md">
		<form class="space-y-4 rounded-2xl border border-gray-100 bg-white px-6 py-8 shadow-sm sm:px-10" on:submit={handleLogin}>
			<!-- Error Message -->
			{#if error}
				<div class="rounded-xl border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-600">
					{error}
				</div>
			{/if}

			<!-- Email Field -->
			<div class="space-y-2">
				<label for="email" class="mb-1 block text-sm font-semibold text-gray-800">
					Email address
				</label>
				<input
					id="email"
					name="email"
					type="email"
					autocomplete="email"
					required
					bind:value={email}
					class="w-full rounded-xl border-2 border-gray-200 bg-white px-4 py-3.5 placeholder-gray-400 transition-colors duration-150 focus:border-indigo-500 focus:ring-2 focus:ring-indigo-500 outline-none"
					placeholder="Enter your email"
				/>
			</div>

			<!-- Password Field -->
			<div class="space-y-2">
				<label for="password" class="mb-1 block text-sm font-semibold text-gray-800">
					Password
				</label>
				<div class="relative">
					<input
						id="password"
						name="password"
						type={showPassword ? 'text' : 'password'}
						autocomplete="current-password"
						required
						bind:value={password}
						class="w-full rounded-xl border-2 border-gray-200 bg-white px-4 py-3.5 pr-12 placeholder-gray-400 transition-colors duration-150 focus:border-indigo-500 focus:ring-2 focus:ring-indigo-500 outline-none"
						placeholder="Enter your password"
					/>
					<button
						type="button"
						on:click={() => (showPassword = !showPassword)}
						class="absolute right-2 top-1/2 -translate-y-1/2 rounded-lg p-1.5 transition-colors duration-150 text-gray-400 hover:text-gray-600"
						title={showPassword ? 'Hide password' : 'Show password'}
						tabindex="-1"
					>
						{#if showPassword}
							<svg class="h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
								<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13.875 18.825A10.05 10.05 0 0112 19c-4.478 0-8.268-2.943-9.543-7a9.97 9.97 0 011.563-3.029m5.858.908a3 3 0 114.243 4.243M9.878 9.878l4.242 4.242M9.878 9.878L3 3m6.878 6.878L21 21" />
							</svg>
						{:else}
							<svg class="h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
								<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
								<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" />
							</svg>
						{/if}
					</button>
				</div>
			</div>

			<!-- Forgot Password Link -->
			<div class="flex items-center justify-between">
				<div class="flex items-center">
					<input
						id="remember-me"
						name="remember-me"
						type="checkbox"
						class="h-4 w-4 rounded border-gray-300 text-indigo-600 focus:ring-indigo-500"
					/>
					<label for="remember-me" class="ml-2 block text-sm text-gray-700"> Remember me </label>
				</div>

				<div class="text-sm">
					<a
						href={'/forgot-password'}
						class="font-medium text-indigo-600 transition-colors hover:text-indigo-500"
					>
						Forgot password?
					</a>
				</div>
			</div>

			<!-- Submit Button -->
			<div>
				<button
					type="submit"
					disabled={isLoading}
					class="w-full rounded-xl bg-indigo-600 px-8 py-3.5 text-sm font-semibold text-white shadow-lg transition-all duration-200 hover:bg-indigo-700 hover:shadow-xl focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
				>
					{#if isLoading}
						<span class="flex items-center justify-center">
							<svg class="mr-3 -ml-1 h-5 w-5 animate-spin text-white" fill="none" viewBox="0 0 24 24">
								<circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
								<path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
							</svg>
							Signing in...
						</span>
					{:else}
						<span class="flex items-center justify-center text-white text-md font-semibold">
							Sign in
						</span>
					{/if}
				</button>
			</div>
			<!-- Social Login (Optional) -->
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
						on:click={signInWithGoogle}
						class="inline-flex w-full justify-center rounded-xl border border-gray-200 bg-white px-4 py-3 text-sm font-medium text-gray-700 transition-all duration-200 hover:bg-gray-100"
					>
						<svg class="mr-2 h-5 w-5" viewBox="0 0 24 24">
							<path fill="#4285F4" d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z" />
							<path fill="#34A853" d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z" />
							<path fill="#FBBC05" d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.93l2.85-2.22.81-.62z" />
							<path fill="#EA4335" d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84c.87-2.6 3.3-4.53 6.16-4.53z" />
						</svg>
						Continue with Google
					</button>
				</div>
			</div>
			<p class="mt-2 text-center text-sm text-gray-600">
				New here? <a
					href={'/register'}
					class="font-medium text-indigo-600 transition-colors hover:text-indigo-500"
					>Create an account</a
				>
			</p>
		</form>
	</div>
</div>

<style>
	/* Additional custom styles if needed */
	input:focus {
		box-shadow: 0 0 0 3px rgba(99, 102, 241, 0.1);
	}
</style>
