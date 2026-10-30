/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{js,ts,jsx,tsx}",
    "./node_modules/@tremor/**/*.{js,ts,jsx,tsx}",
  ],
  darkMode: "class",
  theme: {
    extend: {
      fontFamily: {
<<<<<<< HEAD
        sans: "var(--font-sans)",
        mono: "var(--font-mono)",
=======
        sans: ['"Noto Kufi Arabic"', '"Noto Sans Arabic"', '"Inter"', 'system-ui', 'sans-serif'],
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        label: "var(--label-font-family)",
      },
      colors: {
        border: "hsl(var(--border))",
        input: "hsl(var(--input))",
        ring: "hsl(var(--ring))",
        background: "hsl(var(--background))",
        foreground: "hsl(var(--foreground))",
        primary: {
          DEFAULT: "hsl(var(--primary))",
          foreground: "hsl(var(--primary-foreground))",
          50: "#eef6ff",
          100: "#d9eaff",
          200: "#bbdaff",
          300: "#8cc3ff",
          400: "#56a1ff",
          500: "#2f7cff",
          600: "#1a5cf5",
          700: "#1347e1",
          800: "#163ab6",
          900: "#18358f",
          950: "#142257",
        },
        secondary: {
          DEFAULT: "hsl(var(--secondary))",
          foreground: "hsl(var(--secondary-foreground))",
        },
        destructive: {
          DEFAULT: "hsl(var(--destructive))",
          foreground: "hsl(var(--destructive-foreground))",
        },
        muted: {
          DEFAULT: "hsl(var(--muted))",
          foreground: "hsl(var(--muted-foreground))",
          primary: "hsl(var(--muted-primary))",
        },
        accent: {
          DEFAULT: "hsl(var(--accent))",
          foreground: "hsl(var(--accent-foreground))",
        },
        card: {
          DEFAULT: "hsl(var(--card))",
          foreground: "hsl(var(--card-foreground))",
        },
        table: {
          foreground: "hsl(var(--table-foreground))",
        },
        popover: {
          DEFAULT: "hsl(var(--popover))",
          foreground: "hsl(var(--popover-foreground))",
        },
        success: {
<<<<<<< HEAD
          DEFAULT: "hsl(var(--gov-success))",
          foreground: "#ffffff",
        },
        warning: {
          DEFAULT: "hsl(var(--gov-warning))",
          foreground: "#ffffff",
        },
        danger: {
          DEFAULT: "hsl(var(--gov-danger))",
          foreground: "#ffffff",
        },
        info: {
          DEFAULT: "hsl(var(--gov-info))",
          foreground: "#ffffff",
        },
        special: "hsl(var(--gov-special))",
        hero: {
          DEFAULT: "hsl(var(--gov-hero))",
          mid: "hsl(var(--gov-hero-mid))",
          end: "hsl(var(--gov-hero-end))",
          accent: "hsl(var(--gov-hero-accent))",
          fg: "hsl(var(--gov-hero-fg))",
        },
        overlay: "hsl(var(--overlay))",
=======
          DEFAULT: "#10b981",
          foreground: "#ffffff",
        },
        warning: {
          DEFAULT: "#f59e0b",
          foreground: "#ffffff",
        },
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        brand: {
          DEFAULT: "#1b3a5c",
          foreground: "#ffffff",
        },
        "surface-print": "#e0e0e0",
        "table-header": "#e8edf2",
        "border-print": "#bdc3ce",
        sidebar: {
          DEFAULT: "hsl(var(--sidebar))",
          foreground: "hsl(var(--sidebar-foreground))",
          accent: "hsl(var(--sidebar-accent))",
        },
        tremor: {
          brand: {
            faint: "#eff6ff",
            muted: "#bfdbfe",
            subtle: "#60a5fa",
            DEFAULT: "#3b82f6",
            emphasis: "#1d4ed8",
            inverted: "#ffffff",
          },
          background: {
            muted: "#f9fafb",
            subtle: "#f3f4f6",
            DEFAULT: "#ffffff",
            emphasis: "#374151",
          },
          border: {
            DEFAULT: "#e5e7eb",
          },
          ring: {
            DEFAULT: "#3b82f6",
          },
          content: {
            subtle: "#9ca3af",
            DEFAULT: "#6b7280",
            emphasis: "#111827",
            strong: "#111827",
            inverted: "#ffffff",
          },
        },
      },
      borderRadius: {
        lg: "var(--radius)",
        md: "calc(var(--radius) - 2px)",
        sm: "calc(var(--radius) - 4px)",
      },
<<<<<<< HEAD
      transitionDuration: {
        fast: "var(--transition-duration-fast)",
        base: "var(--transition-duration-base)",
        slow: "var(--transition-duration-slow)",
        bounce: "var(--transition-duration-bounce)",
      },
      transitionTimingFunction: {
        standard: "var(--transition-easing-standard)",
        bounce: "var(--transition-easing-bounce)",
      },
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
      zIndex: {
        dialog: "9999",
        dropdown: "10000",
        notification: "110",
        backdrop: "-1",
        "print-watermark": "1",
        "print-footer": "2",
      },
      keyframes: {
        "fade-in": {
          "0%": { opacity: "0", transform: "translateY(8px)" },
          "100%": { opacity: "1", transform: "translateY(0)" },
        },
        "slide-in-right": {
          "0%": { transform: "translateX(100%)" },
          "100%": { transform: "translateX(0)" },
        },
        "slide-in-left": {
          "0%": { transform: "translateX(-100%)" },
          "100%": { transform: "translateX(0)" },
        },
        "slide-up": {
          "0%": { height: "var(--radix-accordion-content-height)" },
          "100%": { height: "0" },
        },
        "slide-down": {
          "0%": { height: "0" },
          "100%": { height: "var(--radix-accordion-content-height)" },
        },
      },
      fontSize: {
<<<<<<< HEAD
        xs: "var(--font-xs)",
        sm: "var(--font-sm)",
        base: ["var(--font-base)", { fontWeight: "var(--font-weight-base)" }],
        md: "var(--font-md)",
        lg: "var(--font-lg)",
        xl: "var(--font-xl)",
        "2xl": "var(--font-2xl)",
        "3xl": "var(--font-3xl)",
        "4xl": "var(--font-4xl)",
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        label: "var(--label-font-size)",
        micro: ["8px", "12px"],
        xsmall: ["9px", "13px"],
        caption: ["10px", "14px"],
        "caption-md": ["11px", "15px"],
        "body-sm": ["12px", "16px"],
        body: ["13px", "18px"],
        "body-md": ["14px", "20px"],
        "body-lg": ["15px", "21px"],
      },
      animation: {
        "fade-in": "fade-in 0.3s ease-out",
        "slide-in-right": "slide-in-right 0.3s ease-out",
        "slide-in-left": "slide-in-left 0.3s ease-out",
        "slide-up": "slide-up 0.2s ease-out",
        "slide-down": "slide-down 0.2s ease-out",
      },
    },
  },
  plugins: [
    require("tailwindcss-animate"),
    require("@tailwindcss/forms"),
  ],
}
