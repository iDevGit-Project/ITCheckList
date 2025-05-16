function showOverlay(delay = 1250) {
    return new Promise(resolve => {
        $('#loadingOverlay').fadeIn(200);
        setTimeout(() => resolve(), delay);
    });
}

function hideOverlay() {
    $('#loadingOverlay').fadeOut(220);
}

document.addEventListener("DOMContentLoaded", () => {
    // هندل فرم‌هایی که data-loading دارند
    document.querySelectorAll("form[data-loading='true']").forEach(form => {
        form.addEventListener("submit", async e => {
            e.preventDefault();
            await showOverlay();
            form.submit(); // ارسال فرم بعد از نمایش Overlay
        });
    });

    document.querySelectorAll("[data-loading='true']").forEach(elem => {
        if (elem.tagName !== "FORM") {
            elem.addEventListener("click", async e => {
                const action = elem.dataset.action;
                const href = elem.getAttribute("href");

                if (href || action) {
                    e.preventDefault();
                    await showOverlay();

                    if (href) window.location.href = href;
                    else if (action && typeof window[action] === 'function') {
                        window[action]();
                    }
                }
            });
        }
    });
});