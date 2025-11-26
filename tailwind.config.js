/** @type {import('tailwindcss').Config} */
module.exports = {
    darkMode: 'class',
    mode: 'jit',
    content: [
        "./Views/Shared/**/*.cshtml",
        "./Views/Threads/**/*.cshtml",
        "./Views/Users/**/*.cshtml",
        "./Views/**/*.cshtml",
    ],
    theme: {
        extend: {
            screens: {
                'xs': '480px',
                'sm': '640px',
                'md': '768px',
                'lg': '1024px',
                'xl': '1280px',
                'xll': '1524px',
                '2xl': '1921px',
            },
            colors: {
                'coffee': {
                    'brown': '#6F4E37',
                    'brown-dark': '#5C4033',
                    'brown-light': '#8B6F47',
                    'cream': '#FFF8DC',
                    'cream-light': '#FFFEF5',
                },
                'pale-green': '#8fbc8f',
                'pale-green-dark': '#7a9f7a',
                'pale-green-light': '#a4c9a4',
            },
            animation: {
                'slide-in': 'slideIn 0.5s ease-out',
              },
              keyframes: {
                slideIn: {
                  '0%': {
                    opacity: '0',
                    transform: 'translateY(50px)',
                  },
                  '100%': {
                    opacity: '1',
                    transform: 'translateY(0)',
                  },
                },
              },
        },
        color: {
            "rundle": "#5b1c32",
        }
    },
    plugins: [],
}