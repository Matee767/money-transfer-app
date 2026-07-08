import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

// The /api proxy is only used by the local dev server (npm run dev).
// In the docker compose setup nginx handles the same proxying.
export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      '/api': {
        target: 'http://localhost:8080',
        changeOrigin: true,
      },
    },
  },
});
