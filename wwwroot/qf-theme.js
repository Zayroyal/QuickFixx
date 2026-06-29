// =====================================================
// QUICKFIX THEME MANAGER
// =====================================================

window.qfTheme = {
    get: function () {
        const savedTheme = localStorage.getItem("quickfix-theme");

        return savedTheme === "dark";
    },

    set: function (isDark) {
        if (isDark) {
            localStorage.setItem("quickfix-theme", "dark");
        } else {
            localStorage.setItem("quickfix-theme", "light");
        }

        window.qfTheme.apply(isDark);
    },

    apply: function (isDark) {
        if (isDark) {
            document.documentElement.classList.add("dark");
            document.body.classList.add("dark");
            document.body.classList.add("dark-mode");
        } else {
            document.documentElement.classList.remove("dark");
            document.body.classList.remove("dark");
            document.body.classList.remove("dark-mode");
        }
    },

    load: function () {
        const isDark = window.qfTheme.get();

        window.qfTheme.apply(isDark);
    }
};

// =====================================================
// LOAD THEME IMMEDIATELY
// =====================================================

window.qfTheme.load();