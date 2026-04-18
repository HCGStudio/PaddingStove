/** @type {import('tailwindcss').Config} */
module.exports = {
  content: ["./src/**/*.{html,ts,tsx}"],
  theme: {
    extend: {
      colors: {
        parchment: "#f5f4ed",
        ivory: "#faf9f5",
        sand: "#e8e6dc",
        "border-cream": "#f0eee6",
        "border-warm": "#e8e6dc",
        "ring-warm": "#d1cfc5",
        "ring-deep": "#c2c0b6",
        "near-black": "#141413",
        "dark-surface": "#30302e",
        "charcoal-warm": "#4d4c48",
        "olive-gray": "#5e5d59",
        "stone-gray": "#87867f",
        "warm-silver": "#b0aea5",
        terracotta: "#c96442",
        coral: "#d97757",
      },
      fontFamily: {
        serif: [
          '"Anthropic Serif"',
          "Georgia",
          '"Times New Roman"',
          "serif",
        ],
        sans: [
          '"Anthropic Sans"',
          "-apple-system",
          "BlinkMacSystemFont",
          '"Segoe UI"',
          "Inter",
          "Arial",
          "sans-serif",
        ],
        mono: [
          '"Anthropic Mono"',
          "ui-monospace",
          "SFMono-Regular",
          "Menlo",
          "monospace",
        ],
      },
      boxShadow: {
        whisper: "0 4px 24px rgba(0, 0, 0, 0.05)",
        "ring-warm": "0 0 0 1px #d1cfc5",
        "ring-deep": "0 0 0 1px #c2c0b6",
        "ring-terracotta": "0 0 0 1px #c96442",
      },
      keyframes: {
        pulseDot: {
          "0%, 100%": { opacity: "1" },
          "50%": { opacity: "0.3" },
        },
      },
      animation: {
        "pulse-dot": "pulseDot 1.6s ease-in-out infinite",
      },
    },
  },
  plugins: [],
};
