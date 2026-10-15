/** @type {import('tailwindcss').Config} */
export default {
  content: ['./index.html', './src/**/*.{js,jsx}'],
  theme: {
    extend: {
      fontFamily: { sans: ['Inter', 'ui-sans-serif', 'system-ui'] },
      colors: { ink: '#19213d', muted: '#747b94', primary: '#6857ed' }
    }
  },
  plugins: []
}
