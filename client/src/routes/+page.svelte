<script>
  import { onMount } from 'svelte';
	import navLogo from '$lib/assets/navLogo.png';
  
  let mounted = false;
  
  onMount(() => {
    mounted = true;
  });
</script>

<svelte:head>
  <title>Domurion - Secure Password Management</title>
  <meta name="description" content="Keep your passwords safe and secure with Domurion - a simple, elegant password manager built for modern users." />
</svelte:head>

<div class="min-h-screen bg-gray-50">
  <!-- Navigation -->
  <nav class="bg-white/80 backdrop-blur-md border-b border-gray-100 sticky top-0 z-50 shadow-sm">
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
      <div class="flex justify-between items-center h-16">
        <!-- Logo and Brand -->
        <div class="flex items-center">
          <div class="flex items-center justify-center h-16">
            <img src={navLogo} alt="Domurion Logo" class="max-h-32 max-w-32 rounded-lg" />
          </div>
        </div>
        
        <div class="flex items-center space-x-4">
          <a href="/login" class="text-gray-600 hover:text-gray-900 px-3 py-2 text-sm font-medium transition-colors">
            Sign In
          </a>
          <a href="/register" class="bg-indigo-600 hover:bg-indigo-700 text-white px-4 py-2 rounded-lg text-sm font-medium transition-colors">
            Get Started
          </a>
        </div>
      </div>
    </div>
  </nav>

  <!-- Hero Section -->
  <section class="pt-20 pb-16 px-4 sm:px-6 lg:px-8">
    <div class="max-w-4xl mx-auto text-center">
      <div class="mb-8">
        <div class="inline-flex items-center px-4 py-2 rounded-full bg-indigo-50 text-indigo-700 text-sm font-medium mb-6">
          <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
          </svg>
          Secure • Simple • Reliable
        </div>
        
        <h1 class="text-5xl sm:text-6xl font-bold text-gray-900 mb-6 leading-tight">
          Your passwords,
          <span class="text-indigo-600">secured</span>
        </h1>
        
        <p class="text-xl text-gray-600 mb-8 max-w-2xl mx-auto leading-relaxed">
          Domurion keeps your digital life safe with military-grade encryption. Store, generate, and manage all your passwords in one secure vault.
        </p>
        
        <div class="flex flex-col sm:flex-row gap-4 justify-center items-center">
          <a href="/register" class="bg-indigo-600 hover:bg-indigo-700 text-white px-8 py-4 rounded-xl text-lg font-semibold transition-all transform hover:scale-105 shadow-lg hover:shadow-xl">
            Start Free Today
          </a>
          <a href="/login" class="text-gray-700 hover:text-gray-900 px-8 py-4 rounded-xl text-lg font-medium border border-gray-200 hover:border-gray-300 transition-all">
            Sign In
          </a>
        </div>
      </div>
    </div>
  </section>

  <!-- Features Section -->
  <section class="py-20 px-4 sm:px-6 lg:px-8 bg-white">
    <div class="max-w-6xl mx-auto">
      <div class="text-center mb-16">
        <h2 class="text-3xl sm:text-4xl font-bold text-gray-900 mb-4">
          Everything you need to stay secure
        </h2>
        <p class="text-lg text-gray-600 max-w-2xl mx-auto">
          Built with security and simplicity in mind, Domurion provides all the essential features for modern password management.
        </p>
      </div>
      
      <div class="grid md:grid-cols-3 gap-8">
        <!-- Feature 1 -->
        <div class="text-center p-6 rounded-2xl hover:bg-gray-50 transition-colors">
          <div class="w-12 h-12 bg-indigo-100 rounded-xl flex items-center justify-center mx-auto mb-4">
            <svg class="w-6 h-6 text-indigo-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z" />
            </svg>
          </div>
          <h3 class="text-xl font-semibold text-gray-900 mb-2">Secure Vault</h3>
          <p class="text-gray-600">Your passwords are encrypted with AES-256 encryption, ensuring they stay safe from prying eyes.</p>
        </div>
        
        <!-- Feature 2 -->
        <div class="text-center p-6 rounded-2xl hover:bg-gray-50 transition-colors">
          <div class="w-12 h-12 bg-indigo-100 rounded-xl flex items-center justify-center mx-auto mb-4">
            <svg class="w-6 h-6 text-indigo-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 10V3L4 14h7v7l9-11h-7z" />
            </svg>
          </div>
          <h3 class="text-xl font-semibold text-gray-900 mb-2">Lightning Fast</h3>
          <p class="text-gray-600">Access your passwords instantly with our optimized interface designed for speed and efficiency.</p>
        </div>
        
        <!-- Feature 3 -->
        <div class="text-center p-6 rounded-2xl hover:bg-gray-50 transition-colors">
          <div class="w-12 h-12 bg-indigo-100 rounded-xl flex items-center justify-center mx-auto mb-4">
            <svg class="w-6 h-6 text-indigo-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4.318 6.318a4.5 4.5 0 000 6.364L12 20.364l7.682-7.682a4.5 4.5 0 00-6.364-6.364L12 7.636l-1.318-1.318a4.5 4.5 0 00-6.364 0z" />
            </svg>
          </div>
          <h3 class="text-xl font-semibold text-gray-900 mb-2">Simple & Clean</h3>
          <p class="text-gray-600">No clutter, no confusion. Just a beautiful, intuitive interface that gets out of your way.</p>
        </div>
      </div>
    </div>
  </section>

  <!-- How it Works -->
  <section class="py-20 px-4 sm:px-6 lg:px-8">
    <div class="max-w-4xl mx-auto">
      <div class="text-center mb-16">
        <h2 class="text-3xl sm:text-4xl font-bold text-gray-900 mb-4">
          Get started in minutes
        </h2>
        <p class="text-lg text-gray-600">
          Setting up your secure vault is quick and effortless
        </p>
      </div>
      
      <div class="grid md:grid-cols-3 gap-8">
        <div class="text-center">
          <div class="w-16 h-16 bg-indigo-600 text-white rounded-2xl flex items-center justify-center mx-auto mb-4 text-xl font-bold">
            1
          </div>
          <h3 class="text-lg font-semibold text-gray-900 mb-2">Create Account</h3>
          <p class="text-gray-600">Sign up with your email and create a master password</p>
        </div>
        
        <div class="text-center">
          <div class="w-16 h-16 bg-indigo-600 text-white rounded-2xl flex items-center justify-center mx-auto mb-4 text-xl font-bold">
            2
          </div>
          <h3 class="text-lg font-semibold text-gray-900 mb-2">Add Passwords</h3>
          <p class="text-gray-600">Import existing passwords or add new ones to your vault</p>
        </div>
        
        <div class="text-center">
          <div class="w-16 h-16 bg-indigo-600 text-white rounded-2xl flex items-center justify-center mx-auto mb-4 text-xl font-bold">
            3
          </div>
          <h3 class="text-lg font-semibold text-gray-900 mb-2">Stay Secure</h3>
          <p class="text-gray-600">Access your passwords anywhere, anytime, completely secure</p>
        </div>
      </div>
    </div>
  </section>

  <!-- CTA Section -->
  <section class="py-20 px-4 sm:px-6 lg:px-8 bg-indigo-600">
    <div class="max-w-4xl mx-auto text-center">
      <h2 class="text-3xl sm:text-4xl font-bold text-white mb-4">
        Ready to secure your digital life?
      </h2>
      <p class="text-xl text-indigo-100 mb-8 max-w-2xl mx-auto">
        Your personal password vault — built for privacy and simplicity.
      </p>
      <div class="flex flex-col sm:flex-row gap-4 justify-center">
        <a href="/register" class="bg-white hover:bg-gray-50 text-indigo-600 px-8 py-4 rounded-xl text-lg font-semibold transition-all transform hover:scale-105">
          Get Started Free
        </a>
        <a href="/login" class="text-white hover:text-indigo-100 px-8 py-4 rounded-xl text-lg font-medium border border-indigo-400 hover:border-indigo-300 transition-all">
          Sign In
        </a>
      </div>
    </div>
  </section>

  <!-- Footer -->
  <footer class="bg-white border-t border-gray-100 py-12 px-4 sm:px-6 lg:px-8">
    <div class="max-w-6xl mx-auto">
      <div class="flex flex-col md:flex-row justify-between items-center">
        <div class="flex items-center mb-4 md:mb-0">
          <div class="w-8 h-8 bg-indigo-600 rounded-lg flex items-center justify-center mr-3">
            <svg class="w-5 h-5 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z" />
            </svg>
          </div>
          <span class="text-xl font-semibold text-gray-900">Domurion</span>
        </div>
        
        <div class="text-gray-600 text-sm">
          © 2025 Domurion. All rights reserved.
        </div>
      </div>
    </div>
  </footer>
</div>

<style>
  :global(body) {
    font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, sans-serif;
  }
  
  /* Smooth scroll behavior */
  :global(html) {
    scroll-behavior: smooth;
  }
  
  /* Custom animations */
  @keyframes fadeInUp {
    from {
      opacity: 0;
      transform: translateY(20px);
    }
    to {
      opacity: 1;
      transform: translateY(0);
    }
  }
</style>

<!--  VITE_MOCK_API=true npm run dev -->