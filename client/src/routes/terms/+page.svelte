<script lang="ts">
	import { onMount } from 'svelte';
	
	let lastUpdated = 'December 15, 2024';
</script>

<svelte:head>
	<title>Terms of Service - Domurion</title>
	<meta name="description" content="Terms of Service for Domurion password management service. Learn about your rights and responsibilities when using our platform." />
</svelte:head>

<div class="min-h-screen bg-gray-50">
	<!-- Header -->
	<div class="bg-white border-b border-gray-200">
		<div class="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
			<!-- Back Button -->
			<div class="mb-8">
				<button
					on:click={() => history.back()}
					class="inline-flex items-center px-4 py-2 text-sm font-medium text-gray-600 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 hover:text-gray-900 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 transition-colors"
				>
					<svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
						<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7"/>
					</svg>
					Back
				</button>
			</div>
			
			<div class="text-center">
				<h1 class="text-4xl font-bold text-gray-900 mb-4">Terms of Service</h1>
				<p class="text-lg text-gray-600 max-w-2xl mx-auto">
					These terms govern your use of Domurion's password management services. Please read them carefully.
				</p>
				<div class="mt-4 inline-flex items-center px-3 py-1 rounded-full text-sm bg-blue-50 text-blue-700">
					<svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
						<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"/>
					</svg>
					Last updated: {lastUpdated}
				</div>
			</div>
		</div>
	</div>

	<!-- Content -->
	<div class="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
		<div class="bg-white rounded-2xl shadow-sm border border-gray-200 overflow-hidden">
			<div class="p-8 space-y-12">
				
				<!-- Quick Summary -->
				<section class="bg-blue-50 rounded-xl p-6">
					<h2 class="text-xl font-semibold text-blue-900 mb-4 flex items-center">
						<svg class="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
							<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"/>
						</svg>
						Key Points
					</h2>
					<div class="grid md:grid-cols-2 gap-4 text-sm">
						<div class="space-y-2">
							<div class="flex items-center text-blue-800">
								<svg class="w-4 h-4 mr-2 text-blue-600" fill="currentColor" viewBox="0 0 20 20">
									<path fill-rule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clip-rule="evenodd"/>
								</svg>
								You must be 13+ to use Domurion
							</div>
							<div class="flex items-center text-blue-800">
								<svg class="w-4 h-4 mr-2 text-blue-600" fill="currentColor" viewBox="0 0 20 20">
									<path fill-rule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clip-rule="evenodd"/>
								</svg>
								You're responsible for your master password
							</div>
							<div class="flex items-center text-blue-800">
								<svg class="w-4 h-4 mr-2 text-blue-600" fill="currentColor" viewBox="0 0 20 20">
									<path fill-rule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clip-rule="evenodd"/>
								</svg>
								No unlawful or malicious use
							</div>
						</div>
						<div class="space-y-2">
							<div class="flex items-center text-blue-800">
								<svg class="w-4 h-4 mr-2 text-blue-600" fill="currentColor" viewBox="0 0 20 20">
									<path fill-rule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clip-rule="evenodd"/>
								</svg>
								Service provided "as is"
							</div>
							<div class="flex items-center text-blue-800">
								<svg class="w-4 h-4 mr-2 text-blue-600" fill="currentColor" viewBox="0 0 20 20">
									<path fill-rule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clip-rule="evenodd"/>
								</svg>
								Cancel anytime from account settings
							</div>
							<div class="flex items-center text-blue-800">
								<svg class="w-4 h-4 mr-2 text-blue-600" fill="currentColor" viewBox="0 0 20 20">
									<path fill-rule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clip-rule="evenodd"/>
								</svg>
								We can terminate for violations
							</div>
						</div>
					</div>
				</section>

				<!-- Agreement to Terms -->
				<section>
					<h2 class="text-2xl font-bold text-gray-900 mb-6">1. Agreement to Terms</h2>
					
					<div class="space-y-4">
						<p class="text-gray-600">
							By accessing or using Domurion's password management service, website, mobile applications, or browser extensions (collectively, the "Service"), you agree to be bound by these Terms of Service ("Terms"). If you disagree with any part of these Terms, you may not access the Service.
						</p>

						<div class="bg-yellow-50 rounded-lg p-4 border border-yellow-200">
							<p class="text-yellow-800 text-sm">
								<strong>Important:</strong> These Terms constitute a legally binding agreement between you and Domurion, Inc. ("Domurion", "we", "us", or "our"). Please read them carefully before using our Service.
							</p>
						</div>

						<p class="text-gray-600">
							We reserve the right to modify these Terms at any time. Changes will be effective immediately upon posting the revised Terms on our website. Your continued use of the Service after any changes constitutes acceptance of the new Terms.
						</p>
					</div>
				</section>

				<!-- Eligibility -->
				<section>
					<h2 class="text-2xl font-bold text-gray-900 mb-6">2. Eligibility</h2>
					
					<div class="space-y-4">
						<p class="text-gray-600">
							To use the Service, you must meet the following requirements:
						</p>

						<div class="grid md:grid-cols-2 gap-6">
							<div class="border border-gray-200 rounded-lg p-4">
								<h3 class="font-semibold text-gray-800 mb-2 flex items-center">
									<svg class="w-5 h-5 mr-2 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
										<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"/>
									</svg>
									Age Requirement
								</h3>
								<p class="text-gray-600 text-sm">
									You must be at least 13 years old to use the Service. Users under 18 should have parental or guardian consent.
								</p>
							</div>

							<div class="border border-gray-200 rounded-lg p-4">
								<h3 class="font-semibold text-gray-800 mb-2 flex items-center">
									<svg class="w-5 h-5 mr-2 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
										<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"/>
									</svg>
									Legal Capacity
								</h3>
								<p class="text-gray-600 text-sm">
									You must have the legal capacity to enter into a binding contract in your jurisdiction.
								</p>
							</div>

							<div class="border border-gray-200 rounded-lg p-4">
								<h3 class="font-semibold text-gray-800 mb-2 flex items-center">
									<svg class="w-5 h-5 mr-2 text-purple-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
										<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 6l3 1m0 0l-3 9a5.002 5.002 0 006.001 0M6 7l3 9M6 7l6-2m6 2l3-1m-3 1l-3 9a5.002 5.002 0 006.001 0M18 7l3 9m-3-9l-6-2m0-2v2m0 16V5m0 16H9m3 0h3"/>
									</svg>
									Accuracy
								</h3>
								<p class="text-gray-600 text-sm">
									You agree to provide accurate, current, and complete information during registration and use.
								</p>
							</div>

							<div class="border border-gray-200 rounded-lg p-4">
								<h3 class="font-semibold text-gray-800 mb-2 flex items-center">
									<svg class="w-5 h-5 mr-2 text-red-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
										<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.964-.833-2.732 0L3.732 16.5c-.77.833.192 2.5 1.732 2.5z"/>
									</svg>
									No Prohibition
								</h3>
								<p class="text-gray-600 text-sm">
									You must not be prohibited from using the Service under applicable laws or previous violations.
								</p>
							</div>
						</div>
					</div>
				</section>

				<!-- Account Registration -->
				<section>
					<h2 class="text-2xl font-bold text-gray-900 mb-6">3. Account Registration and Security</h2>
					
					<div class="space-y-6">
						<div>
							<h3 class="text-lg font-semibold text-gray-800 mb-3">Account Creation</h3>
							<p class="text-gray-600 mb-3">To access the Service, you must create an account by providing:</p>
							<ul class="space-y-2 text-gray-600">
								<li class="flex items-start">
									<span class="w-2 h-2 bg-blue-500 rounded-full mt-2 mr-3 flex-shrink-0"></span>
									A valid email address
								</li>
								<li class="flex items-start">
									<span class="w-2 h-2 bg-blue-500 rounded-full mt-2 mr-3 flex-shrink-0"></span>
									A strong master password (which we never store in plain text)
								</li>
								<li class="flex items-start">
									<span class="w-2 h-2 bg-blue-500 rounded-full mt-2 mr-3 flex-shrink-0"></span>
									Any additional information requested during registration
								</li>
							</ul>
						</div>

						<div class="bg-red-50 rounded-xl p-6 border border-red-200">
							<h3 class="text-lg font-semibold text-red-800 mb-2 flex items-center">
								<svg class="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
									<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z"/>
								</svg>
								Master Password Responsibility
							</h3>
							<p class="text-red-700 mb-3">
								Your master password is the key to all your encrypted data. Due to our zero-knowledge encryption architecture:
							</p>
							<ul class="space-y-1 text-red-700 text-sm">
								<li class="flex items-center">
									<span class="w-1.5 h-1.5 bg-red-600 rounded-full mr-2"></span>
									We cannot access, reset, or recover your master password
								</li>
								<li class="flex items-center">
									<span class="w-1.5 h-1.5 bg-red-600 rounded-full mr-2"></span>
									If you forget your master password, your data cannot be recovered
								</li>
								<li class="flex items-center">
									<span class="w-1.5 h-1.5 bg-red-600 rounded-full mr-2"></span>
									You are solely responsible for maintaining the confidentiality of your master password
								</li>
								<li class="flex items-center">
									<span class="w-1.5 h-1.5 bg-red-600 rounded-full mr-2"></span>
									You are responsible for all activities that occur under your account
								</li>
							</ul>
						</div>

						<div>
							<h3 class="text-lg font-semibold text-gray-800 mb-3">Account Security</h3>
							<p class="text-gray-600 mb-3">You agree to:</p>
							<ul class="space-y-2 text-gray-600">
								<li class="flex items-start">
									<span class="w-2 h-2 bg-green-500 rounded-full mt-2 mr-3 flex-shrink-0"></span>
									Maintain the security and confidentiality of your account credentials
								</li>
								<li class="flex items-start">
									<span class="w-2 h-2 bg-green-500 rounded-full mt-2 mr-3 flex-shrink-0"></span>
									Notify us immediately of any unauthorized use or security breach
								</li>
								<li class="flex items-start">
									<span class="w-2 h-2 bg-green-500 rounded-full mt-2 mr-3 flex-shrink-0"></span>
									Not share your account credentials with anyone
								</li>
								<li class="flex items-start">
									<span class="w-2 h-2 bg-green-500 rounded-full mt-2 mr-3 flex-shrink-0"></span>
									Enable two-factor authentication (2FA) for enhanced security
								</li>
								<li class="flex items-start">
									<span class="w-2 h-2 bg-green-500 rounded-full mt-2 mr-3 flex-shrink-0"></span>
									Be responsible for all activities under your account, whether authorized or not
								</li>
							</ul>
						</div>
					</div>
				</section>

				<!-- Termination -->
				<section>
					<h2 class="text-2xl font-bold text-gray-900 mb-6">11. Termination</h2>
					
					<div class="space-y-6">
						<div>
							<h3 class="text-lg font-semibold text-gray-800 mb-3">Termination by You</h3>
							<p class="text-gray-600 mb-3">
								You may terminate your account at any time by:
							</p>
							<ul class="space-y-2 text-gray-600">
								<li class="flex items-start">
									<span class="w-2 h-2 bg-blue-500 rounded-full mt-2 mr-3 flex-shrink-0"></span>
									Deleting your account through account settings
								</li>
								<li class="flex items-start">
									<span class="w-2 h-2 bg-blue-500 rounded-full mt-2 mr-3 flex-shrink-0"></span>
									Contacting our support team
								</li>
							</ul>
							<div class="mt-3 p-4 bg-red-50 rounded-lg border border-red-200">
								<p class="text-red-800 text-sm">
									<strong>Warning:</strong> Account deletion is permanent and irreversible. All your vault data will be immediately and permanently deleted from our servers.
								</p>
							</div>
						</div>

						<div>
							<h3 class="text-lg font-semibold text-gray-800 mb-3">Termination by Domurion</h3>
							<p class="text-gray-600 mb-3">
								We may suspend or terminate your account immediately, without prior notice or liability, for any reason, including:
							</p>
							<div class="grid md:grid-cols-2 gap-4">
								<ul class="space-y-2 text-gray-600">
									<li class="flex items-start">
										<span class="w-2 h-2 bg-red-500 rounded-full mt-2 mr-3 flex-shrink-0"></span>
										Violation of these Terms
									</li>
									<li class="flex items-start">
										<span class="w-2 h-2 bg-red-500 rounded-full mt-2 mr-3 flex-shrink-0"></span>
										Fraudulent or illegal activity
									</li>
									<li class="flex items-start">
										<span class="w-2 h-2 bg-red-500 rounded-full mt-2 mr-3 flex-shrink-0"></span>
										Non-payment of fees
									</li>
								</ul>
								<ul class="space-y-2 text-gray-600">
									<li class="flex items-start">
										<span class="w-2 h-2 bg-red-500 rounded-full mt-2 mr-3 flex-shrink-0"></span>
										Harmful behavior toward others
									</li>
									<li class="flex items-start">
										<span class="w-2 h-2 bg-red-500 rounded-full mt-2 mr-3 flex-shrink-0"></span>
										Abuse of the Service
									</li>
									<li class="flex items-start">
										<span class="w-2 h-2 bg-red-500 rounded-full mt-2 mr-3 flex-shrink-0"></span>
										Extended account inactivity
									</li>
								</ul>
							</div>
						</div>

						<div>
							<h3 class="text-lg font-semibold text-gray-800 mb-3">Effects of Termination</h3>
							<p class="text-gray-600 mb-3">
								Upon termination of your account:
							</p>
							<ul class="space-y-2 text-gray-600">
								<li class="flex items-start">
									<span class="w-2 h-2 bg-gray-500 rounded-full mt-2 mr-3 flex-shrink-0"></span>
									Your right to access and use the Service immediately ceases
								</li>
								<li class="flex items-start">
									<span class="w-2 h-2 bg-gray-500 rounded-full mt-2 mr-3 flex-shrink-0"></span>
									Your encrypted vault data will be deleted from our servers
								</li>
								<li class="flex items-start">
									<span class="w-2 h-2 bg-gray-500 rounded-full mt-2 mr-3 flex-shrink-0"></span>
									No refunds will be provided for unused subscription periods (except as required by law)
								</li>
								<li class="flex items-start">
									<span class="w-2 h-2 bg-gray-500 rounded-full mt-2 mr-3 flex-shrink-0"></span>
									Provisions that by their nature should survive termination shall survive
								</li>
							</ul>
						</div>
					</div>
				</section>

				<!-- Data Backup -->
				<section>
					<h2 class="text-2xl font-bold text-gray-900 mb-6">12. Data Backup and Export</h2>
					
					<div class="bg-blue-50 rounded-xl p-6 border border-blue-200">
						<h3 class="text-lg font-semibold text-blue-900 mb-3 flex items-center">
							<svg class="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
								<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.964-.833-2.732 0L3.732 16.5c-.77.833.192 2.5 1.732 2.5z"/>
							</svg>
							Your Backup Responsibility
						</h3>
						<p class="text-blue-800 mb-3">
							While we maintain backups for disaster recovery, <strong>you are solely responsible for maintaining your own backups</strong> of your vault data.
						</p>
						<div class="space-y-2 text-blue-700 text-sm">
							<p class="flex items-start">
								<span class="w-1.5 h-1.5 bg-blue-600 rounded-full mt-1.5 mr-2 flex-shrink-0"></span>
								We recommend regularly exporting your vault data
							</p>
							<p class="flex items-start">
								<span class="w-1.5 h-1.5 bg-blue-600 rounded-full mt-1.5 mr-2 flex-shrink-0"></span>
								Export functionality is available in your account settings
							</p>
							<p class="flex items-start">
								<span class="w-1.5 h-1.5 bg-blue-600 rounded-full mt-1.5 mr-2 flex-shrink-0"></span>
								Store exported data securely in multiple locations
							</p>
							<p class="flex items-start">
								<span class="w-1.5 h-1.5 bg-blue-600 rounded-full mt-1.5 mr-2 flex-shrink-0"></span>
								We are not liable for any data loss
							</p>
						</div>
					</div>
				</section>

				<!-- Dispute Resolution -->
				<section>
					<h2 class="text-2xl font-bold text-gray-900 mb-6">13. Dispute Resolution</h2>
					
					<div class="space-y-6">
						<div>
							<h3 class="text-lg font-semibold text-gray-800 mb-3">Informal Resolution</h3>
							<p class="text-gray-600">
								If you have a dispute with Domurion, you agree to first contact us at legal@domurion.com and attempt to resolve the dispute informally. We'll work in good faith to resolve any issues.
							</p>
						</div>

						<div>
							<h3 class="text-lg font-semibold text-gray-800 mb-3">Binding Arbitration</h3>
							<div class="bg-yellow-50 rounded-lg p-4 border border-yellow-200 mb-3">
								<p class="text-yellow-800 text-sm mb-2">
									<strong>Arbitration Agreement:</strong> You agree that disputes arising from these Terms or the Service will be resolved through binding arbitration rather than in court, except as specified below.
								</p>
							</div>
							<p class="text-gray-600 mb-3">
								Arbitration provisions:
							</p>
							<ul class="space-y-2 text-gray-600">
								<li class="flex items-start">
									<span class="w-2 h-2 bg-blue-500 rounded-full mt-2 mr-3 flex-shrink-0"></span>
									Arbitration will be conducted by the American Arbitration Association (AAA) under its Commercial Arbitration Rules
								</li>
								<li class="flex items-start">
									<span class="w-2 h-2 bg-blue-500 rounded-full mt-2 mr-3 flex-shrink-0"></span>
									The arbitration will take place in the United States
								</li>
								<li class="flex items-start">
									<span class="w-2 h-2 bg-blue-500 rounded-full mt-2 mr-3 flex-shrink-0"></span>
									The arbitrator's decision will be binding and may be entered as a judgment in any court
								</li>
								<li class="flex items-start">
									<span class="w-2 h-2 bg-blue-500 rounded-full mt-2 mr-3 flex-shrink-0"></span>
									Each party will bear its own costs and attorneys' fees unless otherwise awarded by the arbitrator
								</li>
							</ul>
						</div>

						<div>
							<h3 class="text-lg font-semibold text-gray-800 mb-3">Exceptions to Arbitration</h3>
							<p class="text-gray-600 mb-3">
								Either party may seek relief in court for:
							</p>
							<ul class="space-y-2 text-gray-600">
								<li class="flex items-start">
									<span class="w-2 h-2 bg-green-500 rounded-full mt-2 mr-3 flex-shrink-0"></span>
									Intellectual property disputes
								</li>
								<li class="flex items-start">
									<span class="w-2 h-2 bg-green-500 rounded-full mt-2 mr-3 flex-shrink-0"></span>
									Small claims court matters (if claim qualifies)
								</li>
								<li class="flex items-start">
									<span class="w-2 h-2 bg-green-500 rounded-full mt-2 mr-3 flex-shrink-0"></span>
									Injunctive or equitable relief
								</li>
							</ul>
						</div>

						<div>
							<h3 class="text-lg font-semibold text-gray-800 mb-3">Class Action Waiver</h3>
							<div class="bg-red-50 rounded-lg p-4 border border-red-200">
								<p class="text-red-800 text-sm">
									<strong>No Class Actions:</strong> You agree that disputes will be resolved on an individual basis only. You waive any right to participate in a class action lawsuit or class-wide arbitration.
								</p>
							</div>
						</div>
					</div>
				</section>

				<!-- Governing Law -->
				<section>
					<h2 class="text-2xl font-bold text-gray-900 mb-6">14. Governing Law and Jurisdiction</h2>
					
					<div class="space-y-4">
						<p class="text-gray-600">
							These Terms shall be governed by and construed in accordance with the laws of the State of Delaware, United States, without regard to its conflict of law provisions.
						</p>
						<p class="text-gray-600">
							To the extent arbitration does not apply, you agree to submit to the personal and exclusive jurisdiction of the state and federal courts located in Delaware for resolution of any disputes.
						</p>
						<div class="bg-blue-50 rounded-lg p-4 border border-blue-200">
							<p class="text-blue-800 text-sm">
								<strong>International Users:</strong> If you access the Service from outside the United States, you do so at your own risk and are responsible for compliance with local laws.
							</p>
						</div>
					</div>
				</section>

				<!-- General Provisions -->
				<section>
					<h2 class="text-2xl font-bold text-gray-900 mb-6">15. General Provisions</h2>
					
					<div class="space-y-6">
						<div class="grid md:grid-cols-2 gap-6">
							<div class="border border-gray-200 rounded-lg p-4">
								<h3 class="font-semibold text-gray-800 mb-2">Entire Agreement</h3>
								<p class="text-gray-600 text-sm">
									These Terms, along with our Privacy Policy, constitute the entire agreement between you and Domurion regarding the Service.
								</p>
							</div>

							<div class="border border-gray-200 rounded-lg p-4">
								<h3 class="font-semibold text-gray-800 mb-2">Severability</h3>
								<p class="text-gray-600 text-sm">
									If any provision is found unenforceable, the remaining provisions will remain in full force and effect.
								</p>
							</div>

							<div class="border border-gray-200 rounded-lg p-4">
								<h3 class="font-semibold text-gray-800 mb-2">Waiver</h3>
								<p class="text-gray-600 text-sm">
									Our failure to enforce any right or provision does not constitute a waiver of such right or provision.
								</p>
							</div>

							<div class="border border-gray-200 rounded-lg p-4">
								<h3 class="font-semibold text-gray-800 mb-2">Assignment</h3>
								<p class="text-gray-600 text-sm">
									You may not assign these Terms without our consent. We may assign these Terms without restriction.
								</p>
							</div>

							<div class="border border-gray-200 rounded-lg p-4">
								<h3 class="font-semibold text-gray-800 mb-2">Force Majeure</h3>
								<p class="text-gray-600 text-sm">
									We are not liable for delays or failures due to events beyond our reasonable control.
								</p>
							</div>

							<div class="border border-gray-200 rounded-lg p-4">
								<h3 class="font-semibold text-gray-800 mb-2">Export Control</h3>
								<p class="text-gray-600 text-sm">
									You agree to comply with all applicable export and import laws and regulations.
								</p>
							</div>
						</div>

						<div>
							<h3 class="text-lg font-semibold text-gray-800 mb-3">Electronic Communications</h3>
							<p class="text-gray-600">
								By using the Service, you consent to receive electronic communications from us. These communications may include notices, agreements, disclosures, or other information. You agree that all such communications satisfy any legal requirement that communications be in writing.
							</p>
						</div>

						<div>
							<h3 class="text-lg font-semibold text-gray-800 mb-3">Third-Party Beneficiaries</h3>
							<p class="text-gray-600">
								There are no third-party beneficiaries to these Terms. These Terms do not create any rights for any person who is not a party to them.
							</p>
						</div>

						<div>
							<h3 class="text-lg font-semibold text-gray-800 mb-3">Interpretation</h3>
							<p class="text-gray-600">
								Section headings are for convenience only and do not affect interpretation. The words "include," "includes," and "including" mean "including without limitation."
							</p>
						</div>
					</div>
				</section>

				<!-- Contact Information -->
				<section>
					<h2 class="text-2xl font-bold text-gray-900 mb-6">16. Contact Information</h2>
					
					<div class="bg-gradient-to-r from-blue-50 to-indigo-50 rounded-xl p-6">
						<p class="text-gray-700 mb-6">
							If you have questions about these Terms of Service, please contact us:
						</p>

						<div class="grid md:grid-cols-2 gap-6">
							<div class="space-y-4">
								<div class="flex items-start space-x-3">
									<svg class="w-5 h-5 text-blue-600 mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
										<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 8l7.89 4.26a2 2 0 002.22 0L21 8M5 19h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z"/>
									</svg>
									<div>
										<h3 class="font-semibold text-gray-800">Legal Inquiries</h3>
										<a href="mailto:legal@domurion.com" class="text-blue-600 hover:text-blue-700">legal@domurion.com</a>
									</div>
								</div>

								<div class="flex items-start space-x-3">
									<svg class="w-5 h-5 text-blue-600 mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
										<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M7 8h10M7 12h4m1 8l-4-4H5a2 2 0 01-2-2V6a2 2 0 012-2h14a2 2 0 012 2v8a2 2 0 01-2 2h-3l-4 4z"/>
									</svg>
									<div>
										<h3 class="font-semibold text-gray-800">General Support</h3>
										<a href="mailto:support@domurion.com" class="text-blue-600 hover:text-blue-700">support@domurion.com</a>
									</div>
								</div>
							</div>

							<div class="space-y-4">
								<div class="flex items-start space-x-3">
									<svg class="w-5 h-5 text-blue-600 mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
										<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z"/>
										<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 11a3 3 0 11-6 0 3 3 0 016 0z"/>
									</svg>
									<div>
										<h3 class="font-semibold text-gray-800">Mailing Address</h3>
										<p class="text-gray-600 text-sm">
											Domurion, Inc.<br>
											123 Security Street<br>
											Privacy City, PC 12345<br>
											United States
										</p>
									</div>
								</div>
							</div>
						</div>

						<div class="mt-6 p-4 bg-white rounded-lg border border-blue-200">
							<p class="text-blue-800 text-sm">
								<strong>Business Hours:</strong> Our legal team responds to inquiries Monday through Friday, 9 AM - 5 PM EST, excluding federal holidays.
							</p>
						</div>
					</div>
				</section>

				<!-- Acknowledgment -->
				<section class="bg-gray-50 rounded-xl p-6 border-2 border-gray-300">
					<h2 class="text-xl font-bold text-gray-900 mb-4 flex items-center">
						<svg class="w-6 h-6 mr-2 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
							<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"/>
						</svg>
						Acknowledgment
					</h2>
					<p class="text-gray-700 mb-4">
						BY CLICKING "I AGREE," CREATING AN ACCOUNT, OR USING THE SERVICE, YOU ACKNOWLEDGE THAT:
					</p>
					<ul class="space-y-2 text-gray-700">
						<li class="flex items-start">
							<span class="w-2 h-2 bg-blue-600 rounded-full mt-2 mr-3 flex-shrink-0"></span>
							You have read and understood these Terms of Service
						</li>
						<li class="flex items-start">
							<span class="w-2 h-2 bg-blue-600 rounded-full mt-2 mr-3 flex-shrink-0"></span>
							You agree to be bound by these Terms
						</li>
						<li class="flex items-start">
							<span class="w-2 h-2 bg-blue-600 rounded-full mt-2 mr-3 flex-shrink-0"></span>
							You understand the limitations of liability and warranty disclaimers
						</li>
						<li class="flex items-start">
							<span class="w-2 h-2 bg-blue-600 rounded-full mt-2 mr-3 flex-shrink-0"></span>
							You acknowledge your responsibility for your master password
						</li>
						<li class="flex items-start">
							<span class="w-2 h-2 bg-blue-600 rounded-full mt-2 mr-3 flex-shrink-0"></span>
							You agree to the arbitration and class action waiver provisions
						</li>
					</ul>
				</section>
			</div>
		</div>

		<!-- Footer -->
		<div class="mt-12 text-center text-gray-500">
			<p class="text-sm">
				These Terms of Service are effective as of {lastUpdated} and apply to all users of Domurion services.
			</p>
			<p class="text-xs mt-2 text-gray-400">
				Domurion® is a registered trademark of Domurion, Inc. All rights reserved.
			</p>
			<div class="mt-6 flex justify-center space-x-6 text-sm">
				<a href="/privacy" class="hover:text-gray-700 transition-colors">Privacy Policy</a>
				<a href="/security" class="hover:text-gray-700 transition-colors">Security</a>
				<a href="/acceptable-use" class="hover:text-gray-700 transition-colors">Acceptable Use Policy</a>
				<a href="/help" class="hover:text-gray-700 transition-colors">Help Center</a>
			</div>
		</div>
	</div>
</div>

<style>
	/* Smooth scrolling for anchor links */
	:global(html) {
		scroll-behavior: smooth;
	}

	/* Custom scrollbar */
	::-webkit-scrollbar {
		width: 6px;
	}

	::-webkit-scrollbar-track {
		background: #f1f5f9;
	}

	::-webkit-scrollbar-thumb {
		background: #cbd5e1;
		border-radius: 3px;
	}

	::-webkit-scrollbar-thumb:hover {
		background: #94a3b8;
	}

	/* Print styles */
	@media print {
		/* No unused selectors */
	}
</style>
