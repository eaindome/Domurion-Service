<script lang="ts">
	import { onMount } from 'svelte';
	
	let lastUpdated = 'December 15, 2024';
</script>

<svelte:head>
	<title>Privacy Policy - Domurion</title>
	<meta name="description" content="Learn how Domurion protects your privacy and handles your personal data in our password management service." />
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
				<h1 class="text-4xl font-bold text-gray-900 mb-4">Privacy Policy</h1>
				<p class="text-lg text-gray-600 max-w-2xl mx-auto">
					Your privacy is fundamental to how we operate. This policy explains how we collect, use, and protect your personal information.
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
						Privacy at a Glance
					</h2>
					<div class="grid md:grid-cols-2 gap-4 text-sm">
						<div class="space-y-2">
							<div class="flex items-center text-blue-800">
								<svg class="w-4 h-4 mr-2 text-green-600" fill="currentColor" viewBox="0 0 20 20">
									<path fill-rule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clip-rule="evenodd"/>
								</svg>
								Zero-knowledge encryption
							</div>
							<div class="flex items-center text-blue-800">
								<svg class="w-4 h-4 mr-2 text-green-600" fill="currentColor" viewBox="0 0 20 20">
									<path fill-rule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clip-rule="evenodd"/>
								</svg>
								No password data sold
							</div>
							<div class="flex items-center text-blue-800">
								<svg class="w-4 h-4 mr-2 text-green-600" fill="currentColor" viewBox="0 0 20 20">
									<path fill-rule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clip-rule="evenodd"/>
								</svg>
								GDPR & CCPA compliant
							</div>
						</div>
						<div class="space-y-2">
							<div class="flex items-center text-blue-800">
								<svg class="w-4 h-4 mr-2 text-green-600" fill="currentColor" viewBox="0 0 20 20">
									<path fill-rule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clip-rule="evenodd"/>
								</svg>
								End-to-end encryption
							</div>
							<div class="flex items-center text-blue-800">
								<svg class="w-4 h-4 mr-2 text-green-600" fill="currentColor" viewBox="0 0 20 20">
									<path fill-rule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clip-rule="evenodd"/>
								</svg>
								Data deletion rights
							</div>
							<div class="flex items-center text-blue-800">
								<svg class="w-4 h-4 mr-2 text-green-600" fill="currentColor" viewBox="0 0 20 20">
									<path fill-rule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clip-rule="evenodd"/>
								</svg>
								Minimal data collection
							</div>
						</div>
					</div>
				</section>

				<!-- Information We Collect -->
				<section>
					<h2 class="text-2xl font-bold text-gray-900 mb-6">1. Information We Collect</h2>
					
					<div class="space-y-6">
						<div>
							<h3 class="text-lg font-semibold text-gray-800 mb-3">Account Information</h3>
							<p class="text-gray-600 mb-3">When you create a Domurion account, we collect:</p>
							<ul class="space-y-2 text-gray-600">
								<li class="flex items-start">
									<span class="w-2 h-2 bg-blue-500 rounded-full mt-2 mr-3 flex-shrink-0"></span>
									Email address (used for account identification and communication)
								</li>
								<li class="flex items-start">
									<span class="w-2 h-2 bg-blue-500 rounded-full mt-2 mr-3 flex-shrink-0"></span>
									Master password hash (never stored in plain text)
								</li>
								<li class="flex items-start">
									<span class="w-2 h-2 bg-blue-500 rounded-full mt-2 mr-3 flex-shrink-0"></span>
									Account creation and last access dates
								</li>
							</ul>
						</div>

						<div>
							<h3 class="text-lg font-semibold text-gray-800 mb-3">Vault Data</h3>
							<p class="text-gray-600 mb-3">Your password vault contains encrypted data that includes:</p>
							<ul class="space-y-2 text-gray-600">
								<li class="flex items-start">
									<span class="w-2 h-2 bg-blue-500 rounded-full mt-2 mr-3 flex-shrink-0"></span>
									Website names and URLs
								</li>
								<li class="flex items-start">
									<span class="w-2 h-2 bg-blue-500 rounded-full mt-2 mr-3 flex-shrink-0"></span>
									Usernames and passwords (encrypted with your master key)
								</li>
								<li class="flex items-start">
									<span class="w-2 h-2 bg-blue-500 rounded-full mt-2 mr-3 flex-shrink-0"></span>
									Notes and custom fields
								</li>
							</ul>
							<div class="mt-4 p-4 bg-green-50 rounded-lg border border-green-200">
								<p class="text-green-800 text-sm">
									<strong>Important:</strong> All vault data is encrypted using zero-knowledge encryption. We cannot read, access, or decrypt your passwords and sensitive information.
								</p>
							</div>
						</div>

						<div>
							<h3 class="text-lg font-semibold text-gray-800 mb-3">Usage Information</h3>
							<p class="text-gray-600 mb-3">To improve our service, we may collect:</p>
							<ul class="space-y-2 text-gray-600">
								<li class="flex items-start">
									<span class="w-2 h-2 bg-blue-500 rounded-full mt-2 mr-3 flex-shrink-0"></span>
									Device and browser information
								</li>
								<li class="flex items-start">
									<span class="w-2 h-2 bg-blue-500 rounded-full mt-2 mr-3 flex-shrink-0"></span>
									IP addresses and general location data
								</li>
								<li class="flex items-start">
									<span class="w-2 h-2 bg-blue-500 rounded-full mt-2 mr-3 flex-shrink-0"></span>
									Feature usage patterns (anonymized)
								</li>
							</ul>
						</div>
					</div>
				</section>

				<!-- How We Use Information -->
				<section>
					<h2 class="text-2xl font-bold text-gray-900 mb-6">2. How We Use Your Information</h2>
					
					<div class="grid md:grid-cols-2 gap-6">
						<div class="space-y-4">
							<div class="flex items-start space-x-3">
								<div class="flex-shrink-0">
									<div class="w-8 h-8 bg-blue-100 rounded-lg flex items-center justify-center">
										<svg class="w-4 h-4 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
											<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z"/>
										</svg>
									</div>
								</div>
								<div>
									<h3 class="font-semibold text-gray-800">Provide Security Services</h3>
									<p class="text-gray-600 text-sm">Authenticate access, sync data across devices, and maintain vault integrity.</p>
								</div>
							</div>

							<div class="flex items-start space-x-3">
								<div class="flex-shrink-0">
									<div class="w-8 h-8 bg-green-100 rounded-lg flex items-center justify-center">
										<svg class="w-4 h-4 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
											<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"/>
										</svg>
									</div>
								</div>
								<div>
									<h3 class="font-semibold text-gray-800">Improve Our Service</h3>
									<p class="text-gray-600 text-sm">Analyze usage patterns to enhance features and fix bugs.</p>
								</div>
							</div>

							<div class="flex items-start space-x-3">
								<div class="flex-shrink-0">
									<div class="w-8 h-8 bg-purple-100 rounded-lg flex items-center justify-center">
										<svg class="w-4 h-4 text-purple-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
											<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M7 8h10M7 12h4m1 8l-4-4H5a2 2 0 01-2-2V6a2 2 0 012-2h14a2 2 0 012 2v8a2 2 0 01-2 2h-3l-4 4z"/>
										</svg>
									</div>
								</div>
								<div>
									<h3 class="font-semibold text-gray-800">Customer Support</h3>
									<p class="text-gray-600 text-sm">Respond to inquiries and provide technical assistance.</p>
								</div>
							</div>
						</div>

						<div class="space-y-4">
							<div class="flex items-start space-x-3">
								<div class="flex-shrink-0">
									<div class="w-8 h-8 bg-red-100 rounded-lg flex items-center justify-center">
										<svg class="w-4 h-4 text-red-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
											<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.964-.833-2.732 0L3.732 16.5c-.77.833.192 2.5 1.732 2.5z"/>
										</svg>
									</div>
								</div>
								<div>
									<h3 class="font-semibold text-gray-800">Security Monitoring</h3>
									<p class="text-gray-600 text-sm">Detect and prevent fraudulent activity and security threats.</p>
								</div>
							</div>

							<div class="flex items-start space-x-3">
								<div class="flex-shrink-0">
									<div class="w-8 h-8 bg-yellow-100 rounded-lg flex items-center justify-center">
										<svg class="w-4 h-4 text-yellow-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
											<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 8l7.89 4.26a2 2 0 002.22 0L21 8M5 19h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z"/>
										</svg>
									</div>
								</div>
								<div>
									<h3 class="font-semibold text-gray-800">Essential Communications</h3>
									<p class="text-gray-600 text-sm">Send important security updates and service notifications.</p>
								</div>
							</div>

							<div class="flex items-start space-x-3">
								<div class="flex-shrink-0">
									<div class="w-8 h-8 bg-indigo-100 rounded-lg flex items-center justify-center">
										<svg class="w-4 h-4 text-indigo-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
											<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5H7a2 2 0 00-2 2v10a2 2 0 002 2h8a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2"/>
										</svg>
									</div>
								</div>
								<div>
									<h3 class="font-semibold text-gray-800">Legal Compliance</h3>
									<p class="text-gray-600 text-sm">Comply with legal obligations and regulatory requirements.</p>
								</div>
							</div>
						</div>
					</div>
				</section>

				<!-- Data Protection -->
				<section>
					<h2 class="text-2xl font-bold text-gray-900 mb-6">3. How We Protect Your Data</h2>
					
					<div class="bg-gradient-to-r from-blue-50 to-indigo-50 rounded-xl p-6 mb-6">
						<h3 class="text-lg font-semibold text-gray-800 mb-4">Zero-Knowledge Architecture</h3>
						<p class="text-gray-700 mb-4">
							Domurion is built on a zero-knowledge security model. This means that even we cannot access your vault data:
						</p>
						<div class="grid md:grid-cols-2 gap-4 text-sm">
							<div class="space-y-2">
								<div class="flex items-center text-gray-700">
									<svg class="w-4 h-4 mr-2 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
										<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z"/>
									</svg>
									Client-side encryption only
								</div>
								<div class="flex items-center text-gray-700">
									<svg class="w-4 h-4 mr-2 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
										<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"/>
									</svg>
									Your master key never leaves your device
								</div>
							</div>
							<div class="space-y-2">
								<div class="flex items-center text-gray-700">
									<svg class="w-4 h-4 mr-2 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
										<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m5.018.941l2.019 2.019M7.05 14.95l2.019 2.019"/>
									</svg>
									AES-256 encryption standard
								</div>
								<div class="flex items-center text-gray-700">
									<svg class="w-4 h-4 mr-2 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
										<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 3v4M3 5h4M6 17v4m-2-2h4m5-16l2.286 6.857L21 12l-5.714 2.143L13 21l-2.286-6.857L5 12l5.714-2.143L13 3z"/>
									</svg>
									End-to-end encryption
								</div>
							</div>
						</div>
					</div>

					<div class="space-y-4">
						<div>
							<h3 class="text-lg font-semibold text-gray-800 mb-3">Additional Security Measures</h3>
							<ul class="space-y-2 text-gray-600">
								<li class="flex items-start">
									<span class="w-2 h-2 bg-green-500 rounded-full mt-2 mr-3 flex-shrink-0"></span>
									Multi-factor authentication (2FA) support
								</li>
								<li class="flex items-start">
									<span class="w-2 h-2 bg-green-500 rounded-full mt-2 mr-3 flex-shrink-0"></span>
									Regular security audits and penetration testing
								</li>
								<li class="flex items-start">
									<span class="w-2 h-2 bg-green-500 rounded-full mt-2 mr-3 flex-shrink-0"></span>
									SOC 2 Type II compliant infrastructure
								</li>
								<li class="flex items-start">
									<span class="w-2 h-2 bg-green-500 rounded-full mt-2 mr-3 flex-shrink-0"></span>
									Encrypted data transmission (TLS 1.3)
								</li>
								<li class="flex items-start">
									<span class="w-2 h-2 bg-green-500 rounded-full mt-2 mr-3 flex-shrink-0"></span>
									Secure data centers with 24/7 monitoring
								</li>
							</ul>
						</div>
					</div>
				</section>

				<!-- Data Sharing -->
				<section>
					<h2 class="text-2xl font-bold text-gray-900 mb-6">4. Information Sharing and Disclosure</h2>
					
					<div class="bg-red-50 rounded-xl p-6 mb-6 border border-red-200">
						<h3 class="text-lg font-semibold text-red-800 mb-2">We Never Sell Your Data</h3>
						<p class="text-red-700">
							Domurion does not sell, rent, or trade your personal information to third parties for marketing purposes. Your data is not a product.
						</p>
					</div>

					<div class="space-y-6">
						<div>
							<h3 class="text-lg font-semibold text-gray-800 mb-3">Limited Sharing Scenarios</h3>
							<p class="text-gray-600 mb-4">We may share information only in these specific circumstances:</p>
							
							<div class="space-y-4">
								<div class="border border-gray-200 rounded-lg p-4">
									<h4 class="font-medium text-gray-800 mb-2">Service Providers</h4>
									<p class="text-gray-600 text-sm">
										Trusted third-party services that help us operate (cloud hosting, payment processing, customer support). These providers are bound by strict confidentiality agreements.
									</p>
								</div>

								<div class="border border-gray-200 rounded-lg p-4">
									<h4 class="font-medium text-gray-800 mb-2">Legal Requirements</h4>
									<p class="text-gray-600 text-sm">
										When required by law, court order, or government request. We will notify you unless prohibited by law.
									</p>
								</div>

								<div class="border border-gray-200 rounded-lg p-4">
									<h4 class="font-medium text-gray-800 mb-2">Business Transfers</h4>
									<p class="text-gray-600 text-sm">
										In the event of a merger, acquisition, or sale of assets, your information may be transferred. You will be notified of any such change.
									</p>
								</div>

								<div class="border border-gray-200 rounded-lg p-4">
									<h4 class="font-medium text-gray-800 mb-2">Safety and Security</h4>
									<p class="text-gray-600 text-sm">
										To protect the rights, property, or safety of Domurion, our users, or the public as required or permitted by law.
									</p>
								</div>
							</div>
						</div>
					</div>
				</section>

				<!-- Your Rights -->
				<section>
					<h2 class="text-2xl font-bold text-gray-900 mb-6">5. Your Privacy Rights</h2>
					
					<div class="grid md:grid-cols-2 gap-6">
						<div class="space-y-4">
							<div class="border border-gray-200 rounded-lg p-4">
								<h3 class="font-semibold text-gray-800 mb-2 flex items-center">
									<svg class="w-5 h-5 mr-2 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
										<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"/>
									</svg>
									Access Your Data
								</h3>
								<p class="text-gray-600 text-sm">Request a copy of the personal information we have about you.</p>
							</div>

							<div class="border border-gray-200 rounded-lg p-4">
								<h3 class="font-semibold text-gray-800 mb-2 flex items-center">
									<svg class="w-5 h-5 mr-2 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
										<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z"/>
									</svg>
									Correct Your Data
								</h3>
								<p class="text-gray-600 text-sm">Update or correct any inaccurate personal information.</p>
							</div>

							<div class="border border-gray-200 rounded-lg p-4">
								<h3 class="font-semibold text-gray-800 mb-2 flex items-center">
									<svg class="w-5 h-5 mr-2 text-red-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
										<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"/>
									</svg>
									Delete Your Data
								</h3>
								<p class="text-gray-600 text-sm">Request deletion of your personal information (right to be forgotten).</p>
							</div>
						</div>

						<div class="space-y-4">
							<div class="border border-gray-200 rounded-lg p-4">
								<h3 class="font-semibold text-gray-800 mb-2 flex items-center">
									<svg class="w-5 h-5 mr-2 text-purple-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
										<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 10v6m0 0l-3-3m3 3l3-3M3 17V7a2 2 0 012-2h6l2 2h6a2 2 0 012 2v8a2 2 0 01-2 2H5a2 2 0 01-2-2z"/>
									</svg>
									Data Portability
								</h3>
								<p class="text-gray-600 text-sm">Export your vault data in a standard format for transfer to another service.</p>
							</div>

							<div class="border border-gray-200 rounded-lg p-4">
								<h3 class="font-semibold text-gray-800 mb-2 flex items-center">
									<svg class="w-5 h-5 mr-2 text-yellow-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
										<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M18.364 18.364A9 9 0 005.636 5.636m12.728 12.728L5.636 5.636m12.728 12.728L18 18M5.636 5.636L6 6"/>
									</svg>
									Opt-Out
								</h3>
								<p class="text-gray-600 text-sm">Unsubscribe from marketing communications and limit data processing.</p>
							</div>

							<div class="border border-gray-200 rounded-lg p-4">
								<h3 class="font-semibold text-gray-800 mb-2 flex items-center">
									<svg class="w-5 h-5 mr-2 text-indigo-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
										<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"/>
									</svg>
									File Complaints
								</h3>
								<p class="text-gray-600 text-sm">Lodge complaints with data protection authorities in your jurisdiction.</p>
							</div>
						</div>
					</div>

					<div class="mt-6 p-4 bg-blue-50 rounded-lg border border-blue-200">
						<p class="text-blue-800 text-sm">
							<strong>To exercise your rights:</strong> Contact us at privacy@domurion.com or through your account settings. We will respond within 30 days.
						</p>
					</div>
				</section>

				<!-- Data Retention -->
				<section>
					<h2 class="text-2xl font-bold text-gray-900 mb-6">6. Data Retention</h2>
					
					<div class="space-y-4">
						<p class="text-gray-600">
							We retain your personal information only as long as necessary to provide our services and fulfill the purposes outlined in this policy.
						</p>

						<div class="grid md:grid-cols-3 gap-4">
							<div class="bg-gray-50 rounded-lg p-4">
								<h3 class="font-semibold text-gray-800 mb-2">Active Accounts</h3>
								<p class="text-gray-600 text-sm">Data retained while your account is active and for service provision.</p>
							</div>

							<div class="bg-gray-50 rounded-lg p-4">
								<h3 class="font-semibold text-gray-800 mb-2">Inactive Accounts</h3>
								<p class="text-gray-600 text-sm">Account data deleted after 2 years of inactivity (with prior notice).</p>
							</div>

							<div class="bg-gray-50 rounded-lg p-4">
								<h3 class="font-semibold text-gray-800 mb-2">Legal Requirements</h3>
								<p class="text-gray-600 text-sm">Some data may be retained longer to comply with legal obligations.</p>
							</div>
						</div>

						<div class="p-4 bg-green-50 rounded-lg border border-green-200">
							<p class="text-green-800 text-sm">
								<strong>Account Deletion:</strong> When you delete your account, all vault data is immediately and permanently deleted from our servers. This action cannot be undone.
							</p>
						</div>
					</div>
				</section>

				<!-- International Transfers -->
				<section>
					<h2 class="text-2xl font-bold text-gray-900 mb-6">7. International Data Transfers</h2>
					
					<div class="space-y-4">
						<p class="text-gray-600">
							Domurion operates globally and may transfer your personal information to countries outside your residence. We ensure adequate protection through:
						</p>

						<div class="grid md:grid-cols-2 gap-6">
							<div class="space-y-3">
								<div class="flex items-start space-x-3">
									<svg class="w-5 h-5 text-blue-600 mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
										<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"/>
									</svg>
									<div>
										<h3 class="font-semibold text-gray-800">Adequacy Decisions</h3>
										<p class="text-gray-600 text-sm">Transfers to countries recognized as providing adequate protection.</p>
									</div>
								</div>

								<div class="flex items-start space-x-3">
									<svg class="w-5 h-5 text-blue-600 mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
										<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"/>
									</svg>
									<div>
										<h3 class="font-semibold text-gray-800">Standard Contractual Clauses</h3>
										<p class="text-gray-600 text-sm">EU-approved contracts ensuring data protection standards.</p>
									</div>
								</div>
							</div>

							<div class="space-y-3">
								<div class="flex items-start space-x-3">
									<svg class="w-5 h-5 text-blue-600 mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
										<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"/>
									</svg>
									<div>
										<h3 class="font-semibold text-gray-800">Privacy Shield Alternatives</h3>
										<p class="text-gray-600 text-sm">Equivalent protections for US-EU data transfers.</p>
									</div>
								</div>

								<div class="flex items-start space-x-3">
									<svg class="w-5 h-5 text-blue-600 mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
										<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"/>
									</svg>
									<div>
										<h3 class="font-semibold text-gray-800">Technical Safeguards</h3>
										<p class="text-gray-600 text-sm">End-to-end encryption ensures data protection regardless of location.</p>
									</div>
								</div>
							</div>
						</div>
					</div>
				</section>

				<!-- Children's Privacy -->
				<section>
					<h2 class="text-2xl font-bold text-gray-900 mb-6">8. Children's Privacy</h2>
					
					<div class="bg-yellow-50 rounded-xl p-6 border border-yellow-200">
						<p class="text-yellow-800 mb-4">
							<strong>Age Restriction:</strong> Domurion is not intended for children under 13 years of age. We do not knowingly collect personal information from children under 13.
						</p>
						<p class="text-yellow-700 text-sm">
							If we become aware that we have collected personal information from a child under 13, we will take steps to delete such information promptly. If you believe a child under 13 has provided us with personal information, please contact us immediately.
						</p>
					</div>
				</section>

				<!-- Changes to Policy -->
				<section>
					<h2 class="text-2xl font-bold text-gray-900 mb-6">9. Changes to This Privacy Policy</h2>
					
					<div class="space-y-4">
						<p class="text-gray-600">
							We may update this Privacy Policy from time to time to reflect changes in our practices, technology, legal requirements, or other factors.
						</p>

						<div class="bg-blue-50 rounded-lg p-4 border border-blue-200">
							<h3 class="font-semibold text-blue-800 mb-2">How We Notify You</h3>
							<ul class="space-y-1 text-blue-700 text-sm">
								<li class="flex items-center">
									<span class="w-1.5 h-1.5 bg-blue-600 rounded-full mr-2"></span>
									Email notification for significant changes
								</li>
								<li class="flex items-center">
									<span class="w-1.5 h-1.5 bg-blue-600 rounded-full mr-2"></span>
									In-app notifications
								</li>
								<li class="flex items-center">
									<span class="w-1.5 h-1.5 bg-blue-600 rounded-full mr-2"></span>
									Updated date at the top of this policy
								</li>
								<li class="flex items-center">
									<span class="w-1.5 h-1.5 bg-blue-600 rounded-full mr-2"></span>
									30-day notice period for material changes
								</li>
							</ul>
						</div>

						<p class="text-gray-600 text-sm">
							Your continued use of Domurion after any changes constitutes acceptance of the updated Privacy Policy. If you disagree with changes, you may close your account.
						</p>
					</div>
				</section>

				<!-- Contact Information -->
				<section>
					<h2 class="text-2xl font-bold text-gray-900 mb-6">10. Contact Us</h2>
					
					<div class="bg-gradient-to-r from-blue-50 to-indigo-50 rounded-xl p-6">
						<p class="text-gray-700 mb-6">
							If you have questions, concerns, or requests regarding this Privacy Policy or our data practices, please contact us:
						</p>

						<div class="grid md:grid-cols-2 gap-6">
							<div class="space-y-4">
								<div class="flex items-start space-x-3">
									<svg class="w-5 h-5 text-blue-600 mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
										<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 8l7.89 4.26a2 2 0 002.22 0L21 8M5 19h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z"/>
									</svg>
									<div>
										<h3 class="font-semibold text-gray-800">Email</h3>
										<a href="mailto:privacy@domurion.com" class="text-blue-600 hover:text-blue-700">privacy@domurion.com</a>
									</div>
								</div>

								<div class="flex items-start space-x-3">
									<svg class="w-5 h-5 text-blue-600 mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
										<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M7 8h10M7 12h4m1 8l-4-4H5a2 2 0 01-2-2V6a2 2 0 012-2h14a2 2 0 012 2v8a2 2 0 01-2 2h-3l-4 4z"/>
									</svg>
									<div>
										<h3 class="font-semibold text-gray-800">Support</h3>
										<a href="/help" class="text-blue-600 hover:text-blue-700">Contact Support</a>
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
										<h3 class="font-semibold text-gray-800">Address</h3>
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
								<strong>Response Time:</strong> We aim to respond to all privacy-related inquiries within 5 business days and will address requests within 30 days as required by applicable law.
							</p>
						</div>
					</div>
				</section>
			</div>
		</div>

		<!-- Footer -->
		<div class="mt-12 text-center text-gray-500">
			<p class="text-sm">
				This Privacy Policy is effective as of {lastUpdated} and applies to all users of Domurion services.
			</p>
			<div class="mt-4 flex justify-center space-x-6 text-sm">
				<a href="/terms" class="hover:text-gray-700 transition-colors">Terms of Service</a>
				<a href="/security" class="hover:text-gray-700 transition-colors">Security</a>
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
</style>
