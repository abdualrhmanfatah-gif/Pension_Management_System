import tseslint from 'typescript-eslint'

import { restrictedDatePatterns } from './eslint.restricted-date-patterns.js'

export default tseslint.config(
  {
    ignores: ['dist'],
  },
  {
    files: ['src/**/*.{ts,tsx}'],
    extends: [tseslint.configs.base],
    rules: {
      'no-restricted-syntax': ['error', ...restrictedDatePatterns],
    },
  }
)
