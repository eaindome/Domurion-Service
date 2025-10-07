<script lang="ts">
	import { goto } from '$app/navigation';
	import { onMount } from 'svelte';
	import { page } from '$app/stores';
	import navLogo from '$lib/assets/logo.png';
	import { verifyOtp, resendOtp } from '$lib/api/auth';
	import { toast } from '$lib/stores/toast';
	import { authStore } from '$lib/stores/authStore';

	let email = '';
	let otp = '';
	let otpDigits = ['', '', '', '', '', ''];
	let isLoading = false;
	let isResending = false;
	let error = '';
	let timeLeft = 600; // 10 minutes in seconds
	let timerInterval: ReturnType<typeof setInterval>;
	let otpInputs: Array<HTMLInputElement | null> = [null, null, null, null, null, null];

	// Get email from URL parameters
	onMount(() => {
		const urlEmail = $page.url.searchParams.get('email');
		if (urlEmail) {
			email = urlEmail;
		} else {
			// Redirect to login if no email provided
			// goto('/login');
		}

		// Start countdown timer
		startTimer();
		
		// Focus first input after mount
		setTimeout(() => {
			otpInputs[0]?.focus();
		}, 100);
		
		return () => {
			if (timerInterval) {
				clearInterval(timerInterval);
			}
		};
	});

	function assignOtpInput(element: HTMLInputElement, index: number) {
		otpInputs[index] = element;
		return {
			destroy() {
				otpInputs[index] = null;
			}
		};
	}

	function handleOtpInput(event: Event, index: number) {
		const target = event.target as HTMLInputElement;
		let value = target.value.replace(/\D/g, ''); // Remove non-digits
		
		if (value.length > 1) {
			// Handle paste of multiple digits
			const digits = value.slice(0, 6).split('');
			for (let i = 0; i < digits.length && (index + i) < 6; i++) {
				otpDigits[index + i] = digits[i];
				if (otpInputs[index + i]) {
					otpInputs[index + i]!.value = digits[i];
				}
			}
			// Focus the last filled input or the next empty one
			const lastIndex = Math.min(index + digits.length - 1, 5);
			const nextIndex = Math.min(index + digits.length, 5);
			otpInputs[nextIndex < 6 ? nextIndex : lastIndex]?.focus();
		} else {
			// Single digit input
			otpDigits[index] = value;
			target.value = value;
			
			// Auto-move to next input if digit entered and not last input
			if (value && index < 5) {
				otpInputs[index + 1]?.focus();
			}
		}
		
		// Update combined OTP value
		otp = otpDigits.join('');
	}

	function handleOtpKeydown(event: KeyboardEvent, index: number) {
		const target = event.target as HTMLInputElement;
		
		if (event.key === 'Backspace') {
			// Clear current digit
			otpDigits[index] = '';
			target.value = '';
			otp = otpDigits.join('');
			
			// Move to previous input if current is empty and not first input
			if (index > 0 && !target.value) {
				otpInputs[index - 1]?.focus();
			}
		} else if (event.key === 'ArrowLeft' && index > 0) {
			event.preventDefault();
			otpInputs[index - 1]?.focus();
		} else if (event.key === 'ArrowRight' && index < 5) {
			event.preventDefault();
			otpInputs[index + 1]?.focus();
		} else if (event.key === 'Delete') {
			// Clear current digit on Delete key
			otpDigits[index] = '';
			target.value = '';
			otp = otpDigits.join('');
		}
	}

	function startTimer() {
		timerInterval = setInterval(() => {
			timeLeft--;
			if (timeLeft <= 0) {
				clearInterval(timerInterval);
				// Don't redirect when timer expires - let user resend OTP instead
				toast.show('OTP has expired. Click "Resend Code" to get a new one.', 'info');
			}
		}, 1000);
	}

	function formatTime(seconds: number): string {
		const minutes = Math.floor(seconds / 60);
		const remainingSeconds = seconds % 60;
		return `${minutes}:${remainingSeconds.toString().padStart(2, '0')}`;
	}

	async function handleVerifyOtp(event: SubmitEvent) {
		event.preventDefault();
		
		if (otp.length !== 6) {
			error = 'Please enter a 6-digit OTP code';
			return;
		}

		isLoading = true;
		error = '';
		authStore.setLoading(true);

		try {
			const response = await verifyOtp(email, otp);
			if (response.success && response.user) {
				authStore.setUser({
					...response.user,
					username: response.user.username ?? ''
				});
				toast.show('Login successful', 'success');
				clearInterval(timerInterval);
				await new Promise(res => setTimeout(res, 1000));
				window.location.href = '/dashboard';
			} else {
				error = response.message || 'OTP verification failed';
				authStore.setError(error);
			}
		} catch (err) {
			console.log(`Error: ${err}`);
			error = 'Something went wrong. Please try again.';
			authStore.setError(error);
		} finally {
			isLoading = false;
			authStore.setLoading(false);
		}
	}

	function handleBackToLogin() {
		clearInterval(timerInterval);
		goto('/login');
	}

	async function handleResendOtp() {
		if (!email || isResending) return;
		
		isResending = true;
		error = '';

		try {
			const response = await resendOtp(email);
			if (response.success) {
				toast.show(response.message || 'New OTP sent to your email!', 'success');
				// Reset timer to 10 minutes
				timeLeft = 600;
				// Clear current OTP inputs
				otpDigits = ['', '', '', '', '', ''];
				otp = '';
				// Clear all input fields
				otpInputs.forEach((input, index) => {
					if (input) {
						input.value = '';
					}
				});
				// Focus first input
				setTimeout(() => {
					otpInputs[0]?.focus();
				}, 100);
			} else {
				error = response.message || 'Failed to resend OTP';
				toast.show(error, 'error');
			}
		} catch (err) {
			console.log(`Error: ${err}`);
			error = 'Something went wrong. Please try again.';
			toast.show(error, 'error');
		} finally {
			isResending = false;
		}
	}
</script>

<svelte:head>
	<title>Verify OTP - Vault</title>
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
		<h2 class="mt-3 text-center text-2xl font-semibold text-gray-900">Verify Your Identity</h2>
		<p class="mt-2 text-center text-sm text-gray-600">
			We've sent a 6-digit code to <span class="font-medium text-gray-900">{email}</span>
		</p>
	</div>

	<!-- OTP Verification Form -->
	<div class="mt-5 sm:mx-auto sm:w-full sm:max-w-md">
		<form class="space-y-6 rounded-2xl border border-gray-100 bg-white px-6 py-8 shadow-sm sm:px-10" on:submit={handleVerifyOtp}>
			<!-- Timer Display -->
			<div class="text-center">
				{#if timeLeft > 0}
					<div class="inline-flex items-center rounded-lg bg-amber-50 px-3 py-2 text-sm">
						<svg class="mr-2 h-4 w-4 text-amber-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
							<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"></path>
						</svg>
						<span class="text-amber-800">Code expires in: <span class="font-medium">{formatTime(timeLeft)}</span></span>
					</div>
				{:else}
					<div class="inline-flex items-center rounded-lg bg-red-50 px-3 py-2 text-sm">
						<svg class="mr-2 h-4 w-4 text-red-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
							<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"></path>
						</svg>
						<span class="text-red-800 font-medium">Code has expired - Please request a new one</span>
					</div>
				{/if}
			</div>

			<!-- Error Message -->
			{#if error}
				<div class="rounded-xl border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-600">
					{error}
				</div>
			{/if}

			<!-- OTP Input -->
			<div class="space-y-2">
				<label for="otp-input-0" class="mb-3 block text-sm font-semibold text-gray-800 text-center">
					Verification Code
				</label>
				<div class="flex items-center justify-center space-x-3">
					{#each Array(6) as _, index}
						<input
							id={"otp-input-" + index}
							use:assignOtpInput={index}
							type="text"
							inputmode="numeric"
							pattern="[0-9]*"
							maxlength="1"
							class="h-14 w-12 rounded-xl border-2 border-gray-200 bg-white text-center text-2xl font-mono font-bold transition-all duration-150 focus:border-indigo-500 focus:ring-4 focus:ring-indigo-100 focus:outline-none hover:border-gray-300"
							on:input={(e) => handleOtpInput(e, index)}
							on:keydown={(e) => handleOtpKeydown(e, index)}
							on:paste={(e) => {
								e.preventDefault();
								const pastedData = e.clipboardData?.getData('text').replace(/\D/g, '');
								if (pastedData && pastedData.length <= 6) {
									const digits = pastedData.split('');
									for (let i = 0; i < digits.length && i < 6; i++) {
										otpDigits[i] = digits[i];
										if (otpInputs[i]) otpInputs[i]!.value = digits[i];
									}
									otp = otpDigits.join('');
									// Focus last filled input
									const lastIndex = Math.min(digits.length - 1, 5);
									otpInputs[lastIndex]?.focus();
								}
							}}
						/>
					{/each}
				</div>
				<p class="text-xs text-gray-500 text-center mt-3">Enter the 6-digit code sent to your email</p>
			</div>

			<!-- Submit Button -->
			<div>
				<button
					type="submit"
					disabled={isLoading || otp.length !== 6 || timeLeft <= 0}
					class="flex w-full items-center justify-center rounded-xl bg-indigo-600 px-8 py-3.5 text-sm font-semibold text-white shadow-lg transition-all duration-200 hover:bg-indigo-700 hover:shadow-xl focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50 disabled:hover:bg-indigo-600"
				>
					{#if isLoading}
						<svg class="mr-3 -ml-1 h-5 w-5 animate-spin" fill="none" viewBox="0 0 24 24">
							<circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
							<path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
						</svg>
						Verifying...
					{:else if timeLeft <= 0}
						<svg class="mr-2 h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
							<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"></path>
						</svg>
						Code Expired
					{:else}
						<svg class="mr-2 h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
							<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"></path>
						</svg>
						Verify Code
					{/if}
				</button>
			</div>

			<!-- Actions -->
			<div class="space-y-3">
				<div class="flex items-center justify-center space-x-4 text-sm">
					<button
						type="button"
						on:click={handleResendOtp}
						disabled={isResending || !email || (timeLeft > 0)}
						class="flex items-center font-medium transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
						class:text-indigo-600={timeLeft > 0}
						class:hover:text-indigo-500={timeLeft > 0}
						class:text-green-600={timeLeft <= 0}
						class:hover:text-green-500={timeLeft <= 0 && !isResending}
					>
						{#if isResending}
							<svg class="mr-1 h-3 w-3 animate-spin" fill="none" viewBox="0 0 24 24">
								<circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
								<path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
							</svg>
							Sending...
						{:else if timeLeft <= 0}
							<svg class="mr-1 h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
								<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15"></path>
							</svg>
							Get New Code
						{:else}
							Resend Code
						{/if}
					</button>
					<span class="text-gray-300">|</span>
					<button
						type="button"
						on:click={handleBackToLogin}
						class="font-medium text-gray-600 transition-colors hover:text-gray-500"
					>
						Back to Login
					</button>
				</div>
			</div>

			<!-- Security Note -->
			<div class="rounded-lg border border-blue-200 bg-blue-50 p-3">
				<div class="flex">
					<svg class="h-5 w-5 text-blue-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
						<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"></path>
					</svg>
					<div class="ml-3">
						<p class="text-sm text-blue-800">
							<span class="font-medium">Security tip:</span> 
							Never share this code with anyone. Our team will never ask for your verification code.
						</p>
					</div>
				</div>
			</div>
		</form>
	</div>
</div>

<style>
	/* Additional custom styles if needed */
	input:focus {
		box-shadow: 0 0 0 3px rgba(99, 102, 241, 0.1);
	}
</style>