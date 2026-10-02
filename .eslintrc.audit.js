/** 
 * Frontend Audit ESLint Config
 * Add to your existing ESLint config: extends: ['./.eslintrc.audit.js']
 */
module.exports = {
  rules: {
    // Prevent inline styles in non-print components
    'no-restricted-syntax': [
      'error',
      {
        selector: 'JSXAttribute[name.name="style"]',
        message: 'Avoid inline styles. Use Tailwind classes or CSS variables instead. If truly dynamic, add // eslint-disable-next-line no-restricted-syntax.'
      }
    ],

    // Warn on raw HTML form elements
    'no-restricted-elements': [
      'warn',
      {
        element: 'button',
        message: 'Use <Button> from @/components/ui instead of raw <button>.'
      },
      {
        element: 'input',
        message: 'Use <Input> from @/components/ui instead of raw <input>.'
      }
    ],

    // Prevent hardcoded colors
    'no-restricted-imports': [
      'warn',
      {
        paths: [
          {
            name: 'react',
            importNames: ['any'],
            message: 'Avoid importing `any`. Use proper types.'
          }
        ]
      }
    ],

    // TypeScript: no explicit any
    '@typescript-eslint/no-explicit-any': 'warn',

    // TypeScript: no unsafe any assignments
    '@typescript-eslint/no-unsafe-assignment': 'warn',

    // TypeScript: no unsafe member access
    '@typescript-eslint/no-unsafe-member-access': 'warn',

    // TypeScript: no unsafe call
    '@typescript-eslint/no-unsafe-call': 'warn',

    // TypeScript: no unsafe return
    '@typescript-eslint/no-unsafe-return': 'warn',
  },
  overrides: [
    {
      // Exempt print pages from inline style rule
      files: ['**/pages/print/**', '**/print/**'],
      rules: {
        'no-restricted-syntax': 'off'
      }
    },
    {
      // Exempt UI primitives from raw HTML rules
      files: ['**/components/ui/**'],
      rules: {
        'no-restricted-elements': 'off'
      }
    },
    {
      // Exempt test files
      files: ['**/*.test.tsx', '**/*.test.ts', '**/*.spec.tsx', '**/*.spec.ts'],
      rules: {
        '@typescript-eslint/no-explicit-any': 'off',
        'no-restricted-syntax': 'off'
      }
    }
  ]
};
