function printFullTable() {
    fetch('/Checklist/IsCheckItemTableEmpty')
        .then(response => {
            if (!response.ok) {
                throw new Error('خطا در دریافت اطلاعات جدول.');
            }
            return response.json();
        })
        .then(data => {
            if (data.isEmpty) {
                Swal.fire({
                    icon: 'warning',
                    title: 'هشدار!',
                    html: '<div style="direction: rtl;">هیچ داده‌ای برای دریافت خروجی وجود ندارد.</div>',
                    confirmButtonText: 'متوجه شدم',
                    customClass: {
                        popup: 'swal2-rtl'
                    }
                });
            } else {
                // تأخیر کوتاه برای اطمینان از رندر کامل جدول
                setTimeout(() => {
                    const table = document.getElementById("reportTable");
                    if (!table) {
                        alert("جدول پیدا نشد.");
                        return;
                    }

                    const clonedTable = table.cloneNode(true);

                    // حذف ستون عملیات
                    Array.from(clonedTable.rows).forEach(row => {
                        if (row.cells.length > 0) {
                            row.deleteCell(row.cells.length - 1);
                        }
                    });

                    // اعمال رنگ به سطرهایی که وضعیت "انجام شد" دارند
                    Array.from(clonedTable.rows).forEach(row => {
                        const statusCell = row.cells[5];
                        if (statusCell && statusCell.textContent.includes("انجام شد")) {
                            row.style.backgroundColor = "#00b89b14";
                        }
                    });

                    // تاریخ و ساعت شمسی
                    const now = new Date();
                    const faDate = now.toLocaleDateString('fa-IR');
                    const time = now.toLocaleTimeString('fa-IR', { hour: '2-digit', minute: '2-digit' });
                    const dateTimeFooter = `
                        <div class="print-footer">
                            تاریخ و ساعت چاپ گزارش: ${faDate} - ${time}
                        </div>
                    `;

                    // فونت و استایل چاپ
                    const base64Font = `
                        @font-face {
                            font-family: 'IRANYekan';
                            src: url('/fonts/IRANYekanXFaNum-Regular.ttf') format('truetype');
                            font-weight: normal;
                            font-style: normal;
                        }
                    `;

                    const printStyle = `
                        ${base64Font}
                        body {
                            direction: rtl;
                            font-family: 'IRANYekan', sans-serif;
                        }
                        .print-header {
                            text-align: center;
                            font-size: 18px;
                            margin-bottom: 10px;
                            font-weight: bold;
                        }
                        .print-footer {
                            margin-top: 20px;
                            text-align: center;
                            font-size: 12px;
                            font-family: 'IRANYekan', sans-serif;
                            color: #333;
                        }
                        table {
                            width: 100%;
                            border-collapse: collapse;
                            font-size: 13px;
                            direction: rtl;
                        }
                        th, td {
                            border: 1px solid black;
                            padding: 8px;
                        }
                        th {
                            background-color: #f8f9fa;
                            font-weight: bold;
                        }
                        @page {
                            margin: 8mm;
                        }
                    `;

                    const header = '<div class="print-header">چک لیست بررسی تجهیزات دپارتمان واحد فناوری اطلاعات</div>';

                    printJS({
                        printable: header + clonedTable.outerHTML + dateTimeFooter,
                        type: "raw-html",
                        header: "",
                        style: printStyle,
                    });

                }, 100); // تأخیر 100 میلی‌ثانیه برای اطمینان از رندر
            }
        })
        .catch(error => {
            Swal.fire({
                icon: 'error',
                title: 'خطا!',
                html: `<div style="direction: rtl;">${error.message}</div>`,
                confirmButtonText: 'باشه'
            });
        });
}

