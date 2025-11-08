// Theme Helper for LeptonX Lite - Blazor Server Compatible
window.themeHelper = {
    // Get system preference
    getSystemTheme: function () {
        if (window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches) {
            return 'dark';
        }
        return 'light';
    },

    // Get current theme from localStorage
    getCurrentTheme: function () {
        return localStorage.getItem('lpx-theme-name');
    },

    // Apply theme immediately without reload (for Blazor Server)
    applyTheme: function (theme) {
        try {
            console.log('Applying theme:', theme);

            // 1. Set localStorage (for persistence)
            localStorage.setItem('lpx-theme-name', theme);

            // 2. Set HTML attribute (THIS IS THE KEY - LeptonX Blazor doesn't do this automatically!)
            document.documentElement.setAttribute('data-lpx-theme', theme);

            // 3. Also set on body for compatibility
            document.body.setAttribute('data-lpx-theme', theme);

            console.log('Theme applied successfully:', theme);
            return true;
        } catch (error) {
            console.error('Error changing theme:', error);
            return false;
        }
    },

    // Initialize theme on page load
    initTheme: function() {
        var savedTheme = localStorage.getItem('lpx-theme-name');
        if (savedTheme) {
            console.log('Initializing saved theme:', savedTheme);
            document.documentElement.setAttribute('data-lpx-theme', savedTheme);
            document.body.setAttribute('data-lpx-theme', savedTheme);
        }
    }
};

// Auto-initialize theme when script loads
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', function() {
        window.themeHelper.initTheme();
    });
} else {
    window.themeHelper.initTheme();
}
