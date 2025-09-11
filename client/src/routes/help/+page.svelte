<script lang="ts">
  import { onMount } from 'svelte';
  
  let searchQuery = '';
  let selectedCategory = 'all';
  let expandedFaq: number | null = null;
  
  const categories = [
    { id: 'all', name: 'All Topics', icon: 'grid' },
    { id: 'account', name: 'Account', icon: 'user' },
    { id: 'security', name: 'Security', icon: 'shield' },
    { id: 'billing', name: 'Billing', icon: 'credit-card' },
    { id: 'technical', name: 'Technical', icon: 'settings' }
  ];
  
  const faqs = [
    {
      id: 1,
      category: 'account',
      question: 'How do I update my account information?',
      answer: 'You can update your account information by going to Settings > Account Settings. From there, you can modify your name, email, and other personal details. Changes are saved automatically.'
    },
    {
      id: 2,
      category: 'account',
      question: 'How do I change my password?',
      answer: 'To change your password, navigate to Settings > Security. Click on "Change Password" and follow the prompts to enter your current password and set a new one.'
    },
    {
      id: 3,
      category: 'security',
      question: 'Is my data secure in Vault?',
      answer: 'Yes, Vault uses industry-standard encryption both in transit and at rest. We employ AES-256 encryption and follow SOC 2 Type II compliance standards to ensure your data remains secure.'
    },
    {
      id: 4,
      category: 'security',
      question: 'Can I enable two-factor authentication?',
      answer: 'Absolutely! Two-factor authentication (2FA) is available in Settings > Security. You can use authenticator apps like Google Authenticator or Authy for enhanced account security.'
    },
    {
      id: 5,
      category: 'billing',
      question: 'How do I view my billing history?',
      answer: 'Your billing history is available in Settings > Billing. You can view past invoices, download receipts, and update your payment methods from this section.'
    },
    {
      id: 6,
      category: 'billing',
      question: 'Can I change my subscription plan?',
      answer: 'Yes, you can upgrade or downgrade your plan at any time from Settings > Billing > Plan Management. Changes take effect immediately for upgrades or at the next billing cycle for downgrades.'
    },
    {
      id: 7,
      category: 'technical',
      question: 'What browsers are supported?',
      answer: 'Vault works best on modern browsers including Chrome 90+, Firefox 88+, Safari 14+, and Edge 90+. We recommend keeping your browser updated for the best experience.'
    },
    {
      id: 8,
      category: 'technical',
      question: 'Why is Vault running slowly?',
      answer: 'Performance issues can be caused by browser extensions, low memory, or network connectivity. Try clearing your browser cache, disabling extensions, or refreshing the page. Contact support if issues persist.'
    }
  ];
  
  $: filteredFaqs = faqs.filter(faq => {
    const matchesCategory = selectedCategory === 'all' || faq.category === selectedCategory;
    const matchesSearch = searchQuery === '' || 
      faq.question.toLowerCase().includes(searchQuery.toLowerCase()) ||
      faq.answer.toLowerCase().includes(searchQuery.toLowerCase());
    return matchesCategory && matchesSearch;
  });
  
  function toggleFaq(id: number) {
    expandedFaq = expandedFaq === id ? null : id;
  }
  
  type IconName = 'grid' | 'user' | 'shield' | 'credit-card' | 'settings';

  function getIcon(iconName: string) {
    const icons: Record<IconName, string> = {
      grid: 'M4 6a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2H6a2 2 0 01-2-2V6zM14 6a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2h-2a2 2 0 01-2-2V6zM4 16a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2H6a2 2 0 01-2-2v-2zM14 16a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2h-2a2 2 0 01-2-2v-2z',
      user: 'M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z',
      shield: 'M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z',
      'credit-card': 'M3 10h18M7 15h1m4 0h1m-7 4h12a3 3 0 003-3V8a3 3 0 00-3-3H6a3 3 0 00-3 3v8a3 3 0 003 3z',
      settings: 'M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z M15 12a3 3 0 11-6 0 3 3 0 016 0z'
    };
    return icons[iconName as IconName] || icons.grid;
  }
</script>

<div class="min-h-screen bg-gray-50">
  <!-- Header -->
  <div class="bg-white shadow-sm border-b border-gray-200">
    <div class="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <div class="text-center">
        <div class="flex items-center justify-center mb-4">
          <div class="w-12 h-12 bg-indigo-600 rounded-xl flex items-center justify-center">
            <svg class="w-7 h-7 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8.228 9c.549-1.165 2.03-2 3.772-2 2.21 0 4 1.343 4 3 0 1.4-1.278 2.575-3.006 2.907-.542.104-.994.54-.994 1.093m0 3h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
          </div>
        </div>
        <h1 class="text-3xl font-bold text-gray-900 mb-2">Help & Support</h1>
        <p class="text-lg text-gray-600 max-w-2xl mx-auto">
          Find answers to common questions or get in touch with our support team
        </p>
      </div>
    </div>
  </div>

  <div class="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
    <!-- Quick Actions -->
    <div class="grid grid-cols-1 md:grid-cols-3 gap-6 mb-12">
      <div class="bg-white rounded-xl shadow-sm border border-gray-200 p-6 hover:shadow-md transition-shadow">
        <div class="flex items-center mb-4">
          <div class="w-10 h-10 bg-blue-100 rounded-lg flex items-center justify-center">
            <svg class="w-6 h-6 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 12h.01M12 12h.01M16 12h.01M21 12c0 4.418-4.03 8-9 8a9.863 9.863 0 01-4.255-.949L3 20l1.395-3.72C3.512 15.042 3 13.574 3 12c0-4.418 4.03-8 9-8s9 3.582 9 8z" />
            </svg>
          </div>
          <h3 class="text-lg font-semibold text-gray-900 ml-3">Live Chat</h3>
        </div>
        <p class="text-gray-600 mb-4">Get instant help from our support team</p>
        <button class="w-full bg-blue-600 text-white px-4 py-2 rounded-lg hover:bg-blue-700 transition-colors">
          Start Chat
        </button>
      </div>

      <div class="bg-white rounded-xl shadow-sm border border-gray-200 p-6 hover:shadow-md transition-shadow">
        <div class="flex items-center mb-4">
          <div class="w-10 h-10 bg-green-100 rounded-lg flex items-center justify-center">
            <svg class="w-6 h-6 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 8l7.89 4.26a2 2 0 002.22 0L21 8M5 19h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z" />
            </svg>
          </div>
          <h3 class="text-lg font-semibold text-gray-900 ml-3">Email Support</h3>
        </div>
        <p class="text-gray-600 mb-4">Send us a detailed message</p>
        <a href="mailto:support@vault.com" class="block w-full bg-green-600 text-white px-4 py-2 rounded-lg hover:bg-green-700 transition-colors text-center">
          Send Email
        </a>
      </div>

      <div class="bg-white rounded-xl shadow-sm border border-gray-200 p-6 hover:shadow-md transition-shadow">
        <div class="flex items-center mb-4">
          <div class="w-10 h-10 bg-purple-100 rounded-lg flex items-center justify-center">
            <svg class="w-6 h-6 text-purple-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
            </svg>
          </div>
          <h3 class="text-lg font-semibold text-gray-900 ml-3">Documentation</h3>
        </div>
        <p class="text-gray-600 mb-4">Browse our comprehensive guides</p>
        <button class="w-full bg-purple-600 text-white px-4 py-2 rounded-lg hover:bg-purple-700 transition-colors">
          View Docs
        </button>
      </div>
    </div>

    <!-- Search and Filter -->
    <div class="bg-white rounded-xl shadow-sm border border-gray-200 p-6 mb-8">
      <h2 class="text-xl font-semibold text-gray-900 mb-4">Frequently Asked Questions</h2>
      
      <!-- Search Bar -->
      <div class="relative mb-6">
        <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
          <svg class="w-5 h-5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
          </svg>
        </div>
        <input
          type="text"
          bind:value={searchQuery}
          placeholder="Search for help..."
          class="w-full pl-10 pr-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
        />
      </div>

      <!-- Category Filter -->
      <div class="flex flex-wrap gap-2 mb-6">
        {#each categories as category}
          <button
            on:click={() => selectedCategory = category.id}
            class="flex items-center px-4 py-2 rounded-lg border transition-all {selectedCategory === category.id 
              ? 'bg-indigo-100 border-indigo-300 text-indigo-700' 
              : 'bg-white border-gray-300 text-gray-700 hover:bg-gray-50'}"
          >
            <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="{getIcon(category.icon)}" />
            </svg>
            {category.name}
          </button>
        {/each}
      </div>

      <!-- FAQ List -->
      <div class="space-y-4">
        {#each filteredFaqs as faq}
          <div class="border border-gray-200 rounded-lg">
            <button
              on:click={() => toggleFaq(faq.id)}
              class="w-full px-6 py-4 text-left flex items-center justify-between hover:bg-gray-50 transition-colors"
            >
              <span class="font-medium text-gray-900">{faq.question}</span>
              <svg 
                class="w-5 h-5 text-gray-500 transition-transform {expandedFaq === faq.id ? 'rotate-180' : ''}"
                fill="none" 
                stroke="currentColor" 
                viewBox="0 0 24 24"
              >
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7" />
              </svg>
            </button>
            {#if expandedFaq === faq.id}
              <div class="px-6 pb-4 text-gray-700 bg-gray-50 rounded-b-lg">
                {faq.answer}
              </div>
            {/if}
          </div>
        {:else}
          <div class="text-center py-8">
            <svg class="w-16 h-16 text-gray-300 mx-auto mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9.172 16.172a4 4 0 015.656 0M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
            </svg>
            <p class="text-gray-500">No questions found matching your search.</p>
          </div>
        {/each}
      </div>
    </div>

    <!-- Contact Information -->
    <div class="bg-indigo-50 rounded-xl p-8 text-center">
      <h3 class="text-xl font-semibold text-gray-900 mb-4">Still need help?</h3>
      <p class="text-gray-600 mb-6">
        Can't find what you're looking for? Our support team is here to help you 24/7.
      </p>
      <div class="flex flex-col sm:flex-row gap-4 justify-center">
        <a
          href="mailto:support@vault.com"
          class="inline-flex items-center justify-center px-6 py-3 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 transition-colors"
        >
          <svg class="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 8l7.89 4.26a2 2 0 002.22 0L21 8M5 19h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z" />
          </svg>
          Contact Support
        </a>
        <button class="inline-flex items-center justify-center px-6 py-3 bg-white text-indigo-600 border border-indigo-200 rounded-lg hover:bg-indigo-50 transition-colors">
          <svg class="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 12h.01M12 12h.01M16 12h.01M21 12c0 4.418-4.03 8-9 8a9.863 9.863 0 01-4.255-.949L3 20l1.395-3.72C3.512 15.042 3 13.574 3 12c0-4.418 4.03-8 9-8s9 3.582 9 8z" />
          </svg>
          Start Live Chat
        </button>
      </div>
    </div>
  </div>
</div>