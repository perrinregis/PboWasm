window.themeManager = {
    setTheme: (theme) => {
        document.documentElement.setAttribute('data-theme', theme);
        localStorage.setItem('selected-theme', theme);
    },
    getTheme: () => {
        return localStorage.getItem('selected-theme') || 'light';
    },
    initTheme: () => {
        const savedTheme = localStorage.getItem('selected-theme') || 'light';
        document.documentElement.setAttribute('data-theme', savedTheme);
        return savedTheme;
    }
};