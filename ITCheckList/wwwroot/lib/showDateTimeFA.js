function updateHeaderDateTime() {
    const now = new Date();

    const days = ['یک‌شنبه', 'دوشنبه', 'سه‌شنبه', 'چهارشنبه', 'پنج‌شنبه', 'جمعه', 'شنبه'];
    const dayName = days[now.getDay()];

    const faDate = now.toLocaleDateString('fa-IR-u-nu-fa', {
        year: 'numeric',
        month: 'long',
        day: 'numeric'
    });

    const time = now.toLocaleTimeString('fa-IR', {
        hour: '2-digit',
        minute: '2-digit',
        second: '2-digit'
    });

    const dateEl = document.getElementById('dateText');
    const timeEl = document.getElementById('timeText');

    if (dateEl) dateEl.textContent = `${dayName} ${faDate}`;
    if (timeEl) timeEl.textContent = time;
}

updateHeaderDateTime();
setInterval(updateHeaderDateTime, 1000); // بروزرسانی هر ثانیه
