<script lang="ts">
  import { goto } from '$app/navigation';
  function handleBackToLogin() {
    goto('/login');
  }
</script>

<div class="min-h-screen flex items-center justify-center bg-gray-50 px-4">
  <div class="w-full max-w-md bg-white rounded-2xl shadow-lg p-8 border border-gray-100">

  <h1 class="text-2xl font-bold text-gray-900 mb-2 text-center">Verify your email</h1>
  <p class="text-gray-600 mb-6 text-center">We've sent a verification link to your email address. Please check your inbox and click the link to verify your account.</p>

    <button
      class="w-full rounded-xl bg-indigo-600 text-white font-semibold py-2 mt-2 transition hover:bg-indigo-700 focus:ring-2 focus:ring-indigo-400 focus:outline-none shadow"
      on:click={handleBackToLogin}
      type="button"
    >
      Back to login
    </button>
  </div>
</div>

<style>
  /* Add any additional custom styles here if needed */
</style>
