window.qfTheme = {
    get: function () {
        try {
            return localStorage.getItem("qf_dark") === "1";
        } catch {
            return false;
        }
    },
    set: function (isDark) {
        try {
            localStorage.setItem("qf_dark", isDark ? "1" : "0");
        } catch { }
    },
    apply: function (isDark) {
        document.documentElement.classList.toggle("dark", !!isDark);
        document.body.classList.toggle("dark", !!isDark);
    }
};
