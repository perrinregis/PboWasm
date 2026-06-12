/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./Pages/**/*.{razor,html}",
    "./Shared/**/*.{razor,html}",
    "./Layout/**/*.{razor,html}",
    "./wwwroot/index.html",
    "./**/*.razor"
  ],
  theme: {
    extend: {},
  },
  plugins: [require("daisyui")],
  daisyui: {
    themes: ["light", "dark", "cupcake"],
  },
}
