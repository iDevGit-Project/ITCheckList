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

// تبدیل تاریخ میلادی به شمسی (تابع سبک و دقیق)
function toPersianDate(date) {
    const d = new Date(date);
    const gy = d.getFullYear();
    const gm = d.getMonth() + 1;
    const gd = d.getDate();

    // الگوریتم تبدیل (دقیق برای بیشتر کاربردها)
    const g_d_m = [0, 31, 59, 90, 120, 151, 181, 212, 243, 273, 304, 334];
    let jy = (gy <= 1600) ? 0 : 979;
    gy -= (gy <= 1600) ? 621 : 1600;

    let gy2 = (gm > 2) ? gy + 1 : gy;
    let days = (365 * gy) + Math.floor((gy2 + 3) / 4) - Math.floor((gy2 + 99) / 100) + Math.floor((gy2 + 399) / 400) - 80 + gd + g_d_m[gm - 1];
    jy += 33 * Math.floor(days / 12053); days %= 12053;
    jy += 4 * Math.floor(days / 1461); days %= 1461;
    if (days > 365) {
        jy += Math.floor((days - 1) / 365);
        days = (days - 1) % 365;
    }
    const jm = (days < 186) ? 1 + Math.floor(days / 31) : 7 + Math.floor((days - 186) / 30);
    const jd = 1 + ((days < 186) ? (days % 31) : ((days - 186) % 30));

    return `${jy}/${jm.toString().padStart(2, '0')}/${jd.toString().padStart(2, '0')}`;
}
