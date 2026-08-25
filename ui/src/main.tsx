// Polyfill for crypto.randomUUID in non-secure (HTTP) contexts
if (!window.crypto) {
  Object.defineProperty(window, 'crypto', {
    value: {} as Crypto,
    writable: true,
    configurable: true
  });
}
if (!window.crypto.randomUUID) {
  window.crypto.randomUUID = function () {
    const buf = new Uint8Array(16);
    crypto.getRandomValues(buf);
    buf[6] = (buf[6] & 0x0f) | 0x40;
    buf[8] = (buf[8] & 0x3f) | 0x80;
    const hex = Array.from(buf).map(b => b.toString(16).padStart(2, '0')).join('');
    return `${hex.slice(0,8)}-${hex.slice(8,12)}-${hex.slice(12,16)}-${hex.slice(16,20)}-${hex.slice(20)}`;
  } as Crypto['randomUUID'];
}

// Global error handlers for unhandled errors
window.addEventListener('error', (event) => {
  console.error('[Global Error]', event.error);
  event.preventDefault();
});

window.addEventListener('unhandledrejection', (event) => {
  console.error('[Unhandled Promise Rejection]', event.reason);
  event.preventDefault();
});

import './index.css'
import './styles/print.css'

import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'

import { App } from './App'

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <App />
  </StrictMode>,
)
