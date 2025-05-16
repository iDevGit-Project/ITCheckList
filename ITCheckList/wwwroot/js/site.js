// مقدار تاخیر مرکزی (قابل تنظیم در پروژه)
const GLOBAL_LOADING_DELAY = 200; // به میلی‌ثانیه

function executeWithOverlay(callback, delay = GLOBAL_LOADING_DELAY) {
    showGlobalOverlay(); // نمایشی از Overlay شما
    setTimeout(() => {
        hideGlobalOverlay(); // بعد از اجرا می‌تونه پنهان بشه یا تا زمان تمام شدن عملیات بمونه
        callback();
    }, delay);
}
function showGlobalOverlay() {
    const overlay = document.getElementById('globalOverlay');
    if (overlay) overlay.style.display = 'flex';
}

function hideGlobalOverlay() {
    const overlay = document.getElementById('globalOverlay');
    if (overlay) overlay.style.display = 'none';
}
