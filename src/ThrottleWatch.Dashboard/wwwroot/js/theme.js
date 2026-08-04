window.twTheme = {
  get: function () {
    try {
      return localStorage.getItem("tw-theme");
    } catch {
      return null;
    }
  },
  set: function (theme) {
    var next = theme === "light" ? "light" : "dark";
    try {
      localStorage.setItem("tw-theme", next);
    } catch {
      /* ignore quota / private mode */
    }
    document.documentElement.classList.remove("dark", "light");
    document.documentElement.classList.add(next);
  }
};
