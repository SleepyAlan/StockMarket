(() => {
    const root = document.documentElement;
    const toggle = document.getElementById("sidebar-toggle");
    const panel = document.getElementById("app-sidebar");
    if (!toggle || !panel) return;

    const desktop = matchMedia("(min-width: 992px)");
    const isOpen = () => root.dataset.sidebar === "open";

    // data-sidebar drives the hamburger icon. On desktop it also drives the rail;
    // below lg it mirrors whether the offcanvas drawer is open.
    function setState(open, remember = false) {
        root.dataset.sidebar = open ? "open" : "closed";
        toggle.setAttribute("aria-expanded", String(open));
        if (remember) {
            try { localStorage.setItem("sidebar", open ? "open" : "closed"); } catch { }
        }
    }

    setState(isOpen());   // sync aria-expanded with the state the head script chose

    toggle.addEventListener("click", () => {
        if (desktop.matches) {
            setState(!isOpen(), true);                                 // rail: collapse/expand, remembered
        } else {
            bootstrap.Offcanvas.getOrCreateInstance(panel).toggle();   // drawer
        }
    });

    // Keep the icon in step with the drawer, however it was closed (X button, backdrop, Escape).
    // Ignored on desktop, where the offcanvas is inert and the rail owns the state.
    panel.addEventListener("show.bs.offcanvas", () => { if (!desktop.matches) setState(true); });
    panel.addEventListener("hidden.bs.offcanvas", () => { if (!desktop.matches) setState(false); });

    // Crossing the breakpoint: restore the saved desktop state, or start closed for mobile
    desktop.addEventListener("change", (e) => {
        let saved;
        try { saved = localStorage.getItem("sidebar"); } catch { }
        setState(e.matches ? saved !== "closed" : false);
    });
})();