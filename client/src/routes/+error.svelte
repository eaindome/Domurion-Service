<script lang=ts>
  import { page } from '$app/stores';
  import { goto } from '$app/navigation';
  
  // Get error details from the page store
  $: status = $page.status;
  $: message = $page.error?.message;
  
  // Define error configurations
  const errorConfigs: Record<string, {
    title: string;
    subtitle: string;
    description: string;
    icon: string;
    color: string;
    showHome: boolean;
    showBack?: boolean;
    showLogin?: boolean;
    showReload?: boolean;
  }> = {
    400: {
      title: 'Bad Request',
      subtitle: 'The request could not be understood',
      description: 'There was something wrong with your request. Please check and try again.',
      icon: 'exclamation',
      color: 'orange',
      showHome: true,
      showBack: true
    },
    401: {
      title: 'Unauthorized',
      subtitle: 'Access denied',
      description: 'You need to sign in to access this page. Please log in with your credentials.',
      icon: 'lock',
      color: 'red',
      showHome: true,
      showLogin: true
    },
    403: {
      title: 'Forbidden',
      subtitle: 'Access forbidden',
      description: 'You don\'t have permission to access this resource.',
      icon: 'shield',
      color: 'red',
      showHome: true,
      showBack: true
    },
    404: {
      title: 'Page Not Found',
      subtitle: 'This page doesn\'t exist',
      description: 'The page you\'re looking for might have been moved, deleted, or never existed.',
      icon: 'search',
      color: 'gray',
      showHome: true,
      showBack: true
    },
    429: {
      title: 'Too Many Requests',
      subtitle: 'Slow down there!',
      description: 'You\'ve made too many requests. Please wait a moment and try again.',
      icon: 'clock',
      color: 'yellow',
      showHome: true,
      showBack: true
    },
    500: {
      title: 'Server Error',
      subtitle: 'Something went wrong',
      description: 'We\'re experiencing technical difficulties. Our team has been notified and is working on a fix.',
      icon: 'server',
      color: 'red',
      showHome: true,
      showReload: true
    },
    503: {
      title: 'Service Unavailable',
      subtitle: 'Temporarily down',
      description: 'We\'re currently performing maintenance. Please try again in a few minutes.',
      icon: 'tools',
      color: 'blue',
      showHome: true,
      showReload: true
    }
  };
  
  // Get current error config or default to 404
  $: currentError = errorConfigs[String(status)] || errorConfigs['404'];
  
  function goBack() {
    if (window.history.length > 1) {
      window.history.back();
    } else {
      goto('/');
    }
  }
  
  function reload() {
    window.location.reload();
  }
  
  type IconType = 'search' | 'lock' | 'shield' | 'exclamation' | 'server' | 'clock' | 'tools';

  function getIconSvg(iconType: IconType | string) {
    const icons: Record<IconType, string> = {
      search: `<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"/>`,
      lock: `<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z"/>`,
      shield: `<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M20.618 5.984A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z"/>`,
      exclamation: `<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.964-.833-2.732 0L3.732 16.5c-.77.833.192 2.5 1.732 2.5z"/>`,
      server: `<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 12h14M5 12a2 2 0 01-2-2V6a2 2 0 012-2h14a2 2 0 012 2v4a2 2 0 01-2 2M5 12a2 2 0 00-2 2v4a2 2 0 002 2h14a2 2 0 002-2v-4a2 2 0 00-2-2m-2-4h.01M17 16h.01"/>`,
      clock: `<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"/>`,
      tools: `<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z"/> <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"/>`
    };
    return icons[iconType as IconType] || icons.search;
  }
  
  type ColorKey = 'gray' | 'red' | 'orange' | 'yellow' | 'blue';

  function getColorClasses(color: string) {
    const colors: Record<ColorKey, { bg: string; text: string; icon: string }> = {
      gray: {
        bg: 'bg-gray-100',
        text: 'text-gray-600',
        icon: 'text-gray-500'
      },
      red: {
        bg: 'bg-red-100',
        text: 'text-red-600',
        icon: 'text-red-500'
      },
      orange: {
        bg: 'bg-orange-100',
        text: 'text-orange-600',
        icon: 'text-orange-500'
      },
      yellow: {
        bg: 'bg-yellow-100',
        text: 'text-yellow-600',
        icon: 'text-yellow-500'
      },
      blue: {
        bg: 'bg-blue-100',
        text: 'text-blue-600',
        icon: 'text-blue-500'
      }
    };
    return colors[color as ColorKey] || colors.gray;
  }
  
  $: colorClasses = getColorClasses(currentError.color);
</script>

<svelte:head>
  <title>{status} - {currentError.title} - Vault</title>
</svelte:head>

<div class="min-h-screen bg-gray-50 flex flex-col">
  <!-- Simple Header -->
  <nav class="bg-white shadow-sm border-b border-gray-100">
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
      <div class="flex items-center h-16">
        <a href="/" class="flex items-center">
          <div class="w-8 h-8 bg-indigo-600 rounded-lg flex items-center justify-center">
            <svg class="w-5 h-5 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z"/>
            </svg>
          </div>
          <span class="ml-3 text-xl font-semibold text-gray-900">Vault</span>
        </a>
      </div>
    </div>
  </nav>

  <!-- Error Content -->
  <div class="flex-1 flex items-center justify-center py-12 px-4 sm:px-6 lg:px-8">
    <div class="max-w-md w-full">
      <!-- Error Card -->
      <div class="bg-white rounded-2xl shadow-sm border border-gray-100 p-8 text-center">
        <!-- Error Icon -->
        <div class="flex justify-center mb-6">
          <div class="w-16 h-16 {colorClasses.bg} rounded-2xl flex items-center justify-center">
            <svg class="w-10 h-10 {colorClasses.icon}" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              {@html getIconSvg(currentError.icon)}
            </svg>
          </div>
        </div>
        
        <!-- Error Status -->
        <div class="mb-2">
          <span class="text-4xl font-bold {colorClasses.text}">{status}</span>
        </div>
        
        <!-- Error Title -->
        <h1 class="text-2xl font-semibold text-gray-900 mb-2">
          {currentError.title}
        </h1>
        
        <!-- Error Subtitle -->
        <p class="text-lg text-gray-600 mb-4">
          {currentError.subtitle}
        </p>
        
        <!-- Error Description -->
        <p class="text-gray-500 mb-8 leading-relaxed">
          {currentError.description}
        </p>
        
        <!-- Custom Error Message -->
        {#if message && message !== currentError.title}
          <div class="bg-gray-50 rounded-xl p-4 mb-6">
            <p class="text-sm text-gray-700">
              <span class="font-medium">Details:</span> {message}
            </p>
          </div>
        {/if}
        
        <!-- Action Buttons -->
        <div class="space-y-3">
          {#if currentError.showHome}
            <a 
              href="/"
              class="w-full inline-flex justify-center items-center px-6 py-3 border border-transparent text-sm font-medium rounded-xl text-white bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 transition-all duration-200"
            >
              <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6"/>
              </svg>
              Go to Homepage
            </a>
          {/if}
          
          <div class="flex space-x-3">
            {#if currentError.showBack}
              <button
                on:click={goBack}
                class="flex-1 inline-flex justify-center items-center px-4 py-2 border border-gray-200 text-sm font-medium rounded-xl text-gray-700 bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 transition-all duration-200"
              >
                <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 19l-7-7m0 0l7-7m-7 7h18"/>
                </svg>
                Go Back
              </button>
            {/if}
            
            {#if currentError.showReload}
              <button
                on:click={reload}
                class="flex-1 inline-flex justify-center items-center px-4 py-2 border border-gray-200 text-sm font-medium rounded-xl text-gray-700 bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 transition-all duration-200"
              >
                <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15"/>
                </svg>
                Try Again
              </button>
            {/if}
            
            {#if currentError.showLogin}
              <a
                href="/login"
                class="flex-1 inline-flex justify-center items-center px-4 py-2 border border-transparent text-sm font-medium rounded-xl text-indigo-600 bg-indigo-50 hover:bg-indigo-100 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 transition-all duration-200"
              >
                <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 16l-4-4m0 0l4-4m-4 4h14m-5 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h7a3 3 0 013 3v1"/>
                </svg>
                Sign In
              </a>
            {/if}
          </div>
        </div>
        
        <!-- Help Text -->
        <div class="mt-8 pt-6 border-t border-gray-100">
          <p class="text-xs text-gray-400">
            If you continue to experience issues, please contact our support team.
          </p>
        </div>
      </div>
      
      <!-- Additional Help -->
      {#if status === 404}
        <div class="mt-6 bg-white rounded-2xl shadow-sm border border-gray-100 p-6">
          <h3 class="text-sm font-medium text-gray-900 mb-3">Common pages you might be looking for:</h3>
          <div class="space-y-2">
            <a href="/dashboard" class="block text-sm text-indigo-600 hover:text-indigo-500">
              • Your Dashboard
            </a>
            <a href="/vault/add" class="block text-sm text-indigo-600 hover:text-indigo-500">
              • Add New Entry
            </a>
            <a href="/settings" class="block text-sm text-indigo-600 hover:text-indigo-500">
              • Account Settings
            </a>
          </div>
        </div>
      {/if}
    </div>
  </div>
</div>

<style>
  /* Additional custom styles if needed */
  button:focus, a:focus {
    box-shadow: 0 0 0 3px rgba(99, 102, 241, 0.1);
  }
</style>