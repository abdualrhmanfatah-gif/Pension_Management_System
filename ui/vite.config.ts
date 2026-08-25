import react from '@vitejs/plugin-react'
import path from 'path'
import { visualizer } from 'rollup-plugin-visualizer'
import { defineConfig } from 'vite'
import { ViteImageOptimizer } from 'vite-plugin-image-optimizer'

// Vite config — Pension Management System
export default defineConfig({
  plugins: [
    react(),
    ViteImageOptimizer({
      png: { quality: 75, compressionLevel: 9 },
      webp: { quality: 70 },
      jpeg: { quality: 70 },
      include: /\.(png|jpe?g|webp)$/i,
    }),
    ...(process.env.VISUALIZER === 'true'
      ? [visualizer({ filename: 'dist/stats.html', open: true })]
      : []),
  ],
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
    },
    dedupe: ['react', 'react-dom', 'react-router-dom'],
  },
  build: {
    rollupOptions: {
      output: {
        manualChunks(id) {
          if (id.includes('node_modules')) {
            if (/[\\/]node_modules[\\/](react|react-dom|react-router|react-router-dom|scheduler)[\\/]/.test(id)) {
              return 'vendor-react';
            }
            if (id.includes('@tanstack') || id.includes('zustand') || id.includes('immer')) {
              return 'vendor-query';
            }
            if (id.includes('lucide-react') || id.includes('clsx') || id.includes('tailwind-merge') || id.includes('class-variance-authority')) {
              return 'vendor-ui';
            }
            if (id.includes('react-hook-form') || id.includes('@hookform') || id.includes('zod')) {
              return 'vendor-forms';
            }
            if (id.includes('xlsx') || id.includes('file-saver')) {
              return 'vendor-xlsx';
            }
            if (id.includes('qrcode')) {
              return 'vendor-qrcode';
            }
            if (id.includes('jwt-decode') || id.includes('axios')) {
              return 'vendor-auth';
            }
          }
        },
      },
    },
  },
  server: {
    host: true,
    proxy: {
      '/api': {
        target: 'http://localhost:5001',
        changeOrigin: true,
        secure: false,
        xfwd: true,
      },
      '/hubs': {
        target: 'http://localhost:5001',
        ws: true,
        changeOrigin: true,
        secure: false,
      },
      '/uploads': {
        target: 'http://localhost:5001',
        changeOrigin: true,
        secure: false,
        xfwd: true,
      },
      '/api/files': {
        target: 'http://localhost:5001',
        changeOrigin: true,
        secure: false,
        xfwd: true,
      },
      
    },
  },
})
