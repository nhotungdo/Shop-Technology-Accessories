// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Simple toast notification system
window.AppToast = (function () {
    let container;
    function ensureContainer() {
        if (!container) {
            container = document.createElement('div');
            container.id = 'toast-container';
            container.style.position = 'fixed';
            container.style.top = '20px';
            container.style.right = '20px';
            container.style.zIndex = '1080';
            container.style.display = 'flex';
            container.style.flexDirection = 'column';
            container.style.gap = '10px';
            document.body.appendChild(container);
        }
    }
    function show(message, type = 'success', timeout = 3000) {
        ensureContainer();
        const el = document.createElement('div');
        el.textContent = message;
        el.style.padding = '12px 16px';
        el.style.borderRadius = '10px';
        el.style.boxShadow = '0 6px 20px rgba(0,0,0,.15)';
        el.style.background = type === 'success' ? '#10b981' : (type === 'error' ? '#ef4444' : '#6b7280');
        el.style.color = '#fff';
        el.style.fontWeight = '600';
        el.style.opacity = '0';
        el.style.transform = 'translateY(-8px)';
        el.style.transition = 'opacity .2s ease, transform .2s ease';
        container.appendChild(el);
        requestAnimationFrame(() => {
            el.style.opacity = '1';
            el.style.transform = 'translateY(0)';
        });
        setTimeout(() => {
            el.style.opacity = '0';
            el.style.transform = 'translateY(-8px)';
            setTimeout(() => container.removeChild(el), 200);
        }, timeout);
    }
    return { show };
})();
