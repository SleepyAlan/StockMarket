(() => {
    const root = document.documentElement;
    const toggle = document.getElementById("sidebar-toggle");
    if (!toggle) return;

    const desktop = matchMedia("(min-width: 992px)");
    const isOpen = () => root.dataset.sidebar === "open";

    function setSidebar(open, remember = false) {
        root.dataset.sidebar = open ? "open" : "closed";
        toggle.setAttribute("aria-expanded", String(open));
        if (remember) {
            try { localStorage.setItem("sidebar", open ? "open" : "closed"); } catch { }
        }
    }

    setSidebar(isOpen());   // sync aria-expanded with the state the head script chose

    // Only the desktop choice is remembered; mobile always starts closed
    toggle.addEventListener("click", () => setSidebar(!isOpen(), desktop.matches));

    // Backdrop click closes the mobile overlay
    document.addEventListener("click", (e) => {
        if (e.target.closest("[data-sidebar-close]")) setSidebar(false);
    });

    document.addEventListener("keydown", (e) => {
        if (e.key === "Escape" && isOpen() && !desktop.matches) {
            setSidebar(false);
            toggle.focus();
        }
    });

    // Crossing the breakpoint: restore the saved desktop state, or close for mobile
    desktop.addEventListener("change", (e) => {
        let saved;
        try { saved = localStorage.getItem("sidebar"); } catch { }
        setSidebar(e.matches ? saved !== "closed" : false);
    });
})();