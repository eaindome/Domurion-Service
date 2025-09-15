// import tailwindcss from '@tailwindcss/vite';
// import { sveltekit } from '@sveltejs/kit/vite';
// import { defineConfig } from 'vite';

// export default defineConfig({
// 	plugins: [tailwindcss(), sveltekit()],
// 	test: {
// 		expect: { requireAssertions: true },
// 		projects: [
// 			{
// 				extends: './vite.config.ts',
// 				test: {
// 					name: 'client',
// 					environment: 'browser',
// 					browser: {
// 						enabled: true,
// 						provider: 'playwright',
// 						instances: [{ browser: 'chromium' }]
// 					},
// 					include: ['src/**/*.svelte.{test,spec}.{js,ts}'],
// 					exclude: ['src/lib/server/**'],
// 					setupFiles: ['./vitest-setup-client.ts']
// 				}
// 			},
// 			{
// 				extends: './vite.config.ts',
// 				test: {
// 					name: 'server',
// 					environment: 'node',
// 					include: ['src/**/*.{test,spec}.{js,ts}'],
// 					exclude: ['src/**/*.svelte.{test,spec}.{js,ts}']
// 				}
// 			}
// 		]
// 	}
// });

import tailwindcss from '@tailwindcss/vite';
import { sveltekit } from '@sveltejs/kit/vite';

export default () => {
    const useMockApi = process.env.VITE_MOCK_API === 'true';
    const mockApiPath = new URL('./src/lib/api/mock.ts', import.meta.url).pathname;

    return {
        plugins: [tailwindcss(), sveltekit()],
        resolve: {
            alias: useMockApi
                ? {
                        '$lib/api/vault': mockApiPath,
                        '$lib/api/shared': mockApiPath
                  }
                : {}
        },
        test: {
            expect: { requireAssertions: true },
            projects: [
                {
                    extends: './vite.config.ts',
                    test: {
                        name: 'client',
                        environment: 'browser',
                        browser: {
                            enabled: true,
                            provider: 'playwright',
                            instances: [{ browser: 'chromium' }]
                        },
                        include: ['src/**/*.svelte.{test,spec}.{js,ts}'],
                        exclude: ['src/lib/server/**'],
                        setupFiles: ['./vitest-setup-client.ts']
                    }
                },
                {
                    extends: './vite.config.ts',
                    test: {
                        name: 'server',
                        environment: 'node',
                        include: ['src/**/*.{test,spec}.{js,ts}'],
                        exclude: ['src/**/*.svelte.{test,spec}.{js,ts}']
                    }
                }
            ]
        }
    };
};