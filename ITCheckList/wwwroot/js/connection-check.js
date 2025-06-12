const MAX_ATTEMPTS = 3;
const LOCK_DURATION_MS = 60 * 1000;

document.addEventListener("DOMContentLoaded", () => {
    initiateConnectionCheck();
});

function initiateConnectionCheck() {
    // ❗ جلوگیری از نمایش مجدد swal بررسی اتصال اگر قبلاً تایید شده باشد
    if (localStorage.getItem('connectionVerified') === 'true') return;

    Swal.fire({
        title: '🔎 در حال بررسی اتصال به پایگاه داده...',
        allowOutsideClick: false,
        allowEscapeKey: false,
        backdrop: 'rgba(0,0,0,0.6) blur(8px)',
        didOpen: () => Swal.showLoading()
    });

    fetch('/Connection/Check', { cache: "no-store" })
        .then(res => {
            if (!res.ok) throw new Error();
            return res.text();
        })
        .then(() => {
            localStorage.setItem('connectionVerified', 'true'); // ✅ اتصال تأیید شد → دیگر نشان داده نمی‌شود
            Swal.close();
        })
        .catch(() => {
            localStorage.removeItem('connectionVerified'); // ❗ خطا → مطمئن شو flag برداشته بشه
            Swal.close();
            showConnectionForm();
        });
}

function showConnectionForm() {
    const lockUntil = localStorage.getItem('connectionLockUntil');
    const now = Date.now();

    if (lockUntil && now < parseInt(lockUntil)) {
        showLockedForm(parseInt(lockUntil) - now);
        return;
    }

    renderConnectionForm();
}

function renderConnectionForm() {
    Swal.fire({
        title: '⚙️ تنظیم اتصال پایگاه داده',
        html: `
            <input id="server" class="swal2-input" placeholder="نام سرور">
            <input id="database" class="swal2-input" placeholder="نام دیتابیس">
            <select id="authType" class="swal2-select">
                <option value="windows">Windows Authentication</option>
                <option value="sql">SQL Server Authentication</option>
            </select>
            <input id="username" class="swal2-input" placeholder="نام کاربری (SQL)">
            <input id="password" class="swal2-input" type="password" placeholder="کلمه عبور (SQL)">
        `,
        confirmButtonText: 'بررسی اتصال',
        allowOutsideClick: false,
        allowEscapeKey: false,
        allowEnterKey: true,
        showCancelButton: false,
        backdrop: 'rgba(0,0,0,0.6) blur(8px)',
        preConfirm: () => ({
            server: document.getElementById('server').value.trim(),
            database: document.getElementById('database').value.trim(),
            authenticationType: document.getElementById('authType').value,
            username: document.getElementById('username').value.trim(),
            password: document.getElementById('password').value
        }),
        didOpen: () => {
            toggleAuthFields();
            document.getElementById('authType').addEventListener('change', toggleAuthFields);
        }
    }).then((result) => {
        if (result.isConfirmed) {
            testConnection(result.value);
        }
    });
}

function toggleAuthFields() {
    const authType = document.getElementById('authType').value;
    const usernameField = document.getElementById('username');
    const passwordField = document.getElementById('password');

    if (authType === "windows") {
        usernameField.style.display = "none";
        passwordField.style.display = "none";
    } else {
        usernameField.style.display = "";
        passwordField.style.display = "";
    }
}

function testConnection(data) {
    let connectionString = `Server=${data.server};Database=${data.database};`;
    if (data.authenticationType === "windows") {
        connectionString += `Trusted_Connection=True;`;
    } else {
        connectionString += `User ID=${data.username};Password=${data.password};Trusted_Connection=False;`;
    }
    connectionString += `Trust Server Certificate=true;MultipleActiveResultSets=true;`;

    Swal.fire({
        title: 'در حال بررسی اتصال...',
        allowOutsideClick: false,
        allowEscapeKey: false,
        backdrop: 'rgba(0,0,0,0.6) blur(8px)',
        didOpen: () => Swal.showLoading()
    });

    fetch('/Connection/GetCurrentConnectionString')
        .then(res => {
            if (!res.ok) throw new Error();
            return res.text();
        })
        .then(currentConnectionString => {
            if (currentConnectionString.trim() === connectionString.trim()) {
                resetConnectionAttempts();
                localStorage.setItem('connectionVerified', 'true'); // ✅ اتصال موفق → جلوگیری از بررسی‌های مجدد
                Swal.close();
                location.reload();
            } else {
                fetch('/Connection/TestLoginConnection', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(data)
                })
                    .then(response => {
                        if (response.status === 429) return response.text().then(msg => { throw new Error(msg); });
                        if (!response.ok) return response.text().then(msg => { throw new Error(msg); });
                        return response.text();
                    })
                    .then(msg => {
                        resetConnectionAttempts();
                        localStorage.setItem('connectionVerified', 'true'); // ✅ اتصال موفق → جلوگیری از بررسی‌های مجدد
                        Swal.fire("✅ عملیات موفق", msg, "success").then(() => location.reload());
                    })
                    .catch(err => {
                        localStorage.removeItem('connectionVerified'); // ❗ خطا → حذف flag → بررسی مجدد در آینده
                        incrementConnectionAttempts();
                        Swal.fire("❌ خطا", err.message, "error").then(() => {
                            showConnectionForm();
                        });
                    });
            }
        })
        .catch(() => {
            localStorage.removeItem('connectionVerified'); // ❗ خطا → حذف flag → بررسی مجدد در آینده
            Swal.fire("❌ خطا", "عدم دسترسی به رشته اتصال فعلی.", "error");
        });
}

function incrementConnectionAttempts() {
    let attempts = parseInt(localStorage.getItem('connectionAttempts') || '0') + 1;
    localStorage.setItem('connectionAttempts', attempts);

    if (attempts >= MAX_ATTEMPTS) {
        const lockUntil = Date.now() + LOCK_DURATION_MS;
        localStorage.setItem('connectionLockUntil', lockUntil);
        localStorage.removeItem('connectionAttempts');
        showLockedForm(LOCK_DURATION_MS);
    }
}

function showLockedForm(remainingMs) {
    let remaining = Math.ceil(remainingMs / 1000);

    const timerInterval = setInterval(() => {
        remaining--;
        Swal.update({
            title: '!مسدود شدن به دلیل تلاش‌های ناموفق',
            html: `⏳ لطفاً <b>${remaining}</b> ثانیه دیگر صبر کنید...`
        });

        if (remaining <= 0) {
            clearInterval(timerInterval);
            Swal.close();
            renderConnectionForm();
        }
    }, 1000);

    Swal.fire({
        title: '⛔️ مسدود شد',
        html: `⏳ لطفاً <b>${remaining}</b> ثانیه دیگر صبر کنید...`,
        allowOutsideClick: false,
        allowEscapeKey: false,
        showConfirmButton: false,
        backdrop: 'rgba(0,0,0,0.6) blur(8px)'
    });
}

function resetConnectionAttempts() {
    localStorage.removeItem('connectionAttempts');
    localStorage.removeItem('connectionLockUntil');
}
